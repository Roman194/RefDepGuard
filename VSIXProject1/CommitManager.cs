using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject1.Data;

namespace VSIXProject1
{
    public class CommitManager
    {
        public static Dictionary<string, ProjectState> CommitCurrentReferences(DTE dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Dictionary<string, ProjectState> commitedProjState = new Dictionary<string, ProjectState>();
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
                            refsList.Add(vRef.Name);
                    }

                    commitedProjState.Add(vSProject.Project.Name, new ProjectState(projectFrameworkVersion, refsList));
                }
            }

            return commitedProjState;
        }


    }
}
