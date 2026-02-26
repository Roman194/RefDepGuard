using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using RefDepGuard.Data;
using RefDepGuard.Data.ConfigFile;
using RefDepGuard.Managers.Applied;
using RefDepGuard.Managers.CheckRules;
using RefDepGuard.Managers.Import;
using RefDepGuard.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Excel = Microsoft.Office.Interop.Excel;
using Task = System.Threading.Tasks.Task;

namespace RefDepGuard
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
        public const int GetExtendedCurrentRefsId = 0x0105;
        public const int GetChangedRefsId = 0x0110;
        public const int CommitCurrentSolStateId = 0x0120;
        public const int ExportRefsToXLSXId = 0x0130;
        public const int ExportRefsToHTMLId = 0x0140;
        public const int ActivateExtId = 0x0150;

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
        private static bool isSolutionFamiliar = true;

        private static Dictionary<string, ProjectState> commitedProjState = new Dictionary<string, ProjectState>();
        private static ConfigFilesData configFilesData;
        private static RefDepGuardExportParameters refDepGuardExportParameters;

        private static OleMenuCommand getCurrentRefsMenuItem;
        private static OleMenuCommand getExtCurrentRefsMenuItem;
        private static OleMenuCommand getChangedRefsMenuItem;
        private static OleMenuCommand commitCurrentSolStateMenuItem;
        private static OleMenuCommand exportCurrentRefsToXLSXMenuItem;
        private static OleMenuCommand exportCurrentRefsToHTMLMenuItem;
        private static OleMenuCommand activateExtMenuItem;

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
            var getExtCurrentRefsCommandID = new CommandID(CommandSet, GetExtendedCurrentRefsId);
            var getChangedRefsMenuCommandID = new CommandID(CommandSet, GetChangedRefsId);
            var commitCurrentSolStateMenuCommandID = new CommandID(CommandSet, CommitCurrentSolStateId);
            var exportCurrentRefsToXLSXMenuCommandID = new CommandID(CommandSet, ExportRefsToXLSXId);
            var exportCurrentRefsToHTMLMenuCommandID = new CommandID(CommandSet, ExportRefsToHTMLId);
            var activateExtMenuCommandID = new CommandID(CommandSet, ActivateExtId);

            getCurrentRefsMenuItem = new OleMenuCommand(this.ExecuteCurrentRefs, null, this.GetCurrentRefsQueryStatus, getCurrentRefsCommandID);
            getExtCurrentRefsMenuItem = new OleMenuCommand(this.ExecuteExtentionCurrentRefs, null, this.ExtActivationQueryStatus, getExtCurrentRefsCommandID);
            getChangedRefsMenuItem = new OleMenuCommand(this.ExcecuteRefsChanges, null, this.ExtActivationQueryStatus, getChangedRefsMenuCommandID);
            commitCurrentSolStateMenuItem = new OleMenuCommand(this.ForceCommitCurrentSolutionState, null, this.ExtActivationQueryStatus, commitCurrentSolStateMenuCommandID);
            exportCurrentRefsToXLSXMenuItem = new OleMenuCommand(this.ExportRefsToXSLX, null, this.ExtActivationQueryStatus, exportCurrentRefsToXLSXMenuCommandID);
            exportCurrentRefsToHTMLMenuItem = new OleMenuCommand(this.ExportRefsToHTML, null, this.ExtActivationQueryStatus, exportCurrentRefsToHTMLMenuCommandID);
            activateExtMenuItem = new OleMenuCommand(this.ActivateExtention, null, this.ExtActivationQueryStatus, activateExtMenuCommandID);

            commandService.AddCommand(getCurrentRefsMenuItem);
            commandService.AddCommand(getExtCurrentRefsMenuItem);
            commandService.AddCommand(getChangedRefsMenuItem);
            commandService.AddCommand(commitCurrentSolStateMenuItem);
            commandService.AddCommand(exportCurrentRefsToXLSXMenuItem);
            commandService.AddCommand(exportCurrentRefsToHTMLMenuItem);
            commandService.AddCommand(activateExtMenuItem);

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

        private void GetCurrentRefsQueryStatus(object sender, EventArgs e)
        {
            if (isSolutionFamiliar)
            {
                getCurrentRefsMenuItem.Visible = true; //То что это вынесено отдельно это вроде бы моя попытка пофиксить баг
                getCurrentRefsMenuItem.Enabled = true;
            }
            else
            {
                getCurrentRefsMenuItem.Visible = false;
                getCurrentRefsMenuItem.Enabled = false;
            }
        }

        private void ExtActivationQueryStatus(object sender, EventArgs e)
        {
            if (isSolutionFamiliar)
            {
                getChangedRefsMenuItem.Visible = true;
                getExtCurrentRefsMenuItem.Visible= true;
                commitCurrentSolStateMenuItem.Visible = true;
                exportCurrentRefsToXLSXMenuItem.Visible = true;
                exportCurrentRefsToHTMLMenuItem.Visible = true;
                activateExtMenuItem.Visible = false;

                getChangedRefsMenuItem.Enabled = true;
                getExtCurrentRefsMenuItem.Enabled = true;
                commitCurrentSolStateMenuItem.Enabled = true;
                exportCurrentRefsToXLSXMenuItem.Enabled = true;
                exportCurrentRefsToHTMLMenuItem.Enabled = true;
                activateExtMenuItem.Enabled = false;
            }
            else
            {
                getChangedRefsMenuItem.Visible = false;
                getExtCurrentRefsMenuItem.Visible = false;
                commitCurrentSolStateMenuItem.Visible = false;
                exportCurrentRefsToXLSXMenuItem.Visible = false;
                exportCurrentRefsToHTMLMenuItem.Visible = false;
                activateExtMenuItem.Visible = true;

                getChangedRefsMenuItem.Enabled = false;
                getExtCurrentRefsMenuItem.Enabled= false;
                commitCurrentSolStateMenuItem.Enabled = false;
                exportCurrentRefsToXLSXMenuItem.Enabled = false;
                exportCurrentRefsToHTMLMenuItem.Enabled = false;
                activateExtMenuItem.Enabled = true;
            }
        }

        private void ActivateExtention(object sender, EventArgs e)
        {
            isSolutionFamiliar = SettingsManager.UpdateSettingsByMakingSolutionFamiliar();
            isExtentionInitialized = true;
            UpdateSolutionState(false);

            MessageManager.ShowMessageBox(
                    serviceProvider,
                    isSuccessfulCheckingRules ? "Расширение успешно активировано" : "Ошибка фиксации состояния решения:\r\nВ процессе фиксации не были обнаружены какие-либо референсы между проектами",
                    "RefDepGuard"
            );
        }

        private static async void onSolutionOpened()
        {
            isExtentionInitialized = false;
            await Task.Delay(10000);

            CheckSolutionSettings();
            if (isSolutionFamiliar)
            {
                UpdateSolutionState(false);
                isExtentionInitialized = true;
            }
            else
            {
                ELPStoreManager.ClearErrorListProvider(errorListProvider);
            }
        }

        private static void CheckSolutionSettings()
        {
            SolutionNameManager.SetSolutionNameInfoInRightFormat(dte);

            isSolutionFamiliar = SettingsManager.CheckIfSolutionIsFamiliarToExt(uiShell);
        }

        private static void BuildBegined(vsBuildScope scope, vsBuildAction buildAction)
        {
            if (isSolutionFamiliar)
                UpdateSolutionState(true);
        }

        private static void BeforeSolutionClosed()
        {
            excel.Quit();
            GC.Collect();
        }

        private void ExecuteIfInitialized(Action currentAction)
        {
            if (isExtentionInitialized)
                currentAction();
            else
                NotInitializedYetMessage();
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
            ExecuteIfInitialized(() => 
                ExcecuteRefsManager.ExcecuteCurrentRefs(dte, this.package, false)
                );
        }

        private void ExecuteExtentionCurrentRefs(object sender, EventArgs e)
        {
            ExecuteIfInitialized(() =>
                ExcecuteRefsManager.ExcecuteCurrentRefs(dte, this.package, true)
                );
        }

        private void ExcecuteRefsChanges(object sender, EventArgs e)
        {
            ExecuteIfInitialized(() =>
                ExcecuteRefsManager.ExcecuteChangedRefs(dte, this.package, commitedProjState)
                );
        }

        private void ForceCommitCurrentSolutionState(object sender, EventArgs e)
        {
            ExecuteIfInitialized(() =>
            {
                UpdateSolutionState(false);
                MessageManager.ShowMessageBox(
                    serviceProvider,
                    isSuccessfulCheckingRules ? "Референсы успешно зафиксированы" : "Ошибка фиксации референсов:\r\nВ процессе фиксации не были обнаружены какие-либо референсы между проектами",
                    "RefDepGuard"
                    );
            }    
            );
        }

        private static void UpdateSolutionState(bool isBuildCheck)
        {
            CommitCurrentProjectState();
            GetConfigFileInfo();

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

        private static void CommitCurrentProjectState()
        {
            commitedProjState = CurrentStateManager.GetCurrentProjectState(dte);
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
            configFilesData = ConfigFileManager.GetInfoFromConfigFiles(serviceProvider, uiShell, commitedProjState);
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
            ExecuteIfInitialized(() => ExportRefsGeneral("table_type"));
        }

        private void ExportRefsToHTML(object sender, EventArgs e)
        {
            ExecuteIfInitialized(() => ExportRefsGeneral("graph_type"));
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
                        reportSuccessText = "Экспорт в эксель завершён. Открыть папку со сгенерированным отчётом?";
                        reportUnsuccessText = "Не удалось загрузить данные в отчёт, так как файл занят другим процессом. Проверьте, что файл " + configFilesData.SolutionName + "_references_report.xlsx' не открыт в Excel";
                        break;

                    case "graph_type":
                        reportTitleText = "Экспорт в HTML";
                        reportSuccessText = "Графический экспорт завершён. Открыть папку со сгенерированным отчётом?";
                        reportUnsuccessText = "Не удалось загрузить данные в отчёт, так как файл занят другим процессом";
                        break;
                }

                var loadError = ExportManager.LoadReferencesDataToReport(excel, configFilesData, reportType, commitedProjState, refDepGuardExportParameters);

                if (loadError == "")
                {
                    if(MessageManager.ShowYesNoPrompt(uiShell, reportSuccessText, reportTitleText)) //Если пользователь согласен
                    {
                        ExportManager.OpenCurrentReportDirectory(); //то открываем ему папку с текущим экспортом
                    }
                }
                else
                    MessageManager.ShowMessageBox(this.package, reportUnsuccessText + "\r\nТекст ошибки: " + loadError, reportTitleText);
            }
            else
                MessageManager.ShowMessageBox(serviceProvider, "Невозможно запустить экспорт так как расширение не обнаружило референсы в проекте", "RefDepGuard");
        }
    }
}