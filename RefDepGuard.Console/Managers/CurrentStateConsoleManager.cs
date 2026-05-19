using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using RefDepGuard.Applied.Models.Project;
using RefDepGuard.Console.Resources;
using RefDepGuard.TargetFramework;

namespace RefDepGuard.Console.Managers
{
    /// <summary>
    /// This class is responsible for managing the current state of the solution. 
    /// It provides method to get the current state of the solution, which includes the projects in the solution, their target frameworks, and their references.
    /// </summary>
    public class CurrentStateConsoleManager
    {
        /// <summary>
        /// The main method of the class. 
        /// It gets the current state of the solution, which includes the projects in the solution, their target frameworks, and their references.
        /// </summary>
        /// <param name="solutionFileFullPath">full string path to the curr solution file</param>
        /// <returns>a tuple with dict of current solution state by projects and bool that shows if there are any refs inside solution</returns>
        public static Tuple<Dictionary<string, ProjectState>, bool> GetCurrentSolutionState(string solutionFileFullPath)
        {
            Dictionary<string, ProjectState> commitedSolState = new Dictionary<string, ProjectState>();
            bool isNoReferencesInSolution = true;

            if (File.Exists(solutionFileFullPath))
            {
                var solutionFile = SolutionFile.Parse(solutionFileFullPath);
                var projects = solutionFile.ProjectsInOrder;

                if (projects.Count > 0)
                {
                    System.Console.WriteLine(Resource.Solution_State_Parse_Start_Message);

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

                            System.Console.WriteLine("\r\n" + Resource.Project_String + projectName + " (" + targetFramework + ")");

                            projectReferences = currentProject.GetItems("ProjectReference").ToList();

                            List<string> referenceNames = projectReferences.ConvertAll(projReference =>
                                    projReference.EvaluatedInclude
                                        .Split("\\").Last() 
                                        .Split("/").Last() //As there are refs that are written through backslash for some reason
                                        .Replace(".csproj", "")
                                );

                            if(referenceNames.Count > 0 && isNoReferencesInSolution == true)
                                isNoReferencesInSolution = false;

                            referenceNames.ForEach(referenceName =>
                                System.Console.WriteLine("   -" + referenceName)
                                );

                            commitedSolState.Add(projectName, new ProjectState(targetFrameworkNums, targetFramework, referenceNames));
                        }
                    }
                }
            }
            
            return new Tuple<Dictionary<string, ProjectState>, bool>(commitedSolState, isNoReferencesInSolution);
        }
    }
}