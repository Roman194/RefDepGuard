using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RefDepGuard.Data;

namespace RefDepGuard
{
    /// <summary>
    /// This class is responsible for managing the current state of the solution.
    /// </summary>
    public class CurrentStateManager
    {
        /// <summary>
        /// Gets the current state of the projects in the solution. It includes the target framework(s) and references of each project.
        /// </summary>
        /// <param name="dte">DTE interface value</param>
        /// <returns>current projects state dictionary</returns>
        public static Dictionary<string, ProjectState> GetCurrentProjectState(DTE dte)
        {
            return GetCurrentRequiredState(dte, false);
        }

        /// <summary>
        /// Gets the current state of the project references in the solution. 
        /// It includes only the references of each project (without target framework(s) info).
        /// </summary>
        /// <param name="dte">DTE interface value</param>
        /// <returns>current references state dictionary</returns>
        public static Dictionary<string, List<string>> GetCurrentReferencesState(DTE dte)
        {
            Dictionary<string, List<string>> currentReferences = 
                GetCurrentRequiredState(dte, true).ToDictionary(
                    project => project.Key, 
                    project => project.Value.CurrentReferences
                );  

            return currentReferences;
        }

        /// <summary>
        /// Gets the current required state of the projects in the solution (with or without TF-s). It includes the target framework(s) and references of each 
        /// project.
        /// </summary>
        /// <param name="dte">DTE interface value</param>
        /// <param name="isOnlyRefsNeeded">shows if only refs is needed or also TF-s</param>
        /// <returns>current projects state dictionary</returns>
        private static Dictionary<string, ProjectState> GetCurrentRequiredState(DTE dte, bool isOnlyRefsNeeded)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Dictionary<string, ProjectState> commitedProjState = new Dictionary<string, ProjectState>();
            EnvDTE.Solution solution = dte.Solution;

            foreach (EnvDTE.Project project in solution.Projects)//For each project of the solution
            {
                if (project.FullName != null && project.FullName.Length != 0)//If the project is loaded
                {
                    string projectFrameworkVersions = "";
                    Dictionary<string, List<int>> projectFrameworkNumVersions = new Dictionary<string, List<int>>();

                    if (!isOnlyRefsNeeded)
                    {
                        projectFrameworkVersions = MSBuildManager.GetTargetFrameworkForProject(project.FullName);
                        projectFrameworkNumVersions = ConvertTargetFrameworkToTransferFormat(projectFrameworkVersions);
                    }

                    VSLangProj.VSProject vSProject = project.Object as VSLangProj.VSProject;

                    if (vSProject != null)
                    {
                        var refsList = new List<string>();

                        foreach (VSLangProj.Reference vRef in vSProject.References)
                        {
                            if (vRef.SourceProject != null)
                                refsList.Add(vRef.Name);
                        }
                        //adds the project state to the dictionary with the project name as a key and the ProjectState object as a value (optionally adds TF-s info)
                        commitedProjState.Add(vSProject.Project.Name, new ProjectState(projectFrameworkNumVersions, projectFrameworkVersions, refsList));
                    }
                }
            }

            return commitedProjState;
        }

        /// <summary>
        /// Converts the target framework string to a dictionary format that can be easily compared with the target frameworks from the rules.
        /// </summary>
        /// <param name="targetFrameworkString">current target framework string</param>
        /// <returns>target framework string in a dictionary format</returns>
        private static Dictionary<string, List<int>> ConvertTargetFrameworkToTransferFormat(string targetFrameworkString)
        {
            Dictionary<string, List<int>> currentTargetFrameworksDict = new Dictionary<string, List<int>>();

            if (String.IsNullOrEmpty(targetFrameworkString)) //if the string is empty, then we return an empty dictionary
                return currentTargetFrameworksDict;

            //In case when string came from TargetFrameworks, we needs to firstly split it by ";"
            var currentProjectSupportedFrameworksArray = targetFrameworkString.Split(';');

            foreach (string currentProjectFramework in currentProjectSupportedFrameworksArray)//For each TargetFramework parameter
            {
                //Split the TargetFramework by "-" to separate the main TF version and the additional TF info (if it exists).
                //Example: net5.0-windows1.2 -> net5.0 и windows1.2
                var currentProjFrameworkArray = currentProjectFramework.Split('-');

                //Creates a list of nums of the TF version and determines its type
                //Important: not all TF-s contain dots! Example: net45 - it shouldn't be a problem, because we will split it by each num later
                //and get the same result as for net4.5
                var currentProjFrameworkVersionArray = currentProjFrameworkArray[0].Split('.');
                var currentProjFrameworkVersionArrayLength = currentProjFrameworkVersionArray.Length;

                //We need to remove all space to make match work correctly!
                currentProjFrameworkVersionArray[0] = currentProjFrameworkVersionArray[0].Replace(" ", "");

                var currentProjFrameworkMatch = Regex.Match(currentProjFrameworkVersionArray[0], @"^([a-zA-Z]+)(\d+)$");
                var currentProjFrameworkType = "-";

                if (currentProjFrameworkMatch.Success)//If the match is successful,  
                {
                    //then we can determine the type of the framework and get the version numbers without any letters.
                    currentProjFrameworkType = currentProjFrameworkMatch.Groups[1].Value;
                    currentProjFrameworkVersionArray[0] = currentProjFrameworkMatch.Groups[2].Value;

                    switch (currentProjFrameworkType)
                    {//cases with old and new .net framework project with TargetFrameworkVersion and .NET needs to be determined separately
                        case "v":
                            currentProjFrameworkType = "netf";
                            break;
                        case "net":
                            currentProjFrameworkType = currentProjFrameworkVersionArrayLength < 2 ? "netf" : "net";
                            break;
                            //As .NET and .NET Framework has the same TFM-s, the "netf" for .net framework TFM were determined inside this extention!
                    }
                    //As the new "netf" is writes without dots, we need to customly split it by each num to get the same result as for old "netf" with dots.
                    //Example: net5 -> net5.0
                    if (currentProjFrameworkType == "netf" && currentProjFrameworkVersionArrayLength < 2)
                        currentProjFrameworkVersionArray = SplitStrByEachNum(currentProjFrameworkVersionArray[0]);

                }

                List<int> currentProjFrameworkVersionList = ConvertTargetFrameworkVersionToIntNums(currentProjFrameworkVersionArray);

                // At this point if the currentProjFrameworkVersionList is empty, it means that we couldn't parse the TF version to int nums, so we just returns
                //previous successful parsed TF-s incide the dictionary
                if (currentProjFrameworkVersionList.Count == 0) 
                    return currentTargetFrameworksDict;

                if (currentTargetFrameworksDict.ContainsKey(currentProjFrameworkType))//If there is already some TF version for this project type,
                {
                    //then we need to compare them and commit the MAX one of them
                    List<int> commitedTargetFrameworkVersionList = currentTargetFrameworksDict[currentProjFrameworkType];
                    for(int i = 0; i < currentProjFrameworkVersionList.Count; i++)
                    {
                        int currentProjTargetFrameworkNum = currentProjFrameworkVersionList[i];
                        int commitedTargetFrameworkNum = commitedTargetFrameworkVersionList[i];

                        if (currentProjTargetFrameworkNum > commitedTargetFrameworkNum)
                        {
                            currentTargetFrameworksDict[currentProjFrameworkType] = currentProjFrameworkVersionList;
                            break;
                        }
                    }
                }
                else
                {   //Alernatively just add the TF version to the dictionary
                    currentTargetFrameworksDict.Add(currentProjFrameworkType, currentProjFrameworkVersionList);
                }
            }

            return currentTargetFrameworksDict;
        }

        /// <summary>
        /// Splits the string by each num. Example: net5 -> net, 5; net5.0 -> net, 5, 0. 
        /// It is needed to make the comparison of the TF versions correct, because not all TF-s contain dots and we need to get the same result for net50 and net5.0.
        /// </summary>
        /// <param name="currentString">current string value</param>
        /// <returns>result string array</returns>
        private static string[] SplitStrByEachNum(string currentString)
        {
            int currentStringLength = currentString.Length;
            string[] resultString = new string[currentStringLength];

            for (int i = 0; i < currentStringLength; i++)
                resultString[i] = currentString[i].ToString();

            return resultString;
        }

        /// <summary>
        /// Converts the target framework version string array to a list of int nums. Example: net5.0 -> 5, 0; net45 -> 4, 5.
        /// </summary>
        /// <param name="targetFrameworkVersionsArray">target framework version array</param>
        /// <returns>list of ints of target framework versions nums</returns>
        private static List<int> ConvertTargetFrameworkVersionToIntNums(string[] targetFrameworkVersionsArray)
        {
            List<int> targetFrameworkVersionsNums = new List<int>();
            for (int i = 0; i < targetFrameworkVersionsArray.Length; i++)
            {
                int currentVersionNum = 0;
                if (!Int32.TryParse(targetFrameworkVersionsArray[i], out currentVersionNum))
                {
                    return new List<int>();
                }
                targetFrameworkVersionsNums.Add(currentVersionNum);
            }

            return targetFrameworkVersionsNums; 
        }
    }
}