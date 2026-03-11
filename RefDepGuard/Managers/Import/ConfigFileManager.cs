using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using RefDepGuard.Data;
using RefDepGuard.Data.ConfigFile;
using RefDepGuard.Managers.Applied;
using RefDepGuard.Managers.Import;
using RefDepGuard.Models;

namespace RefDepGuard
{
    /// <summary>
    /// This class is responsible for managing the config files of the solution. 
    /// It contains methods for loading the config file data, updating it based on the changes in the solution projects and references, and handling errors related 
    /// to config files.
    /// </summary>
    public class ConfigFileManager
    {
        private static IServiceProvider serviceProvider;
        private static IVsUIShell uiShell;

        private static ConfigFileSolutionDTO configFileSolution;
        private static ConfigFileGlobalDTO configFileGlobal;
        private static string solutionName;
        private static string packageExtendedName;
        private static FileParseError parseError;

        private static ConfigFileServiceInfo currentSolutionConfigFileServiceInfo;
        private static ConfigFileServiceInfo globalSolutionConfigFileServiceInfo;


        private static Dictionary<string, ProjectState> commitedProjState;

        /// <summary>
        /// Sets the names of the config files in the right format based on the solution name and package name.
        /// </summary>
        public static void SetSolutionNameInfoInRightFormat()
        {
            packageExtendedName = SolutionNameManager.GetPackageName();
            solutionName = SolutionNameManager.GetSolutionName();
            string solutionExtendedName = SolutionNameManager.GetSolutionExtendedName();

            string solutionConfigGuardFile = solutionExtendedName + "_config_guard.rdg";
            string globalConfigGuardFile = packageExtendedName + "\\global_config_guard.rdg";

            string solutionConfigGuardRollbackFile = solutionExtendedName + "_config_guard_rollback.rdg";
            string globalConfigGuardRollbackFile = packageExtendedName + "\\global_config_guard_rollback.rdg";

            currentSolutionConfigFileServiceInfo = new ConfigFileServiceInfo(false, solutionConfigGuardFile, solutionConfigGuardRollbackFile);
            globalSolutionConfigFileServiceInfo = new ConfigFileServiceInfo(true, globalConfigGuardFile, globalConfigGuardRollbackFile);
        }

        /// <summary>
        /// The main method of the class. Loads the config file data of the solution and global config file, handles errors related to config files, and returns the 
        /// loaded data in ConfigFilesData object.
        /// </summary>
        /// <param name="currentServiceProvider">IServiceProvider interface value</param>
        /// <param name="currentUiShell">IVsUIShell interface value</param>
        /// <param name="currentCommitedProjState">commited projects state dictionary</param>
        /// <returns>ConfigFilesData value</returns>
        public static ConfigFilesData GetInfoFromConfigFiles(
            IServiceProvider currentServiceProvider, IVsUIShell currentUiShell, Dictionary<string, ProjectState> currentCommitedProjState)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            serviceProvider = currentServiceProvider;
            uiShell = currentUiShell;

            commitedProjState = currentCommitedProjState;
            parseError = FileParseError.None;

            GetCurrentConfigFileInfo(currentSolutionConfigFileServiceInfo);
            GetCurrentConfigFileInfo(globalSolutionConfigFileServiceInfo);

            return new ConfigFilesData(configFileSolution, configFileGlobal, parseError, solutionName, packageExtendedName);
        }

        /// <summary>
        /// Updates the solution config file data based on the changes in the solution projects and references, and returns the updated data
        /// </summary>
        /// <param name="currentConfigFilesData">ConfigFilesData value</param>
        /// <param name="differProjectsList">List of strings of differ projects between solution ones and config file ones</param>
        /// <param name="isProjectAdding">shows if it's a project adding or deleting</param>
        /// <returns>updated ConfigFilesData value</returns>
        public static ConfigFilesData UpdateSolutionConfigFile(ConfigFilesData currentConfigFilesData, List<string> differProjectsList, bool isProjectAdding)
        {
            configFileSolution = currentConfigFilesData.ConfigFileSolution;
            configFileGlobal = currentConfigFilesData.ConfigFileGlobal; //Протаскиваю здесь global, хотя он тут никак не может измениться, а надо ли?

            ConfigFileSolutionDTO currentProj = isProjectAdding ? 
                updateConfigFileSolutionByAddingProjects(differProjectsList) : 
                updateConfigFileSolutionByRemovingProjects(differProjectsList);

            string json = JsonConvert.SerializeObject(currentProj, Formatting.Indented);
            string updateFileName = packageExtendedName + "\\" + solutionName + "_config_guard.rdg";

            FileStreamManager.WriteInfoToFile(updateFileName, json);

            CacheManager.UpdateConfigFilesBackup(json, false);

            return new ConfigFilesData(configFileSolution, configFileGlobal, parseError, solutionName, packageExtendedName);
        }

        /// <summary>
        /// Checks if the config file exists, if it does, it tries to load the data from it. 
        /// If there are any errors during loading, it shows the error messages and offers the user to restore the last successfully saved data from the backup or 
        /// generate a new config file with default settings. 
        /// If the config file doesn't exist, it shows the error message and offers the user to restore the last successfully saved data from the backup or 
        /// generate a new config file with default settings.
        /// </summary>
        /// <param name="configFileServiceInfo">ConfigFileServiceInfo value</param>
        /// <param name="isSecondAttempt">shows if it's already a second attempt to parse file info or not</param>
        private static void GetCurrentConfigFileInfo(ConfigFileServiceInfo configFileServiceInfo, bool isSecondAttempt = false)
        {
            string typePrefix = configFileServiceInfo.IsGlobal ? "Глобальный ф" : "Ф";
            
            FileErrorMessage fileErrorMessage = new FileErrorMessage(
                "Не получилось загрузить " + typePrefix + "айл конфигурации", typePrefix + "айл конфигурации не найден");

            if (File.Exists(configFileServiceInfo.CurrentConfigGuardFile))
            {
                try
                {
                    string currentFileContent = FileStreamManager.ReadInfoFromFile(configFileServiceInfo.CurrentConfigGuardFile);

                    if (String.IsNullOrEmpty(currentFileContent))
                        throw new Exception();

                    if (configFileServiceInfo.IsGlobal)
                        configFileGlobal = JsonConvert.DeserializeObject<ConfigFileGlobalDTO>(currentFileContent);
                    else
                        configFileSolution = JsonConvert.DeserializeObject<ConfigFileSolutionDTO>(currentFileContent);

                    //It's considering that at this moment files with JSON syntax errors are already in catch, and Null value parameters can't be backed up anymore
                    CacheManager.UpdateConfigFilesBackup(currentFileContent, configFileServiceInfo.IsGlobal);
                }
                catch (Exception)
                {
                    // if syntax error in file actions
                    showConfigFileParseErrorMessageAndRestoreInfoIfNeeded(configFileServiceInfo, fileErrorMessage.BadDataErrorMessage, configFileServiceInfo.IsGlobal, isSecondAttempt);
                }
            }
            else
            {
                //if file doesn't exist actions
                var backupFileInfo = showConfigFileNotFoundErrorMessage(fileErrorMessage.FileNotFoundErrorMessage, configFileServiceInfo.IsGlobal, isSecondAttempt);

                if(backupFileInfo != "")
                    CopyInfoFromBackupToConfigFile(configFileServiceInfo, backupFileInfo);
                else
                    CreateNewConfigFile(configFileServiceInfo.CurrentConfigGuardFile, configFileServiceInfo.IsGlobal);
            }
        }

        /// <summary>
        /// Shows the error message when the config file is not found, and offers the user to restore the last successfully saved data from the backup
        /// </summary>
        /// <param name="errorReason">error reason string</param>
        /// <param name="isErrorGlobal">shows if it's global file error or solution</param>
        /// <param name="isSecondAttempt">shows if it's already a second attempt to parse file info or not</param>
        /// <returns>backup file info string</returns>
        private static string showConfigFileNotFoundErrorMessage(string errorReason, bool isErrorGlobal, bool isSecondAttempt)
        {
            string solutionNameInfo = "";
            string backupFileInfo = "";

            if (!isErrorGlobal)
                solutionNameInfo = " для solution '" + solutionName + "'";

            if (!isSecondAttempt)
                backupFileInfo = CacheManager.GetInfoFromBackupFile(isErrorGlobal);
            
            var actionAnnounc = backupFileInfo != "" ? "Файл конфигурации будет сгенерирован по последнему сохранению" : "Шаблон файла конфигурации будет сгенерирован расширением";

            MessageManager.ShowMessageBox(
                    serviceProvider,
                    errorReason + solutionNameInfo + ".\r\n" + actionAnnounc,
                    "RefDepGuard Error: Ошибка загрузки файла конфигурации"
            );

            return backupFileInfo;
        }

        /// <summary>
        /// Shows the error message when there are some errors during loading the config file, and offers the user to restore the last successfully saved data from 
        /// the backup
        /// </summary>
        /// <param name="configFileServiceInfo">ConfigFileServiceInfo value</param>
        /// <param name="errorReason">error reason string</param>
        /// <param name="isErrorGlobal">shows if it's global file error or solution</param>
        /// <param name="isSecondAttempt">shows if it's already a second attempt to parse file info or not</param>
        private static void showConfigFileParseErrorMessageAndRestoreInfoIfNeeded(ConfigFileServiceInfo configFileServiceInfo, string errorReason, bool isErrorGlobal, bool isSecondAttempt)
        {
            bool rollbackAction = true;
            string solutionNameInfo = "";
            string backupFileInfo = "";

            if (!isErrorGlobal)
                solutionNameInfo = " для solution '" + solutionName + "'";

            if (!isSecondAttempt)
            {
                backupFileInfo = CacheManager.GetInfoFromBackupFile(isErrorGlobal);

                var offeredOption = backupFileInfo != "" ? "Загрузить для вас последнее успешно сохранённое содержимое файла конфигурации?" : "Сгенерирвать для вас стандартный шаблон файла конфигурации?";

                rollbackAction = MessageManager.ShowYesNoPrompt(
                    uiShell,
                    errorReason + solutionNameInfo + ".\r\n" + offeredOption + "\r\nВсё текущее содержимое файла конфигурации будет перенесено в Rollback-файл",
                    "RefDepGuard Error: Ошибка загрузки файла конфигурации"
                    );
            }
            else
            {
                MessageManager.ShowMessageBox(serviceProvider,
                    "Не удалось загрузить последние сохранённые данные.\r\nШаблон файла конфигурации будет сгенерирован расширением", 
                    "RefDepGuard Error: Ошибка загрузки файла конфигурации");
            }

            if (rollbackAction)
            {
                if(!isSecondAttempt)
                    RestoreInfoToRollbackFile(configFileServiceInfo.CurrentConfigGuardFile, configFileServiceInfo.CurrentConfigGuardRollbackFile);

                if (backupFileInfo != "")
                    CopyInfoFromBackupToConfigFile(configFileServiceInfo, backupFileInfo);
                else
                    CreateNewConfigFile(configFileServiceInfo.CurrentConfigGuardFile, configFileServiceInfo.IsGlobal);
            }
            else
            {
                //Case when Global file is already loaded, and then Solution file with errors is trying to be loaded isn't possible, because solution file always
                //loads before global one.
                if (isErrorGlobal && parseError == FileParseError.Solution)
                    parseError = FileParseError.All;
                else
                    parseError = isErrorGlobal ? FileParseError.Global : FileParseError.Solution;

                //Even if user doesn't want to restore the last successfully saved data from the backup, we still need to generate default config file data inside
                //the program.
                if (isErrorGlobal)
                    generateDefaultConfigFileDataGlobal();
                else
                    generateDefaultConfigFileDataSolution();
            }    
        }

        /// <summary>
        /// Copies the config file data from the backup to the config file, and then tries to read the config file info again. 
        /// This method is used when there are some errors during loading the config file, and the user chooses to restore the last successfully saved data from the 
        /// backup.
        /// </summary>
        /// <param name="configFileServiceInfo">ConfigFileServiceInfo value</param>
        /// <param name="backupFileInfo">backup file info string</param>
        private static void CopyInfoFromBackupToConfigFile(ConfigFileServiceInfo configFileServiceInfo, string backupFileInfo)
        {
            FileStreamManager.WriteInfoToFile(configFileServiceInfo.CurrentConfigGuardFile, backupFileInfo);
            GetCurrentConfigFileInfo(configFileServiceInfo, true); //Second attempt to read config file
        }

        /// <summary>
        /// Creates a new config file with default settings. This method is used when the config file doesn't exist, or when there are some errors during loading the
        /// config file, and the user doesn't want to restore the last successfully saved data from the backup.
        /// </summary>
        /// <param name="currentConfigGuardFile">config guard file name string</param>
        /// <param name="isGlobal">shows if it's global file error or solution</param>
        private static void CreateNewConfigFile(string currentConfigGuardFile, bool isGlobal)
        {
            string json;

            if (isGlobal)
                json = JsonConvert.SerializeObject(generateDefaultConfigFileDataGlobal(), Formatting.Indented);
            else
                json = JsonConvert.SerializeObject(generateDefaultConfigFileDataSolution(), Formatting.Indented);

            FileStreamManager.WriteInfoToFile(currentConfigGuardFile, json);

            CacheManager.UpdateConfigFilesBackup(json, isGlobal);
        }

        /// <summary>
        /// Restores the config file data to the Rollback file. 
        /// This method is used when there are some errors during loading the config file, and the user chooses to restore syntax error data to rollback file.
        /// </summary>
        /// <param name="currentConfigGuardFile">config guard file name string</param>
        /// <param name="currentConfigGuardRollbackFile">config guard rollback file name string</param>
        private static void RestoreInfoToRollbackFile(string currentConfigGuardFile, string currentConfigGuardRollbackFile)
        {
            try
            {
                string fileInfo = FileStreamManager.ReadInfoFromFile(currentConfigGuardFile);
                FileStreamManager.WriteInfoToFile(currentConfigGuardRollbackFile, fileInfo);
            }
            catch (Exception)
            {
                MessageManager.ShowMessageBox(
                    serviceProvider,
                    "Проверьте, что глобальный и локальные .rdg файлы не имеют запретов на чтение, а в корневой папке solution нет запрета на создание файлов",
                    "RefDepGuard: Ошибка генерации Rollback файла"
                    );
            }
        }

        /// <summary>
        /// Updates the solution config file data by adding the added projects to the config file, and then returns the updated config file data.
        /// </summary>
        /// <param name="addedProjectsList">list of strings with adding projects</param>
        /// <returns>updated ConfigFileSolutionDTO value</returns>
        private static ConfigFileSolutionDTO updateConfigFileSolutionByAddingProjects(List<string> addedProjectsList)
        {
            foreach (var projectName in addedProjectsList)
                configFileSolution.projects.Add(projectName, GenerateDefaultConfigFileProject());
            
            return configFileSolution;
        }

        /// <summary>
        /// Updates the solution config file data by removing the deleted projects from the config file, and then returns the updated config file data.
        /// </summary>
        /// <param name="removedProjectsList">list of strings with removing projects</param>
        /// <returns>updated ConfigFileSolutionDTO value</returns>
        private static ConfigFileSolutionDTO updateConfigFileSolutionByRemovingProjects(List<string> removedProjectsList)
        {
            foreach(var project in removedProjectsList)
                configFileSolution.projects.Remove(project);
            
            return configFileSolution;
        }

        /// <summary>
        /// Generates the default config file data for the solution config file. 
        /// This method is used when the solution config file doesn't exist, or when there are some errors during loading the backup file
        /// </summary>
        /// <returns>default ConfigFileSolutionDTO value</returns>
        private static ConfigFileSolutionDTO generateDefaultConfigFileDataSolution()
        {
            configFileSolution = new ConfigFileSolutionDTO();
            configFileSolution.name = solutionName;
            configFileSolution.framework_max_version = "-";
            configFileSolution.report_on_transit_references = false;
            configFileSolution.solution_required_references = new List<string>();
            configFileSolution.solution_unacceptable_references = new List<string>();
            configFileSolution.projects = new Dictionary<string, ConfigFileProjectDTO>();

            foreach (var projectName in commitedProjState.Keys)
                configFileSolution.projects.Add(projectName, GenerateDefaultConfigFileProject());

            return configFileSolution;
        }

        /// <summary>
        /// Generates the default config file data for the global config file.
        /// </summary>
        /// <returns>default ConfigFileGlobalDTO value</returns>
        private static ConfigFileGlobalDTO generateDefaultConfigFileDataGlobal()
        {
            configFileGlobal = new ConfigFileGlobalDTO();
            configFileGlobal.name = "Global";
            configFileGlobal.framework_max_version = "-";
            configFileGlobal.report_on_transit_references = false;
            configFileGlobal.global_required_references = new List<string>();
            configFileGlobal.global_unacceptable_references = new List<string>();

            return configFileGlobal;
        }

        /// <summary>
        /// Generates the default config file data for the project in the solution config file.
        /// </summary>
        /// <returns>default ConfigFileProjectDTO value</returns>
        private static ConfigFileProjectDTO GenerateDefaultConfigFileProject()
        {
            ConfigFileProjectRefsConsideringDTO configFileProjectRefsConsidering = new ConfigFileProjectRefsConsideringDTO();
            configFileProjectRefsConsidering.required = true;
            configFileProjectRefsConsidering.unacceptable = true;

            ConfigFileProjectDTO fileProject = new ConfigFileProjectDTO();
            fileProject.framework_max_version = "-";
            fileProject.report_on_transit_references = false;
            fileProject.consider_global_and_solution_references = configFileProjectRefsConsidering;
            fileProject.required_references = new List<string>();
            fileProject.unacceptable_references = new List<string>();

            return fileProject;
        }
    }
}
