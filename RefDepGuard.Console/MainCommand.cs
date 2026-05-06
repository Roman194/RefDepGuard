// See https://aka.ms/new-console-template for more information
using Microsoft.Build.Locator;
using RefDepGuard.Console.Managers;
using RefDepGuard.CheckRules;
using RefDepGuard.Applied.Models.ConfigFile;
using RefDepGuard.Applied.Models.Project;
using RefDepGuard.Applied.Models.RefDepGuard;
using System.Globalization;
using RefDepGuard.Console.Resources;

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

            if (args is { Length: 1 }) //Gets an argument to set Culture Info on current load
            {
                CultureInfo.CurrentCulture =
                    CultureInfo.CurrentUICulture =
                        CultureInfo.GetCultureInfo(args[0]);
            }

            System.Console.WriteLine(Resource.Hello_Message);

            MSBuildLocator.RegisterDefaults();
            
            GetCurrentSolutionState();
            GetConfigFilesData();
            CheckRules();
            ShowFindedProblemsOnCurrentCheck();
        }

        private static void GetCurrentSolutionState()//]WinFormApp
        {
            #if DEBUG //Должно будет быть равно Directory.GetCurrentDirectory(), когда .exe будет лежать в руте!
                rootDirectory = @"C:\Users\zuzinra\source\repos\Mir.Controller.Cfg";
            #else 
                rootDirectory = Directory.GetCurrentDirectory();
            #endif

            solutionName = rootDirectory.Split("\\").Last();

            System.Console.WriteLine(Resource.Solution_Title + solutionName + "\r\n");
            System.Console.WriteLine("1. " + Resource.Solution_State_Parse_Title);

            (CurrentSolState, IsNoRefsInSolution) = CurrentStateConsoleManager.GetCurrentSolutionState(rootDirectory + @"\" + solutionName + ".sln");

            if(CurrentSolState.Count == 0 || IsNoRefsInSolution)
            {
                System.Console.WriteLine("\r\n-> " + Resource.Solution_State_Parse_Title + " - Fail\r\n");

                if (CurrentSolState.Count == 0)
                    ProblemsUploadToConsoleManager.UploadInvalidDirectoryError();
                else
                    ProblemsUploadToConsoleManager.UploadRefsNotFoundError();

                Environment.Exit(-1); //Завершение проги с ошибкой
            }

            System.Console.WriteLine("\r\n-> " + Resource.Solution_State_Parse_Title + " - Success");
        }

        private static void GetConfigFilesData()
        {
            System.Console.WriteLine("\r\n2. " + Resource.Config_Files_State_Parse_Title);

            configFilesData = ConfigFileConsoleManager.GetInfoFromConfigFiles(rootDirectory, solutionName, CurrentSolState);

            if(configFilesData.ParseError != FileParseError.None)
            {
                System.Console.WriteLine("\r\n-> " + Resource.Config_Files_State_Parse_Title + " - Fail");

                if(configFilesData.ParseError == FileParseError.Global || configFilesData.ParseError == FileParseError.All)
                    ProblemsUploadToConsoleManager.UploadConfigFileSyntaxError(true);

                if(configFilesData.ParseError == FileParseError.Solution || configFilesData.ParseError == FileParseError.All)
                    ProblemsUploadToConsoleManager.UploadConfigFileSyntaxError(false);

                Environment.Exit(-1);
            }

            System.Console.WriteLine("-> " + Resource.Config_Files_State_Parse_Title + " - Success");
        }

        private static void CheckRules()
        {
            System.Console.WriteLine("\r\n3. " + Resource.Check_Rules_Title);

            refDepGuardFindedProblems = CheckRulesManager.CheckConfigFileRulesForConsole(configFilesData, CurrentSolState);

            System.Console.WriteLine("-> " + Resource.Check_Rules_Title + " - Success");
        }

        private static void ShowFindedProblemsOnCurrentCheck()
        {
            if (refDepGuardFindedProblems.IsEmpty())
            {
                System.Console.WriteLine("\r\n" + Resource.No_Find_Problems_On_Check);
            }
            else
            { //Если обнаружены какие-то "проблемы"
                var errorsCount = refDepGuardFindedProblems.RefDepGuardErrors.Count();
                var warningsCount = refDepGuardFindedProblems.RefDepGuardWarnings.Count();

                System.Console.WriteLine("\r\n" + Resource.Find_Problems_on_Check + errorsCount + Resource.Find_Problems_On_Check_1 + warningsCount +")");
                ProblemsUploadToConsoleManager.UploadCheckRuleProblems(refDepGuardFindedProblems, configFilesData);

                if(errorsCount > 0)
                    Environment.Exit(-1);
            }
        }
    }  
}