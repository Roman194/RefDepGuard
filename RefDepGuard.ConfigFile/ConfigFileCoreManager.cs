using Newtonsoft.Json;
using RefDepGuard.Applied;
using System;
using System.Collections.Generic;
using System.IO;
using RefDepGuard.Applied.Models.ConfigFile;
using RefDepGuard.Applied.Models.Project;
using RefDepGuard.Applied.Models.ConfigFile.DTO;

namespace RefDepGuard.ConfigFile
{
    public class ConfigFileCoreManager
    {
        private static ConfigFileSolutionDTO configFileSolution;
        private static ConfigFileGlobalDTO configFileGlobal;
        private static FileParseError ParseError;
        private static ConfigFileFoundState IsFilesFound;
        private static string SolutionName;

        private static Dictionary<string, ProjectState> CommitedSolState;

        public static ConfigFilesData GetInfoFromConfigFiles(
            ConfigFileServiceInfo currentSolutionConfigFileServiceInfo, ConfigFileServiceInfo globalSolutionConfigFileServiceInfo, string solutionName, string rootDir,
            Dictionary<string, ProjectState> currentCommitedSolState)
        {
            ParseError = FileParseError.None;
            IsFilesFound = new ConfigFileFoundState(true, true);
            SolutionName = solutionName;
            CommitedSolState = currentCommitedSolState;

            GetCurrentConfigFileInfo(currentSolutionConfigFileServiceInfo);
            GetCurrentConfigFileInfo(globalSolutionConfigFileServiceInfo);

            return new ConfigFilesData(configFileSolution, configFileGlobal, ParseError, IsFilesFound, solutionName, rootDir);
        }

        private static void GetCurrentConfigFileInfo(ConfigFileServiceInfo configFileServiceInfo)
        {
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
                }
                catch (Exception)
                {
                    // if syntax error in file actions
                    ErrorCasesHandle(configFileServiceInfo, false);
                }
            }
            else
            {
                //if file doesn't exist actions
                ErrorCasesHandle(configFileServiceInfo, true);
            }
        }

        private static void ErrorCasesHandle(ConfigFileServiceInfo configFileServiceInfo, bool isFileNotFound)
        {
            if (configFileServiceInfo.IsGlobal)
            {
                generateDefaultGlobalConfigFile();
                ParseError = (ParseError == FileParseError.None) ? FileParseError.Global : FileParseError.All;
                if (isFileNotFound) 
                    IsFilesFound.Global = false;
            }
            else
            {
                generateDefaultSolutionConfigFile();
                ParseError = FileParseError.Solution;
                if (isFileNotFound) 
                    IsFilesFound.Solution = false;
            }
        }

        /// <summary>
        /// Generates the default config file data for the solution config file. 
        /// This method is used when the solution config file doesn't exist, or when there are some errors during loading the backup file
        /// </summary>
        /// <returns>default ConfigFileSolutionDTO value</returns>
        private static void generateDefaultSolutionConfigFile()
        {
            configFileSolution = new ConfigFileSolutionDTO();
            configFileSolution.name = SolutionName;
            configFileSolution.framework_max_version = "-";
            configFileSolution.report_on_transit_references = false;
            configFileSolution.solution_required_references = new List<string>();
            configFileSolution.solution_unacceptable_references = new List<string>();
            configFileSolution.projects = new Dictionary<string, ConfigFileProjectDTO>();

            foreach (var projectName in CommitedSolState.Keys)
                configFileSolution.projects.Add(projectName, GenerateDefaultProjectConfigFile());
        }

        /// <summary>
        /// Generates the default config file data for the global config file.
        /// </summary>
        /// <returns>default ConfigFileGlobalDTO value</returns>
        private static void generateDefaultGlobalConfigFile()
        {
            configFileGlobal = new ConfigFileGlobalDTO();
            configFileGlobal.name = "Global";
            configFileGlobal.framework_max_version = "-";
            configFileGlobal.report_on_transit_references = false;
            configFileGlobal.global_required_references = new List<string>();
            configFileGlobal.global_unacceptable_references = new List<string>();
        }

        /// <summary>
        /// Generates the default config file data for the project in the solution config file.
        /// </summary>
        /// <returns>default ConfigFileProjectDTO value</returns>
        private static ConfigFileProjectDTO GenerateDefaultProjectConfigFile()
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