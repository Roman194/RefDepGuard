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

        static ConfigFileSolution configFileSolution;

        static ErrorListProvider errorListProvider;
        static IVsOutputWindowPane generalPane;
        static Guid generalPaneGuid;
        static IVsOutputWindow outWindow;

        static bool isConfigFileExsistedBeforeInit = true;


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

            //CommitCurrentReferences();
            
            //GetConfigFileInfo();

            //if (isConfigFileExsistedBeforeInit)
             //   CheckRulesFromConfigFile();


        }

        //private async Task InitializeCommand1Async()
        //{
        //    await GetConfigFileInfo(package);
        //}

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
            //await Instance.InitializeCommand1Async();

            //dte2 = (DTE2)Package.GetGlobalService(typeof(EnvDTE.DTE));



            outWindow = (IVsOutputWindow) Package.GetGlobalService(typeof(SVsOutputWindow)); //Создание собственного окна (м.б. полезно для вывода варнингов)
            generalPaneGuid = VSConstants.GUID_OutWindowGeneralPane;
            outWindow.CreatePane(ref generalPaneGuid, "Warning pane", 1, 0);
            outWindow.GetPane(ref generalPaneGuid, out generalPane);
            generalPane.OutputString("Nope!");

        }

        private static void BuildBegined(vsBuildScope scope, vsBuildAction buildAction)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            //Здесь прописать отслеживание соответствия референсов правилам

            //Взять configFileSolution и проверить его обязательные и запрещённые связи

            //Если не соответствует конфигу, то не даём зафикисровать изменения рефов и сделать билд

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

            //if (changedRefs.Count > 0)
            //{
            //    message += "Обновлены рефы:";
            //    foreach (Reference changedRef in changedRefs)
            //    {
            //        message += ("В проекте " + changedRef.SourceProject.Name + ": \r\n");
            //        message += changedRef.Name + "\r\n";

            //    }
            //}

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
            //changedRefs.Clear();
            removedRefs.Clear();
        }

        private void CommitCurrentReferences(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            commitedProjState.Clear();

            //var title = "Фиксация текущих референсов";
            //var message = "Текущие референсы успешно зафиксированы";

            //EnvDTE.Solution solution = dte.Solution;

            //foreach (EnvDTE.Project project in solution.Projects)
            //{
            //    VSLangProj.VSProject vSProject = project.Object as VSLangProj.VSProject;
            //    if (vSProject != null)
            //    {
            //        var refsList = new List<string>();

            //        foreach (VSLangProj.Reference vRef in vSProject.References)
            //        {
            //            if (vRef.SourceProject != null)
            //            {
            //                refsList.Add(vRef.Name);
            //            }
            //        }

            //        commitedProjState.Add(vSProject.Project.Name, refsList);

            //    }
            //}

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

        private static void CheckRulesFromConfigFile()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            //commitedProjState.Clear();

            if (errorListProvider != null)
                errorListProvider.Tasks.Clear();

            List<string> solutionRequiredReferences = new List<string>();
            List<string> solutionUnacceptableReferences = new List<string>();

            foreach (ConfigFileReference requiredReference in configFileSolution.solution_required_references)
                solutionRequiredReferences.Add(requiredReference.reference);

            foreach (ConfigFileReference unnacceptedReference in configFileSolution.solution_unnacceptable_references)
                solutionUnacceptableReferences.Add(unnacceptedReference.reference);

            foreach(KeyValuePair<string, List<string>> currentProjState in commitedProjState)
            {
                var projName = currentProjState.Key;
                var projReferences = currentProjState.Value;

                foreach (string solutionRequiredReference in solutionRequiredReferences)
                {
                    if (!projReferences.Contains(solutionRequiredReference))
                    {
                        //Ошибка отсутствия обязательного референса уровня решения

                        ErrorTask errorTask = new ErrorTask
                        {
                            Category = TaskCategory.User,
                            ErrorCategory = TaskErrorCategory.Error,
                            Text = "RefDepGuard error: Отсутсвует обязательный референс уровня Solution '" + solutionRequiredReference + "' для проекта '" + projName + "'. Добавьте его через обозреватель решений"

                        };

                        errorListProvider.Tasks.Add(errorTask);
                    }
                }

                foreach (string solutionUnacceptableReference in solutionUnacceptableReferences)
                {
                    if (projReferences.Contains(solutionUnacceptableReference))
                    {
                        //Ошибка присутствия недопутсимого референса уровня решения
                        ErrorTask errorTask = new ErrorTask
                        {
                            Category = TaskCategory.User,
                            ErrorCategory = TaskErrorCategory.Error,
                            Text = "RefDepGuard error: Присутствует недопустимый референс уровня Solution '" + solutionUnacceptableReference + "' для проекта '" + projName + "'. Удалите его через обозреватель решений"

                        };

                        errorListProvider.Tasks.Add(errorTask);
                    }
                }


                if (configFileSolution.projects.ContainsKey(projName))
                {
                    ConfigFileProject currentProjectConfigFileSettings = configFileSolution.projects[projName];

                    foreach (ConfigFileReference requiredReference in currentProjectConfigFileSettings.required_references)
                    {
                        if (!projReferences.Contains(requiredReference.reference))
                        {
                            //Ошибка отсутствия обязательного референса

                            ErrorTask errorTask = new ErrorTask
                            {
                                Category = TaskCategory.User,
                                ErrorCategory = TaskErrorCategory.Error,
                                Document = projName + ".csproj",
                                Text = "RefDepGuard error: Отсутсвует обязательный референс '" + requiredReference.reference + "' для проекта '" + projName + "'. Добавьте его через обозреватель решений"

                            };

                            errorListProvider.Tasks.Add(errorTask);

                        }

                    }

                    foreach (ConfigFileReference unacceptableReference in currentProjectConfigFileSettings.unnacceptable_references)
                    {
                        if (projReferences.Contains(unacceptableReference.reference))
                        {
                            //Ошибка присутствия недопутсимого референса
                            ErrorTask errorTask = new ErrorTask
                            {
                                Category = TaskCategory.User,
                                ErrorCategory = TaskErrorCategory.Error,
                                Document = projName + ".csproj",
                                Text = "RefDepGuard error: Присутствует недопустимый референс '" + unacceptableReference.reference + "' для проекта '" + projName + "'. Удалите его через обозреватель решений"

                            };

                            errorListProvider.Tasks.Add(errorTask);

                        }

                    }
                }
                else
                {

                    //Проект есть в solution но его нет в config
                }

                //А что делать если проекта нет в solution, но он есть в config?

            }

            if (errorListProvider != null)
                errorListProvider.Show();
        }

        private static void CommitCurrentReferences()
        {
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
            string solutionExtendedName = dteSolutionFullName.Substring(0, lastDotIndex);

            try
            {
                using (FileStream fileStream = new FileStream(solutionExtendedName + "_config_guard.rdg", FileMode.Open))
                {

                    //var reference = new ConfigFileReference { reference = "System.Win.Forms" };

                    //configFileSolution = new ConfigFileSolution { name = solutionName, framework_max_version = "-", global_required_references = new List<ConfigFileReference>(), global_unnacceptable_references = new List<ConfigFileReference>()};

                    //string json = JsonSerializer.Serialize(configFileSolution);

                    //string json = JsonSerializer.Serialize(reference);

                    StreamReader sr = new StreamReader(fileStream);


                    configFileSolution = JsonConvert.DeserializeObject<ConfigFileSolution>(sr.ReadToEnd());

                    //string json = JsonConvert.SerializeObject(configFileSolution);


                    //configFileSolution = JsonSerializer.Deserialize<ConfigFileSolution>(fileStream);




                    //Надо актуализировать файл конфигурации по количеству проектов?
                    //Или хотя бы выводить сообщение о том, что такого-то проекта в файле конфигурации нет, добавьте его?
                    //С помощью porjectAdded или projectDeleted events

                    //int iterationsCount = 0;
                    //if (configFileSolution.projects.Count < commitedProjState.Count)
                    //{
                    //    iterationsCount = configFileSolution.projects.Count;
                    //}
                    //else
                    //{
                    //    iterationsCount = commitedProjState.Count;
                    //}


                    //for (int i = 0; i < iterationsCount; i++)
                    //{

                    //}
                }
            }
            catch (Exception ex)
            {

                VsShellUtilities.ShowMessageBox(
                    this.package,
                    "Не получилось загрузить файл конфигурации. \r\n Шаблон файла конфигурации будет создан расширением",
                    "RefDepGuard: Ошибка загрузки файла конфигурации",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                var solutionName = solutionExtendedName.Split('\\');

                
                configFileSolution = new ConfigFileSolution();
                configFileSolution.name = solutionName[solutionName.Length - 1];
                configFileSolution.framework_max_version = "-";
                configFileSolution.solution_required_references = new List<ConfigFileReference>();
                configFileSolution.solution_unnacceptable_references = new List<ConfigFileReference>();
                configFileSolution.projects = new Dictionary<string, ConfigFileProject>();

                foreach (var projectName in commitedProjState.Keys)
                {
                    ConfigFileProject fileProject = new ConfigFileProject();
                    fileProject.framework_max_version = "-";
                    fileProject.name = projectName;
                    fileProject.required_references = new List<ConfigFileReference>();
                    fileProject.unnacceptable_references = new List<ConfigFileReference>();

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

        }
    }
}
