using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using VSIXProject1.Data;
using VSIXProject1.Data.ConfigFile;
using VSIXProject1.Managers.CheckRules;
using VSIXProject1.Models;
using Excel = Microsoft.Office.Interop.Excel;
using Task = System.Threading.Tasks.Task;

namespace VSIXProject1
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class MainCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int GetCurrentRefsId = 0x0100;
        public const int GetChangedRefsId = 0x0110;
        public const int CommitCurrentRefsId = 0x0120;
        public const int ExportRefsToXLSXId = 0x0130;
        public const int ExportRefsToHTMLId = 0x0140;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("c19eaee0-a475-4f4d-821f-194a1447a90d");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;
        private static IServiceProvider serviceProvider;
        private static IVsUIShell uiShell;
        private static DTE dte;
        private static ErrorListProvider errorListProvider;
        private static Excel.Application excel = new Excel.Application();
        private static bool isExtentionInitialized = false;
        private static bool isSuccessfulCheckingRules = true;

        private static Dictionary<string, ProjectState> commitedProjState = new Dictionary<string, ProjectState>();
        private static ConfigFilesData configFilesData;
        private static RefDepGuardExportParameters refDepGuardExportParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private MainCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            serviceProvider = this.package;

            var getCurrentRefsCommandID = new CommandID(CommandSet, GetCurrentRefsId);
            var getChangedRefsMenuCommandID = new CommandID(CommandSet, GetChangedRefsId);
            var commitCurrentRefsMenuCommandID = new CommandID(CommandSet, CommitCurrentRefsId);
            var exportCurrentRefsToXLSXMenuCommandID = new CommandID(CommandSet, ExportRefsToXLSXId);
            var exportCurrentRefsToHTMLMenuCommandID = new CommandID(CommandSet, ExportRefsToHTMLId);

            var getCurrentRefsMenuItem = new MenuCommand(this.ExecuteCurrentRefs, getCurrentRefsCommandID);
            var getChangedRefsMenuItem = new MenuCommand(this.ExcecuteRefsChanges, getChangedRefsMenuCommandID);
            var commitCurrentRefsMenuItem = new MenuCommand(this.ForceCommitCurrentReferences, commitCurrentRefsMenuCommandID);
            var exportCurrentRefsToXLSXMenuItem = new MenuCommand(this.ExportRefsToXSLX, exportCurrentRefsToXLSXMenuCommandID);
            var exportCurrentRefsToHTMLMenuItem = new MenuCommand(this.ExportRefsToHTML, exportCurrentRefsToHTMLMenuCommandID);

            commandService.AddCommand(getCurrentRefsMenuItem);
            commandService.AddCommand(getChangedRefsMenuItem);
            commandService.AddCommand(commitCurrentRefsMenuItem);
            commandService.AddCommand(exportCurrentRefsToXLSXMenuItem);
            commandService.AddCommand(exportCurrentRefsToHTMLMenuItem);

            onSolutionOpened();
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static MainCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in Command1's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            errorListProvider = new ErrorListProvider(package);

            dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));
            dte.Events.BuildEvents.OnBuildBegin += new _dispBuildEvents_OnBuildBeginEventHandler(BuildBegined);
            dte.Events.SolutionEvents.BeforeClosing += new _dispSolutionEvents_BeforeClosingEventHandler(BeforeSolutionClosed);
            dte.Events.SolutionEvents.Opened += onSolutionOpened;

            Instance = new MainCommand(package, commandService);

            uiShell = (IVsUIShell)await package.GetServiceAsync(typeof(SVsUIShell));
        }

        private static void BuildBegined(vsBuildScope scope, vsBuildAction buildAction)
        {
            CommitReferencesAndCheckRules(true);
        }

        private static async void onSolutionOpened()
        {
            isExtentionInitialized = false;
            await Task.Delay(10000);

            CommitCurrentReferences();
            GetConfigFileInfo();//Загрузка данных из конфиг файлов производится только при загрузке/открытии нового solution
            CommitReferencesAndCheckRules(false);//Повторный коммит не производится, так как при загрзуке он уже произведён до этого

            isExtentionInitialized = true;
        }

        private static void BeforeSolutionClosed()
        {
            excel.Quit();
            GC.Collect();
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void ExecuteCurrentRefs(object sender, EventArgs e)
        {
            if(isExtentionInitialized)
                ExcecuteRefsManager.ExcecuteCurrentRefs(dte, this.package);
            else
                NotInitializedYetMessage();
        }

        private void ExcecuteRefsChanges(object sender, EventArgs e)
        {
            if(isExtentionInitialized)
                ExcecuteRefsManager.ExcecuteChangedRefs(dte, this.package, commitedProjState);
            else
                NotInitializedYetMessage();
        }

        private void ForceCommitCurrentReferences(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (isExtentionInitialized)
            {
                GetConfigFileInfo();//Добавил обнову правил для принудительной фиксации!!! (временно или постоянно)
                CommitReferencesAndCheckRules(false);
                MessageManager.ShowMessageBox(
                    serviceProvider, 
                    isSuccessfulCheckingRules? "Референсы успешно зафиксированы": "Ошибка фиксации референсов:\r\nВ процессе фиксации не были обнаружены какие-либо референсы между проектами", 
                    "RefDepGuard");
            }
            else
                NotInitializedYetMessage();
        }

        private static void CommitReferencesAndCheckRules(bool isBuildCheck)
        {
            if(isExtentionInitialized) // ЕСли расширение инициализировано, то функция была вызвана не из загрузки Solution, а значит коммита ещё не было
                CommitCurrentReferences();

            if (IsReferencesAddedCorrectly())
            {
                CheckRulesFromConfigFile(isBuildCheck); //Отслеживание соответствия референсов правилам
                isSuccessfulCheckingRules = true;
            }
            else
            {
                ELPStoreManager.ShowUnsuccessfulCheckingRulesWarning(errorListProvider);
                isSuccessfulCheckingRules = false;
            }

            ShowProblemsWithConfigFiles();
        }

        private static void CommitCurrentReferences()
        {
            commitedProjState = CommitManager.CommitCurrentReferences(dte);
        }

        private static bool IsReferencesAddedCorrectly() //Срабатывает не только в случаях, когда не успели прогрузиться рефы, но и когда рефов попросту нет (что на самом деле странно и тоже заслуживает предупреждения)
        {
            foreach (KeyValuePair<string, ProjectState> keyValuePair in commitedProjState)
            {
                if (keyValuePair.Value.CurrentReferences.Count > 0)
                    return true;
            }
            return false;
        }

        private static void GetConfigFileInfo()
        {
            configFilesData = ConfigFileManager.GetInfoFromConfigFiles(dte, serviceProvider, uiShell, errorListProvider, commitedProjState);
        }

        private static void CheckRulesFromConfigFile(bool isBuildCheck)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            (refDepGuardExportParameters, configFilesData) = CheckRulesManager.CheckRulesFromConfigFiles(configFilesData, errorListProvider, commitedProjState, uiShell);

            if (refDepGuardExportParameters.RefDepGuardFindedProblemsData.IsEmpty())
                ELPStoreManager.ShowNoProblemsFindedMessage(errorListProvider); //Вывод сообщения о том, что проблемы не найдены
            else
            {
                if(isBuildCheck && !refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardErrors.IsEmpty()) //В случае если buildCheck и обнаружены ошибки
                    dte.ExecuteCommand("Build.Cancel"); //Отмена билда
            }
        }

        private static void ShowProblemsWithConfigFiles()
        {
            //Вывод обнаруженных проблем по ограничениям конфиг-файлов
            FileParseError parseErrors = configFilesData.ParseError;

            if (parseErrors != FileParseError.None) //Вывод предупреждений о неудаче парсинга конфиг-файлов
            {
                if (parseErrors == FileParseError.Global || parseErrors == FileParseError.All)
                    ELPStoreManager.ShowUnsuccessfulConfigFileParseWarning(errorListProvider, "глобального файла конфигурации");

                if (parseErrors == FileParseError.Solution || parseErrors == FileParseError.All)
                    ELPStoreManager.ShowUnsuccessfulConfigFileParseWarning(errorListProvider, "файла конфигурации конкретного solution");
            }
        }

        private void ExportRefsToXSLX(object sender, EventArgs e)
        {
            if (isExtentionInitialized)
                ExportRefsGeneral("table_type");
            else
                NotInitializedYetMessage();
        }

        private void ExportRefsToHTML(object sender, EventArgs e)
        {
            if (isExtentionInitialized)
                ExportRefsGeneral("graph_type");
            else
                NotInitializedYetMessage();
        }

        private void NotInitializedYetMessage()
        {
            MessageManager.ShowMessageBox(serviceProvider, "Расширение ещё не загружено. Дождитесь его загрузки, чтобы выполнить действие", "RefDepGuard");
        }

        private void ExportRefsGeneral(string reportType)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (isSuccessfulCheckingRules)
            {
                string reportTitleText = "", reportSuccessText = "", reportUnsuccessText = "";
                switch (reportType)
                {
                    case "table_type":
                        reportTitleText = "Экспорт в XLSX";
                        reportSuccessText = "Экспорт в эксель завершён";
                        reportUnsuccessText = "Не удалось загрузить данные в отчёт, так как файл занят другим процессом. Проверьте, что файл " + configFilesData.solutionName + "_references_report.xlsx' не открыт в Excel";
                        break;

                    case "graph_type":
                        reportTitleText = "Экспорт в HTML";
                        reportSuccessText = "Графический экспорт завершён";
                        reportUnsuccessText = "Не удалось загрузить данные в отчёт, так как файл занят другим процессом";
                        break;
                }

                var loadError = ExportManager.LoadReferencesDataToReport(excel, configFilesData, reportType, commitedProjState, refDepGuardExportParameters);

                if (loadError == "")
                    MessageManager.ShowMessageBox(this.package, reportSuccessText, reportTitleText);
                else
                    MessageManager.ShowMessageBox(this.package, reportUnsuccessText + "\r\nТекст ошибки: " + loadError, reportTitleText);
            }
            else
                MessageManager.ShowMessageBox(serviceProvider, "Невозможно запустить экспорт так как расширение не обнаружило референсы в проекте", "RefDepGuard");
        }
    }
}