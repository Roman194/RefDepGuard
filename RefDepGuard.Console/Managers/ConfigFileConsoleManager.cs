using RefDepGuard.ConfigFile;
using RefDepGuard.Applied.Models.ConfigFile;
using RefDepGuard.Applied.Models.Project;

namespace RefDepGuard.Console.Managers
{
    /// <summary>
    /// This class is responsible for managing the configuration files of the solution and global configuration. 
    /// It provides methods to read and write the configuration files, as well as to get the information from the configuration files and return 
    /// it in a structured way.
    /// </summary>
    public class ConfigFileConsoleManager
    {
        /// <summary>
        /// The main method of the class. It gets the information from the configuration files and returns it in a structured way.
        /// </summary>
        /// <param name="rootDirectory">root directory string</param>
        /// <param name="solutionName">solution name string</param>
        /// <param name="currentSolState">curr solution state dict</param>
        /// <returns>parsed config files data</returns>
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