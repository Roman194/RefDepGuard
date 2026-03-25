using RefDepGuard.Applied.Models.DTO;
using RefDepGuard.Applied.Models;

using RefDepGuard.ConfigFile;
using RefDepGuard.Applied.Models.ConfigFile;

namespace RefDepGuard.Console.Managers
{
    public class ConfigFileConsoleManager
    {

        public static ConfigFilesData GetInfoFromConfigFiles(string rootDirectory, string solutionName, Dictionary<string, ProjectState> currentSolState)
        {
            string solutionConfigGuardFile = rootDirectory + "\\" + solutionName + "_config_guard.rdg";
            string globalConfigGuardFile = rootDirectory + "\\global_config_guard.rdg";

            string solutionConfigGuardRollbackFile = rootDirectory + "\\"+ solutionName + "_config_guard_rollback.rdg";
            string globalConfigGuardRollbackFile = rootDirectory + "\\global_config_guard_rollback.rdg";

            var currentSolutionConfigFileServiceInfo = new ConfigFileServiceInfo(false, solutionConfigGuardFile, solutionConfigGuardRollbackFile);
            var globalSolutionConfigFileServiceInfo = new ConfigFileServiceInfo(true, globalConfigGuardFile, globalConfigGuardRollbackFile);


            return ConfigFileCoreManager.GetInfoFromConfigFiles(currentSolutionConfigFileServiceInfo, globalSolutionConfigFileServiceInfo, solutionName, rootDirectory,
                currentSolState);
        }
    }
}