using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using RefDepGuard.Data;
using RefDepGuard.Data.ConfigFile;
using RefDepGuard.Models;

namespace RefDepGuard.Managers.CheckRules.SubManagers
{
    /// <summary>
    /// This class is responsible for checking if the projects in the solution match the projects listed in the configuration file.
    /// </summary>
    public class CheckProjectsMatchSubManager
    {
        private static List<ProjectMatchWarning> projectMatchWarningList = new List<ProjectMatchWarning>();

        /// <summary>
        /// Clears the list of project match warnings. This method can be called before performing a new check.
        /// </summary>
        public static void ClearErrorLists()
        {
            if(projectMatchWarningList != null)
                projectMatchWarningList.Clear();
        }

        /// <summary>
        /// The main method of the SSubManager. Checks if the projects in the solution match the projects listed in the configuration file. 
        /// If there are discrepancies (added or removed projects), it prompts the user to either update the configuration file accordingly 
        /// or to ignore the discrepancies and add warnings to the list.
        /// </summary>
        /// <param name="configFilesData">ConfigFilesData commited value</param>
        /// <param name="currentCommitedProjState">Solution projects commited state</param>
        /// <param name="uIShell">IVsUIShell interface value</param>
        /// <returns>ConfigFilesData value and list of current ProjectMatchWarning</returns>
        /// <see cref="ShowPromptAndSolveDifferProblems"/>

        public static Tuple<ConfigFilesData, List<ProjectMatchWarning>> CheckAndUpdateProjectsOnMatch(
            ConfigFilesData configFilesData, Dictionary<string, ProjectState> currentCommitedProjState, IVsUIShell uIShell)
        {
            var addedProjectsList = new List<string>();
            var removedProjectsList = new List<string>();

            foreach (KeyValuePair<string, ProjectState> currentProjState in currentCommitedProjState)//Check on added to solution projects
            {
                var projName = currentProjState.Key;

                if (!configFilesData.ConfigFileSolution?.projects?.ContainsKey(projName) ?? false)
                {
                    addedProjectsList.Add(projName);
                }
            }

            foreach (var dictValue in configFilesData.ConfigFileSolution?.projects) //Check on deleted projects
            {
                var projName = dictValue.Key;
                if (!currentCommitedProjState.ContainsKey(projName))
                {
                    removedProjectsList.Add(projName);
                }
            }

            //If there are added or removed projects, we show the prompt to the user and offer to update the config file.
            if (addedProjectsList.Count > 0)
                configFilesData = ShowPromptAndSolveDifferProblems(configFilesData, addedProjectsList, uIShell, true);

            if (removedProjectsList.Count > 0)
                configFilesData = ShowPromptAndSolveDifferProblems(configFilesData, removedProjectsList, uIShell, false);

            return new Tuple<ConfigFilesData, List<ProjectMatchWarning>>(configFilesData, projectMatchWarningList);
        }

        /// <summary>
        /// Shows a prompt to the user about the detected discrepancies between the projects in the solution and the projects listed in the configuration file.
        /// If the user agrees to update the configuration file, it calls the method to update the config file with the added or removed projects.
        /// If the user chooses not to update the configuration file, it adds warnings about the discrepancies to the list of project match warnings.
        /// </summary>
        /// <param name="configFilesData">ConfigFileData current value</param>
        /// <param name="currentProjList">List of current project names in the solution</param>
        /// <param name="uIShell">IVsUIShell interface value</param>
        /// <param name="isAddedList">Shows whether it added or removed projects problem</param>
        /// <returns>ConfigFilesData current value</returns>

        private static ConfigFilesData ShowPromptAndSolveDifferProblems(ConfigFilesData configFilesData, List<string> currentProjList, IVsUIShell uIShell, bool isAddedList)
        {
            bool isSingleProject = currentProjList.Count == 1;
            string projectDetectionStr = (isSingleProject) ? "обнаружен проект '" : "обнаружены проекты '";
            string projectNotFindedStr = (isSingleProject) ? "отсутствующий" : "отсутствующие";
            string projectNounStr = (isSingleProject) ? "его" : "их";

            string problemPlaceStr = (isAddedList) ? "config-файле ('" + configFilesData.SolutionName + "_config_guard.rdg')" : "solution";
            string findedProjPlaceStr = (isAddedList) ? "solution " : "config-файле ";
            string offeredSolutionStr = (isAddedList) ? ". Добавить " + projectNounStr + " в файл конфигураций?" : ". Удалить "+ projectNounStr +" из файла конфигураций?";

            var message = "В "+ findedProjPlaceStr + projectDetectionStr;

            for (int i = 0; i < currentProjList.Count; i++)
            {
                if (i > 5)
                    break;

                message += "'" + currentProjList[i] + "', ";
            }

            message += projectNotFindedStr + " в " + problemPlaceStr + offeredSolutionStr;
            

            if (MessageManager.ShowYesNoPrompt(uIShell, message, "RefDepGuard"))//If user agrees
            {
                //Update config file with added or removed projects
                configFilesData = ConfigFileManager.UpdateSolutionConfigFile(configFilesData, currentProjList, isAddedList);
            }
            else
            {
                //add "projects match warning"
                foreach (var currentProjectElements in currentProjList)
                {
                    projectMatchWarningList.Add(
                        new ProjectMatchWarning(currentProjectElements, isAddedList)
                        );
                }
            }

            return configFilesData;
        }
    }
}
