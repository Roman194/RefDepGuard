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
    /// <summary>
    /// This is the main class of the console application. 
    /// It is responsible for managing the flow of the application, calling the necessary methods to get the current state of the solution, 
    /// get the information from the configuration files, check the rules, and show the found problems on the console.
    /// </summary>
    class MainCommand
    {
        private static Dictionary<string, ProjectState> CurrentSolState = new Dictionary<string, ProjectState>();
        private static bool IsNoRefsInSolution = false;
        private static ConfigFilesData configFilesData;
        private static RefDepGuardFindedProblems refDepGuardFindedProblems;

        private static string rootDirectory = "";
        private static string solutionName = "";

        /// <summary>
        /// The main method of the console application. 
        /// It manages the flow of the application, calling the necessary methods to get the current state of the solution,
        /// </summary>
        /// <param name="args">input arguments string (include localization ones: en-US)</param>
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

        /// <summary>
        /// This method is responsible for getting the current state of the solution, which includes the projects in the solution, 
        /// their target frameworks, and their references.
        /// </summary>
        private static void GetCurrentSolutionState()
        {
            #if DEBUG //In debug mode the root directory is set to the fixed path, which is the path to the solution on the local machine of the developer.
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

                Environment.Exit(-1); //Exit with error code
            }

            System.Console.WriteLine("\r\n-> " + Resource.Solution_State_Parse_Title + " - Success");
        }

        /// <summary>
        /// This method is responsible for getting the information from the configuration files and returning it in a structured way.
        /// </summary>
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

        /// <summary>
        /// This method is responsible for checking the rules of the configuration files against the current state of the solution.
        /// </summary>
        private static void CheckRules()
        {
            System.Console.WriteLine("\r\n3. " + Resource.Check_Rules_Title);

            refDepGuardFindedProblems = CheckRulesManager.CheckConfigFileRulesForConsole(configFilesData, CurrentSolState);

            System.Console.WriteLine("-> " + Resource.Check_Rules_Title + " - Success");
        }

        /// <summary>
        /// This method is responsible for showing the found problems of the check rules on the console.
        /// </summary>
        private static void ShowFindedProblemsOnCurrentCheck()
        {
            if (refDepGuardFindedProblems.IsEmpty())
            {
                System.Console.WriteLine("\r\n" + Resource.No_Find_Problems_On_Check);
            }
            else // If there are any finded "problems"
            {
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