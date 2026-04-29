using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using RefDepGuard.Applied.Models.Project;
using RefDepGuard.UI.Resources.StringResources;

namespace RefDepGuard
{
    /// <summary>
    /// This class is responsible for managing the execution of the messages with the current and changed references.
    /// </summary>
    public class ExcecuteRefsManager
    {
        private static Dictionary<int, string> tabsAtDeepDict = new Dictionary<int, string>();
        private static HashSet<string> shownProjectsHashSet = new HashSet<string>();

        /// <summary>
        /// Gets the current references state of the solution and shows it to the user in a message box.
        /// </summary>
        /// <param name="dte">DTE interface value</param>
        /// <param name="serviceProvider">IServiceProvider interface value</param>
        /// <param name="showMessageWithTransitRefs">shows if its message box with straight or transit refs</param>
        public static void ExcecuteCurrentRefs(DTE dte, IServiceProvider serviceProvider, bool showMessageWithTransitRefs)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = "";
            string title = Resource.Excecute_Current_Refs_Title;

            var currentReferencesState = CurrentStateManager.GetCurrentReferencesState(dte);

            var findedRefs = currentReferencesState.Where(el => el.Value.Count > 0).Count();

            if (findedRefs == 0)//If there are no refs in the solution, then show message about it.
                message = Resource.Excecute_Current_Refs_On_Empty;
            else
            {//If there are refs in the solution, then show them in the message box (in streaight or transit format).
                message = showMessageWithTransitRefs ? 
                    ExcecuteMessageWithTransitRefs(currentReferencesState) : 
                    ExcecuteMessageWithoutTransitRefs(currentReferencesState);

            }

            MessageManager.ShowMessageBox(serviceProvider, message, title);
        }

        /// <summary>
        /// Compares the current references state of the solution with the commited references state and shows the changes to the user in a message box.
        /// </summary>
        /// <param name="dte">DTE interface value</param>
        /// <param name="serviceProvider">IServiceProvider interface value</param>
        /// <param name="commitedProjState">commited project state dictionary</param>
        public static void ExcecuteChangedRefs(DTE dte, IServiceProvider serviceProvider, Dictionary<string, ProjectState> commitedProjState)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Dictionary<string, List<string>> addedRefs = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> removedRefs = new Dictionary<string, List<string>>();

            string message = Resource.Excecute_Changed_Refs_Start_Message;
            string title = Resource.Excecute_Changed_Refs_Title;

            var currentReferencesState = CurrentStateManager.GetCurrentReferencesState(dte);

            foreach (var currentReferencesKeyValues in currentReferencesState)//for each project in the current state
            {
                string currentProject = currentReferencesKeyValues.Key;
                List<string> currentReferences = currentReferencesKeyValues.Value;

                //If the project was added after the commit, then we just initialize its commit refs as an empty
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

                if (vsCurrentProjHashSet.Count > 0)//adds added refs to the addedRefs dict
                {
                    addedRefs.Add(currentProject, vsCurrentProjHashSet.ToList());
                }

                if (vsCommitedProjRefsHashSet.Count > 0)// remove deleted refs to the removedRefs dict
                {
                    removedRefs.Add(currentProject, vsCommitedProjRefsHashSet.ToList());
                }
            }

            //If the project was deleted after the commit, then we just initialize its current refs as an empty and add all its commited refs to the removedRefs dict
            var deletedProjectKeys = commitedProjState.Keys.Except(currentReferencesState.Keys).ToList();
            foreach (var currentKey in deletedProjectKeys)
            {
                var currentDeletedProjRefs = commitedProjState[currentKey].CurrentReferences;

                if (currentDeletedProjRefs.Count > 0)
                    removedRefs.Add(currentKey, commitedProjState[currentKey].CurrentReferences);
            }

            message += ConvertCurrentRefsDictToStringFormat(addedRefs, true);
            message += ConvertCurrentRefsDictToStringFormat(removedRefs, false);

            if (addedRefs.Count == 0 && removedRefs.Count == 0)
                message = Resource.Excecute_Changed_Refs_On_Empty;

            MessageManager.ShowMessageBox(serviceProvider, message, title);
        }

        /// <summary>
        /// Formats the current references state of the solution to the string message without transit refs and returns it.
        /// </summary>
        /// <param name="currentReferencesState">current references state dictionary</param>
        /// <returns>message string</returns>
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

        /// <summary>
        /// Formats the current references state of the solution to the string message with transit refs and returns it.
        /// </summary>
        /// <param name="currentReferencesState">current references state dictionary</param>
        /// <returns>message string</returns>
        private static string ExcecuteMessageWithTransitRefs(Dictionary<string, List<string>> currentReferencesState)
        {
            string message = "";

            shownProjectsHashSet.Clear();

            var stillNotShownProjects  = currentReferencesState.Keys.ToHashSet();
            var currentStartTabs = "";
            var isFirstIteration = true;

            while(stillNotShownProjects.Count() > 0)
            {
                var maxRefsCount = currentReferencesState
                    .Where(x => stillNotShownProjects.Contains(x.Key))
                    .Max(x => x.Value.Count);

                var maxRefsProject = currentReferencesState.First(project => 
                    project.Value.Count == maxRefsCount && stillNotShownProjects.Contains(project.Key));

                if (isFirstIteration)
                {
                    //Determines the number of tabs of the first level message as the union number of spaces for all projects by the count of project name symbols
                    //with the max refs count.
                    currentStartTabs = GetReqTabsCount(Convert.ToInt32(maxRefsProject.Key.Length / 2));
                    tabsAtDeepDict.Clear();
                    tabsAtDeepDict.Add(1, currentStartTabs);

                    isFirstIteration = false;
                }

                if (maxRefsProject.Value.Count > 0)//If project has refs, then we need to show them with transit refs.
                    message += (GetTransitRefsMessageForCurrentProject(maxRefsProject.Key, currentReferencesState, currentStartTabs, 1) + "\r\n");
                else
                    message += (maxRefsProject.Key + "\r\n-\r\n\r\n");

                shownProjectsHashSet.Add(maxRefsProject.Key);

                stillNotShownProjects = currentReferencesState.Keys.ToHashSet().Except(shownProjectsHashSet).ToHashSet();
            }

            return message;
        }

        /// <summary>
        /// Formats the current references of the project to the string message with transit refs and returns it. 
        /// It is recursive function which calls itself for each ref of the project.
        /// </summary>
        /// <param name="currentProject">current project string</param>
        /// <param name="currentReferencesState">current refs state dict</param>
        /// <param name="currentStartTabs">current start tabs string</param>
        /// <param name="refDeep">current references deep int value</param>
        /// <returns>a string message with transit refs</returns>
        private static string GetTransitRefsMessageForCurrentProject(string currentProject, Dictionary<string, List<string>> currentReferencesState, string currentStartTabs, int refDeep)
        {
            string message = "";
            List<string> currentReferences = currentReferencesState[currentProject];
            int futureRefDeep = refDeep + 1;

            if (refDeep == 1) 
                message += (currentProject + "\r\n");

            if (currentReferences.Count > 0)//If the project has refs
            {
                string nextIterationExtraTabs = "";

                var nextIterationProjectsWithRefs = currentReferencesState
                        .Where(project => currentReferences.Contains(project.Key) && project.Value.Count > 0);

                if (nextIterationProjectsWithRefs.Count() > 0)//If there are projects with refs on the next iteration,
                {
                    //then we need to determine the number of tabs for them based on the average count of symbols in their names.
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

                foreach (var currRef in currentReferences)//for each ref of the project
                {
                    //add its to a message string with the right tabs and symbols based on the deep.
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

        /// <summary>
        /// Determines the number of tabs for the message based on the count of symbols in the project names and returns the string with the needed count of tabs.
        /// </summary>
        /// <param name="count">count int value</param>
        /// <returns>string message with required tabs</returns>
        private static string GetReqTabsCount(int count)
        {
            string message = "";
            for (int i = 0; i < count - 2; i++)
                message += "  ";

            return message;
        }

        /// <summary>
        /// Formats the current refs dict to the string message with added or removed refs and returns it.
        /// </summary>
        /// <param name="currentRefs">current refs dict</param>
        /// <param name="isAddedRefsDict">shows if its added or removed refs</param>
        /// <returns></returns>
        private static string ConvertCurrentRefsDictToStringFormat(Dictionary<string, List<string>> currentRefs, bool isAddedRefsDict)
        {
            string outputMessage = "";
            if (currentRefs.Count > 0)
            {
                outputMessage += "\r\n" + (isAddedRefsDict? Resource.Excecute_Refs_Adds_Title : Resource.Excecute_Refs_Remove_Title);
                foreach (var currentRefDict in currentRefs)
                {
                    outputMessage += (Resource.Excecute_Refs_Project_Title + currentRefDict.Key + ":");

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