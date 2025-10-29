using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using VSIXProject1.Comparators;
using VSIXProject1.Data;
using VSIXProject1.Data.FrameworkVersion;
using VSIXProject1.Data.Reference;
using VSLangProj;
using Excel = Microsoft.Office.Interop.Excel;
using Task = System.Threading.Tasks.Task;

namespace VSIXProject1
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class Command1
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;
        public const int GetChangedRefsId = 0x0110;
        public const int CommitCurrentRefsId = 0x0120;
        public const int ExportRefsToXSLXId = 0x0130;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("c19eaee0-a475-4f4d-821f-194a1447a90d");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        private static DTE dte;
        private static DTE2 dte2;

        static Dictionary<string, List<string>> addedRefs = new Dictionary<string, List<string>>();
        static List<string> changedRefs = new List<string>();
        static Dictionary<string, List<string>> removedRefs = new Dictionary<string, List<string>>();

        static Dictionary<string, ProjectState> commitedProjState = new Dictionary<string, ProjectState>();
        static List<string> projectFrameworkVersionsList = new List<string>();

        static List<ConfigFilePropertyNullError> configPropertyNullErrorList = new List<ConfigFilePropertyNullError>();
        static List<ReferenceError> refsErrorList = new List<ReferenceError>();
        static List<ReferenceMatchError> refsMatchErrorList = new List<ReferenceMatchError>();

        static List<MaxFrameworkVersionDeviantValue> maxFrameworkVersionDeviantValueList = new List<MaxFrameworkVersionDeviantValue>();
        static List<FrameworkVersionComparabilityError> frameworkVersionComparabilityErrorList = new List<FrameworkVersionComparabilityError>();

        static RefDepGuardErrors refDepGuardErrors; //Реализовать работу с этой структурой вместо верхних трёх-пяти?
        static List<ReferenceMatchWarning> refsMatchWarningList = new List<ReferenceMatchWarning>();
        static List<MaxFrameworkVersionConflictWarning> maxFrameworkVersionConflictWarningsList = new List<MaxFrameworkVersionConflictWarning>();

        static List<RequiredReference> requiredReferencesList = new List<RequiredReference>();

        static ConfigFileSolution configFileSolution;
        static ConfigFileGlobal configFileGlobal;

        static string solutionName;
        static string packageExtendedName;

        static ErrorListProvider errorListProvider;
        static IVsOutputWindowPane generalPane;
        static Guid generalPaneGuid;
        static IVsOutputWindow outWindow;

        static Excel.Application excel = new Excel.Application();


        /// <summary>
        /// Initializes a new instance of the <see cref="Command1"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private Command1(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var getChangedRefsMenuCommandID = new CommandID(CommandSet, GetChangedRefsId);
            var commitCurrentRefsMenuCommandID = new CommandID(CommandSet, CommitCurrentRefsId);
            var exportCurrentRefsToXSLXMenuCommandID = new CommandID(CommandSet, ExportRefsToXSLXId);

            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            var getChangedRefsMenuItem = new MenuCommand(this.ExcecuteChanges, getChangedRefsMenuCommandID);
            var commitCurrentRefsMenuItem = new MenuCommand(this.CommitCurrentReferences, commitCurrentRefsMenuCommandID);
            var exportCurrentRefsToXSLXMenuItem = new MenuCommand(this.ExportRefsToXSLX, exportCurrentRefsToXSLXMenuCommandID);

            commandService.AddCommand(menuItem);
            commandService.AddCommand(getChangedRefsMenuItem);
            commandService.AddCommand(commitCurrentRefsMenuItem);
            commandService.AddCommand(exportCurrentRefsToXSLXMenuItem);

            onSolutionOpened();
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static Command1 Instance
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
            dte2 = Package.GetGlobalService(typeof(EnvDTE.DTE)) as DTE2;
            dte.Events.BuildEvents.OnBuildBegin += new _dispBuildEvents_OnBuildBeginEventHandler(BuildBegined);
            dte.Events.SolutionEvents.BeforeClosing += new _dispSolutionEvents_BeforeClosingEventHandler(BeforeSolutionClosed);
            //dte.Events.SolutionEvents.Opened += onSolutionOpened; 

            //if (dte.Solution.IsOpen)
            //{
            //    onSolutionOpened();
            //}

            await Task.Delay(10000);

            Instance = new Command1(package, commandService);

            outWindow = (IVsOutputWindow) Package.GetGlobalService(typeof(SVsOutputWindow)); //Создание собственного окна (м.б. полезно для вывода варнингов)
            generalPaneGuid = VSConstants.GUID_OutWindowGeneralPane;
            outWindow.CreatePane(ref generalPaneGuid, "Warning pane", 1, 0);
            outWindow.GetPane(ref generalPaneGuid, out generalPane);
            generalPane.OutputString("Nope!");

            //Microsoft.Build.Framework.

        }

        private static void BeforeSolutionClosed()
        {
            excel.Quit();
            GC.Collect();
        }

        private static void BuildBegined(vsBuildScope scope, vsBuildAction buildAction)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            //Здесь прописаны отслеживание соответствия референсов правилам

            CommitCurrentReferences();

            CheckRulesFromConfigFile();

            if (errorListProvider.Tasks.Count > 0) //Коммит завершился с ошибками
            {
                generalPane.Activate();

                dte.ExecuteCommand("Build.Cancel");

            }
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

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = "";
            string title = "Связи между проектами на текущий момент";

            
            EnvDTE.Solution solution = dte.Solution;

            //commitedProjState.Clear();

            foreach (EnvDTE.Project project in solution.Projects)
            {
                message += ("Рефы в проекте:" + project.Name + "\r\n");
                VSLangProj.VSProject vSProject = project.Object as VSLangProj.VSProject;
                if (vSProject != null) {

                    var refsList = new List<string>();

                    foreach (VSLangProj.Reference vRef in vSProject.References) 
                    {
                        if (vRef.SourceProject != null)
                        {

                            refsList.Add(vRef.Name);
                            message += (vRef.Name + "\r\n");
                        }
                    }

                    //commitedProjState.Add(vSProject.Project.Name, refsList); //???? Думаю, что здесь бессмысленно фиксировать состояние
                }
            }

            

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private void ExcecuteChanges(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string message = "С момента последней проверки рефов произошли следующие изменения:\r\n";

            string title = "Изменения в рефах";

            EnvDTE.Solution solution = dte.Solution;

            foreach (EnvDTE.Project project in solution.Projects)
            {
                VSLangProj.VSProject vSProject = project.Object as VSLangProj.VSProject;
                if (vSProject != null)
                {
                    var vsCommitedProjRefsHashSet = new HashSet<string> (commitedProjState[vSProject.Project.Name].CurrentReferences);

                    var vsCurrentProjHashSet = new HashSet<string>();
                    
                    foreach (Reference currRef in vSProject.References)
                    {
                        if (currRef.SourceProject != null)
                        {
                            vsCurrentProjHashSet.Add(currRef.Name);
                        }
                    }

                    if (!(vsCurrentProjHashSet.SetEquals(vsCommitedProjRefsHashSet)))
                    {
                        commitedProjState[vSProject.Project.Name].CurrentReferences = vsCurrentProjHashSet.ToList();

                        var commonRefsHashSet = vsCurrentProjHashSet.Intersect(vsCommitedProjRefsHashSet).ToHashSet();

                        vsCurrentProjHashSet.RemoveWhere(commonRefsHashSet.Contains);
                        vsCommitedProjRefsHashSet.RemoveWhere(commonRefsHashSet.Contains);

                        if (vsCurrentProjHashSet.Count > 0)
                        {
                            var addedRefsList = new List<string>();

                            foreach (string currRef in vsCurrentProjHashSet)
                                addedRefsList.Add(currRef);

                            addedRefs.Add(vSProject.Project.Name, addedRefsList);

                        }

                        if(vsCommitedProjRefsHashSet.Count > 0)
                        {
                            var removedRefsList = new List<string>();

                            foreach (string currRef in vsCommitedProjRefsHashSet)
                                removedRefsList.Add(currRef);

                            removedRefs.Add(vSProject.Project.Name, removedRefsList);
                        }
                    }
                }
            }

            if (addedRefs.Count > 0)
            {
                message += "Добавлены рефы:";
                foreach(var addedRefDict in addedRefs)
                {
                    message += ("В проекте "+ addedRefDict.Key + ": \r\n");

                    foreach (string addedRef in addedRefDict.Value)
                    {
                        message += addedRef + "\r\n";

                    }
                }
            }

            if (removedRefs.Count > 0)
            {
                message += "Удалены рефы:";
                foreach(var removedRefDict in removedRefs)
                {
                    message += ("В проекте "+ removedRefDict.Key + ": \r\n");

                    foreach (string removedRef in removedRefDict.Value)
                    { 
                        message += removedRef + "\r\n";

                    }
                }
            }

            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

            addedRefs.Clear();
            removedRefs.Clear();
        }

        private void CommitCurrentReferences(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            CommitCurrentReferences();
            CheckRulesFromConfigFile();
        }

        private void ExportRefsToXSLX(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            refDepGuardErrors = new RefDepGuardErrors(configPropertyNullErrorList, refsErrorList, refsMatchErrorList, 
                maxFrameworkVersionDeviantValueList, frameworkVersionComparabilityErrorList);

            if(XLSXManager.LoadReferencesDataToCurrentReport(excel, solutionName, packageExtendedName, commitedProjState, refDepGuardErrors, requiredReferencesList))
                ShowMessageBox("Экспорт в эксель завершён", "Экспорт в XSLX");
            else
                ShowMessageBox("Не удалось загрузить данные в отчёт, так как файл занят другим процессом. Проверьте, что файл '" + solutionName + "_references_report.xlsx' не открыт в Excel", "Экспорт в XSLX");
                
        }

        private void ShowMessageBox(string message, string title)
        {
            VsShellUtilities.ShowMessageBox(
                    this.package,
                    message,
                    title,
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private static void ShowUnsuccessfulCheckingRulesWarning() //Может ли возникнуть что-то подобное при TargetFramework проверке?
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

        private static void CheckRulesForSolutionOrGlobalReferences(string projName, List<string> projReferences, List<string> currentReferences,  ErrorLevel referenceLevel, bool isReferenceRequired, List<List<string>> generalReferences)
        {
            if (currentReferences != null) {

                foreach (string currentReference in currentReferences)
                {

                    if ((isReferenceRequired && !projReferences.Contains(currentReference)) ||
                        (!isReferenceRequired && projReferences.Contains(currentReference)))
                    {
                        if (refsMatchErrorList.Contains(new ReferenceMatchError(referenceLevel, currentReference, "", false), new ReferenceMatchErrorComparer()))
                            continue;

                        if (IsRuleConflict(currentReference, referenceLevel, generalReferences))
                            continue;

                        if (isReferenceRequired && currentReference == projName)
                            continue;

                        refsErrorList.Add(new ReferenceError(currentReference, projName, isReferenceRequired, referenceLevel));
                    }
                }
            }
        }

        private static void CheckRulesForProjectReferences(string projName, List<string> projReferences, List<string> configFileReferences, bool isReferenceRequired)
        {
            if (configFileReferences != null)
            {
                foreach (string fileReference in configFileReferences)
                {
                    if ((isReferenceRequired && !projReferences.Contains(fileReference)) ||
                        (!isReferenceRequired && projReferences.Contains(fileReference)))
                    {
                        if(fileReference == projName) //Для Project рефов не допускается совпадение рефа и его проекта. Это "замыкание на себя"
                        {
                            refsMatchErrorList.Add(
                                new ReferenceMatchError(ErrorLevel.Project, fileReference, projName, true)
                                );

                            continue;
                        }
                            
                        //Если реф с таким же названием содежится в MatchError, то пофиг уже на Level: важнеее устранить конфликт рефов, чем вывести по уровню
                        if (refsMatchErrorList.Contains(new ReferenceMatchError(ErrorLevel.Project, fileReference, projName, false), new ReferenceMatchErrorComparer()))
                            continue;

                        refsErrorList.Add(
                            new ReferenceError(fileReference, projName, isReferenceRequired, ErrorLevel.Project)
                            );
                    }
                }
            }
        }

        private static void StoreErrorTask(string currentText, string currentDocument, bool isWarning)
        {
            TaskErrorCategory currentTask = TaskErrorCategory.Error;

            if(isWarning)
                currentTask = TaskErrorCategory.Warning;

            ErrorTask errorTask = new ErrorTask
            {
                Category = TaskCategory.User,
                ErrorCategory = currentTask,
                Document = currentDocument,
                Text = currentText
            };

            errorListProvider.Tasks.Add(errorTask);
        }

        private static void StoreErrorListProviderByValues()
        {
            foreach(MaxFrameworkVersionDeviantValue maxFrameworkVersionDeviantValue in maxFrameworkVersionDeviantValueList)
            {
                string documentName = solutionName + "_config_guard.rdg";
                string relevantProjectName = "";
                string globalPrefix = "";

                switch (maxFrameworkVersionDeviantValue.ErrorLevel)
                {
                    case ErrorLevel.Global: documentName = "global_config_guard.rdg"; globalPrefix = "глобального "; break;
                    case ErrorLevel.Solution: relevantProjectName = " уровня Solution"; break;
                    case ErrorLevel.Project: relevantProjectName = " проекта '" + maxFrameworkVersionDeviantValue.ErrorRelevantProjectName + "'"; break;
                }

                string errorText = "RefDepGuard framework_max_version deviant value error: параметр 'framework_max_version' "+ globalPrefix + "Config-файла " + relevantProjectName + " содержит некорректную запись своего значения. Проверьте его на предмет отсутствия синтаксических ошибок и соответствия шаблону файла конфигурации";

                StoreErrorTask(errorText, documentName, false);
            }

            foreach (FrameworkVersionComparabilityError frameworkVersionComparabilityError in frameworkVersionComparabilityErrorList)
            {
                string documentName = solutionName + "_config_guard.rdg";
                string ruleLevel = "";


                switch (frameworkVersionComparabilityError.ErrorLevel)
                {
                    case ErrorLevel.Global: documentName = "global_config_guard.rdg"; ruleLevel = "ограничение глобального уровня"; break;
                    case ErrorLevel.Solution: ruleLevel = "ограничение уровня решения"; break;
                    case ErrorLevel.Project: ruleLevel = "ограничение уровня проекта"; break;
                }

                string errorText = "RefDepGuard framework comparability version error: 'TargetFrameworkVersion' проекта '" + frameworkVersionComparabilityError.ErrorRelevantProjectName + "' имеет версию '" + frameworkVersionComparabilityError.TargetFrameworkVersion 
                    + "', в то время как максимально допустимой для него версией является '"+ frameworkVersionComparabilityError.MaxFrameworkVersion  +"' (" + ruleLevel +"). Измените версию проекта или модифицируйте конфигурацию Config-файла";

                StoreErrorTask(errorText, documentName, false);
            }

            foreach(ConfigFilePropertyNullError configFilePropertyNullError in configPropertyNullErrorList)
            {
                string documentName = solutionName + "_config_guard.rdg";
                string relevantProjectName = "";

                if (configFilePropertyNullError.IsGlobal)
                    documentName = "global_config_guard.rdg";

                if (configFilePropertyNullError.ErrorRelevantProjectName != "")
                    relevantProjectName = " для проекта '" + configFilePropertyNullError.ErrorRelevantProjectName + "'";

                string errorText = "RefDepGuard Null property error: Config-файл не содержит свойство '" + configFilePropertyNullError.PropertyName + "'" + relevantProjectName + ". Проверьте его на предмет отсутствия синтаксических ошибок и соответствия шаблону файла конфигурации";

                StoreErrorTask(errorText, documentName, false);
            }

            foreach (ReferenceMatchError referenceMatchError in refsMatchErrorList)
            {
                string projectName = "";
                string referenceLevelText = "";
                string documentName = solutionName + "_config_guard.rdg";
                string matchErrorDescription = "";

                if (referenceMatchError.IsProjNameMatchError)
                    matchErrorDescription = " совпадает с именем проекта";
                else
                    matchErrorDescription = " одновременно заявлен как обязательный и недопустимый";

                if (referenceMatchError.ProjectName != "")
                    projectName = "' проекта '" + referenceMatchError.ProjectName;
                

                switch (referenceMatchError.ReferenceLevelValue)
                {
                    case ErrorLevel.Solution: referenceLevelText = "уровня Solution"; break;
                    case ErrorLevel.Global: referenceLevelText = "глобального уровня"; documentName = "global_config_guard.rdg"; break;
                    case ErrorLevel.Project: break;
                }

                string errorText = "RefDepGuard Match error: референс '" + referenceMatchError.ReferenceName + projectName + "' " + referenceLevelText + matchErrorDescription + ". Устраните противоречие в правиле";

                StoreErrorTask(errorText, documentName, false);
            }


            foreach (ReferenceError error in refsErrorList)
            {
                string referenceTypeText = "";
                string referenceLevelText = "";
                string documentName = error.ErrorRelevantProjectName + ".csproj";
                string actionForUser = "";

                if (error.IsReferenceRequired)
                {
                    referenceTypeText = "Отсутсвует обязательный";
                    actionForUser = "Добавьте";
                }
                else
                {
                    referenceTypeText = "Присутствует недопустимый";
                    actionForUser = "Удалите";
                }
                    
                switch (error.CurrentReferenceLevel)
                {
                    case ErrorLevel.Solution: referenceLevelText = "уровня Solution"; break;
                    case ErrorLevel.Global: referenceLevelText = "глобального уровня"; break;
                    case ErrorLevel.Project: break;
                }

                string errorText = "RefDepGuard Reference error: " + referenceTypeText + " референс " + referenceLevelText + " '" + error.ReferenceName + "' для проекта '" + error.ErrorRelevantProjectName + "'. " + actionForUser + " его через обозреватель решений";

                StoreErrorTask(errorText, documentName, false);
            }

            foreach (MaxFrameworkVersionConflictWarning maxFrameworkVersionConflictValue in maxFrameworkVersionConflictWarningsList)
            {
                string documentName = solutionName + "_config_guard.rdg";
                string highErrorLevelText = "";
                string lowErrorLevelText = "";

                switch (maxFrameworkVersionConflictValue.HighErrorLevel)
                {
                    case ErrorLevel.Global: highErrorLevelText = "глобального уровня"; break;
                    case ErrorLevel.Solution: highErrorLevelText = "уровня Solution"; break;
                }

                switch (maxFrameworkVersionConflictValue.LowErrorLevel)
                {
                    case ErrorLevel.Solution: lowErrorLevelText = "уровня Solution"; break;
                    case ErrorLevel.Project: lowErrorLevelText = "в проекте '" + maxFrameworkVersionConflictValue.ErrorRelevantProjectName + "'"; break;
                }

                string errorText = "RefDepGuard framework_max_version conflict warning: значение '"+  maxFrameworkVersionConflictValue.LowLevelMaxFrameVersion  
                    +"' параметра 'framework_max_version' " + lowErrorLevelText + " превосходит значение '"+ maxFrameworkVersionConflictValue.HighLevelMaxFrameVersion 
                    + "' одноимённого параметра " + highErrorLevelText  + ". Устраните противоречие";

                StoreErrorTask(errorText, documentName, true);
            }

            foreach (ReferenceMatchWarning referenceMatchWarning in refsMatchWarningList)
            {
                string documentName = solutionName + "_config_guard.rdg";
                string projectName = "";
                string highReferenceLevelText = "";
                string lowReferenceLevelText = "";
                string referenceTypeText = "";
                string warningDescription = "";
                string warningAction = "";


                if (referenceMatchWarning.ProjectName != "")
                {
                    projectName = "' проекта '" + referenceMatchWarning.ProjectName;
                }

                if (referenceMatchWarning.LowReferenceLevel == ErrorLevel.Solution)
                {
                    lowReferenceLevelText = "уровня Solution";
                }

                switch (referenceMatchWarning.HighReferenceLevel)
                {
                    case ErrorLevel.Solution: highReferenceLevelText = "уровня Solution"; break;
                    case ErrorLevel.Global: highReferenceLevelText = "глобального уровня"; documentName = "global_config_guard.rdg"; break;
                    case ErrorLevel.Project: break;
                }

                if (referenceMatchWarning.IsReferenceStraight)
                {
                    warningDescription = " дубирует правило с одноимённым референсом ";
                    warningAction = ". Устраните дублирование правила";

                    if (referenceMatchWarning.IsHighLevelReq)
                        referenceTypeText = " является обязательным и";
                    else
                        referenceTypeText = " является недопустимым и";
                }
                else //В противном случае рассматривается cross match errors, а значит они имеют тип рефа, противиположный более "верхнему" праивлу
                {
                    warningDescription = " противоречит правилу с одноимённым референсом ";
                    warningAction = ". Устраните противоречие в правиле";

                    if (referenceMatchWarning.IsHighLevelReq)
                        referenceTypeText = " является недопустимым и";
                    else
                        referenceTypeText = " является обязательным и";
                }

                string errorText = "RefDepGuard Match Warning: референс '" + referenceMatchWarning.ReferenceName + projectName + "' " + lowReferenceLevelText + referenceTypeText + warningDescription + highReferenceLevelText + warningAction;

                StoreErrorTask(errorText, documentName, true);
            }
        }

        private static void CheckConfigFileSolutionProperties() //How to make it better? Reflection doesn't work
        {
            if (configFileSolution.name is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("name", false, ""));

            if (configFileSolution.framework_max_version is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("framework_max_version", false, ""));

            if (configFileSolution.solution_required_references is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("solution_required_references", false, ""));

            if (configFileSolution.solution_unacceptable_references is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("solution_unacceptable_references", false, ""));
        }

        private static void CheckConfigFileProjectProperties(string projectKey, ConfigFileProject currentProject)
        {
            if (currentProject.framework_max_version is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("framework_max_version", false, projectKey));

            if (currentProject.consider_global_and_solution_references is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("consider_global_and_solution_references", false, projectKey));

            if (currentProject.required_references is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("required_references", false, projectKey));

            if (currentProject.unacceptable_references is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("unacceptable_references", false, projectKey));
        }

        private static void CheckConfigFileGlobalProperties()
        {
            if (configFileGlobal.name is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("name", true, ""));

            if (configFileGlobal.framework_max_version is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("framework_max_version", true, ""));

            if (configFileGlobal.global_required_references is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("global_required_references", true, ""));

            if (configFileGlobal.global_unacceptable_references is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("global_unacceptable_references", true, ""));
        }

        private static void CheckConfigPropertiesOnNotNull()
        {
            if (configFileSolution != null)
            {
                CheckConfigFileSolutionProperties();

                if (configFileSolution.projects != null)
                {
                    foreach (var project in configFileSolution.projects)
                    {
                        if (project.Value != null)
                            CheckConfigFileProjectProperties(project.Key, project.Value);
                     
                        else
                            configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("project_value", false, project.Key));
                    }
                }
                else
                    configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("projects", false, ""));
            }
            else
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError(solutionName, false, ""));


            if (configFileGlobal != null)
                CheckConfigFileGlobalProperties();
            else
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("Global", true, ""));
        }


        //реализовать в проге работу с несколькими Solution - не требуется так как VS 22 поддерживает только работу с одним solution в одном окне.
        //однако это не означает, что 2 и более solution с одним и тем же root не могут иметь общий файл глобальных правил


        private static bool IsRuleConflict(string currentReference, ErrorLevel referenceType, List<List<string>> generalReferences)//Перебрать для каждого solution и Global рефа все нижестоящие на предмет противоречий
        {
            for(int i = 0; i < generalReferences.Count; i++)
            {
                if (referenceType != ErrorLevel.Global && i > 1) //generalReferences содержит все Project и Solution рефы, которые могут конфликтовать с текущим рефом (i 0 и 1 - project рефы, 2 и 3 - solution рефы)
                    break;

                if(generalReferences[i].Contains(currentReference))
                    return true;
            }

            return false;
        }

        private static void CheckRulesOnMatchConflicts(List<string> solutionRequiredReferences, List<string> solutionUnacceptableReferences, List<string> globalRequiredReferences, List<string> globalUnacceptableReferences)
        {
            List<string> solutionReferencesIntersect = solutionRequiredReferences.Intersect(solutionUnacceptableReferences).ToList();
            List<string> globalReferencesIntersect = globalRequiredReferences.Intersect(globalUnacceptableReferences).ToList();

            List<string> solutionReqAndGlobalUnacceptIntersect = solutionRequiredReferences.Intersect(globalUnacceptableReferences).ToList();
            List<string> solutionReqStraightLevelIntersect = solutionRequiredReferences.Intersect(globalRequiredReferences).ToList();

            List<string> solutionUnacceptAndGlobalReqIntersect = solutionUnacceptableReferences.Intersect(globalRequiredReferences).ToList();
            List<string> solutionUnacceptStraightLevelIntersect = solutionUnacceptableReferences.Intersect(globalUnacceptableReferences).ToList();

            List<List<string>> solutionCrossLevelIntersects = new List<List<string>> { solutionReqAndGlobalUnacceptIntersect, solutionUnacceptAndGlobalReqIntersect };
            List<List<string>> solutionStraightLevelIntersects = new List<List<string>> { solutionUnacceptStraightLevelIntersect, solutionReqStraightLevelIntersect };

            AddReferenceMatchErrorsToList(ErrorLevel.Solution, "", false, solutionReferencesIntersect);
            AddReferenceMatchErrorsToList(ErrorLevel.Global, "", false, globalReferencesIntersect);

            AddReferenceMatchWarningsToList(ErrorLevel.Global, ErrorLevel.Solution, "", false, solutionCrossLevelIntersects);
            AddReferenceMatchWarningsToList(ErrorLevel.Global, ErrorLevel.Solution, "", true, solutionStraightLevelIntersects);
        }

        private static void CheckProjectRulesOnMatchConflicts(List<string> solutionRequiredReferences, List<string> solutionUnacceptableReferences, List<string> globalRequiredReferences, List<string> globalUnacceptableReferences, List<string> requiredReferences, List<string> unacceptableReferences, string projName)
        {
            List<string> projectReferencesIntersect = requiredReferences.Intersect(unacceptableReferences).ToList();

            List<string> projectReqAndGlobalUnacceptIntersect = requiredReferences.Intersect(globalUnacceptableReferences).ToList();
            List<string> projectReqAndSolutionUnacceptIntersect = requiredReferences.Intersect(solutionUnacceptableReferences).ToList();
            List<string> projectReqGlobalIntersect = requiredReferences.Intersect(globalRequiredReferences).ToList();
            List<string> projectReqSolutionIntersect = requiredReferences.Intersect(solutionRequiredReferences).ToList();

            List<string> projectUnacceptAndGlobalReqIntersect = unacceptableReferences.Intersect(globalRequiredReferences).ToList();
            List<string> projectUnacceptAndSolutionReqIntersect = unacceptableReferences.Intersect(solutionRequiredReferences).ToList();
            List<string> projectUnacceptGlobalIntersect = unacceptableReferences.Intersect(globalUnacceptableReferences).ToList();
            List<string> projectUnacceptSolutionIntersect = unacceptableReferences.Intersect(solutionUnacceptableReferences).ToList();

            List<List<string>> projectGlobalCrossLevelIntersects = new List<List<string>>() { projectReqAndGlobalUnacceptIntersect, projectUnacceptAndGlobalReqIntersect };
            List<List<string>> projectSoluionCrossLevelIntesects = new List<List<string>>() { projectReqAndSolutionUnacceptIntersect, projectUnacceptAndSolutionReqIntersect };
            List<List<string>> projectGlobalStraightLevelIntersects = new List<List<string>>() { projectUnacceptGlobalIntersect, projectReqGlobalIntersect };
            List<List<string>> projectSolutionStraightLevelIntersects = new List<List<string>>() { projectUnacceptSolutionIntersect, projectReqSolutionIntersect };

            AddReferenceMatchErrorsToList(ErrorLevel.Project, projName, false, projectReferencesIntersect);

            AddReferenceMatchWarningsToList(ErrorLevel.Global, ErrorLevel.Project, projName, false, projectGlobalCrossLevelIntersects);
            AddReferenceMatchWarningsToList(ErrorLevel.Solution, ErrorLevel.Project, projName, false, projectSoluionCrossLevelIntesects);

            AddReferenceMatchWarningsToList(ErrorLevel.Global, ErrorLevel.Project, projName, true, projectGlobalStraightLevelIntersects);
            AddReferenceMatchWarningsToList(ErrorLevel.Solution, ErrorLevel.Project, projName, true, projectSolutionStraightLevelIntersects);
        }

        private static void AddReferenceMatchErrorsToList(ErrorLevel referenceLevel, string projName, bool isProjectNameMatchError, List<string> currentIntersect)
        {
            refsMatchErrorList.AddRange(
                currentIntersect.ConvertAll(currentReference =>
                    new ReferenceMatchError(referenceLevel, currentReference, projName, isProjectNameMatchError)
                )
            );
        }

        private static void AddReferenceMatchWarningsToList(ErrorLevel highReferenceLevel, ErrorLevel lowReferenceLevel, string projName, bool isReferenceStraight, List<List<string>> currentIntersect)
        {
            bool isHighLevelReq = false;

            foreach (List<string> currentCrossLevelIntersect in currentIntersect)
            {
                refsMatchWarningList.AddRange(
                    currentCrossLevelIntersect.ConvertAll(currentReference =>
                        new ReferenceMatchWarning(highReferenceLevel, lowReferenceLevel, currentReference, projName, isReferenceStraight, isHighLevelReq)
                    )
                );

                isHighLevelReq = !isHighLevelReq;
            }
        }

        private static void CheckProjectTargetFrameworkVersion(string currentProjectFrameworkVersion, string maxFrameworkVersion, string projName, ErrorLevel errorLevel)
        {
            var currentProjFrameworkVersionArray = currentProjectFrameworkVersion.Split('.');
            var currentProjFrWorkVerArrayLength = currentProjFrameworkVersionArray.Length;
            var maxFrameworkVersionArray = maxFrameworkVersion.Split('.');
            var maxFrameworkVersionArrayLength = maxFrameworkVersionArray.Length;
            var minLengthValue = Math.Min(maxFrameworkVersionArrayLength, currentProjFrWorkVerArrayLength);

            currentProjFrameworkVersionArray[0] = currentProjFrameworkVersionArray[0].Substring(currentProjFrameworkVersionArray[0].Length - 1); //Это не будет работать для "Xamarin!"
            currentProjFrameworkVersionArray[currentProjFrWorkVerArrayLength - 1] = currentProjFrameworkVersionArray[currentProjFrWorkVerArrayLength - 1].Substring(0, 1);

            for (int i = 0; i < minLengthValue; i++)
            {
                int currentProjCurrentNum;
                int maxVersionCurrentNum;
                if (! Int32.TryParse(currentProjFrameworkVersionArray[i], out currentProjCurrentNum))
                {
                    //предупреждение без типа о том, что не удалось спарсить название проекта и проверка версии фреймворка не получилась

                    ErrorTask errorTask = new ErrorTask
                    {
                        Category = TaskCategory.User,
                        ErrorCategory = TaskErrorCategory.Warning,
                        Text = "RefDepGuard warning: Не получилось произвести проверку версии 'TargetFramework' для проекта '"+ projName  + "', так как программе не удалось получить из .csproj файла корректное значение для этого свойства. Проверьте, что проект имеет корректную версию 'TargetFramework'"
                    };

                    errorListProvider.Tasks.Add(errorTask);

                    return;
                }

                if (!Int32.TryParse(maxFrameworkVersionArray[i], out maxVersionCurrentNum))
                {
                    //Ошибка когда найдено некорректное значение max_framework_version в config-файле 
                    //Фиксируется раньше при проверке противоречий между значениями max_framework_version различных уровней

                    return;
                }

                if (currentProjCurrentNum > maxVersionCurrentNum)
                {
                    //Ошибка, когда "TargetFramework" оказался больше чем максимально допустимый
                    var currentFrameworkVersionComparabilityError = 
                        new FrameworkVersionComparabilityError(errorLevel, GetTargetFrameworkVersionString(currentProjFrameworkVersionArray), maxFrameworkVersion, projName);
                    frameworkVersionComparabilityErrorList.Add(currentFrameworkVersionComparabilityError);

                    return;
                }
                else
                {
                    if (currentProjCurrentNum < maxVersionCurrentNum)
                        return;
                }
            }

            if(currentProjFrWorkVerArrayLength > maxFrameworkVersionArrayLength) //Если в текущей версии есть ещё не рассмотренные цифры
            {
                for (int i = minLengthValue; i < currentProjFrWorkVerArrayLength; i++)
                {
                    int currentProjVersionCurrentNum;

                    if (!Int32.TryParse(currentProjFrameworkVersionArray[i], out currentProjVersionCurrentNum))
                    {
                        ErrorTask errorTask = new ErrorTask
                        {
                            Category = TaskCategory.User,
                            ErrorCategory = TaskErrorCategory.Warning,
                            Text = "RefDepGuard warning: Не получилось произвести проверку версии 'TargetFramework' для проекта '" + projName + "', так как программе не удалось получить из .csproj файла корректное значение для этого свойства. Проверьте, что проект имеет корректную версию 'TargetFramework'"
                        };

                        errorListProvider.Tasks.Add(errorTask);

                        break;
                    }

                    if(currentProjVersionCurrentNum > 0)
                    {
                        var currentFrameworkVersionComparabilityError =
                        new FrameworkVersionComparabilityError(errorLevel, GetTargetFrameworkVersionString(currentProjFrameworkVersionArray), maxFrameworkVersion, projName);

                        frameworkVersionComparabilityErrorList.Add(currentFrameworkVersionComparabilityError);

                        break;
                    }
                }
            }
        }

        private static string GetTargetFrameworkVersionString(string[] targetFrameworkVersionArray)
        {
            string outputString = "";
            bool isFirstIteration = true;

            foreach (var item in targetFrameworkVersionArray)
            {
                if (isFirstIteration)
                {
                    outputString += item;
                    isFirstIteration = false;
                }else
                    outputString += "." + item;
            }

            return outputString;
        }

        private static void CheckPrjMaxFrwrkVrsnDifferentLevelsConflicts(string maxLowLevelFrameworkVersion, string maxHighLevelFrameworkVersion, string projName, ErrorLevel lowRuleLevel, ErrorLevel highRuleLevel)
        {
            var maxLowLevelFrameworkVersionArray = maxLowLevelFrameworkVersion.Split('.');
            var maxHighLevelFrameworkVersionArray = maxHighLevelFrameworkVersion.Split('.');
            var maxHighLevelFrameworkVersionArrayLength = maxHighLevelFrameworkVersionArray.Length;
            var maxLowLevelFrameworkVersionArrayLength = maxLowLevelFrameworkVersionArray.Length;

            var minLengthValue = Math.Min(maxLowLevelFrameworkVersionArrayLength, maxHighLevelFrameworkVersionArrayLength);

            for (int i = 0; i < minLengthValue; i++)
            {
                int currentLowLevelFrameworkVersionNum;
                int currentHighLevelFrameworkVersionNum;

                if (!Int32.TryParse(maxLowLevelFrameworkVersionArray[i], out currentLowLevelFrameworkVersionNum))
                {
                    //Ошибка когда найдено некорректное значение max_framework_version в config-файле 
                    MaxFrameworkVersionDeviantValue potentialMaxFrameworkVersionDeviantValueError;
                    if (lowRuleLevel == ErrorLevel.Project)
                    {
                        potentialMaxFrameworkVersionDeviantValueError = new MaxFrameworkVersionDeviantValue(lowRuleLevel, projName);
                        maxFrameworkVersionDeviantValueList.Add(potentialMaxFrameworkVersionDeviantValueError);
                    }
                    else
                    {
                        potentialMaxFrameworkVersionDeviantValueError = new MaxFrameworkVersionDeviantValue(lowRuleLevel, "");

                        if (!maxFrameworkVersionDeviantValueList.Contains(potentialMaxFrameworkVersionDeviantValueError, new MaxFrameworkVersionDeviantValueContainsComparer()))
                            maxFrameworkVersionDeviantValueList.Add(potentialMaxFrameworkVersionDeviantValueError);
                    }
                    return;
                }

                if (!Int32.TryParse(maxHighLevelFrameworkVersionArray[i], out currentHighLevelFrameworkVersionNum))
                {
                    MaxFrameworkVersionDeviantValue potentialMaxFrameworkVersionDeviantValueError = new MaxFrameworkVersionDeviantValue(highRuleLevel, "");

                    if (!maxFrameworkVersionDeviantValueList.Contains(potentialMaxFrameworkVersionDeviantValueError, new MaxFrameworkVersionDeviantValueContainsComparer()))
                        maxFrameworkVersionDeviantValueList.Add(potentialMaxFrameworkVersionDeviantValueError);
                    
                    return;
                }

                if(currentHighLevelFrameworkVersionNum < currentLowLevelFrameworkVersionNum)
                {
                    //реализовать contains проверку!
                    //Warning о противоречии между рефами
                    maxFrameworkVersionConflictWarningsList.Add(
                        new MaxFrameworkVersionConflictWarning(highRuleLevel, lowRuleLevel, maxHighLevelFrameworkVersion, maxLowLevelFrameworkVersion, projName));

                    return;
                }
                else
                {
                    if (currentHighLevelFrameworkVersionNum > currentLowLevelFrameworkVersionNum)
                        return;
                }
            }

            if (maxHighLevelFrameworkVersionArrayLength < maxLowLevelFrameworkVersionArrayLength)
            {
                for (int i = 0; i < maxLowLevelFrameworkVersionArrayLength; i++)
                {
                    int currentLowLevelFrameworkVersionNum;

                    if (!Int32.TryParse(maxLowLevelFrameworkVersionArray[i], out currentLowLevelFrameworkVersionNum))
                    {
                        MaxFrameworkVersionDeviantValue potentialMaxFrameworkVersionDeviantValueError = new MaxFrameworkVersionDeviantValue(lowRuleLevel, "");

                        if (!maxFrameworkVersionDeviantValueList.Contains(potentialMaxFrameworkVersionDeviantValueError, new MaxFrameworkVersionDeviantValueContainsComparer()))
                            maxFrameworkVersionDeviantValueList.Add(potentialMaxFrameworkVersionDeviantValueError);

                        break;
                    }

                    if(currentLowLevelFrameworkVersionNum > 0)
                    {
                        //Warning о противоречии между рефами
                        maxFrameworkVersionConflictWarningsList.Add(
                            new MaxFrameworkVersionConflictWarning(highRuleLevel, lowRuleLevel, maxHighLevelFrameworkVersion, maxLowLevelFrameworkVersion, projName));

                        break;
                    }
                }
            }
        }

        private static void CheckRulesFromConfigFile()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (errorListProvider != null)
                errorListProvider.Tasks.Clear();

            if(configPropertyNullErrorList !=  null)
                configPropertyNullErrorList.Clear();

            if (refsErrorList!= null)
                refsErrorList.Clear();

            if(refsMatchErrorList != null)
                refsMatchErrorList.Clear();

            CheckConfigPropertiesOnNotNull();

            string maxGlobalFrameworkVersion = configFileGlobal?.framework_max_version ?? "-";
            string maxSolutionFrameworkVersion = configFileSolution?.framework_max_version ?? "-";

            List<string> solutionRequiredReferences = configFileSolution?.solution_required_references ?? new List<string>();
            List<string> solutionUnacceptableReferences = configFileSolution?.solution_unacceptable_references ?? new List<string>();

            List<string> globalRequiredReferences = configFileGlobal?.global_required_references ?? new List<string>();
            List<string> globalUnacceptableReferences = configFileGlobal?.global_unacceptable_references ?? new List<string>();

            List<ReferenceAffiliation> unionSolutionAndGlobalReferencesByType = new List<ReferenceAffiliation>
            {
                new ReferenceAffiliation(ErrorLevel.Solution, solutionRequiredReferences, solutionUnacceptableReferences),
                new ReferenceAffiliation(ErrorLevel.Global, globalRequiredReferences, globalUnacceptableReferences)
            };

            requiredReferencesList.AddRange(globalRequiredReferences.ConvertAll(value => new RequiredReference(value, "")));
            requiredReferencesList.AddRange(solutionRequiredReferences.ConvertAll(value => new RequiredReference(value, "")));

            CheckRulesOnMatchConflicts(solutionRequiredReferences, solutionUnacceptableReferences, globalRequiredReferences, globalUnacceptableReferences);

            foreach (KeyValuePair<string, ProjectState> currentProjState in commitedProjState)//для каждого project
            {
                var projName = currentProjState.Key;
                var projReferences = currentProjState.Value.CurrentReferences;
                var projFrameworkVersion = currentProjState.Value.CurrentFrameworkVersion;

                if (configFileSolution?.projects?.ContainsKey(projName) ?? false)
                {
                    ConfigFileProject currentProjectConfigFileSettings = configFileSolution.projects[projName];

                    bool isConsiderRequiredReferences = currentProjectConfigFileSettings.consider_global_and_solution_references?.required ?? true; //Проверка на отключение глобальных и solution рефов для проекта
                    bool isConsiderUnacceptableReferences = currentProjectConfigFileSettings.consider_global_and_solution_references?.unacceptable ?? true;
                    string maxFrameworkVersion = currentProjectConfigFileSettings?.framework_max_version ?? "-";

                    List<string> requiredReferences = currentProjectConfigFileSettings?.required_references ?? new List<string>();
                    List<string> unacceptableReferences = currentProjectConfigFileSettings?.unacceptable_references ?? new List<string>();

                    List<List<string>> configFileProjectAndSolutionReferences = new List<List<string>>
                    {
                        requiredReferences, unacceptableReferences, solutionRequiredReferences, solutionUnacceptableReferences
                    };

                    if (maxFrameworkVersion == "-") {
                        if(maxSolutionFrameworkVersion != "-")//проверить на противоречие с global
                        {
                            if(maxGlobalFrameworkVersion != "-")
                                CheckPrjMaxFrwrkVrsnDifferentLevelsConflicts(maxSolutionFrameworkVersion, maxGlobalFrameworkVersion, "", ErrorLevel.Solution, ErrorLevel.Global);

                            CheckProjectTargetFrameworkVersion(projFrameworkVersion, maxSolutionFrameworkVersion, projName, ErrorLevel.Solution);
                        } 
                        else
                        {
                            if(maxGlobalFrameworkVersion != "-")
                                CheckProjectTargetFrameworkVersion(projFrameworkVersion, maxGlobalFrameworkVersion, projName, ErrorLevel.Global);
                        }
                    }
                    else//Проверить на противоречие с уровнем solution и global
                    {
                        if(maxSolutionFrameworkVersion != "-")
                            CheckPrjMaxFrwrkVrsnDifferentLevelsConflicts(maxFrameworkVersion, maxSolutionFrameworkVersion, projName, ErrorLevel.Project, ErrorLevel.Solution);

                        if(maxGlobalFrameworkVersion != "-")
                            CheckPrjMaxFrwrkVrsnDifferentLevelsConflicts(maxFrameworkVersion, maxGlobalFrameworkVersion, projName, ErrorLevel.Project, ErrorLevel.Global);

                        CheckProjectTargetFrameworkVersion(projFrameworkVersion, maxFrameworkVersion, projName, ErrorLevel.Project);
                    } 

                    requiredReferencesList.AddRange(requiredReferences.ConvertAll(value => new RequiredReference(value, projName)));

                    CheckProjectRulesOnMatchConflicts(solutionRequiredReferences, solutionUnacceptableReferences, globalRequiredReferences, 
                        globalUnacceptableReferences, requiredReferences, unacceptableReferences, projName);
                    
                    CheckRulesForProjectReferences(projName, projReferences, requiredReferences, true); 
                    CheckRulesForProjectReferences(projName, projReferences, unacceptableReferences, false);

                    foreach (ReferenceAffiliation referenceAffiliation in unionSolutionAndGlobalReferencesByType)
                    {
                        if (isConsiderRequiredReferences)//если заявлено
                            //применяем глобальные референсы
                            CheckRulesForSolutionOrGlobalReferences(projName, projReferences, referenceAffiliation.RequiredReferences, referenceAffiliation.ReferenceTypeValue, true, configFileProjectAndSolutionReferences);

                        if (isConsiderUnacceptableReferences)
                            CheckRulesForSolutionOrGlobalReferences(projName, projReferences, referenceAffiliation.UnacceptableReferences, referenceAffiliation.ReferenceTypeValue, false, configFileProjectAndSolutionReferences);
                    }
                        
                }
                else
                {

                    //Проект есть в solution но его нет в config
                }

                //А что делать если проекта нет в solution, но он есть в config?
                //Рассмотреть в т.ч. случаи когда свойство projects пустое

            }

            

            refsErrorList.Sort(new ReferenceErrorComparer());
            refsMatchErrorList.Sort(new ReferenceMatchErrorSortComparer());
            configPropertyNullErrorList.Sort(new ConfigFilePropertyNullErrorSortComparer());
            maxFrameworkVersionDeviantValueList.Sort(new MaxFrameworkVersionDeviantValueSortComparer());
            frameworkVersionComparabilityErrorList.Sort(new FrameworkVersionComparabilityErrorSortComparer());

            StoreErrorListProviderByValues();

            if (errorListProvider != null)
                errorListProvider.Show();
        }

        private static void CommitCurrentReferences()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            commitedProjState.Clear();

            EnvDTE.Solution solution = dte.Solution;

            foreach (EnvDTE.Project project in solution.Projects)
            {

                var projectFrameworkVersion = MSBuildManager.GetTargetFrameworkForProject(project.FullName);

                VSLangProj.VSProject vSProject = project.Object as VSLangProj.VSProject;
                
                if (vSProject != null)
                {
                    var refsList = new List<string>();

                    foreach (VSLangProj.Reference vRef in vSProject.References)
                    {

                        if (vRef.SourceProject != null)
                        {
                            refsList.Add(vRef.Name);
                        }
                    }

                    commitedProjState.Add(vSProject.Project.Name, new ProjectState(projectFrameworkVersion, refsList));

                }
            }
        }

        private static bool IsReferencesAddedCorrectly() //Срабатывает не только в случаях, когда не успели прогрузиться рефы, но и когда рефов попросту нет (что на самом деле странно и тоже заслуживает предупреждения)
        {
            foreach (KeyValuePair<string, ProjectState> keyValuePair in commitedProjState)
            {
                if (keyValuePair.Value.CurrentReferences.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void RestoreInfoToRollbackFile(string currentConfigGuardFile, string currentConfigGuardRollbackFile)
        {
            try
            {
                string fileInfo;

                using (FileStream fileStream = new FileStream(currentConfigGuardFile, FileMode.Open))
                {
                    StreamReader sr = new StreamReader(fileStream);

                    fileInfo = sr.ReadToEnd();
                }

                using (FileStream fileStream = File.Create(currentConfigGuardRollbackFile))
                {
                    StreamWriter streamWriter = new StreamWriter(fileStream);

                    streamWriter.Write(fileInfo);

                    streamWriter.Flush();
                    fileStream.Flush();

                    streamWriter.Close();

                }
            }
            catch (Exception ex)
            {

                VsShellUtilities.ShowMessageBox( 
                        this.package,
                        "Проверьте, что глобальный и локальные .rdg файлы не имеют запретов на чтение, а в корневой папке solution нет запрета на создание файлов",
                        "RefDepGuard: Ошибка генерации Rollback файла",
                        OLEMSGICON.OLEMSGICON_INFO,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST
                     );

            }
        }

        private void CreateNewConfigFile(string currentConfigGuardFile, bool isGlobal)
        {

            using (FileStream fileStream = File.Create(currentConfigGuardFile))
            {
                StreamWriter streamWriter = new StreamWriter(fileStream);
                string json;

                if(isGlobal)
                    json = JsonConvert.SerializeObject(generateDefaultConfigFileGlobal());
                else
                    json = JsonConvert.SerializeObject(generateDefaultConfigFileSolution());

                streamWriter.Write(json);

                streamWriter.Flush();
                fileStream.Flush();

                streamWriter.Close();
            }
        }

        private ConfigFileSolution generateDefaultConfigFileSolution()
        {
            configFileSolution = new ConfigFileSolution();
            configFileSolution.name = solutionName;
            configFileSolution.framework_max_version = "-";
            configFileSolution.solution_required_references = new List<string>();
            configFileSolution.solution_unacceptable_references = new List<string>();
            configFileSolution.projects = new Dictionary<string, ConfigFileProject>();

            foreach (var projectName in commitedProjState.Keys)
            {
                ConfigFileProjectRefsConsidering configFileProjectRefsConsidering = new ConfigFileProjectRefsConsidering();
                configFileProjectRefsConsidering.required = true;
                configFileProjectRefsConsidering.unacceptable = true;

                ConfigFileProject fileProject = new ConfigFileProject();
                fileProject.framework_max_version = "-";
                fileProject.consider_global_and_solution_references = configFileProjectRefsConsidering;
                fileProject.required_references = new List<string>();
                fileProject.unacceptable_references = new List<string>();

                configFileSolution.projects.Add(projectName, fileProject);
            }

            return configFileSolution;
        }

        private ConfigFileGlobal generateDefaultConfigFileGlobal()
        {
            configFileGlobal = new ConfigFileGlobal();
            configFileGlobal.name = "Global";
            configFileGlobal.framework_max_version = "-";
            configFileGlobal.global_required_references = new List<string>();
            configFileGlobal.global_unacceptable_references = new List<string>();

            return configFileGlobal;
        }

        private void showConfigFileParseErrorMessage(string errorReason, bool isErrorGlobal, bool isFileExists) 
        {
            string rollbackAction = "";
            string solutionNameInfo = "";

            if (isFileExists)
                rollbackAction = "Информация, содержащаяся в файле конфигурации будет перезаписана в rollback-файл.\r\nПроверьте её на предмет отсутствия синтаксических ошибок и соответствия шаблону файла конфигурации!";
              
            if (!isErrorGlobal)
                solutionNameInfo = " для solution '" + solutionName + "'";
            
                VsShellUtilities.ShowMessageBox(
                            this.package,
                            errorReason + solutionNameInfo + ".\r\n Шаблон файла конфигурации будет сгенерирован расширением" + ". \r\n" + rollbackAction,
                            "RefDepGuard Error: Ошибка загрузки файла конфигурации",
                            OLEMSGICON.OLEMSGICON_INFO,
                            OLEMSGBUTTON.OLEMSGBUTTON_OK,
                            OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST
                         );
        }

        private void GetCurrentConfigFileInfo(ConfigFileServiceInfo configFileServiceInfo)
        {
            if (File.Exists(configFileServiceInfo.SolutionConfigGuardFile))
            {
                try
                {
                    using (FileStream fileStream = new FileStream(configFileServiceInfo.SolutionConfigGuardFile, FileMode.Open))
                    {
                        StreamReader sr = new StreamReader(fileStream);

                        if(configFileServiceInfo.IsGlobal)
                            configFileGlobal = JsonConvert.DeserializeObject<ConfigFileGlobal>(sr.ReadToEnd());
                        else
                            configFileSolution = JsonConvert.DeserializeObject<ConfigFileSolution>(sr.ReadToEnd());
                    }
                }
                catch (Exception ex)
                {
                    showConfigFileParseErrorMessage(configFileServiceInfo.FileErrorMessage.BadDataErrorMessage, false, true);

                    RestoreInfoToRollbackFile(configFileServiceInfo.SolutionConfigGuardFile, configFileServiceInfo.SolutionConfigGuardRollbackFile);

                    CreateNewConfigFile(configFileServiceInfo.SolutionConfigGuardFile, configFileServiceInfo.IsGlobal);

                }

            }
            else
            {
                showConfigFileParseErrorMessage(configFileServiceInfo.FileErrorMessage.FileNotFoundErrorMessage, false, false);

                CreateNewConfigFile(configFileServiceInfo.SolutionConfigGuardFile, configFileServiceInfo.IsGlobal);
            }
        }

        private void GetConfigFileInfo()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string dteSolutionFullName = dte.Solution.FullName;
            int lastDotIndex = dteSolutionFullName.LastIndexOf('.');
            int lastSlashIndex = dteSolutionFullName.LastIndexOf('\\');
            string solutionExtendedName = dteSolutionFullName.Substring(0, lastDotIndex);
            packageExtendedName = dteSolutionFullName.Substring(0, lastSlashIndex);

            solutionName = dteSolutionFullName.Substring(lastSlashIndex + 1, lastDotIndex - lastSlashIndex - 1);

            string solutionConfigGuardFile = solutionExtendedName + "_config_guard.rdg";
            string globalConfigGuardFile = packageExtendedName + "\\global_config_guard.rdg";

            string solutionConfigGuardRollbackFile = solutionExtendedName + "_config_guard_rollback.rdg";
            string globalConfigGuardRollbackFile = packageExtendedName + "\\global_config_guard_rollback.rdg";

            FileErrorMessage currentSolutionFileErrorMessages = new FileErrorMessage("Не получилось загрузить файл конфигурации", "Файл конфигурации не найден");
            FileErrorMessage globalFileErrorMessages = new FileErrorMessage("Не получилось загрузить глобальный файл конфигурации", "Глобальный файл конфигурации не найден");

            ConfigFileServiceInfo currentSolutionConfigFileServiceInfo = new ConfigFileServiceInfo(false, solutionConfigGuardFile, solutionConfigGuardRollbackFile, currentSolutionFileErrorMessages);
            ConfigFileServiceInfo globalSolutionConfigFileServiceInfo = new ConfigFileServiceInfo(true, globalConfigGuardFile, globalConfigGuardRollbackFile, globalFileErrorMessages);

            GetCurrentConfigFileInfo(currentSolutionConfigFileServiceInfo);

            GetCurrentConfigFileInfo(globalSolutionConfigFileServiceInfo);
        }
    }
}
