using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using RefDepGuard.Data;
using RefDepGuard.Managers.CheckRules.SubManagers;

namespace RefDepGuard
{
    public class ExcecuteRefsManager
    {

        private static Dictionary<int, string> tabsAtDeepDict = new Dictionary<int, string>();
        private static HashSet<string> shownProjectsHashSet = new HashSet<string>();

        //Оптимизировать алгоритмы показа рефов!
        public static void ExcecuteCurrentRefs(DTE dte, IServiceProvider serviceProvider, bool showMessageWithTransitRefs)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = "";
            string title = "Связи между проектами на текущий момент";

            var currentReferencesState = CurrentStateManager.GetCurrentReferencesState(dte);

            if (currentReferencesState.Count == 0)
                message = "На текущий момент в Solution не обнаружены референсы";
            else
            {
                message = showMessageWithTransitRefs ? 
                    ExcecuteMessageWithTransitRefs(currentReferencesState) : 
                    ExcecuteMessageWithoutTransitRefs(currentReferencesState);

            }

            MessageManager.ShowMessageBox(serviceProvider, message, title);
        }

        private static string ExcecuteMessageWithoutTransitRefs(Dictionary<string, List<string>> currentReferencesState)
        {
            string message = "";

            string currentStartTabs = GetReqTabsCount(
                    Convert.ToInt32(currentReferencesState
                        .Where(proj => proj.Value.Count > 0) //Чтобы не учитывать те проекты, у которых нет рефов
                        .Select(proj => proj.Key)
                        .Average(x => x.Length) / 2
                        )
                    );

            foreach (var currentReferencesKeyValues in currentReferencesState)
            {
                string currentProject = currentReferencesKeyValues.Key;
                List<string> currentReferences = currentReferencesKeyValues.Value;

                message += (currentProject + "\r\n");

                foreach (var currRef in currentReferences)
                {
                    if (currentReferences.Last() != currRef)
                        message += (currentStartTabs + "┣━" + currRef + "\r\n");
                    else
                        message += (currentStartTabs + "┗━" + currRef + "\r\n");
                }

                if (currentReferences.Count == 0)
                    message += ("   ─\r\n");

                message += ("\r\n");
            }

            return message;
        }

        private static string GetReqTabsCount(int count)
        {
            string message = "";
            for (int i = 0; i < count - 2; i++)
                message += "  ";
            
            return message;
        }

        private static string ExcecuteMessageWithTransitRefs(Dictionary<string, List<string>> currentReferencesState)
        {
            string message = "";

            shownProjectsHashSet.Clear();

            var stillNotShownProjects  = currentReferencesState.Keys.ToHashSet();

            while(stillNotShownProjects.Count() > 0)
            {
                var maxRefsCount = currentReferencesState
                    .Where(x => stillNotShownProjects.Contains(x.Key))
                    .Max(x => x.Value.Count);

                var maxRefsProject = currentReferencesState.First(project => project.Value.Count == maxRefsCount);
                var currentStartTabs = GetReqTabsCount(Convert.ToInt32(maxRefsProject.Key.Length / 2));

                tabsAtDeepDict.Clear();
                tabsAtDeepDict.Add(1, currentStartTabs);

                message += GetTransitRefsMessageForCurrentProject(maxRefsProject.Key, currentReferencesState, currentStartTabs, 1);
                shownProjectsHashSet.Add(maxRefsProject.Key);

                stillNotShownProjects = currentReferencesState.Keys.ToHashSet().Except(shownProjectsHashSet).ToHashSet();
            }

            return message;
        }

        //Сверить этот алгоритм с определением транзитивных связей в TransitRefsSubManager и объединить?

        private static string GetTransitRefsMessageForCurrentProject(string currentProject, Dictionary<string, List<string>> currentReferencesState, string currentStartTabs, int refDeep)
        {
            string message = "";
            List<string> currentReferences = currentReferencesState[currentProject];
            int futureRefDeep = refDeep + 1;

            if (refDeep == 1) 
                message += (currentProject + "\r\n");

            if (currentReferences.Count > 0)
            {
                string nextIterationExtraTabs = "";

                var nextIterationProjectsWithRefs = currentReferencesState
                        .Where(project => currentReferences.Contains(project.Key) && project.Value.Count > 0);

                if (nextIterationProjectsWithRefs.Count() > 0)
                {
                    if (tabsAtDeepDict.ContainsKey(futureRefDeep))
                    {
                        nextIterationExtraTabs = tabsAtDeepDict[futureRefDeep];
                    }
                    else
                    {
                        nextIterationExtraTabs = GetReqTabsCount(
                            Convert.ToInt32(
                                nextIterationProjectsWithRefs.Average(x => x.Key.Length) / 2
                            )
                        );

                        tabsAtDeepDict.Add(futureRefDeep, nextIterationExtraTabs);
                    }
                }

                foreach (var currRef in currentReferences)
                {
                    bool isCurrRefNotLast = currentReferences.Last() != currRef;
                    string currentRefInjectSymbol = (isCurrRefNotLast) ? "┣━" : "┗━";
                    string nextDeepStartTabs = (isCurrRefNotLast) ? currentStartTabs + "┃" + nextIterationExtraTabs : currentStartTabs + " " + nextIterationExtraTabs;

                    message += (currentStartTabs + currentRefInjectSymbol + currRef + "\r\n");
                    message += GetTransitRefsMessageForCurrentProject(currRef, currentReferencesState, nextDeepStartTabs, futureRefDeep);

                    shownProjectsHashSet.Add(currRef);

                    if (refDeep == 1 && isCurrRefNotLast)
                        message += currentStartTabs + "┃\r\n";
                }
            }

            return message;
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


