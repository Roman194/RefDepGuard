using EnvDTE;
using Microsoft.Office.Interop.Excel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using VSIXProject1.Data;
using VSIXProject1.Data.ConfigFile;
using VSIXProject1.Managers.Applied;
using VSIXProject1.Managers.Import;
using VSIXProject1.Models;

namespace VSIXProject1
{
    public class ConfigFileManager
    {
        private static IServiceProvider serviceProvider;
        private static IVsUIShell uiShell;

        private static ConfigFileSolutionDTO configFileSolution;
        private static ConfigFileGlobalDTO configFileGlobal;
        private static string solutionName;
        private static string packageExtendedName;
        private static FileParseError parseError;


        private static Dictionary<string, ProjectState> commitedProjState;

        public static ConfigFilesData GetInfoFromConfigFiles(
            IServiceProvider currentServiceProvider, IVsUIShell currentUiShell, Dictionary<string, ProjectState> currentCommitedProjState)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            serviceProvider = currentServiceProvider;
            uiShell = currentUiShell;

            commitedProjState = currentCommitedProjState;
            parseError = FileParseError.None;
            
            packageExtendedName = SolutionNameManager.GetPackageName();
            solutionName = SolutionNameManager.GetSolutionName();
            string solutionExtendedName = SolutionNameManager.GetSolutionExtendedName();

            string solutionConfigGuardFile = solutionExtendedName + "_config_guard.rdg";
            string globalConfigGuardFile = packageExtendedName + "\\global_config_guard.rdg";

            string solutionConfigGuardRollbackFile = solutionExtendedName + "_config_guard_rollback.rdg";
            string globalConfigGuardRollbackFile = packageExtendedName + "\\global_config_guard_rollback.rdg";

            ConfigFileServiceInfo currentSolutionConfigFileServiceInfo = new ConfigFileServiceInfo(false, solutionConfigGuardFile, solutionConfigGuardRollbackFile);
            ConfigFileServiceInfo globalSolutionConfigFileServiceInfo = new ConfigFileServiceInfo(true, globalConfigGuardFile, globalConfigGuardRollbackFile);

            GetCurrentConfigFileInfo(currentSolutionConfigFileServiceInfo);
            GetCurrentConfigFileInfo(globalSolutionConfigFileServiceInfo);

            return new ConfigFilesData(configFileSolution, configFileGlobal, parseError, solutionName, packageExtendedName);
        }

        public static ConfigFilesData UpdateSolutionConfigFile(ConfigFilesData currentConfigFilesData, List<string> differProjectsList, bool isProjectAdding)
        {
            configFileSolution = currentConfigFilesData.configFileSolution;
            configFileGlobal = currentConfigFilesData.configFileGlobal; //???

            using (FileStream fileStream = File.Create(packageExtendedName + "\\" + solutionName + "_config_guard.rdg"))
            {
                StreamWriter streamWriter = new StreamWriter(fileStream);
                string json;

                if (isProjectAdding)
                    json = JsonConvert.SerializeObject(updateConfigFileSolutionByAddingProjects(differProjectsList), Formatting.Indented);
                else
                    json = JsonConvert.SerializeObject(updateConfigFileSolutionByRemovingProjects(differProjectsList), Formatting.Indented);

                streamWriter.Write(json);

                streamWriter.Flush();
                fileStream.Flush();

                streamWriter.Close();

                CacheManager.UpdateConfigFilesBackup(json, false);
            }

            return new ConfigFilesData(configFileSolution, configFileGlobal, parseError, solutionName, packageExtendedName);// Предполагается, что эти параметры не могут нигде измениться после инициализации до вызова этого метода
        }

        private static void GetCurrentConfigFileInfo(ConfigFileServiceInfo configFileServiceInfo, bool isSecondAttempt = false)
        {
            FileErrorMessage fileErrorMessage;

            if (configFileServiceInfo.IsGlobal)
                fileErrorMessage = new FileErrorMessage("Не получилось загрузить глобальный файл конфигурации", "Глобальный файл конфигурации не найден");
            else
                fileErrorMessage = new FileErrorMessage("Не получилось загрузить файл конфигурации", "Файл конфигурации не найден");

            if (File.Exists(configFileServiceInfo.SolutionConfigGuardFile))
            {
                try
                {
                    using (FileStream fileStream = new FileStream(configFileServiceInfo.SolutionConfigGuardFile, FileMode.Open))
                    {
                        StreamReader sr = new StreamReader(fileStream);

                        string currentFileContent = sr.ReadToEnd();
                        if (String.IsNullOrEmpty(currentFileContent))
                            throw new Exception();

                        if (configFileServiceInfo.IsGlobal)
                            configFileGlobal = JsonConvert.DeserializeObject<ConfigFileGlobalDTO>(currentFileContent);
                        else
                            configFileSolution = JsonConvert.DeserializeObject<ConfigFileSolutionDTO>(currentFileContent);

                        //Предполагается, что к текущему моменту настройки с ошибками JSON-синтаксиса уже будут в catch, а Null value parameters можно уже и бэкапить
                        CacheManager.UpdateConfigFilesBackup(currentFileContent, configFileServiceInfo.IsGlobal); 
                    }
                }
                catch (Exception ex)
                {
                    showConfigFileParseErrorMessageAndRestoreInfoIfNeeded(configFileServiceInfo, fileErrorMessage.BadDataErrorMessage, configFileServiceInfo.IsGlobal, isSecondAttempt);
                }
            }
            else
            {
                var backupFileInfo = showConfigFileNotFoundErrorMessage(fileErrorMessage.FileNotFoundErrorMessage, configFileServiceInfo.IsGlobal, isSecondAttempt);

                if(backupFileInfo != "")
                    CopyInfoFromBackupToConfigFile(configFileServiceInfo, backupFileInfo);
                else
                    CreateNewConfigFile(configFileServiceInfo.SolutionConfigGuardFile, configFileServiceInfo.IsGlobal);
            }
        }

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
                    errorReason + solutionNameInfo + ".\r\n " + actionAnnounc,
                    "RefDepGuard Error: Ошибка загрузки файла конфигурации"
            );

            return backupFileInfo;
        }

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
                    RestoreInfoToRollbackFile(configFileServiceInfo.SolutionConfigGuardFile, configFileServiceInfo.SolutionConfigGuardRollbackFile);

                if (backupFileInfo != "")
                    CopyInfoFromBackupToConfigFile(configFileServiceInfo, backupFileInfo);
                else
                    CreateNewConfigFile(configFileServiceInfo.SolutionConfigGuardFile, configFileServiceInfo.IsGlobal);
            }
            else
            {
                //Случай, когда уже занесён Глобал и попал сюда ещё и Solution невозможен, так как всегда сначала парсится Solution, а затем Global
                if (isErrorGlobal && parseError == FileParseError.Solution)
                    parseError = FileParseError.All;
                else
                    parseError = isErrorGlobal ? FileParseError.Global : FileParseError.Solution;

                if (isErrorGlobal)//Если пользователь не хочет "откатывать" файл, то всё-равно внутри проги нужно сгенерировать дефолт конфигурационные данные
                    generateDefaultConfigFileDataGlobal();
                else
                    generateDefaultConfigFileDataSolution();
            }    
        }

        private static void CopyInfoFromBackupToConfigFile(ConfigFileServiceInfo configFileServiceInfo, string backupFileInfo) //Оптимизировать!
        {
            using (FileStream fileStream = File.Create(configFileServiceInfo.SolutionConfigGuardFile))
            {
                StreamWriter streamWriter = new StreamWriter(fileStream);
                
                streamWriter.Write(backupFileInfo);

                streamWriter.Flush();
                fileStream.Flush();

                streamWriter.Close();
            }

            GetCurrentConfigFileInfo(configFileServiceInfo, true); //Second attempt to read config file
        }

        private static void CreateNewConfigFile(string currentConfigGuardFile, bool isGlobal)
        {
            using (FileStream fileStream = File.Create(currentConfigGuardFile))
            {
                StreamWriter streamWriter = new StreamWriter(fileStream);
                string json;

                if (isGlobal)
                    json = JsonConvert.SerializeObject(generateDefaultConfigFileDataGlobal(), Formatting.Indented);
                else
                    json = JsonConvert.SerializeObject(generateDefaultConfigFileDataSolution(), Formatting.Indented);

                streamWriter.Write(json);

                streamWriter.Flush();
                fileStream.Flush();

                streamWriter.Close();

                CacheManager.UpdateConfigFilesBackup(json, isGlobal);
            }
        }

        private static ConfigFileSolutionDTO updateConfigFileSolutionByAddingProjects(List<string> addedProjectsList)
        {
            foreach (var projectName in addedProjectsList)
                configFileSolution.projects.Add(projectName, GenerateDefaultConfigFileProject());
            
            return configFileSolution;
        }

        private static ConfigFileSolutionDTO updateConfigFileSolutionByRemovingProjects(List<string> removedProjectsList)
        {

            foreach(var project in removedProjectsList)
                configFileSolution.projects.Remove(project);
            
            return configFileSolution;
        }

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

        private static ConfigFileProjectDTO GenerateDefaultConfigFileProject()
        {
            ConfigFileProjectRefsConsidering configFileProjectRefsConsidering = new ConfigFileProjectRefsConsidering();
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

        private static void RestoreInfoToRollbackFile(string currentConfigGuardFile, string currentConfigGuardRollbackFile)
        {
            try
            {
                string fileInfo;

                using (FileStream fileStream = new FileStream(currentConfigGuardFile, FileMode.Open))
                {
                    StreamReader sr = new StreamReader(fileStream);

                    fileInfo = sr.ReadToEnd();
                }

                using (FileStream fileStream = File.Create(currentConfigGuardRollbackFile))
                {
                    StreamWriter streamWriter = new StreamWriter(fileStream);

                    streamWriter.Write(fileInfo);

                    streamWriter.Flush();
                    fileStream.Flush();

                    streamWriter.Close();
                }
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
    }
}
