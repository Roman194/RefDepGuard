using RefDepGuard.Models;

namespace RefDepGuard.Data.ConfigFile
{
    public class ConfigFilesData
    {
        public ConfigFileSolutionDTO ConfigFileSolution;
        public ConfigFileGlobalDTO ConfigFileGlobal;
        public FileParseError ParseError;

        public string SolutionName;
        public string PackageExtendedName;


        public ConfigFilesData(ConfigFileSolutionDTO configFileSolution, ConfigFileGlobalDTO configFileGlobal, FileParseError parseError, string solutionName, string packageExtendedName)
        {
            ConfigFileSolution = configFileSolution;
            ConfigFileGlobal = configFileGlobal;
            ParseError = parseError;
            SolutionName = solutionName; //Nam-ы парсятся не из конфиг-файла
            PackageExtendedName = packageExtendedName;
        }
    }
}
