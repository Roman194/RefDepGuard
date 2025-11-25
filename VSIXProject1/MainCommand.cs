using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using VSIXProject1.Data;
using VSIXProject1.Data.ConfigFile;
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
        public const int ExportRefsToXSLXId = 0x0130;
        public const int ExportRefsToHTMLId = 0x0140;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("c19eaee0-a475-4f4d-821f-194a1447a90d");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;
        private static DTE dte;
        static ErrorListProvider errorListProvider;
        static Excel.Application excel = new Excel.Application();

        static Dictionary<string, ProjectState> commitedProjState = new Dictionary<string, ProjectState>();
        static ConfigFilesData configFilesData;
        static RefDepGuardExportParameters refDepGuardExportParameters;

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

            var getCurrentRefsCommandID = new CommandID(CommandSet, GetCurrentRefsId);
            var getChangedRefsMenuCommandID = new CommandID(CommandSet, GetChangedRefsId);
            var commitCurrentRefsMenuCommandID = new CommandID(CommandSet, CommitCurrentRefsId);
            var exportCurrentRefsToXSLXMenuCommandID = new CommandID(CommandSet, ExportRefsToXSLXId);
            var exportCurrentRefsToHTMLMenuCommandID = new CommandID(CommandSet, ExportRefsToHTMLId);

            var getCurrentRefsMenuItem = new MenuCommand(this.ExecuteCurrentRefs, getCurrentRefsCommandID);
            var getChangedRefsMenuItem = new MenuCommand(this.ExcecuteRefsChanges, getChangedRefsMenuCommandID);
            var commitCurrentRefsMenuItem = new MenuCommand(this.ForceCommitCurrentReferences, commitCurrentRefsMenuCommandID);
            var exportCurrentRefsToXSLXMenuItem = new MenuCommand(this.ExportRefsToXSLX, exportCurrentRefsToXSLXMenuCommandID);
            var exportCurrentRefsToHTMLMenuItem = new MenuCommand(this.ExportRefsToHTML, exportCurrentRefsToHTMLMenuCommandID);

            commandService.AddCommand(getCurrentRefsMenuItem);
            commandService.AddCommand(getChangedRefsMenuItem);
            commandService.AddCommand(commitCurrentRefsMenuItem);
            commandService.AddCommand(exportCurrentRefsToXSLXMenuItem);
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
            
            await Task.Delay(10000);

            Instance = new MainCommand(package, commandService);
        }

        private static void BuildBegined(vsBuildScope scope, vsBuildAction buildAction)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            CommitCurrentReferences();
            CheckRulesFromConfigFile(); //Отслеживание соответствия референсов правилам

            if (errorListProvider.Tasks.Count > 0) //Коммит завершился с ошибками
                dte.ExecuteCommand("Build.Cancel");
        }

        private void onSolutionOpened()
        {
            CommitCurrentReferences();
            GetConfigFileInfo();//message

            if (IsReferencesAddedCorrectly()) //иначе - вывести warning
                CheckRulesFromConfigFile();
            else
                ShowUnsuccessfulCheckingRulesWarning();
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
            ExcecuteRefsManager.ExcecuteCurrentRefs(dte, this.package);
        }

        private void ExcecuteRefsChanges(object sender, EventArgs e)
        {
            ExcecuteRefsManager.ExcecuteChangedRefs(dte, this.package, commitedProjState);
        }

        private void ForceCommitCurrentReferences(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            CommitCurrentReferences();
            CheckRulesFromConfigFile();
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

        private void GetConfigFileInfo()
        {
            configFilesData = ConfigFileManager.GetInfoFromConfigFiles(dte, this.package, commitedProjState);
        }

        private static void CheckRulesFromConfigFile()
        {
            refDepGuardExportParameters = CheckRulesManager.CheckRulesFromConfigFiles(configFilesData, errorListProvider, commitedProjState);
        }

        private static void ShowUnsuccessfulCheckingRulesWarning() //Вынести куда-то в подменджеры CheckRulesManager?
        {
            if (errorListProvider != null)
                errorListProvider.Tasks.Clear();

            ErrorTask errorTask = new ErrorTask
            {
                Category = TaskCategory.User,
                ErrorCategory = TaskErrorCategory.Warning,
                Text = "RefDepGuard warning: Не получилось проверить соответствие референсов правилам во время загрузки solution, так как они не были обнаружены на момент фиксации состояния. Проверьте, что в solution действительно содержатся референсы между проектами и произведите проверку вручную или автоматически вместе со сборкой"
            };

            errorListProvider.Tasks.Add(errorTask);

            errorListProvider.Show();
        }

        private void ExportRefsToXSLX(object sender, EventArgs e)
        {
            ExportRefsGeneral("table_type");
        }

        private void ExportRefsToHTML(object sender, EventArgs e)
        {
            ExportRefsGeneral("graph_type");
        }

        private void ExportRefsGeneral(string reportType)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string reportTitleText = "", reportSuccessText = "", reportUnsuccessText = "";
            switch (reportType)
            {
                case "table_type": 
                    reportTitleText = "Экспорт в XSLX"; 
                    reportSuccessText = "Экспорт в эксель завершён";
                    reportUnsuccessText = "Не удалось загрузить данные в отчёт, так как файл занят другим процессом. Проверьте, что файл " + configFilesData.solutionName + "_references_report.xlsx' не открыт в Excel";
                    break;

                case "graph_type":
                    reportTitleText = "Экспорт в HTML";
                    reportSuccessText = "Графический экспорт завершён";
                    reportUnsuccessText = "Не удалось загрузить данные в отчёт, так как файл занят другим процессом";
                    break;
            }

            if(ExportManager.LoadReferencesDataToReport(
                excel, configFilesData.solutionName, configFilesData.packageExtendedName, reportType, commitedProjState, refDepGuardExportParameters))
                MessageManager.ShowMessageBox(this.package, reportSuccessText, reportTitleText);
            else
                MessageManager.ShowMessageBox(this.package, reportUnsuccessText, reportTitleText);
        }
    }
}