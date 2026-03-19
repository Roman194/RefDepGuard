using RefDepGuard.Console.Models;
using Newtonsoft.Json;

namespace RefDepGuard.Console.Managers
{
    public class ConfigFileConsoleManager
    {
        private static ConfigFileSolutionDTO configFileSolution;
        private static ConfigFileGlobalDTO configFileGlobal;
        private static FileParseError parseError;

        private static ConfigFileServiceInfo currentSolutionConfigFileServiceInfo;
        private static ConfigFileServiceInfo globalSolutionConfigFileServiceInfo;


        
        public static ConfigFilesData GetInfoFromConfigFiles(string rootDirectory, string solutionName)
        {
            string solutionConfigGuardFile = rootDirectory + "\\" + solutionName + "_config_guard.rdg";
            string globalConfigGuardFile = rootDirectory + "\\global_config_guard.rdg";

            string solutionConfigGuardRollbackFile = rootDirectory + "\\"+ solutionName + "_config_guard_rollback.rdg";
            string globalConfigGuardRollbackFile = rootDirectory + "\\global_config_guard_rollback.rdg";

            currentSolutionConfigFileServiceInfo = new ConfigFileServiceInfo(false, solutionConfigGuardFile, solutionConfigGuardRollbackFile);
            globalSolutionConfigFileServiceInfo = new ConfigFileServiceInfo(true, globalConfigGuardFile, globalConfigGuardRollbackFile);

            parseError = FileParseError.None;

            GetCurrentConfigFileInfo(currentSolutionConfigFileServiceInfo);
            GetCurrentConfigFileInfo(globalSolutionConfigFileServiceInfo);

            return new ConfigFilesData(configFileSolution, configFileGlobal, parseError, solutionName, rootDirectory);
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
                    if (configFileServiceInfo.IsGlobal)
                        parseError = (parseError == FileParseError.Solution) ? FileParseError.All : FileParseError.Global;
                    else
                        parseError = FileParseError.Solution;
                }
            }
            else
            {
                //if file doesn't exist actions
                if (configFileServiceInfo.IsGlobal)
                    parseError = (parseError == FileParseError.Solution) ? FileParseError.All : FileParseError.Global;
                else
                    parseError = FileParseError.Solution;
            }
        }
    }
}
