using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
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

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("c19eaee0-a475-4f4d-821f-194a1447a90d");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        private static ReferencesEvents _refEvents;
        private static Events2 _dteEvents;
        private static EnvDTE.DTE dte;

        static List<Reference> addedRefs = new List<Reference>();
        static List<Reference> changedRefs = new List<Reference>();
        static List<Reference> removedRefs = new List<Reference>();

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

            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            var getChangedRefsMenuItem = new MenuCommand(this.ExcecuteChanges, getChangedRefsMenuCommandID);

            commandService.AddCommand(menuItem);
            commandService.AddCommand(getChangedRefsMenuItem);
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
            Instance = new Command1(package, commandService);

            
            dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));

            _dteEvents = dte.Events as Events2;
            //VSProject vSProject = (VSProject) dte.Solution.Projects.Item(1).Object;
            //_refEvents = vSProject.Events.ReferencesEvents;
            _refEvents = (ReferencesEvents)_dteEvents.GetObject("CSharpReferencesEvents");
            _refEvents.ReferenceAdded += new _dispReferencesEvents_ReferenceAddedEventHandler(ReferenceAdded);
            _refEvents.ReferenceChanged += new _dispReferencesEvents_ReferenceChangedEventHandler(ReferenceChanged);
            _refEvents.ReferenceRemoved += new _dispReferencesEvents_ReferenceRemovedEventHandler(ReferenceRemoved);
        }

        public void subscribeRefEvents()
        {
            
        }

        private static void ReferenceAdded(Reference pReference)
        {
            addedRefs.Add(pReference);
        }

        private static void ReferenceChanged(Reference pReference)
        {
            changedRefs.Add(pReference);
        }

        private static void ReferenceRemoved(Reference pReference)
        {
            removedRefs.Add(pReference);
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
            List<string> referencesList = new List<string>();

            foreach (EnvDTE.Project project in solution.Projects)
            {
                message += ("Рефы в проекте:" + project.Name + "\r\n");
                VSLangProj.VSProject vSProject = project.Object as VSLangProj.VSProject;
                if (vSProject != null) {
                    //vSProject.References.ContainingProject.name;
                    //vSProject.References.ContainingProject.ProjectItems

                    //foreach( EnvDTE.ProjectItem projectItem in vSProject.References.ContainingProject.ProjectItems)
                    //{
                    //    referencesList.Add(projectItem.Name);
                    //}

                    foreach (VSLangProj.Reference vRef in vSProject.References) 
                    {
                        //referencesList.Add(vRef.DTE.Name);
                        if (vRef.SourceProject != null)
                        {
                            message += (vRef.Name + "\r\n");
                        }
                    }
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

            if (addedRefs.Count > 0) 
            {
                message += "Добавлены рефы:";
                foreach (Reference addedRef in addedRefs)
                {
                    message += ("В проекте " + addedRef.SourceProject.Name + ": \r\n");
                    message += addedRef.Name + "\r\n";

                }
            }

            if (changedRefs.Count > 0)
            {
                message += "Обновлены рефы:";
                foreach (Reference changedRef in changedRefs)
                {
                    message += ("В проекте " + changedRef.SourceProject.Name + ": \r\n");
                    message += changedRef.Name + "\r\n";

                }
            }

            if (removedRefs.Count > 0)
            {
                message += "Удалены рефы:";
                foreach (Reference removedRef in removedRefs)
                {
                    message += ("В проекте " + removedRef.SourceProject.Name + ": \r\n");
                    message += removedRef.Name + "\r\n";

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
            changedRefs.Clear();
            removedRefs.Clear();
        }
    }
}
