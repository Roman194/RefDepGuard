// See https://aka.ms/new-console-template for more information
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using Microsoft.Build.Construction;
using RefDepGuard.Console.Models;
using RefDepGuard.Console.Managers;

namespace RefDepGuard.Console
{
    class MainCommand
    {
        private static Dictionary<string, ProjectState> currentSolState = new Dictionary<string, ProjectState>();
        private static ConfigFilesData configFilesData;

        private static string rootDirectory = "";
        private static string solutionName = "";

        private static void Main(string[] args)
        {
            System.Console.WriteLine("Вас приветсвует консольный RefDepGuard! Дождитесь полного завершения проверки");

            //return -1;

            MSBuildLocator.RegisterDefaults();
            
            GetCurrentSolutionState();
            GetConfigFilesData();
            CheckRules();
        }

        private static void GetCurrentSolutionState()
        {
            //Console.WriteLine(Directory.GetCurrentDirectory());

            rootDirectory = @"C:\Users\zuzinra\source\repos\Mir.Controller.Cfg"; //Должно будет быть равно Directory.GetCurrentDirectory(), когда .exe будет лежать в руте!
            solutionName = rootDirectory.Split("\\").Last();

            System.Console.WriteLine("Выполняется проверка для Solution: " + solutionName + "\r\n");
            System.Console.WriteLine("1. Парсинг состояния решения");

            currentSolState = CurrentStateConsoleManager.GetCurrentSolutionState(rootDirectory + @"\" + solutionName + ".sln");

            if(currentSolState.Count == 0)
            {
                System.Console.WriteLine("-> Парсинг состояния решения - Fail");
                //Show File not found error / File deviant value error

                Environment.Exit(-1); //Завершение проги с ошибкой
            }

            System.Console.WriteLine("-> Парсинг состояния решения - Success");
        }

        private static void GetConfigFilesData()
        {
            System.Console.WriteLine("\r\n2. Парсинг значений конфиг-файлов");

            configFilesData = ConfigFileConsoleManager.GetInfoFromConfigFiles(rootDirectory, solutionName);

            if(configFilesData.ParseError != FileParseError.None)
            {
                System.Console.WriteLine("-> Парсинг значений конфиг-файлов - Fail");

                //Show problems with config files errors (syntax error / file not found)

                Environment.Exit(-1);
            }

            System.Console.WriteLine("-> Парсинг значений конфиг-файлов - Success");
        }

        private static void CheckRules()
        {
            System.Console.WriteLine("\r\n3. Проверка соответствия состояния заявленным правилам");

            //Вызов соотв. метода (CheckRulesManager)

            System.Console.WriteLine("-> Проверка соответствия состояния заявленным правилам - Success");

            //Если обнаружены какие-то "проблемы"
            System.Console.WriteLine("\r\nОбнаруженные в результате проверки 'проблемы': (ошибок - 3; предупреждений - 0)");

            //Вызов соотв метода (ProblemsUploadToConsoleManager)
        }
    }  
}