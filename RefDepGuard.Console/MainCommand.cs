// See https://aka.ms/new-console-template for more information
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using Microsoft.Build.Construction;

namespace RefDepGuard.Console
{
    class MainCommand
    {
        private static void Main(string[] args)
        {
            System.Console.WriteLine("Вас приветсвует консольный RefDepGuard! Дождитесь полного завершения проверки");

            //return -1;

            //MSBuildLocator.RegisterMSBuildPath(@"C:\Program Files\dotnet\sdk\9.0.302");
            MSBuildLocator.RegisterDefaults();
            
            LoadProject();
            
        }

        private static void LoadProject()
        {
            //Console.WriteLine(Directory.GetCurrentDirectory());

            string rootDirectory = @"C:\Users\zuzinra\source\repos\Mir.Controller.Cfg"; //Должно будет быть равно Directory.GetCurrentDirectory(), когда .exe будет лежать в руте!
            string solutionName = rootDirectory.Split("\\").Last() + ".sln";

            System.Console.WriteLine("Выполняется проверка для Solution: " + solutionName + "\r\n");
            System.Console.WriteLine("1. Парсинг состояния решения");

            var solutionFile = SolutionFile.Parse(rootDirectory + @"\" + solutionName);//@"C:\Users\zuzinra\source\repos\Mir.Controller.Cfg\Mir.Controller.Cfg.sln"

            var projects = solutionFile.ProjectsInOrder;

            if (projects.Count > 0)
            {
                System.Console.WriteLine("В решении обнаружены следующие проекты и связи между ними:");

                foreach (var project in projects)
                {
                    //Console.WriteLine(project.ProjectName + ": " + project.AbsolutePath);

                    var projectCollection = new ProjectCollection();

                    var currentProject = projectCollection.LoadProject(project.AbsolutePath);
                    string targetFramework = "";
                    List<ProjectItem> projectReferences = new List<ProjectItem>();


                    if (currentProject != null)
                    {
                        targetFramework = currentProject.GetPropertyValue("TargetFramework") ?? "-";

                        System.Console.WriteLine("Проект: " + project.ProjectName + " (" + targetFramework + ")");

                        projectReferences = currentProject.GetItems("ProjectReference").ToList();

                        foreach (var projectReference in projectReferences)
                        {
                            string refName = projectReference.EvaluatedInclude.Split("\\").Last().Replace(".csproj", "");
                            System.Console.WriteLine("   -" + refName);
                        }

                    }
                }

                System.Console.WriteLine("-> Парсинг состояния решения - Success");
            }
        }
    }
   
}
