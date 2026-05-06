using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace RefDepGuard
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(RefDepGuardPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class RefDepGuardPackage : AsyncPackage, IVsSolutionEvents, IVsSolutionLoadEvents
    {
        /// <summary>
        /// RefDepGuardPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "819893ec-1c1d-407d-b6fa-adeddc52a84d";

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        /// 
        /// 
        private uint _solutionLoadEventsCookie;
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await MainCommand.InitializeAsync(this);

            IVsSolution solution = GetServiceAsync(typeof(SVsSolution)) as IVsSolution;
            if (solution != null)
            {
                // Register a subscription on the user events
                solution.AdviseSolutionEvents(this, out _solutionLoadEventsCookie);//Big parts of the functions doesn't work, so they are not calling and can be deleted
            }

        }

        protected override void Dispose(bool disposing)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                if (_solutionLoadEventsCookie != 0)
                {
                    IVsSolution solution = GetService(typeof(SVsSolution)) as IVsSolution;
                    if (solution != null)
                    {
                        solution.UnadviseSolutionEvents(_solutionLoadEventsCookie);
                        _solutionLoadEventsCookie = 0;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public int OnBeforeLoadProjectBatch(bool isBackgroundIdleBatch)
        {
            // Код до начала пакетной загрузки проектов
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProjectBatch(bool isBackgroundIdleBatch)
        {
            // Код после завершения пакетной загрузки проектов
            return VSConstants.S_OK;
        }

        public int OnBeforeBackgroundSolutionLoadBegins()
        {
            // Код перед началом фоновой загрузки решения
            return VSConstants.S_OK;
        }

        

        public int OnAfterBackgroundSolutionLoadComplete()
        {
            // Код после завершения фоновой загрузки решения
            // Здесь можно безопасно работать с загруженным деревом проектов
            return VSConstants.S_OK;
        }

        public int OnBeforeOpenSolution(string pszSolutionFilename)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryBackgroundLoadProjectBatch(out bool pfShouldDelayLoadToNextIdle)
        {
            throw new NotImplementedException();
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        #endregion
    }
}