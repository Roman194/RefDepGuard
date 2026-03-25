using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using RefDepGuard.Applied.Models.ConfigFile;
using RefDepGuard.Managers.Applied;
using RefDepGuard.Managers.CheckRules;
using RefDepGuard.Managers.Import;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Excel = Microsoft.Office.Interop.Excel;
using Task = System.Threading.Tasks.Task;
using RefDepGuard.Applied.Models.Project;
using RefDepGuard.Applied.Models.RefDepGuard;

namespace RefDepGuard
{
    /// <summary>
    /// Main command handler class. It's an entry point for the managers starts. It's also handles all user/IDE events and manages extention behaviour by the starting managers
    /// and analyse their results
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
        /// Also there are IServiceProvider, IVsUIShell, DTE, ErrorListProvider and Excel.Application interfaces values initialization.
        /// It's the only place in program where such an init can be helds
        /// </summary>
        private readonly AsyncPackage package;
        private static IServiceProvider serviceProvider;
        private static IVsUIShell uiShell;
        private static DTE dte;
        private static ErrorListProvider errorListProvider;
        private static Excel.Application excel = new Excel.Application();

        /// <summary>
        /// Extention global flags and variables
        /// </summary>
        private static bool isExtentionInitialized = false;
        private static bool isSuccessfulCheckingRules = true;
        private static bool isSolutionFamiliar = true;

        private static Dictionary<string, ProjectState> commitedProjState = new Dictionary<string, ProjectState>();
        private static ConfigFilesData configFilesData;
        private static RefDepGuardExportParameters refDepGuardExportParameters;

        /// <summary>
        /// OleMenuInputCommand variables determine
        /// </summary>
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
            // Switch to the main thread - the call to AddCommand in MainCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            errorListProvider = new ErrorListProvider(package);

            //DTE initilaization with subscribe on events
            dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));
            dte.Events.BuildEvents.OnBuildBegin += new _dispBuildEvents_OnBuildBeginEventHandler(BuildBegined);
            dte.Events.SolutionEvents.BeforeClosing += new _dispSolutionEvents_BeforeClosingEventHandler(BeforeSolutionClosed);
            dte.Events.SolutionEvents.Opened += onSolutionOpened;

            Instance = new MainCommand(package, commandService);

            //IVsUIShell initialization
            uiShell = (IVsUIShell)await package.GetServiceAsync(typeof(SVsUIShell));
        }

        /// <summary>
        /// A function to update the properties of the refs menu item.
        /// It was taken out from the <see cref="ExtActivationQueryStatus"/> as an attempt to fix bug with still seing this item when extentin is deactivated
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Evnet args</param>
        private void GetCurrentRefsQueryStatus(object sender, EventArgs e)
        {
            if (isSolutionFamiliar)
            {
                getCurrentRefsMenuItem.Visible = true;
                getCurrentRefsMenuItem.Enabled = true;
            }
            else
            {
                getCurrentRefsMenuItem.Visible = false;
                getCurrentRefsMenuItem.Enabled = false;
            }
        }

        /// <summary>
        /// A function to update the properties of the other menu items
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Evnet args</param>
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

        /// <summary>
        /// A func to acgtivate the extention after the user command
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Evnet args</param>
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

        /// <summary>
        /// A function that is always calls after the new solution opened. It checks solution settings and starts updating solution state if needed
        /// <see cref="CheckSolutionSettings"/>
        /// <see cref="UpdateSolutionState"/>
        /// </summary>
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

        /// <summary>
        /// This function checks whether this solution is familiar to extention or not.
        /// It also calls all the "SetSolutionNameInfoInRightFormat" functions
        /// just to prevent them from calling on every solution state update, but still to provide them to call it after every solution opens
        /// </summary>
        private static void CheckSolutionSettings()
        {
            SolutionNameManager.SetSolutionNameInfoInRightFormat(dte);
            ConfigFileManager.SetSolutionNameInfoInRightFormat();
            CacheManager.SetSolutionNameInfoInRightFormat();

            isSolutionFamiliar = SettingsManager.CheckIfSolutionIsFamiliarToExt(uiShell);
        }

        /// <summary>
        /// This function is handels on build begined extention actions: starts solution state update if solution is familiar to extention
        /// </summary>
        /// <param name="scope">vsBuildScope value</param>
        /// <param name="buildAction">vsBuildAction value</param>
        private static void BuildBegined(vsBuildScope scope, vsBuildAction buildAction)
        {
            if (isSolutionFamiliar)
                UpdateSolutionState(true);

        }

        /// <summary>
        /// This func is close working with Excel.Application and clear its cash data after the user starts solution closing
        /// </summary>
        private static void BeforeSolutionClosed()
        {
            excel.Quit();
            GC.Collect();
        }

        /// <summary>
        /// This func checks if extention is already initialized or not. If yes - starts an action, if not - shows not yet init message
        /// <see cref="NotInitializedYetMessage"/>
        /// </summary>
        /// <param name="currentAction">Action value</param>
        private void ExecuteIfInitialized(Action currentAction)
        {
            if (isExtentionInitialized)
                currentAction();
            else
                NotInitializedYetMessage();
        }

        /// <summary>
        /// This function and <see cref="ExecuteExtentionCurrentRefs"/>, <see cref="ExcecuteRefsChanges"/>, <see cref="ForceCommitCurrentSolutionState"/>,
        /// <see cref="ExportRefsToXSLX"/>, <see cref="ExportRefsToHTML"/>
        /// are the callbacks used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// 
        /// This func is used to start excecuting functions on user decided to show Message box with current streight refs 
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void ExecuteCurrentRefs(object sender, EventArgs e)
        {
            ExecuteIfInitialized(() => 
                ExcecuteRefsManager.ExcecuteCurrentRefs(dte, this.package, false)
                );
        }

        /// <summary>
        /// This func is used to start excecuting functions on user decided to show Message box with current transitive refs 
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void ExecuteExtentionCurrentRefs(object sender, EventArgs e)
        {
            ExecuteIfInitialized(() =>
                ExcecuteRefsManager.ExcecuteCurrentRefs(dte, this.package, true)
                );
        }

        /// <summary>
        /// This func is used to start excecuting functions on user decided to show Message box with current changed after last commit refs 
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void ExcecuteRefsChanges(object sender, EventArgs e)
        {
            ExecuteIfInitialized(() =>
                ExcecuteRefsManager.ExcecuteChangedRefs(dte, this.package, commitedProjState)
                );
        }

        /// <summary>
        /// This func is used to start force commit of the current refs, update config files settings and check rules on updated info 
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
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

        /// <summary>
        /// This func is used to update solution state: commit current project state, get config files info and check rules if refs added correctly (if not shows problems
        /// with config file message)
        /// </summary>
        /// <param name="isBuildCheck">shows if its a build event check or not</param>
        private static void UpdateSolutionState(bool isBuildCheck)
        {
            CommitCurrentProjectState();
            GetConfigFileInfo();

            if (IsReferencesAddedCorrectly())
            {
                CheckRulesFromConfigFile(isBuildCheck);
                isSuccessfulCheckingRules = true;
            }
            else
            {
                ELPStoreManager.ShowUnsuccessfulCheckingRulesWarning(errorListProvider);
                isSuccessfulCheckingRules = false;
            }

            ShowProblemsWithConfigFiles();
        }

        /// <summary>
        /// This function starts and helds commitimg the current project state
        /// </summary>
        private static void CommitCurrentProjectState()
        {
            commitedProjState = CurrentStateManager.GetCurrentSolutionState(dte);
        }

        /// <summary>
        /// Checks if there are some refs in the solution right now.
        /// If there are not its a signal that something went wrong (solution is not init yet or just hasn't refs what is quite weird for a solution that is going to be used
        /// by such extention as this)
        /// </summary>
        /// <returns>the result of the check</returns>
        private static bool IsReferencesAddedCorrectly() 
        {
            foreach (KeyValuePair<string, ProjectState> keyValuePair in commitedProjState)
            {
                if (keyValuePair.Value.CurrentReferences.Count > 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Helds an updating or getting the info from the config files
        /// </summary>
        private static void GetConfigFileInfo()
        {
            configFilesData = ConfigFileManager.GetInfoFromConfigFiles(serviceProvider, uiShell, commitedProjState);
        }

        /// <summary>
        /// Helds a checking rules that have gotten from the config files with updated solution commit state
        /// </summary>
        /// <param name="isBuildCheck">shows if its a build event check or not</param>
        private static void CheckRulesFromConfigFile(bool isBuildCheck)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            (refDepGuardExportParameters, configFilesData) = CheckRulesExtentionManager.CheckRulesFromConfigFiles(configFilesData, errorListProvider, commitedProjState, uiShell);

            if (refDepGuardExportParameters.RefDepGuardFindedProblemsData.IsEmpty())
                ELPStoreManager.ShowNoProblemsFindedMessage(errorListProvider);
            else
            {
                if(isBuildCheck && !refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardErrors.IsEmpty()) //If there are errors when event is build check
                    dte.ExecuteCommand("Build.Cancel"); //Build should be cancel
                
            }
        }

        /// <summary>
        /// This func show a problems with config files depended on the FileParseError value
        /// </summary>
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

        /// <summary>
        /// This func is used to start export in the table report format 
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void ExportRefsToXSLX(object sender, EventArgs e)
        {
            ExecuteIfInitialized(() => ExportRefsGeneral("table_type"));
        }

        /// <summary>
        /// This func is used to start export in the graph report format 
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void ExportRefsToHTML(object sender, EventArgs e)
        {
            ExecuteIfInitialized(() => ExportRefsGeneral("graph_type"));
        }

        /// <summary>
        /// Shows a message box with info that the extention is not initialized yet
        /// </summary>
        private void NotInitializedYetMessage()
        {
            MessageManager.ShowMessageBox(serviceProvider, "Расширение ещё не загружено. Дождитесь его загрузки, чтобы выполнить действие", "RefDepGuard");
        }

        /// <summary>
        /// This is a general export method for a starting graph or table export
        /// </summary>
        /// <param name="reportType">report type string: graph or table</param>
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

                if (loadError == "")//If export is successful
                {
                    if(MessageManager.ShowYesNoPrompt(uiShell, reportSuccessText, reportTitleText)) //If user agrees
                    {
                        ExportManager.OpenCurrentReportDirectory(); //open directory with current successful export
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