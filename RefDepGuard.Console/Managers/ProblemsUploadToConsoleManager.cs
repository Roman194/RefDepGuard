using RefDepGuard.Applied;
using RefDepGuard.Applied.Models.ConfigFile;
using RefDepGuard.Applied.Models.Problem;
using RefDepGuard.Applied.Models.RefDepGuard;

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

            System.Console.WriteLine("\r\n    -> ОШИБКИ:\r\n");

            errorsStringList.ForEach(error => 
                System.Console.WriteLine(error.ProblemText + (error.DocumentName != "" ? " (Файл: " + error.DocumentName + ")" : "") + "\r\n")
                );

            System.Console.WriteLine("\r\n    -> ПРЕДУПРЕЖДЕНИЯ:\r\n");

            warningsStringList.ForEach(warning => 
                System.Console.WriteLine(warning.ProblemText + (warning.DocumentName != "" ? " (Файл: " + warning.DocumentName + ")" : "") + "\r\n")
                );
        }

        public static void UploadRefsNotFoundError()
        {
            var currentText = "    - Error: Не получилось проверить соответствие референсов правилам, так как они не были обнаружены на момент фиксации" +
                "состояния решения.\r\nПроверьте, что в solution действительно содержатся референсы между проектами и произведите проверку вручную или " +
                "автоматически вместе со сборкой";

            System.Console.WriteLine(currentText);
        }

        public static void UploadConfigFileSyntaxError(bool isGlobal)
        {
            var globalPrefix = isGlobal ? "глобального" : "";
            var solFilePrefix = isGlobal ? "" : " текущего решения";

            var currentText = "\r\n    - Error: Не получилось спарсить данные из " + globalPrefix + " файла конфигурации"+ solFilePrefix + 
                ".\r\nПроверьте файл на отсутствие синтаксических ошибок";

            System.Console.WriteLine(currentText);
        }

        public static void UploadConfigFileNotFoundError(bool isGlobal)
        {
            var globalPrefix = isGlobal ? "глобальный" : "";
            var solFilePrefix = isGlobal ? "" : " текущего решения";

            var currentText = "\r\n    - Error: Не получилось найти " + globalPrefix + " файл конфигурации" + solFilePrefix +
                " в корневой папке.\r\nПроверьте корневую папку на наличие этого файла и корректность его названия согласно USER_GUIDE";

            System.Console.WriteLine(currentText);
        }
    }
}