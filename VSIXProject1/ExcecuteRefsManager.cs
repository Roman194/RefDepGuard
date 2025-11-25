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
    public class ExcecuteRefsManager
    {
        public static void ExcecuteCurrentRefs(DTE dte, IServiceProvider serviceProvider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = "";
            string title = "Связи между проектами на текущий момент";

            EnvDTE.Solution solution = dte.Solution;

            foreach (EnvDTE.Project project in solution.Projects)
            {
                message += ("Рефы в проекте:" + project.Name + "\r\n");
                VSLangProj.VSProject vSProject = project.Object as VSLangProj.VSProject;
                if (vSProject != null)
                {

                    var refsList = new List<string>();

                    foreach (VSLangProj.Reference vRef in vSProject.References)
                    {
                        if (vRef.SourceProject != null)
                        {

                            refsList.Add(vRef.Name);
                            message += (vRef.Name + "\r\n");
                        }
                    }
                }
            }

            MessageManager.ShowMessageBox(serviceProvider, message, title);
        }

        public static void ExcecuteChangedRefs(DTE dte, IServiceProvider serviceProvider, Dictionary<string, ProjectState> commitedProjState)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Dictionary<string, List<string>> addedRefs = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> removedRefs = new Dictionary<string, List<string>>();

            string message = "С момента последней проверки рефов произошли следующие изменения:\r\n";
            string title = "Изменения в рефах";

            EnvDTE.Solution solution = dte.Solution;

            foreach (EnvDTE.Project project in solution.Projects)
            {
                VSLangProj.VSProject vSProject = project.Object as VSLangProj.VSProject;
                if (vSProject != null)
                {
                    var vsCommitedProjRefsHashSet = new HashSet<string>(commitedProjState[vSProject.Project.Name].CurrentReferences);

                    var vsCurrentProjHashSet = new HashSet<string>();

                    foreach (VSLangProj.Reference currRef in vSProject.References)
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

                        if (vsCommitedProjRefsHashSet.Count > 0)
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
                foreach (var addedRefDict in addedRefs)
                {
                    message += ("В проекте " + addedRefDict.Key + ": \r\n");

                    foreach (string addedRef in addedRefDict.Value)
                    {
                        message += addedRef + "\r\n";

                    }
                }
            }

            if (removedRefs.Count > 0)
            {
                message += "Удалены рефы:";
                foreach (var removedRefDict in removedRefs)
                {
                    message += ("В проекте " + removedRefDict.Key + ": \r\n");

                    foreach (string removedRef in removedRefDict.Value)
                    {
                        message += removedRef + "\r\n";

                    }
                }
            }

            MessageManager.ShowMessageBox(serviceProvider, message, title);
        }
    }
}
