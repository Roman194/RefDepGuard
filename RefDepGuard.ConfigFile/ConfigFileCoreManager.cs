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
    /// <summary>
    /// This class is responsible for managing the configuration files of the solution and global settings. 
    /// It provides methods to read and parse the configuration files, handle errors during parsing, and generate default configuration data when necessary. 
    /// The class also maintains the state of the configuration files, including whether they were found and if there were any parsing errors. 
    /// It serves as a central point for accessing and managing the configuration data used by the application.
    /// </summary>
    public class ConfigFileCoreManager
    {
        private static ConfigFileSolutionDTO configFileSolution;
        private static ConfigFileGlobalDTO configFileGlobal;
        private static FileParseError ParseError;
        private static ConfigFileFoundState IsFilesFound;
        private static string SolutionName;
        private static string RootDir;

        private static Dictionary<string, ProjectState> CommitedSolState;
        private static ConfigFilesData ConfigFilesData;

        /// <summary>
        /// The main method of the ConfigFileCoreManager. 
        /// It reads and parses the configuration files for both the solution and global settings, handles any errors that may occur during parsing, 
        /// and generates default configuration data if necessary for the extension callings.
        /// </summary>
        /// <param name="currentSolutionConfigFileServiceInfo">solution config file service info</param>
        /// <param name="globalSolutionConfigFileServiceInfo">global config file service info</param>
        /// <param name="solutionName">solution name string</param>
        /// <param name="rootDir">root directory string</param>
        /// <param name="currentCommitedSolState">commited solution state dictionary</param>
        /// <returns>a truple of parsed config files data and their state</returns>
        public static Tuple<ConfigFilesData, ConfigFileFoundState> GetInfoFromConfigFilesForExtension(
            ConfigFileServiceInfo currentSolutionConfigFileServiceInfo, ConfigFileServiceInfo globalSolutionConfigFileServiceInfo, string solutionName, string rootDir,
            Dictionary<string, ProjectState> currentCommitedSolState)
        {
            GetInfoFromConfigFiles(currentSolutionConfigFileServiceInfo, globalSolutionConfigFileServiceInfo, solutionName, rootDir, currentCommitedSolState, true);

            return new Tuple<ConfigFilesData, ConfigFileFoundState>(ConfigFilesData, IsFilesFound);
        }

        /// <summary>
        /// The main method of the ConfigFileCoreManager.
        /// It reads and parses the configuration files for both the solution and global settings, handles any errors that may occur during parsing, 
        /// and generates default configuration data if necessary for the console callings.
        /// </summary>
        /// <param name="currentSolutionConfigFileServiceInfo">solution config file service info</param>
        /// <param name="globalSolutionConfigFileServiceInfo">global config file service info</param>
        /// <param name="solutionName">solution name string</param>
        /// <param name="rootDir">root directory string</param>
        /// <param name="currentCommitedSolState">commited solution state dictionary</param>
        /// <param name="isExtentionCall">shows if its extension or console call</param>
        /// <returns>a parsed config files data state</returns>
        public static ConfigFilesData GetInfoFromConfigFiles(
            ConfigFileServiceInfo currentSolutionConfigFileServiceInfo, ConfigFileServiceInfo globalSolutionConfigFileServiceInfo, string solutionName, string rootDir,
            Dictionary<string, ProjectState> currentCommitedSolState, bool isExtentionCall = false
            )
        {
            ParseError = FileParseError.None;
            IsFilesFound = new ConfigFileFoundState(true, true);
            SolutionName = solutionName;
            RootDir = rootDir;
            CommitedSolState = currentCommitedSolState;

            GetCurrentConfigFileInfo(currentSolutionConfigFileServiceInfo, isExtentionCall);
            GetCurrentConfigFileInfo(globalSolutionConfigFileServiceInfo, isExtentionCall);

            ConfigFilesData = new ConfigFilesData(configFileSolution, configFileGlobal, ParseError, SolutionName, RootDir);

            return ConfigFilesData;
        }

        /// <summary>
        /// This method is used for the second attempt of reading the config files, when there were some errors during the first attempt. Is calling only for
        /// extension calls
        /// </summary>
        /// <param name="configFileServiceInfo">current config file service info</param>
        /// <param name="parseErrorPredict">file parse error value</param>
        /// <returns>a parsed config files data state</returns>
        public static ConfigFilesData GetConfigFileInfoSecondAttempt(ConfigFileServiceInfo configFileServiceInfo, FileParseError parseErrorPredict)
        {
            ParseError = parseErrorPredict;
            GetCurrentConfigFileInfo(configFileServiceInfo, true);

            ConfigFilesData = new ConfigFilesData(configFileSolution, configFileGlobal, ParseError, SolutionName, RootDir);
            return ConfigFilesData;
        }

        /// <summary>
        /// This method is used for updating the parsing error state when the default config file is created. 
        /// It is called from the extension after creating the default config file
        /// </summary>
        /// <param name="isGlobal">shows if its global or solution config file</param>
        /// <returns>a parsed config files data state</returns>
        public static ConfigFilesData UpdateParseErrorStateOnDefaultFileCreation(bool isGlobal)
        {
            if(isGlobal)
                ParseError = (ConfigFilesData.ParseError == FileParseError.All) ? FileParseError.Solution : FileParseError.None;
            else
                ParseError = (ConfigFilesData.ParseError == FileParseError.All) ? FileParseError.Global : FileParseError.None;

            ConfigFilesData.ParseError = ParseError;
            return ConfigFilesData;
        }

        /// <summary>
        /// This method reads and parses the configuration file based on the provided service info. 
        /// If the file exists, it attempts to read its content and deserialize it into the appropriate DTO object.
        /// </summary>
        /// <param name="configFileServiceInfo">config file service info instance</param>
        /// <param name="isExtentionCall">shows if its extension or console call</param>
        private static void GetCurrentConfigFileInfo(ConfigFileServiceInfo configFileServiceInfo, bool isExtentionCall)
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

                    //It's considering that at this moment files with JSON syntax errors are already in catch, and Null value parameters can't be backed up anymore
                    if (isExtentionCall)
                        CacheManager.UpdateConfigFilesBackup(currentFileContent, configFileServiceInfo.IsGlobal);
                }
                catch (Exception ex)
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

        /// <summary>
        /// This method handles the error cases during reading and parsing the configuration files. 
        /// It is called when there are exceptions during file reading or deserialization, as well as when the file is not found.
        /// </summary>
        /// <param name="configFileServiceInfo">config file service info instance</param>
        /// <param name="isFileNotFound">shows if file not found or has invalid sintax</param>
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
                ParseError = (ParseError == FileParseError.None) ? FileParseError.Solution : FileParseError.All;
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
            configFileSolution.project_names_semantic_check = true;
            configFileSolution.report_on_transit_references = true;
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
            configFileGlobal.project_names_semantic_check = true;
            configFileGlobal.report_on_transit_references = true;
            configFileGlobal.global_required_references = new List<string>();
            configFileGlobal.global_unacceptable_references = new List<string>();
        }

        /// <summary>
        /// Generates the default config file data for the project in the solution config file.
        /// </summary>
        /// <returns>default ConfigFileProjectDTO value</returns>
        public static ConfigFileProjectDTO GenerateDefaultProjectConfigFile()
        {
            ConfigFileProjectRefsConsideringDTO configFileProjectRefsConsidering = new ConfigFileProjectRefsConsideringDTO();
            configFileProjectRefsConsidering.required = true;
            configFileProjectRefsConsidering.unacceptable = true;

            ConfigFileProjectDTO fileProject = new ConfigFileProjectDTO();
            fileProject.framework_max_version = "-";
            fileProject.report_on_transit_references = true;
            fileProject.consider_global_and_solution_references = configFileProjectRefsConsidering;
            fileProject.required_references = new List<string>();
            fileProject.unacceptable_references = new List<string>();

            return fileProject;
        }
    }
}