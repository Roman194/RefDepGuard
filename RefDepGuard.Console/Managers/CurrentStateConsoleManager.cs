using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using RefDepGuard.Applied.Models;
using RefDepGuard.TargetFramework;

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
                            (targetFramework, targetFrameworkNums) = TFManager.GetTargetFrameworkInStringNTransferFormats(currentProject);

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
    }
}