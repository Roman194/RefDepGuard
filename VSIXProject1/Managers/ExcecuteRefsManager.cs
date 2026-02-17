using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using VSIXProject1.Data;
using VSIXProject1.Managers.CheckRules.SubManagers;

namespace VSIXProject1
{
    public class ExcecuteRefsManager
    {
        public static void ExcecuteCurrentRefs(DTE dte, IServiceProvider serviceProvider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = "";
            string title = "Связи между проектами на текущий момент";

            var currentReferencesState = CurrentStateManager.GetCurrentReferencesState(dte);

            if (currentReferencesState.Count == 0)
                message = "На текущий момент в Solution не обнаружены референсы";
            else
            {
                ExcecuteMessageWithTransitRefs(currentReferencesState);

                foreach (var currentReferencesKeyValues in currentReferencesState)
                {
                    string currentProject = currentReferencesKeyValues.Key;
                    List<string> currentReferences = currentReferencesKeyValues.Value;

                    message += (currentProject + "\r\n");

                    foreach (var currRef in currentReferences)
                    {
                        if (currentReferences.Last() != currRef)
                            message += ("   ├─" + currRef + "\r\n");
                        else
                            message += ("   └─" + currRef + "\r\n");
                    }

                    if (currentReferences.Count == 0)
                        message += ("   ─\r\n");
                }
            }

            MessageManager.ShowMessageBox(serviceProvider, message, title);
        }

        private static void ExcecuteMessageWithTransitRefs(Dictionary<string, List<string>> currentReferencesState)
        {
            var maxRefsCount = currentReferencesState.Values.Max(x => x.Count);
            var maxRefsProject = currentReferencesState.First(project => project.Value.Count == maxRefsCount);

            var maxRefsProjectState = new ProjectState(new Dictionary<string, List<int>>(), "", maxRefsProject.Value);

            var maxProjectTransitRefs = TransitRefsDetectSubManager.CheckCurrentProjectOnTransitReferencesSeparetely(maxRefsProject.Key, maxRefsProjectState);


        }

         

        public static void ExcecuteChangedRefs(DTE dte, IServiceProvider serviceProvider, Dictionary<string, ProjectState> commitedProjState)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Dictionary<string, List<string>> addedRefs = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> removedRefs = new Dictionary<string, List<string>>();

            string message = "С момента последней проверки рефов произошли следующие изменения:\r\n";
            string title = "Изменения в рефах";

            var currentReferencesState = CurrentStateManager.GetCurrentReferencesState(dte);

            foreach (var currentReferencesKeyValues in currentReferencesState)
            {
                string currentProject = currentReferencesKeyValues.Key;
                List<string> currentReferences = currentReferencesKeyValues.Value;

                //Если проект был добавлен посе коммита, то просто инициализируем его коммит-рефы как пустые
                var vsCommitedProjRefsList = commitedProjState.ContainsKey(currentProject) ? commitedProjState[currentProject].CurrentReferences : new List<string>();
                var vsCommitedProjRefsHashSet = new HashSet<string>(vsCommitedProjRefsList);
                var vsCurrentProjHashSet = new HashSet<string>();

                foreach (var currRef in currentReferences)
                {
                    vsCurrentProjHashSet.Add(currRef);
                }

                var commonRefsHashSet = vsCurrentProjHashSet.Intersect(vsCommitedProjRefsHashSet).ToHashSet();
                vsCurrentProjHashSet.RemoveWhere(commonRefsHashSet.Contains);
                vsCommitedProjRefsHashSet.RemoveWhere(commonRefsHashSet.Contains);

                if (vsCurrentProjHashSet.Count > 0)
                {
                    addedRefs.Add(currentProject, vsCurrentProjHashSet.ToList());
                }

                if (vsCommitedProjRefsHashSet.Count > 0)
                {
                    removedRefs.Add(currentProject, vsCommitedProjRefsHashSet.ToList());
                }
            }

            //Если проект был удалён после коммита, то добавляем все его рефы в список удалённых
            var deletedProjectKeys = commitedProjState.Keys.Except(currentReferencesState.Keys).ToList();
            foreach (var currentKey in deletedProjectKeys)
            {
                var currentDeletedProjRefs = commitedProjState[currentKey].CurrentReferences;

                if(currentDeletedProjRefs.Count > 0) 
                    removedRefs.Add(currentKey, commitedProjState[currentKey].CurrentReferences);
            }

            message += ConvertCurrentRefsDictToStringFormat(addedRefs, true);
            message += ConvertCurrentRefsDictToStringFormat(removedRefs, false);
           
            if (addedRefs.Count == 0 && removedRefs.Count == 0)
                message = "Изменения в рефах не обнаружены";

            MessageManager.ShowMessageBox(serviceProvider, message, title);
        }

        private static string ConvertCurrentRefsDictToStringFormat(Dictionary<string, List<string>> currentRefs, bool isAddedRefsDict)
        {
            string outputMessage = "";
            if (currentRefs.Count > 0)
            {
                outputMessage += "\r\n" + (isAddedRefsDict? "Добавлены" : "\r\nУдалены") + " рефы:";
                foreach (var currentRefDict in currentRefs)
                {
                    outputMessage += ("\r\nВ проекте " + currentRefDict.Key + ":");

                    foreach (string currentRef in currentRefDict.Value)
                    {
                        outputMessage += ("\r\n - " + currentRef);
                    }
                }
            }

            return outputMessage;
        }
    }
}


