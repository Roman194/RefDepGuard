using EnvDTE;
using EnvDTE80;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VSIXProject1.Data;
using VSIXProject1.Data.Reference;
using VSLangProj;
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

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("c19eaee0-a475-4f4d-821f-194a1447a90d");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        private static DTE dte;
        //private static DTE2 dte2;

        static Dictionary<string, List<string>> addedRefs = new Dictionary<string, List<string>>();
        static List<string> changedRefs = new List<string>();
        static Dictionary<string, List<string>> removedRefs = new Dictionary<string, List<string>>();

        static Dictionary<string, List<string>> commitedProjState = new Dictionary<string, List<string>>();

        static List<ConfigFilePropertyNullError> configPropertyNullErrorList = new List<ConfigFilePropertyNullError>();
        static List<ReferenceError> refsErrorList = new List<ReferenceError>();
        static List<ReferenceMatchError> refsMatchErrorList = new List<ReferenceMatchError>();

        static ConfigFileSolution configFileSolution;
        static ConfigFileGlobal configFileGlobal;

        static string solutionName;

        static ErrorListProvider errorListProvider;
        static IVsOutputWindowPane generalPane;
        static Guid generalPaneGuid;
        static IVsOutputWindow outWindow;


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

            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            var getChangedRefsMenuItem = new MenuCommand(this.ExcecuteChanges, getChangedRefsMenuCommandID);
            var commitCurrentRefsMenuItem = new MenuCommand(this.CommitCurrentReferences, commitCurrentRefsMenuCommandID);

            commandService.AddCommand(menuItem);
            commandService.AddCommand(getChangedRefsMenuItem);
            commandService.AddCommand(commitCurrentRefsMenuItem);

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
            dte.Events.BuildEvents.OnBuildBegin += new _dispBuildEvents_OnBuildBeginEventHandler(BuildBegined);
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
            //dte.Solution.Parent.Solution
            //Пример проекта с несколькими Solution?

            commitedProjState.Clear();

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

                    commitedProjState.Add(vSProject.Project.Name, refsList);
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
                    var vsCommitedProjRefsHashSet = new HashSet<string> (commitedProjState[vSProject.Project.Name]);

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
                        commitedProjState[vSProject.Project.Name] = vsCurrentProjHashSet.ToList();

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
            //commitedProjState.Clear();

            CommitCurrentReferences();

            CheckRulesFromConfigFile();

            //VsShellUtilities.ShowMessageBox(
            //    this.package,
            //    message,
            //    title,
            //    OLEMSGICON.OLEMSGICON_INFO,
            //    OLEMSGBUTTON.OLEMSGBUTTON_OK,
            //    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private static void ShowUnsuccessfulCheckingRulesWarning()
        {
            if (errorListProvider != null)
                errorListProvider.Tasks.Clear();

            ErrorTask errorTask = new ErrorTask
            {
                Category = TaskCategory.User,
                ErrorCategory = TaskErrorCategory.Warning,
                Text = "RefDepGuard warning: Не получилось проверить соответствие референсов правилам во время загрузки solution. Произведите проверку вручную или начните сборку для запуска проверки"

            };

            errorListProvider.Tasks.Add(errorTask);

            errorListProvider.Show();
        }

        private static void CheckRulesForSolutionOrGlobalReferences(string projName, List<string> projReferences, List<string> currentReferences,  ReferenceLevel referenceLevel, bool isReferenceRequired, List<List<string>> generalReferences)
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
                        if(fileReference == projName)
                        {
                            refsMatchErrorList.Add(
                                new ReferenceMatchError(ReferenceLevel.Project, fileReference, projName, true)
                                );

                            continue;
                        }
                            
                        if (refsMatchErrorList.Contains(new ReferenceMatchError(ReferenceLevel.Project, fileReference, projName, false), new ReferenceMatchErrorComparer()))
                            continue;

                        refsErrorList.Add(
                            new ReferenceError(fileReference, projName, isReferenceRequired, ReferenceLevel.Project)
                            );
                    }
                }
            }
        }

        private static void StoreErrorListProviderByValues() //Оптимизировать
        {
            foreach(ConfigFilePropertyNullError configFilePropertyNullError in configPropertyNullErrorList)
            {
                string documentName = solutionName + "_config_guard.rdg";
                string relevantProjectName = "";

                if (configFilePropertyNullError.IsGlobal)
                    documentName = "global_config_guard.rdg";

                if (configFilePropertyNullError.ErrorRelevantProjectName != "")
                    relevantProjectName = " для проекта '" + configFilePropertyNullError.ErrorRelevantProjectName + "'";

                ErrorTask errorTask = new ErrorTask
                {
                    Category = TaskCategory.User,
                    ErrorCategory = TaskErrorCategory.Error,
                    Document = documentName,
                    Text = "RefDepGuard Null property error: Config-файл не содержит свойство '" + configFilePropertyNullError.PropertyName + "'" + relevantProjectName + ". Проверьте его на предмет отсутствия синтаксических ошибок и соответствия шаблону файла конфигурации"
                };

                errorListProvider.Tasks.Add(errorTask);
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
                {
                    projectName = "' проекта '" + referenceMatchError.ProjectName;
                }

                switch (referenceMatchError.ReferenceLevelValue)
                {
                    case ReferenceLevel.Solution: referenceLevelText = "уровня Solution"; break;
                    case ReferenceLevel.Global: referenceLevelText = "глобального уровня"; documentName = "global_config_guard.rdg"; break;
                    case ReferenceLevel.Project: break;
                }

                ErrorTask errorTask = new ErrorTask
                {
                    Category = TaskCategory.User,
                    ErrorCategory = TaskErrorCategory.Error,
                    Document = documentName,
                    Text = "RefDepGuard Match error: референс '" + referenceMatchError.ReferenceName + projectName + "' "+ referenceLevelText + matchErrorDescription + ". Устраните противоречие в правиле"

                };

                errorListProvider.Tasks.Add(errorTask);
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
                    
                switch (error.CurrentReferenceType)
                {
                    case ReferenceLevel.Solution: referenceLevelText = "уровня Solution"; break;
                    case ReferenceLevel.Global: referenceLevelText = "глобального уровня"; break;
                    case ReferenceLevel.Project: break;
                }

                ErrorTask errorTask = new ErrorTask
                {
                    Category = TaskCategory.User,
                    ErrorCategory = TaskErrorCategory.Error,
                    Document = documentName,
                    Text = "RefDepGuard Reference error: " + referenceTypeText + " референс " + referenceLevelText + " '" + error.ReferenceName + "' для проекта '" + error.ErrorRelevantProjectName + "'. " + actionForUser + " его через обозреватель решений"

                };

                errorListProvider.Tasks.Add(errorTask);
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


        //реализовать в проге работу с несколькими Solution


        private static bool IsRuleConflict(string currentReference, ReferenceLevel referenceType, List<List<string>> generalReferences)//Перебрать для каждого solution и Global рефа все нижестоящие на предмет противоречий
        {
            for(int i = 0; i < generalReferences.Count; i++)
            {
                if (referenceType != ReferenceLevel.Global && i > 1)
                    break;

                if(generalReferences[i].Contains(currentReference))
                    return true;
            }

            return false;
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

            List<string> solutionRequiredReferences = configFileSolution?.solution_required_references ?? new List<string>();
            List<string> solutionUnacceptableReferences = configFileSolution?.solution_unacceptable_references ?? new List<string>();
            List<string> solutionReferencesIntersect = solutionRequiredReferences.Intersect(solutionUnacceptableReferences).ToList();

            List<string> globalRequiredReferences = configFileGlobal?.global_required_references ?? new List<string>();
            List<string> globalUnacceptableReferences = configFileGlobal?.global_unacceptable_references ?? new List<string>();
            List<string> globalReferencesIntersect = globalRequiredReferences.Intersect(globalUnacceptableReferences).ToList();

            List<ReferenceAffiliation> unionSolutionAndGlobalReferencesByType = new List<ReferenceAffiliation>
            {
                new ReferenceAffiliation(ReferenceLevel.Solution, solutionRequiredReferences, solutionUnacceptableReferences),
                new ReferenceAffiliation(ReferenceLevel.Global, globalRequiredReferences, globalUnacceptableReferences)
            };


            foreach (string currentReference in solutionReferencesIntersect)
            {
                refsMatchErrorList.Add(
                    new ReferenceMatchError(ReferenceLevel.Solution, currentReference, "", false)
                    );
            }

            foreach (string currentReference in globalReferencesIntersect)
            {
                refsMatchErrorList.Add(
                    new ReferenceMatchError(ReferenceLevel.Global, currentReference, "", false)
                    );
            }

            foreach (KeyValuePair<string, List<string>> currentProjState in commitedProjState)//для каждого project
            {
                var projName = currentProjState.Key;
                var projReferences = currentProjState.Value;

                if (configFileSolution?.projects?.ContainsKey(projName) ?? false)
                {
                    ConfigFileProject currentProjectConfigFileSettings = configFileSolution.projects[projName];

                    bool isConsiderRequiredReferences = currentProjectConfigFileSettings.consider_global_and_solution_references?.required ?? true; //Проверка на отключение глобальных и solution рефов для проекта
                    bool isConsiderUnacceptableReferences = currentProjectConfigFileSettings.consider_global_and_solution_references?.unacceptable ?? true;

                    List<string> requiredReferences = currentProjectConfigFileSettings?.required_references ?? new List<string>();
                    List<string> unacceptableReferences = currentProjectConfigFileSettings?.unacceptable_references ?? new List<string>();

                    List<List<string>> configFileProjectAndSolutionReferences = new List<List<string>>
                    {
                        requiredReferences, unacceptableReferences, solutionRequiredReferences, solutionUnacceptableReferences
                    };

                    List<string> projectReferencesIntersect = requiredReferences.Intersect(unacceptableReferences).ToList();

                    foreach (string currentReference in projectReferencesIntersect)
                    {
                        refsMatchErrorList.Add(
                            new ReferenceMatchError(ReferenceLevel.Project, currentReference, projName, false)
                            );
                    }
                    
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

            

            refsErrorList.Sort(new ReferenceErrorComparer()); //Глобально отсортировать все типы ошибок?

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

                    commitedProjState.Add(vSProject.Project.Name, refsList);

                }
            }
        }

        private static bool IsReferencesAddedCorrectly()
        {
            foreach (KeyValuePair<string, List<string>> keyValuePair in commitedProjState)
            {
                if (keyValuePair.Value.Count > 0)
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
                    showConfigFileParseErrorMessage(configFileServiceInfo.FileErrorMessage.BadDataErrorMessage, false, true); //"Не получилось загрузить файл конфигурации"

                    RestoreInfoToRollbackFile(configFileServiceInfo.SolutionConfigGuardFile, configFileServiceInfo.SolutionConfigGuardRollbackFile);

                    CreateNewConfigFile(configFileServiceInfo.SolutionConfigGuardFile, configFileServiceInfo.IsGlobal);

                }

            }
            else
            {
                showConfigFileParseErrorMessage(configFileServiceInfo.FileErrorMessage.FileNotFoundErrorMessage, false, false); //"Файл конфигурации не найден"

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
            string packageExtendedName = dteSolutionFullName.Substring(0, lastSlashIndex);

            solutionName = dteSolutionFullName.Substring(lastSlashIndex + 1, lastDotIndex - lastSlashIndex - 1); //Проблемно при нескольких Solution

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
