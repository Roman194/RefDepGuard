using EnvDTE;
using EnvDTE80;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Services;
using System.Text.Json;
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

            CommitCurrentReferences();
            GetConfigFileInfo();



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


            dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));
            Instance = new Command1(package, commandService);
            //dte2 = (DTE2)Package.GetGlobalService(typeof(EnvDTE.DTE));

            
            dte.Events.BuildEvents.OnBuildBegin += new _dispBuildEvents_OnBuildBeginEventHandler(BuildBegined);

            errorListProvider = new ErrorListProvider(package);

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



            ErrorTask errorTask = new ErrorTask
            {
                Category = TaskCategory.User,
                ErrorCategory = TaskErrorCategory.Error,
                Text = "Вам запрещено",
                Column = 0

            };

            errorListProvider.Tasks.Add(errorTask);
            errorListProvider.Show();

            
            generalPane.Activate();

            dte.ExecuteCommand("Build.Cancel");
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

            //var title = "Фиксация текущих референсов";
            //var message = "Текущие референсы успешно зафиксированы";

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

            //VsShellUtilities.ShowMessageBox(
            //    this.package,
            //    message,
            //    title,
            //    OLEMSGICON.OLEMSGICON_INFO,
            //    OLEMSGBUTTON.OLEMSGBUTTON_OK,
            //    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private void CommitCurrentReferences()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

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

        private async void GetConfigFileInfo() //Не вызывается
        {
            string [] solutionNameArray = dte.Solution.FullName.Split('.');
            string solutionName = "";

            for (int i = 0; i < solutionNameArray.Length - 1; i++)
            {
                solutionName += solutionNameArray[i];
            }

            try
            {
                using (FileStream fileStream = File.OpenRead(solutionName + ".json"))
                {

                    configFileSolution = await JsonSerializer.DeserializeAsync<ConfigFileSolution>(fileStream);

                    //Надо актуализировать файл конфигурации по количеству проектов?

                    int iterationsCount = 0;
                    if (configFileSolution.projects.Count < commitedProjState.Count)
                    {
                        iterationsCount = configFileSolution.projects.Count;
                    }
                    else
                    {
                        iterationsCount = commitedProjState.Count;
                    }


                    //for (int i = 0; i < iterationsCount; i++)
                    //{




                    //}
                }
            }
            catch (Exception ex)
            {

                VsShellUtilities.ShowMessageBox(
                    this.package,
                    "Не получилось загрузить файл конфигурации",
                    "Ошибка загрузки файла конфигурации",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                configFileSolution.name = solutionName;
                configFileSolution.framework_max_version = "-";
                configFileSolution.global_required_references = new List<ConfigFileReference>();
                configFileSolution.global_unnacceptable_references = new List<ConfigFileReference>();

                
                foreach (var projectName in commitedProjState.Keys)
                {
                    ConfigFileProject fileProject = new ConfigFileProject();
                    fileProject.framework_max_version = "-";
                    fileProject.name = projectName;
                    fileProject.required_references = new List<ConfigFileReference>();
                    fileProject.unnacceptable_references = new List<ConfigFileReference>();

                    configFileSolution.projects.Add(fileProject);
                }

                using (FileStream fileStream = File.Create(solutionName + ".json"))
                {

                    //configFileSolution = new ConfigFileSolution()

                    string json = JsonSerializer.Serialize(configFileSolution);
                }
            }

        }
    }
}
