using EnvDTE;
using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio.Shell;
using RefDepGuard.Applied.Models.Project;
using RefDepGuard.TargetFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RefDepGuard
{
    /// <summary>
    /// This class is responsible for managing the current state of the solution.
    /// </summary>
    public class CurrentStateManager
    {
        /// <summary>
        /// Gets the current state of the projects in the solution. It includes the target framework(s) and references of each project.
        /// </summary>
        /// <param name="dte">DTE interface value</param>
        /// <returns>current projects state dictionary</returns>
        public static Dictionary<string, ProjectState> GetCurrentSolutionState(DTE dte)
        {
            return GetCurrentRequiredState(dte, false);
        }

        /// <summary>
        /// Gets the current state of the project references in the solution. 
        /// It includes only the references of each project (without target framework(s) info).
        /// </summary>
        /// <param name="dte">DTE interface value</param>
        /// <returns>current references state dictionary</returns>
        public static Dictionary<string, List<string>> GetCurrentReferencesState(DTE dte)
        {
            Dictionary<string, List<string>> currentReferences = 
                GetCurrentRequiredState(dte, true).ToDictionary(
                    project => project.Key, 
                    project => project.Value.CurrentReferences
                );  

            return currentReferences;
        }

        /// <summary>
        /// Gets the current required state of the projects in the solution (with or without TF-s). It includes the target framework(s) and references of each 
        /// project.
        /// </summary>
        /// <param name="dte">DTE interface value</param>
        /// <param name="isOnlyRefsNeeded">shows if only refs is needed or also TF-s</param>
        /// <returns>current projects state dictionary</returns>
        private static Dictionary<string, ProjectState> GetCurrentRequiredState(DTE dte, bool isOnlyRefsNeeded)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Dictionary<string, ProjectState> commitedSolState = new Dictionary<string, ProjectState>();
            EnvDTE.Solution solution = dte.Solution;

            foreach (EnvDTE.Project project in solution.Projects)//For each project of the solution
            {
                if (project.FullName != null && project.FullName.Length != 0)//If the project is loaded
                {
                    string projectFrameworkVersions = "";
                    Dictionary<string, List<int>> projectFrameworkNumVersions = new Dictionary<string, List<int>>();

                    if (!isOnlyRefsNeeded)
                    {
                        var projectCollection = new ProjectCollection();

                        var currentProject = projectCollection.LoadProject(project.FullName);
                        (projectFrameworkVersions, projectFrameworkNumVersions) = TFManager.GetTargetFrameworkInStringNTransferFormats(currentProject);

                    }

                    VSLangProj.VSProject vSProject = project.Object as VSLangProj.VSProject;

                    if (vSProject != null)
                    {
                        var refsList = new List<string>();//пЕРЕНЕСТИ ПОВЫШЕ К ИНИЦИАЛИЗАЦИИ ПРОЧИХ ЛОКАЛЬНЫХ ПЕРЕМЕННЫХ?   

                        foreach (VSLangProj.Reference vRef in vSProject.References)
                        {
                            if (vRef.SourceProject != null)
                                refsList.Add(vRef.Name);
                        }
                        //adds the project state to the dictionary with the project name as a key and the ProjectState object as a value (optionally adds TF-s info)
                        commitedSolState.Add(vSProject.Project.Name, new ProjectState(projectFrameworkNumVersions, projectFrameworkVersions, refsList));
                    }
                }
            }

            return commitedSolState;
        }
    }
}