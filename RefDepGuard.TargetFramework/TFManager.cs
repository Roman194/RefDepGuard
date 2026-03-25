using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RefDepGuard.TargetFramework
{
    public class TFManager
    {
        public static Tuple<string, Dictionary<string, List<int>>> GetTargetFrameworkInStringNTransferFormats(Project currentProject)
        {
            var targetFrameworkString = GetTargetFrameworkStringForProject(currentProject);
            var targetFrameworkNums = ConvertTargetFrameworkToTransferFormat(targetFrameworkString);

            return Tuple.Create(targetFrameworkString, targetFrameworkNums);
        }

        private static string GetTargetFrameworkStringForProject(Project currentProject)
        {
            string targetFramework = currentProject.GetPropertyValue("TargetFramework");

            if (!string.IsNullOrEmpty(targetFramework))
            {
                return targetFramework;
            }

            string targetFrameworks = currentProject.GetPropertyValue("TargetFrameworks");

            if (!string.IsNullOrEmpty(targetFrameworks))
            {
                return targetFrameworks;
            }

            string targetFrameworkVersion = currentProject.GetPropertyValue("TargetFrameworkVersion");

            if (!string.IsNullOrEmpty(targetFrameworkVersion))
            {
                return targetFrameworkVersion;
            }

            return "";
        }

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
                    for (int i = 0; i < currentProjFrameworkVersionList.Count; i++)
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

        private static string[] SplitStrByEachNum(string currentString)
        {
            int currentStringLength = currentString.Length;
            string[] resultString = new string[currentStringLength];

            for (int i = 0; i < currentStringLength; i++)
                resultString[i] = currentString[i].ToString();

            return resultString;
        }

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