using Microsoft.VisualStudio.Shell;
using RefDepGuard.Applied;
using RefDepGuard.Applied.Models.ConfigFile;
using RefDepGuard.Applied.Models.Problem;
using RefDepGuard.Applied.Models.RefDepGuard;
using System.Collections.Generic;

namespace RefDepGuard.Managers.CheckRules
{
    /// <summary>
    /// This class is responsible for managing the storage of the IDE (ErrorListProvider) of the errors and warnings found during the checks of the solution's state.
    /// </summary>
    public class ELPStoreManager
    {
        /// <summary>
        /// Stores the errors and warnings found during the checks of the solution's state in the ErrorListProvider of the IDE.
        /// </summary>
        /// <param name="refDepGuardFindedProblems">RefDepGuardFindedProblems value</param>
        /// <param name="configFilesData">ConfigFilesData value</param>
        /// <param name="errorListProvider">ErrorListProvider value</param>
        public static void StoreErrorListProviderByValues(
            RefDepGuardFindedProblems refDepGuardFindedProblems, ConfigFilesData configFilesData, ErrorListProvider errorListProvider)
        {
            ClearErrorListProvider(errorListProvider);

            List<ProblemString> errorsStringList = ProblemsStringStoreManager.ConvertCurrentErrorsToStringFormat(
                refDepGuardFindedProblems.RefDepGuardErrors, configFilesData, false);

            List<ProblemString> warningsStringList = ProblemsStringStoreManager.ConvertCurrentWarningsToStringFormat(
                refDepGuardFindedProblems.RefDepGuardWarnings, configFilesData, false);

            errorsStringList.ForEach(error =>
                StoreErrorTask(errorListProvider, error.ProblemText, error.DocumentName, TaskErrorCategory.Error));

            warningsStringList.ForEach(warning =>
                StoreErrorTask(errorListProvider, warning.ProblemText, warning.DocumentName, TaskErrorCategory.Warning));

            if (errorListProvider != null) //If there are some "problems" to show, then we show the Error List window of the IDE with these problems
                errorListProvider.Show();
        }

        /// <summary>
        /// Clears the ErrorListProvider of the IDE from the previous check results before storing the new ones.
        /// </summary>
        /// <param name="errorListProvider">ErrorListProvider value</param>
        public static void ClearErrorListProvider(ErrorListProvider errorListProvider)
        {
            if (errorListProvider != null)
                errorListProvider.Tasks.Clear();
        }

        /// <summary>
        /// Shows  in the ErrorListProvider of the IDE the message that no problems were found during the check rules of the solution's state.
        /// </summary>
        /// <param name="errorListProvider">ErrorListProvider value</param>
        public static void ShowNoProblemsFindedMessage(ErrorListProvider errorListProvider)
        {
            var currentText = "RefDepGuard: проблемы не обнаружены";
            StoreErrorTask(errorListProvider, currentText, "", TaskErrorCategory.Message);
            errorListProvider.Show();
        }

        /// <summary>
        /// Shows in the ErrorListProvider of the IDE the warning message that it was not possible to check the rules of the solution's state, as the references were 
        /// not detected at the moment of fixing the state. It can be happens when the solution is still loads but the extention is already trying to get its info 
        /// </summary>
        /// <param name="errorListProvider">ErrorListProvider value</param>
        public static void ShowUnsuccessfulCheckingRulesWarning(ErrorListProvider errorListProvider)
        {
            ClearErrorListProvider(errorListProvider);

            var currentText = "RefDepGuard warning: Не получилось проверить соответствие референсов правилам, так как они не были обнаружены на момент фиксации состояния. Проверьте, что в solution действительно содержатся референсы между проектами и произведите проверку вручную или автоматически вместе со сборкой";
            StoreErrorTask(errorListProvider, currentText, "", TaskErrorCategory.Warning);
            errorListProvider.Show();
        }

        /// <summary>
        /// Shows in the ErrorListProvider of the IDE the warning message that it was not possible to parse the data from the config file, and therefore the rules 
        /// from this file were not taken into account during the check of the solution's state. 
        /// It can be happens when the config file has syntax errors or doesn't correspond to the template of the config file and user decided not to fix it automatically.
        /// </summary>
        /// <param name="errorListProvider">ErrorListProvider value</param>
        /// <param name="fileName">file name string</param>
        public static void ShowUnsuccessfulConfigFileParseWarning(ErrorListProvider errorListProvider, string fileName)
        {
            var currentText = "RefDepGuard warning: Не получилось спарсить данные из " + fileName + ". Правила из этого файла не учтены в проверке";
            StoreErrorTask(errorListProvider, currentText, "", TaskErrorCategory.Warning);
        }

        /// <summary>
        /// Stores the error or warning task with the given text, document and error category in the ErrorListProvider of the IDE.
        /// </summary>
        /// <param name="errorListProvider">ErrorListProvider value</param>
        /// <param name="currentText">current text string</param>
        /// <param name="currentDocument">current doc string</param>
        /// <param name="currentTask">current TaskErrorCategory</param>
        private static void StoreErrorTask(ErrorListProvider errorListProvider, string currentText, string currentDocument, TaskErrorCategory currentTask)
        {
            ErrorTask errorTask = new ErrorTask
            {
                Category = TaskCategory.User,
                ErrorCategory = currentTask,
                Document = currentDocument,
                Text = currentText
            };

            errorListProvider.Tasks.Add(errorTask);
        }
    }
}