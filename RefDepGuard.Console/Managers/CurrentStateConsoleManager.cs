using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using RefDepGuard.CheckRules.Models.Project;
using System.Text.RegularExpressions;

namespace RefDepGuard.Console.Managers
{
    public class CurrentStateConsoleManager
    {

        public static Dictionary<string, ProjectState> GetCurrentSolutionState(string solutionFileFullPath)
        {

            Dictionary<string, ProjectState> commitedSolState = new Dictionary<string, ProjectState>();

            if (File.Exists(solutionFileFullPath))
            {
                var solutionFile = SolutionFile.Parse(solutionFileFullPath);//@"C:\Users\zuzinra\source\repos\Mir.Controller.Cfg\Mir.Controller.Cfg.sln"
                var projects = solutionFile.ProjectsInOrder;

                if (projects.Count > 0)
                {
                    System.Console.WriteLine("В решении обнаружены следующие проекты и связи между ними:");

                    foreach (var project in projects)
                    {
                        var projectCollection = new ProjectCollection();
                        var currentProject = projectCollection.LoadProject(project.AbsolutePath);
                        var projectName = project.ProjectName;

                        List<ProjectItem> projectReferences = new List<ProjectItem>();
                        string targetFramework = "";
                        Dictionary<string, List<int>> targetFrameworkNums = new Dictionary<string, List<int>>();


                        if (currentProject != null)
                        {
                            targetFramework = MSBuildManager.GetTargetFrameworkForProject(currentProject);
                            targetFrameworkNums = ConvertTargetFrameworkToTransferFormat(targetFramework);

                            System.Console.WriteLine("\r\nПроект: " + projectName + " (" + targetFramework + ")");

                            projectReferences = currentProject.GetItems("ProjectReference").ToList();

                            List<string> referenceNames = projectReferences.ConvertAll(projReference =>
                                    projReference.EvaluatedInclude
                                        .Split("\\").Last()
                                        .Split("/").Last() //Т.к. встречаются рефы, которые почему-то записаны через обратный слэш
                                        .Replace(".csproj", "")
                                );

                            referenceNames.ForEach(referenceName =>
                                System.Console.WriteLine("   -" + referenceName)
                                );

                            commitedSolState.Add(projectName, new ProjectState(targetFrameworkNums, targetFramework, referenceNames));//А если tF и tFN идут null?
                        }
                    }
                }
            }
            
            return commitedSolState;
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