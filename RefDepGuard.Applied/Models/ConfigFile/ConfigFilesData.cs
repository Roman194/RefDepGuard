using System;
using System.Collections.Generic;
using System.Text;
using RefDepGuard.Applied.Models.ConfigFile.DTO;

namespace RefDepGuard.Applied.Models.ConfigFile
{
    /// <summary>
    /// It's a model that incapsulate all important data from the ConfigFileManager for the other parts of the program
    /// </summary>
    public class ConfigFilesData
    {
        public ConfigFileSolutionDTO ConfigFileSolution;
        public ConfigFileGlobalDTO ConfigFileGlobal;
        public FileParseError ParseError;
        public ConfigFileFoundState IsFilesFound;

        public string SolutionName;
        public string PackageExtendedName;

        /// <param name="configFileSolution">ConfigFileSolutionDTO value</param>
        /// <param name="configFileGlobal">ConfigFileGlobalDTO value</param>
        /// <param name="parseError">FileParseError value</param>
        /// <param name="solutionName">solution name string</param>
        /// <param name="packageExtendedName">package extended name string</param>
        public ConfigFilesData(ConfigFileSolutionDTO configFileSolution, ConfigFileGlobalDTO configFileGlobal, FileParseError parseError,
            ConfigFileFoundState isFilesFound, string solutionName, string packageExtendedName)
        {
            ConfigFileSolution = configFileSolution;
            ConfigFileGlobal = configFileGlobal;
            ParseError = parseError;
            IsFilesFound = isFilesFound;
            SolutionName = solutionName; //Important: names are not parsed from the ConfigFileManager, but still it's easier to transfer them inside ConfigFilesData
            PackageExtendedName = packageExtendedName;
        }
    }
}