// See https://aka.ms/new-console-template for more information
using Microsoft.Build.Locator;
using RefDepGuard.Console.Managers;
using RefDepGuard.CheckRules;
using RefDepGuard.Applied.Models.ConfigFile;
using RefDepGuard.Applied.Models.Project;
using RefDepGuard.Applied.Models.RefDepGuard;

namespace RefDepGuard.Console
{
    class MainCommand
    {
        private static Dictionary<string, ProjectState> CurrentSolState = new Dictionary<string, ProjectState>();
        private static bool IsNoRefsInSolution = false;
        private static ConfigFilesData configFilesData;
        private static RefDepGuardFindedProblems refDepGuardFindedProblems;

        private static string rootDirectory = "";
        private static string solutionName = "";

        private static void Main(string[] args)
        {
            System.Console.SetWindowSize(160, 35);

            System.Console.WriteLine("Вас приветсвует консольный RefDepGuard! Дождитесь полного завершения проверки");

            MSBuildLocator.RegisterDefaults();
            
            GetCurrentSolutionState();
            GetConfigFilesData();
            CheckRules();
            ShowFindedProblemsOnCurrentCheck();
        }

        private static void GetCurrentSolutionState()//Mir.Controller.Cfg
        {
            #if DEBUG
                rootDirectory = @"C:\Users\zuzinra\source\repos\Mir.Controller.Cfg"; //Должно будет быть равно Directory.GetCurrentDirectory(), когда .exe будет лежать в руте!
            #else 
                rootDirectory = Directory.GetCurrentDirectory();
            #endif

            solutionName = rootDirectory.Split("\\").Last();

            System.Console.WriteLine("Выполняется проверка для Solution: " + solutionName + "\r\n");
            System.Console.WriteLine("1. Парсинг состояния решения");

            (CurrentSolState, IsNoRefsInSolution) = CurrentStateConsoleManager.GetCurrentSolutionState(rootDirectory + @"\" + solutionName + ".sln");

            if(CurrentSolState.Count == 0 || IsNoRefsInSolution)
            {
                System.Console.WriteLine("\r\n-> Парсинг состояния решения - Fail\r\n");

                if (CurrentSolState.Count == 0)
                    ProblemsUploadToConsoleManager.UploadInvalidDirectoryError();
                else
                    ProblemsUploadToConsoleManager.UploadRefsNotFoundError();

                Environment.Exit(-1); //Завершение проги с ошибкой
            }

            System.Console.WriteLine("\r\n-> Парсинг состояния решения - Success");
        }

        private static void GetConfigFilesData()
        {
            System.Console.WriteLine("\r\n2. Парсинг значений конфиг-файлов");

            configFilesData = ConfigFileConsoleManager.GetInfoFromConfigFiles(rootDirectory, solutionName, CurrentSolState);

            if(configFilesData.ParseError != FileParseError.None)
            {
                System.Console.WriteLine("\r\n-> Парсинг значений конфиг-файлов - Fail");

                if(configFilesData.ParseError == FileParseError.Global || configFilesData.ParseError == FileParseError.All)
                    ProblemsUploadToConsoleManager.UploadConfigFileSyntaxError(true);

                if(configFilesData.ParseError == FileParseError.Solution || configFilesData.ParseError == FileParseError.All)
                    ProblemsUploadToConsoleManager.UploadConfigFileSyntaxError(false);

                Environment.Exit(-1);
            }

            System.Console.WriteLine("-> Парсинг значений конфиг-файлов - Success");
        }

        private static void CheckRules()
        {
            System.Console.WriteLine("\r\n3. Проверка соответствия состояния заявленным правилам");

            refDepGuardFindedProblems = CheckRulesManager.CheckConfigFileRulesForConsole(configFilesData, CurrentSolState);

            System.Console.WriteLine("-> Проверка соответствия состояния заявленным правилам - Success");
        }

        private static void ShowFindedProblemsOnCurrentCheck()
        {
            if (refDepGuardFindedProblems.IsEmpty())
            {
                System.Console.WriteLine("\r\nВ результате проверки никакие проблемы не обнаружены");
            }
            else
            { //Если обнаружены какие-то "проблемы"
                var errorsCount = refDepGuardFindedProblems.RefDepGuardErrors.Count();
                var warningsCount = refDepGuardFindedProblems.RefDepGuardWarnings.Count();

                System.Console.WriteLine("\r\nОбнаруженные в результате проверки 'проблемы': (ошибок - "+ errorsCount +"; предупреждений - "+ warningsCount +")");
                ProblemsUploadToConsoleManager.UploadCheckRuleProblems(refDepGuardFindedProblems, configFilesData);

                if(errorsCount > 0)
                    Environment.Exit(-1);
            }
        }
    }  
}