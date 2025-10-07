using EnvDTE;
using EnvDTE80;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Services;
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
using VSLangProj;
using Task = System.Threading.Tasks.Task;
using Newtonsoft.Json;

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

        static List<ReferenceError> refsErrorList = new List<ReferenceError>();

        static ConfigFileSolution configFileSolution;
        static ConfigFileGlobal configFileGlobal;

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

            if (errorListProvider.Tasks.Count > 0) //Коммит завершился с ошибками?
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
            commitedProjState.Clear();

            CommitCurrentReferences();

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

        private static List<ReferenceAffiliation> GetReferenceAffilitaionsList(List<ConfigFileReference> configFileReferences, bool isReferenceGlobal)
        {
            List<ReferenceAffiliation> referenceAffiliations = new List<ReferenceAffiliation>();

            foreach (ConfigFileReference currentReference in configFileReferences)
                referenceAffiliations.Add(new ReferenceAffiliation(currentReference.reference, isReferenceGlobal));

            return referenceAffiliations;
        }

        private static void CheckRulesForSolutionAndGlobalReferences(string projName, List<string> projReferences, List<ReferenceAffiliation> referenceAffiliations, bool isReferenceRequired)
        {
            foreach(ReferenceAffiliation referenceAffiliation in referenceAffiliations)
            {
                if((isReferenceRequired && !projReferences.Contains(referenceAffiliation.Reference)) || 
                    (!isReferenceRequired && projReferences.Contains(referenceAffiliation.Reference)))
                {

                    if (isReferenceRequired && referenceAffiliation.Reference == projName)
                        continue;

                    ReferenceType currentReferenceType;

                    if (referenceAffiliation.IsReferenceGlobal)
                        currentReferenceType = ReferenceType.Global;
                    else
                        currentReferenceType = ReferenceType.Solution;

                    refsErrorList.Add(new ReferenceError(referenceAffiliation.Reference, projName, isReferenceRequired, currentReferenceType));
                }
            }
        }

        private static void CheckRulesForProjectReferences(string projName, List<string> projReferences, List<ConfigFileReference> configFileReferences, bool isReferenceRequired)
        {
            foreach(ConfigFileReference fileReference in configFileReferences)
            {
                if((isReferenceRequired && !projReferences.Contains(fileReference.reference)) ||
                    (!isReferenceRequired && projReferences.Contains(fileReference.reference)))
                {
                    ReferenceError projectRefError = new ReferenceError(fileReference.reference, projName, isReferenceRequired, ReferenceType.Project);
                    
                    var referenceErrorContainsComparer = new ReferenceErrorContainsComparer();

                    if (refsErrorList.Contains(projectRefError, referenceErrorContainsComparer)) //Проверка на "дублирование" текущей ошибки рефа глобальными и solution правилами
                    {
                        refsErrorList.RemoveAll(refError => refError.ReferenceName == projectRefError.ReferenceName && refError.ErrorRelevantProjectName == projectRefError.ErrorRelevantProjectName);
                    }

                    refsErrorList.Add(projectRefError);
                }
            }
        }

        private static void StoreErrorListProviderByValues(List<ReferenceError> referenceErrorList)
        {
            foreach (ReferenceError error in referenceErrorList)
            {
                string referenceTypeText = "";
                string referenceLevelText = "";
                string documentName = "";

                if (error.IsReferenceRequired)
                    referenceTypeText = "Отсутсвует обязательный";
                else
                    referenceTypeText = "Присутствует недопустимый";

                switch (error.CurrentReferenceType)
                {
                    case ReferenceType.Solution: referenceLevelText = "уровня Solution"; break;
                    case ReferenceType.Global: referenceLevelText = "глобального уровня"; break;
                    case ReferenceType.Project: documentName = error.ErrorRelevantProjectName + ".csproj"; break;
                }

                ErrorTask errorTask = new ErrorTask
                {
                    Category = TaskCategory.User,
                    ErrorCategory = TaskErrorCategory.Error,
                    Document = documentName,
                    Text = "RefDepGuard error: " + referenceTypeText + " референс " + referenceLevelText + " '" + error.ReferenceName + "' для проекта '" + error.ErrorRelevantProjectName + "'. Добавьте его через обозреватель решений"

                };

                errorListProvider.Tasks.Add(errorTask);
            }

        }

        private static void CheckRulesFromConfigFile()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (errorListProvider != null)
                errorListProvider.Tasks.Clear();

            List<ReferenceAffiliation> solutionRequiredReferences = GetReferenceAffilitaionsList(configFileSolution.solution_required_references, false);//Нет обработки нескольких solution!
            List<ReferenceAffiliation> solutionUnacceptableReferences = GetReferenceAffilitaionsList(configFileSolution.solution_unacceptable_references, false);

            List<ReferenceAffiliation> globalRequiredReferences = GetReferenceAffilitaionsList(configFileGlobal.global_required_references, true);
            List<ReferenceAffiliation> globalUnacceptableReferences = GetReferenceAffilitaionsList(configFileGlobal.global_unacceptable_references, true);

            List<ReferenceAffiliation> unionRequiredReferences = globalRequiredReferences.Union(solutionRequiredReferences, new ReferenceAffiliationComparer()).ToList();
            List<ReferenceAffiliation> unionUnacceptableRefrences = globalUnacceptableReferences.Union(solutionUnacceptableReferences, new ReferenceAffiliationComparer()).ToList();


            foreach(KeyValuePair<string, List<string>> currentProjState in commitedProjState)//для каждого project
            {
                var projName = currentProjState.Key;
                var projReferences = currentProjState.Value;

                if (configFileSolution.projects.ContainsKey(projName))
                {
                    ConfigFileProject currentProjectConfigFileSettings = configFileSolution.projects[projName];

                    bool isConsiderRequiredReferences = currentProjectConfigFileSettings.consider_global_and_solution_references.required; //Проверка на отключение глобальных  и solution рефов для проекта
                    bool isConsiderUnacceptableReferences = currentProjectConfigFileSettings.consider_global_and_solution_references.unacceptable;

                    if (isConsiderRequiredReferences) //если заявлено
                        CheckRulesForSolutionAndGlobalReferences(projName, projReferences, unionRequiredReferences, true); //применяем глобальные референсы
                    
                    if (isConsiderUnacceptableReferences)
                        CheckRulesForSolutionAndGlobalReferences(projName, projReferences, unionUnacceptableRefrences, false);

                    CheckRulesForProjectReferences(projName, projReferences, currentProjectConfigFileSettings.required_references, true); 
                    CheckRulesForProjectReferences(projName, projReferences, currentProjectConfigFileSettings.unacceptable_references, false);
                }
                else
                {

                    //Проект есть в solution но его нет в config
                }

                //А что делать если проекта нет в solution, но он есть в config?

            }

            

            refsErrorList.Sort(new ReferenceErrorComparer());

            StoreErrorListProviderByValues(refsErrorList);

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

        private void GetConfigFileInfo()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string dteSolutionFullName = dte.Solution.FullName;
            int lastDotIndex = dteSolutionFullName.LastIndexOf('.');
            int lastSlashIndex = dteSolutionFullName.LastIndexOf('\\');
            string solutionExtendedName = dteSolutionFullName.Substring(0, lastDotIndex);
            string packageExtendedName = dteSolutionFullName.Substring(0, lastSlashIndex);

            try
            {
                using (FileStream fileStream = new FileStream(solutionExtendedName + "_config_guard.rdg", FileMode.Open))
                {

                    StreamReader sr = new StreamReader(fileStream);

                    configFileSolution = JsonConvert.DeserializeObject<ConfigFileSolution>(sr.ReadToEnd());
                }
            }
            catch (Exception)
            {

                var solutionName = solutionExtendedName.Split('\\');

                VsShellUtilities.ShowMessageBox(
                    this.package,
                    "Не получилось загрузить файл конфигурации для solution '"+ solutionName + "'. \r\n Шаблон файла конфигурации будет создан расширением",
                    "RefDepGuard: Ошибка загрузки файла конфигурации",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                configFileSolution = new ConfigFileSolution();
                configFileSolution.name = solutionName[solutionName.Length - 1];
                configFileSolution.framework_max_version = "-";
                configFileSolution.solution_required_references = new List<ConfigFileReference>();
                configFileSolution.solution_unacceptable_references = new List<ConfigFileReference>();
                configFileSolution.projects = new Dictionary<string, ConfigFileProject>();

                foreach (var projectName in commitedProjState.Keys)
                {
                    ConfigFileProject fileProject = new ConfigFileProject();
                    fileProject.framework_max_version = "-";
                    fileProject.name = projectName;
                    fileProject.required_references = new List<ConfigFileReference>();
                    fileProject.unacceptable_references = new List<ConfigFileReference>();

                    configFileSolution.projects.Add(projectName, fileProject);
                }

                using (FileStream fileStream = File.Create(solutionExtendedName + "_config_guard.rdg"))
                {
                    StreamWriter streamWriter = new StreamWriter(fileStream);

                    string json = JsonConvert.SerializeObject(configFileSolution);
                    streamWriter.Write(json);

                    streamWriter.Flush();
                    fileStream.Flush();

                    streamWriter.Close();

                }
            }

            try
            {
                using (FileStream fileStream = new FileStream(packageExtendedName + "\\global_config_guard.rdg", FileMode.Open))
                {
                    StreamReader sr = new StreamReader(fileStream);


                    configFileGlobal = JsonConvert.DeserializeObject<ConfigFileGlobal>(sr.ReadToEnd());
                }

            }
            catch (Exception)
            {
                VsShellUtilities.ShowMessageBox(
                    this.package,
                    "Не получилось загрузить глобальный файл конфигурации. \r\n Шаблон файла конфигурации будет создан расширением",
                    "RefDepGuard: Ошибка загрузки файла конфигурации",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                configFileGlobal = new ConfigFileGlobal();
                configFileGlobal.name = "Global";
                configFileGlobal.framework_max_version = "-";
                configFileGlobal.global_required_references = new List<ConfigFileReference>();
                configFileGlobal.global_unacceptable_references = new List<ConfigFileReference>();


                using (FileStream fileStream = File.Create(packageExtendedName + "\\global_config_guard.rdg"))
                {
                    StreamWriter streamWriter = new StreamWriter(fileStream);

                    string json = JsonConvert.SerializeObject(configFileGlobal);
                    streamWriter.Write(json);

                    streamWriter.Flush();
                    fileStream.Flush();

                    streamWriter.Close();

                }
            }

        }
    }
}
