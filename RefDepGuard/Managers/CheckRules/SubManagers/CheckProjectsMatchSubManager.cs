using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using RefDepGuard.Data;
using RefDepGuard.Data.ConfigFile;
using RefDepGuard.Models;

namespace RefDepGuard.Managers.CheckRules.SubManagers
{
    public class CheckProjectsMatchSubManager
    {
        private static List<ProjectMatchWarning> projectMatchWarningList = new List<ProjectMatchWarning>();

        public static void ClearErrorLists()
        {
            if(projectMatchWarningList != null)
                projectMatchWarningList.Clear();
        }

        public static Tuple<ConfigFilesData, List<ProjectMatchWarning>> CheckAndUpdateProjectsOnMatch(ConfigFilesData configFilesData, Dictionary<string, ProjectState> currentCommitedProjState, IVsUIShell uIShell)
        {
            var addedProjectsList = new List<string>();
            var removedProjectsList = new List<string>();

            foreach (KeyValuePair<string, ProjectState> currentProjState in currentCommitedProjState)//проверка на наличие добавленных проектов
            {
                var projName = currentProjState.Key;

                if (!configFilesData.configFileSolution?.projects?.ContainsKey(projName) ?? false)
                {
                    addedProjectsList.Add(projName);
                }
            }

            foreach (var dictValue in configFilesData.configFileSolution?.projects) //проверка на наличие удалённых проектов
            {
                var projName = dictValue.Key;
                if (!currentCommitedProjState.ContainsKey(projName))
                {
                    removedProjectsList.Add(projName);
                }
            }

            if (addedProjectsList.Count > 0)
                configFilesData = ShowPromptAndSolveDifferProblems(configFilesData, addedProjectsList, uIShell, true);

            if (removedProjectsList.Count > 0) //Проект есть в config, но его нет в Solution - значит он был удалён
                configFilesData = ShowPromptAndSolveDifferProblems(configFilesData, removedProjectsList, uIShell, false);

            return new Tuple<ConfigFilesData, List<ProjectMatchWarning>>(configFilesData, projectMatchWarningList);
        }

        private static ConfigFilesData ShowPromptAndSolveDifferProblems(ConfigFilesData configFilesData, List<string> currentProjList, IVsUIShell uIShell, bool isAddedList)
        {
            bool isSingleProject = currentProjList.Count == 1;
            string projectDetectionStr = (isSingleProject) ? "обнаружен проект '" : "обнаружены проекты '";
            string projectNotFindedStr = (isSingleProject) ? "отсутствующий" : "отсутствующие";
            string projectNounStr = (isSingleProject) ? "его" : "их";

            string problemPlaceStr = (isAddedList) ? "config-файле ('" + configFilesData.solutionName + "_config_guard.rdg')" : "solution";
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
            

            if (MessageManager.ShowYesNoPrompt(uIShell, message, "RefDepGuard"))
            {
                //Добавить в config-файл или удалить проекты из него
                configFilesData = ConfigFileManager.UpdateSolutionConfigFile(configFilesData, currentProjList, isAddedList);
            }
            else
            {
                //вывести warning типа "RefDepGuard projects match warning"
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
