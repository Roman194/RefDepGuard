using RefDepGuard.Applied;
using RefDepGuard.Applied.Models.ConfigFile;
using RefDepGuard.Applied.Models.Problem;
using RefDepGuard.Applied.Models.RefDepGuard;
using RefDepGuard.Console.Resources;

namespace RefDepGuard.Console.Managers
{
    /// <summary>
    /// This class is responsible for uploading the found problems of the check rules to the console.
    /// </summary>
    public class ProblemsUploadToConsoleManager
    {
        /// <summary>
        /// The main method of the class. 
        /// It uploads the found problems of the check rules to the console. It converts the found problems to string format and prints their info.
        /// </summary>
        /// <param name="refDepGuardFindedProblems">finded problems object</param>
        /// <param name="configFilesData">config files data instance</param>
        public static void UploadCheckRuleProblems(RefDepGuardFindedProblems refDepGuardFindedProblems, ConfigFilesData configFilesData)
        {
            List<ProblemString> errorsStringList = ProblemsStringStoreManager.ConvertCurrentErrorsToStringFormat(
                refDepGuardFindedProblems.RefDepGuardErrors, configFilesData, true);

            List<ProblemString> warningsStringList = ProblemsStringStoreManager.ConvertCurrentWarningsToStringFormat(
                refDepGuardFindedProblems.RefDepGuardWarnings, configFilesData, true);

            if (errorsStringList.Count > 0) {
                System.Console.WriteLine("\r\n    -> "+ Resource.Errors_String_Title + "\r\n");

                errorsStringList.ForEach(error =>
                    System.Console.WriteLine(error.ProblemText + (error.DocumentName != "" ? " (" + Resource.File_String + error.DocumentName + ")" : "") + "\r\n")
                    );
            }

            if (warningsStringList.Count > 0) {
                System.Console.WriteLine("\r\n    -> "+ Resource.Warnings_String_Title + "\r\n");

                warningsStringList.ForEach(warning =>
                    System.Console.WriteLine(warning.ProblemText + (warning.DocumentName != "" ? " (" + Resource.File_String + warning.DocumentName + ")" : "") + "\r\n")
                    );
            }
        }

        /// <summary>
        /// Uploads the error message to the console when there are no references found in the solution.
        /// </summary>
        public static void UploadRefsNotFoundError()
        {
            System.Console.WriteLine(Resource.Refs_Not_Found_Error);
        }

        /// <summary>
        /// Uploads the error message to the console when the provided directory is invalid.
        /// </summary>
        public static void UploadInvalidDirectoryError()
        {
            System.Console.WriteLine(Resource.Invalid_Directory_Error);
        }

        /// <summary>
        /// Uploads the error message to the console when there is a syntax error in the configuration file. 
        /// It specifies whether the error is in the global or solution configuration file.
        /// </summary>
        /// <param name="isGlobal">shows if there is problem with global config file or not</param>
        public static void UploadConfigFileSyntaxError(bool isGlobal)
        {
            var filePrefix = isGlobal ? Resource.Global_Config_File : Resource.Solution_Config_File;
            System.Console.WriteLine("\r\n" + Resource.Config_File_Syntax_Error + filePrefix + Resource.Config_File_Syntax_Error_1);
        }
    }
}