using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using RefDepGuard.Managers.CheckRules;
using RefDepGuard.CheckRules;
using RefDepGuard.Applied.Models.ConfigFile;
using RefDepGuard.Applied.Models.Project;
using RefDepGuard.Applied.Models.RefDepGuard;
using RefDepGuard.CheckRules.SubManagers;
using System.Linq;

namespace RefDepGuard
{
    /// <summary>
    /// This class is responsible for managing the checking of rules based on the configuration files and the current state of the projects in the solution.
    /// </summary>
    public class CheckRulesExtentionManager
    {
        /// <summary>
        /// This is the main method of checking rules module.
        /// It is responsible for checking the rules based on the configuration files and the current state of the projects in the solution. 
        /// It performs various checks, such as not null checks, max framework version checks, references checks, and transit references detection. 
        /// It collects all the errors and warnings found during the process and exports them to ELP together with the required parameters for other parts 
        /// of the extension.
        /// </summary>
        /// <param name="configFilesData">ConfigFilesData current commited value</param>
        /// <param name="errorListProvider">ELP class value</param>
        /// <param name="currentCommitedProjState">Current commited projects state values</param>
        /// <param name="uIShell">IVsUIShell interface value</param>
        /// <returns>RefDepGuardExportParameters and ConfigFilesData (to provide "Single source of truth" and "One flow through modules" principes)</returns>
        /// <see cref="RefDepGuardExportParameters"/>
        public static Tuple<RefDepGuardExportParameters, ConfigFilesData> CheckRulesFromConfigFiles(
            ConfigFilesData configFilesData, ErrorListProvider errorListProvider, Dictionary<string, ProjectState> currentCommitedProjState, IVsUIShell uIShell
            )
        {
            //Check on project match warnings
            List<string> addedProjectsList, removedProjectsList = new List<string>();
            (addedProjectsList, removedProjectsList) = CheckProjectsMatchSubManager.CheckSolutionNConfigFileProjectsOnMatch(configFilesData, currentCommitedProjState);

            // If there are 1 added and 1 deleted project it can be a rename
            if(addedProjectsList.Count == 1 && removedProjectsList.Count == 1)
                configFilesData = ShowPromptAndSolveDifferProblemOnPotentialRename(configFilesData, addedProjectsList.First(), removedProjectsList.First(), uIShell);
            else
            {
                //In other cases if there are added or removed projects, we show the prompt to the user and offer to update the config file.
                if (addedProjectsList.Count > 0)
                    configFilesData = ShowPromptAndSolveDifferProblems(configFilesData, addedProjectsList, uIShell, true);

                if (removedProjectsList.Count > 0)
                    configFilesData = ShowPromptAndSolveDifferProblems(configFilesData, removedProjectsList, uIShell, false);
            }

            //Then start all other (general) check rules
            var exportParametersNConfigFilesDataTuple = CheckRulesManager.CheckConfigFileRulesForExtension(configFilesData, currentCommitedProjState);

            //Export to ELP to show all finded problems in the error list of the IDE.
            ELPStoreManager.StoreErrorListProviderByValues(exportParametersNConfigFilesDataTuple.Item1.RefDepGuardFindedProblemsData, configFilesData, errorListProvider);

            return exportParametersNConfigFilesDataTuple;
        }

        private static ConfigFilesData ShowPromptAndSolveDifferProblemOnPotentialRename(
            ConfigFilesData configFilesData, string addedProj, string removedProj, IVsUIShell uIShell)
        {
            var message = "В solution обнаружено добавление проекта '" + addedProj + "' и удаление '" + removedProj + "'. " +
                "Переименовать проект в файле конфигураций с сохранением ограничений?";

            if (MessageManager.ShowYesNoPrompt(uIShell, message, "RefDepGuard"))//If user agrees
            {
                //Rename project
                configFilesData = ConfigFileExtentionManager.RenameProjectInConfigFile(configFilesData, addedProj, removedProj);
            }
            else
            { //Still asks for adding empty project sample and remove deleted project
                message = "Добавить в конфигурационный файл пустой шаблон для проекта '" + addedProj + "'?";

                if (MessageManager.ShowYesNoPrompt(uIShell, message, "RefDepGuard"))//If user agrees
                {
                    configFilesData = ConfigFileExtentionManager.UpdateSolutionConfigFile(configFilesData, new List<string> { addedProj }, true);
                }

                message = "Удалить из конфигурационного файла проект '" + removedProj + "'?";

                if (MessageManager.ShowYesNoPrompt(uIShell, message, "RefDepGuard"))//If user agrees
                {
                    configFilesData = ConfigFileExtentionManager.UpdateSolutionConfigFile(configFilesData, new List<string> { removedProj }, false);
                }

                //If user rejects it, he will see relevant "project match waring"
            }

                return configFilesData;

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
            string offeredSolutionStr = (isAddedList) ? ". Добавить " + projectNounStr + " в файл конфигураций?" : ". Удалить " + projectNounStr + " из файла конфигураций?";

            var message = "В " + findedProjPlaceStr + projectDetectionStr;

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
                configFilesData = ConfigFileExtentionManager.UpdateSolutionConfigFile(configFilesData, currentProjList, isAddedList);
            }
            else
            {
                //add "projects match warning" on later check
                
            }

            return configFilesData;
        }
    }
}