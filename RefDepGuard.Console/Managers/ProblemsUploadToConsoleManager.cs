using RefDepGuard.Applied;
using RefDepGuard.Applied.Models.ConfigFile;
using RefDepGuard.Applied.Models.Problem;
using RefDepGuard.Applied.Models.RefDepGuard;
using RefDepGuard.Console.Resources;

namespace RefDepGuard.Console.Managers
{
    public class ProblemsUploadToConsoleManager
    {
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

        public static void UploadRefsNotFoundError()
        {
            System.Console.WriteLine(Resource.Refs_Not_Found_Error);
        }

        public static void UploadInvalidDirectoryError()
        {
            System.Console.WriteLine(Resource.Invalid_Directory_Error);
        }

        public static void UploadConfigFileSyntaxError(bool isGlobal)
        {
            var filePrefix = isGlobal ? Resource.Global_Config_File : Resource.Solution_Config_File;
            System.Console.WriteLine("\r\n" + Resource.Config_File_Syntax_Error + filePrefix + Resource.Config_File_Syntax_Error_1);
        }
    }
}