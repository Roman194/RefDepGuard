using EnvDTE;
using Microsoft.Office.Interop.Excel;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using VSIXProject1.Data;
using VSIXProject1.Data.ConfigFile;

namespace VSIXProject1
{
    public class ConfigFileManager
    {
        static IServiceProvider serviceProvider;

        static ConfigFileSolution configFileSolution;
        static ConfigFileGlobal configFileGlobal;
        static string solutionName;
        static string packageExtendedName;

        static Dictionary<string, ProjectState> commitedProjState;

        public static ConfigFilesData GetInfoFromConfigFiles(
            DTE dte, IServiceProvider currentServiceProvider, Dictionary<string, ProjectState> currentCommitedProjState
            )
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            serviceProvider = currentServiceProvider;
            commitedProjState = currentCommitedProjState;

            string dteSolutionFullName = dte.Solution.FullName;
            int lastDotIndex = dteSolutionFullName.LastIndexOf('.');
            int lastSlashIndex = dteSolutionFullName.LastIndexOf('\\');
            string solutionExtendedName = dteSolutionFullName.Substring(0, lastDotIndex);
            packageExtendedName = dteSolutionFullName.Substring(0, lastSlashIndex);

            solutionName = dteSolutionFullName.Substring(lastSlashIndex + 1, lastDotIndex - lastSlashIndex - 1);

            string solutionConfigGuardFile = solutionExtendedName + "_config_guard.rdg";
            string globalConfigGuardFile = packageExtendedName + "\\global_config_guard.rdg";

            string solutionConfigGuardRollbackFile = solutionExtendedName + "_config_guard_rollback.rdg";
            string globalConfigGuardRollbackFile = packageExtendedName + "\\global_config_guard_rollback.rdg";

            ConfigFileServiceInfo currentSolutionConfigFileServiceInfo = new ConfigFileServiceInfo(false, solutionConfigGuardFile, solutionConfigGuardRollbackFile);
            ConfigFileServiceInfo globalSolutionConfigFileServiceInfo = new ConfigFileServiceInfo(true, globalConfigGuardFile, globalConfigGuardRollbackFile);

            GetCurrentConfigFileInfo(currentSolutionConfigFileServiceInfo);
            GetCurrentConfigFileInfo(globalSolutionConfigFileServiceInfo);

            return new ConfigFilesData(configFileSolution, configFileGlobal, solutionName, packageExtendedName);
        }

        private static void GetCurrentConfigFileInfo(ConfigFileServiceInfo configFileServiceInfo)
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

                        string currentFileConetnt = sr.ReadToEnd();
                        if (String.IsNullOrEmpty(currentFileConetnt))
                            throw new Exception();

                        if (configFileServiceInfo.IsGlobal)
                            configFileGlobal = JsonConvert.DeserializeObject<ConfigFileGlobal>(currentFileConetnt);
                        else
                            configFileSolution = JsonConvert.DeserializeObject<ConfigFileSolution>(currentFileConetnt);
                    }
                }
                catch (Exception)
                {
                    showConfigFileParseErrorMessage(fileErrorMessage.BadDataErrorMessage, false, true);
                    RestoreInfoToRollbackFile(configFileServiceInfo.SolutionConfigGuardFile, configFileServiceInfo.SolutionConfigGuardRollbackFile);
                    CreateNewConfigFile(configFileServiceInfo.SolutionConfigGuardFile, configFileServiceInfo.IsGlobal);
                }
            }
            else
            {
                showConfigFileParseErrorMessage(fileErrorMessage.FileNotFoundErrorMessage, false, false);
                CreateNewConfigFile(configFileServiceInfo.SolutionConfigGuardFile, configFileServiceInfo.IsGlobal);
            }
        }

        private static void showConfigFileParseErrorMessage(string errorReason, bool isErrorGlobal, bool isFileExists)
        {
            string rollbackAction = "";
            string solutionNameInfo = "";

            if (isFileExists)
                rollbackAction = "Информация, содержащаяся в файле конфигурации будет перезаписана в rollback-файл.\r\nПроверьте её на предмет отсутствия синтаксических ошибок и соответствия шаблону файла конфигурации!";

            if (!isErrorGlobal)
                solutionNameInfo = " для solution '" + solutionName + "'";

            MessageManager.ShowMessageBox(
                serviceProvider,
                errorReason + solutionNameInfo + ".\r\n Шаблон файла конфигурации будет сгенерирован расширением" + ". \r\n" + rollbackAction,
                "RefDepGuard Error: Ошибка загрузки файла конфигурации"
                );
        }

        private static void CreateNewConfigFile(string currentConfigGuardFile, bool isGlobal)
        {
            using (FileStream fileStream = File.Create(currentConfigGuardFile))
            {
                StreamWriter streamWriter = new StreamWriter(fileStream);
                string json;

                if (isGlobal)
                    json = JsonConvert.SerializeObject(generateDefaultConfigFileGlobal());
                else
                    json = JsonConvert.SerializeObject(generateDefaultConfigFileSolution());

                streamWriter.Write(json);

                streamWriter.Flush();
                fileStream.Flush();

                streamWriter.Close();
            }
        }

        public static ConfigFilesData UpdateSolutionConfigFile(ConfigFilesData currentConfigFilesData, List<string> differProjectsList, bool isProjectAdding)
        {
            configFileSolution = currentConfigFilesData.configFileSolution;
            configFileGlobal = currentConfigFilesData.configFileGlobal; //???

            using (FileStream fileStream = File.Create(packageExtendedName + "\\" + solutionName + "_config_guard.rdg"))
            {
                StreamWriter streamWriter = new StreamWriter(fileStream);
                string json;

                if(isProjectAdding)
                    json = JsonConvert.SerializeObject(updateConfigFileSolutionByAddingProjects(differProjectsList));
                else
                    json = JsonConvert.SerializeObject(updateConfigFileSolutionByRemovingProjects(differProjectsList));

                streamWriter.Write(json);

                streamWriter.Flush();
                fileStream.Flush();

                streamWriter.Close();
            }

            return new ConfigFilesData(configFileSolution, configFileGlobal, solutionName, packageExtendedName);// Предполагается, что эти параметры не могут нигде измениться после инициализации до вызова этого метода
        }

        private static ConfigFileSolution updateConfigFileSolutionByAddingProjects(List<string> addedProjectsList)
        {
            foreach (var projectName in addedProjectsList)
            {
                ConfigFileProjectRefsConsidering configFileProjectRefsConsidering = new ConfigFileProjectRefsConsidering();
                configFileProjectRefsConsidering.required = true;
                configFileProjectRefsConsidering.unacceptable = true;

                ConfigFileProject fileProject = new ConfigFileProject();
                fileProject.framework_max_version = "-";
                fileProject.consider_global_and_solution_references = configFileProjectRefsConsidering;
                fileProject.required_references = new List<string>();
                fileProject.unacceptable_references = new List<string>();

                configFileSolution.projects.Add(projectName, fileProject);
            }

            return configFileSolution;
        }

        private static ConfigFileSolution updateConfigFileSolutionByRemovingProjects(List<string> removedProjectsList)
        {

            foreach(var project in removedProjectsList)
                configFileSolution.projects.Remove(project);
            
            return configFileSolution;
        }

        private static ConfigFileSolution generateDefaultConfigFileSolution()
        {
            configFileSolution = new ConfigFileSolution();
            configFileSolution.name = solutionName;
            configFileSolution.framework_max_version = "-";
            configFileSolution.solution_required_references = new List<string>();
            configFileSolution.solution_unacceptable_references = new List<string>();
            configFileSolution.projects = new Dictionary<string, ConfigFileProject>();

            foreach (var projectName in commitedProjState.Keys)
            {
                ConfigFileProjectRefsConsidering configFileProjectRefsConsidering = new ConfigFileProjectRefsConsidering();
                configFileProjectRefsConsidering.required = true;
                configFileProjectRefsConsidering.unacceptable = true;

                ConfigFileProject fileProject = new ConfigFileProject();
                fileProject.framework_max_version = "-";
                fileProject.consider_global_and_solution_references = configFileProjectRefsConsidering;
                fileProject.required_references = new List<string>();
                fileProject.unacceptable_references = new List<string>();

                configFileSolution.projects.Add(projectName, fileProject);
            }

            return configFileSolution;
        }

        private static ConfigFileGlobal generateDefaultConfigFileGlobal()
        {
            configFileGlobal = new ConfigFileGlobal();
            configFileGlobal.name = "Global";
            configFileGlobal.framework_max_version = "-";
            configFileGlobal.global_required_references = new List<string>();
            configFileGlobal.global_unacceptable_references = new List<string>();

            return configFileGlobal;
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
