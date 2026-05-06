using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using RefDepGuard.Managers.Applied;
using RefDepGuard.Applied.Models.Project;
using RefDepGuard.Applied.Models.ConfigFile;
using RefDepGuard.Applied.Models.ConfigFile.DTO;
using RefDepGuard.CheckRules.Models;
using RefDepGuard.Applied;
using RefDepGuard.ConfigFile;
using RefDepGuard.StringResources;

namespace RefDepGuard
{
    /// <summary>
    /// This class is responsible for managing the config files of the solution. 
    /// It contains methods for loading the config file data, updating it based on the changes in the solution projects and references, and handling errors related 
    /// to config files.
    /// </summary>
    public class ConfigFileExtensionManager
    {
        private static IServiceProvider serviceProvider;
        private static IVsUIShell uiShell;

        private static string solutionName;
        private static string packageExtendedName;

        private static ConfigFileFoundState filesFoundState;
        private static ConfigFilesData configFilesData;

        private static ConfigFileServiceInfo currentSolutionConfigFileServiceInfo;
        private static ConfigFileServiceInfo globalSolutionConfigFileServiceInfo;

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
        /// <param name="currentCommitedSolState">commited projects state dictionary</param>
        /// <returns>ConfigFilesData value</returns>
        public static ConfigFilesData GetInfoFromConfigFiles(
            IServiceProvider currentServiceProvider, IVsUIShell currentUiShell, Dictionary<string, ProjectState> currentCommitedSolState)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            serviceProvider = currentServiceProvider;
            uiShell = currentUiShell;
            
            (configFilesData, filesFoundState) = ConfigFileCoreManager.GetInfoFromConfigFilesForExtension(currentSolutionConfigFileServiceInfo, globalSolutionConfigFileServiceInfo,
                solutionName, packageExtendedName, currentCommitedSolState);

            var parseErrorCommitAfterGetInfo = configFilesData.ParseError;

            if (parseErrorCommitAfterGetInfo != FileParseError.None)
            {
                if(parseErrorCommitAfterGetInfo != FileParseError.Global) //Условие от обратного
                    HandleConfigFileErrorCase(currentSolutionConfigFileServiceInfo, filesFoundState.Solution);

                if (parseErrorCommitAfterGetInfo != FileParseError.Solution)
                    HandleConfigFileErrorCase(globalSolutionConfigFileServiceInfo, filesFoundState.Global);
            }
            
            return configFilesData;
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
            configFilesData.ConfigFileSolution = currentConfigFilesData.ConfigFileSolution;
            configFilesData.ConfigFileGlobal = currentConfigFilesData.ConfigFileGlobal; //Протаскиваю здесь global, хотя он тут никак не может измениться, а надо ли?

            ConfigFileSolutionDTO currentProj = isProjectAdding ? 
                updateConfigFileSolutionByAddingProjects(differProjectsList) : 
                updateConfigFileSolutionByRemovingProjects(differProjectsList);

            WriteInfoToSolutionConfigFileAndItsBackup(currentProj);

            return configFilesData;
        }

        public static ConfigFilesData RenameProjectInConfigFile(ConfigFilesData currentConfigFilesData, string newName, string oldName)
        {
            configFilesData.ConfigFileSolution = currentConfigFilesData.ConfigFileSolution;
            var projValue = configFilesData.ConfigFileSolution.projects[oldName];
            configFilesData.ConfigFileSolution.projects.Remove(oldName);
            configFilesData.ConfigFileSolution.projects.Add(newName, projValue);

            WriteInfoToSolutionConfigFileAndItsBackup(configFilesData.ConfigFileSolution);

            return configFilesData;
        }

        private static void WriteInfoToSolutionConfigFileAndItsBackup(ConfigFileSolutionDTO configFileSolution)
        {
            string json = JsonConvert.SerializeObject(configFileSolution, Formatting.Indented);
            string updateFileName = packageExtendedName + "\\" + solutionName + "_config_guard.rdg";

            FileStreamManager.WriteInfoToFile(updateFileName, json);

            CacheManager.UpdateConfigFilesBackup(json, false);
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
        private static void HandleConfigFileErrorCase(ConfigFileServiceInfo configFileServiceInfo, bool isFileFound, bool isSecondAttempt = false)
        {
            string typePrefix = configFileServiceInfo.IsGlobal ? Resource.Config_File_Type_Prefix_Global : Resource.Config_File_Type_Prefix_Solution;
            
            FileErrorMessage fileErrorMessage = new FileErrorMessage(
                Resource.Config_File_Load_Error + typePrefix + Resource.Config_File_Load_Error_1, 
                Resource.Config_File_Not_Found_Error + typePrefix + Resource.Config_File_Not_Found_Error_1);

            if (isFileFound)
            {
                // if syntax error in file actions
                showConfigFileParseErrorMessageAndRestoreInfoIfNeeded(configFileServiceInfo, fileErrorMessage.BadDataErrorMessage, configFileServiceInfo.IsGlobal, isSecondAttempt);
            }
            else
            {
                //if file doesn't exist actions
                var backupFileInfo = showConfigFileNotFoundErrorMessage(fileErrorMessage.FileNotFoundErrorMessage, configFileServiceInfo.IsGlobal, isSecondAttempt);

                if (backupFileInfo != "")
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
                solutionNameInfo = Resource.Solution_Name_String + solutionName + "'";

            if (!isSecondAttempt)
                backupFileInfo = CacheManager.GetInfoFromBackupFile(isErrorGlobal);

            var actionAnnounce = backupFileInfo != "" ? Resource.Config_File_On_Load_From_Cache_Action : Resource.Config_File_On_Create_From_Sample_Action;

            MessageManager.ShowMessageBox(
                    serviceProvider,
                    errorReason + solutionNameInfo + ".\r\n" + actionAnnounce,
                    Resource.Extension_Name + Resource.Config_File_load_Error_Title
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
                solutionNameInfo = Resource.Solution_Name_String + solutionName + "'";

            if (!isSecondAttempt)
            {
                backupFileInfo = CacheManager.GetInfoFromBackupFile(isErrorGlobal);

                var offeredOption = backupFileInfo != "" ? Resource.Config_File_On_Load_From_Cache_Question : Resource.Config_File_On_Create_From_Sample_Question;

                rollbackAction = MessageManager.ShowYesNoPrompt(
                    uiShell,
                    errorReason + solutionNameInfo + ".\r\n" + offeredOption + "\r\n" + Resource.Config_File_Transfer_To_Rollback_File_Message,
                    Resource.Extension_Name + Resource.Config_File_load_Error_Title
                    );
            }
            else
            {
                MessageManager.ShowMessageBox(serviceProvider,
                    Resource.Config_File_Load_From_Cache_Fail + ".\r\n" + Resource.Config_File_On_Create_From_Sample_Action,
                    Resource.Extension_Name + Resource.Config_File_load_Error_Title);
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
            
        }

        /// <summary>
        /// Copies the config file data from the backup to the config file, and then tries to read the config file info again. 
        /// This method is used when there are some errors during the config file loading, and the user chooses to restore the last successfully saved data from the 
        /// backup.
        /// </summary>
        /// <param name="configFileServiceInfo">ConfigFileServiceInfo value</param>
        /// <param name="backupFileInfo">backup file info string</param>
        private static void CopyInfoFromBackupToConfigFile(ConfigFileServiceInfo configFileServiceInfo, string backupFileInfo)
        {
            FileParseError parseErrorPredict;
            if (configFileServiceInfo.IsGlobal) //Делаем допущение, что проблема парсинга сейчас будет исправлена
                parseErrorPredict = (configFilesData.ParseError == FileParseError.All) ? FileParseError.Solution : FileParseError.None;
            else
                parseErrorPredict = (configFilesData.ParseError == FileParseError.All) ? FileParseError.Global : FileParseError.None;

            FileStreamManager.WriteInfoToFile(configFileServiceInfo.CurrentConfigGuardFile, backupFileInfo);
            configFilesData = ConfigFileCoreManager.GetConfigFileInfoSecondAttempt(configFileServiceInfo, parseErrorPredict); //Second attempt to read config file
            if (configFilesData.ParseError != parseErrorPredict) //Если ошибки парсинга не изменились, значит опять обрабатываем ошибку
                HandleConfigFileErrorCase(configFileServiceInfo, true, true); 
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

            configFilesData = ConfigFileCoreManager.UpdateParseErrorStateOnDefaultFileCreation(isGlobal);

            if (isGlobal)
                json = JsonConvert.SerializeObject(configFilesData.ConfigFileGlobal, Formatting.Indented); //Предполагается, что если не получилось спарсить, то из core вернуться дефолт значения
            else
                json = JsonConvert.SerializeObject(configFilesData.ConfigFileSolution, Formatting.Indented);

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
                    Resource.Rollback_File_Generation_Fail_Message,
                    Resource.Extension_Name + Resource.Rollback_File_Generation_Fail_Title
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
                configFilesData.ConfigFileSolution.projects.Add(projectName, ConfigFileCoreManager.GenerateDefaultProjectConfigFile());
            
            return configFilesData.ConfigFileSolution;
        }

        /// <summary>
        /// Updates the solution config file data by removing the deleted projects from the config file, and then returns the updated config file data.
        /// </summary>
        /// <param name="removedProjectsList">list of strings with removing projects</param>
        /// <returns>updated ConfigFileSolutionDTO value</returns>
        private static ConfigFileSolutionDTO updateConfigFileSolutionByRemovingProjects(List<string> removedProjectsList)
        {
            foreach(var project in removedProjectsList)
                configFilesData.ConfigFileSolution.projects.Remove(project);
            
            return configFilesData.ConfigFileSolution;
        }
    }
}