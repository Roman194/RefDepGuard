using System;
using System.IO;

namespace RefDepGuard.Applied
{
    /// <summary>
    /// This class is responsible for managing the cache files of the extension. 
    /// It contains methods for saving and loading the backup of the config files data in json format.
    /// </summary>
    public class CacheManager
    {
        private static string configFilesBackupExtendedPackageName;
        private static string globalConfigBackupName;
        private static string solutionConfigBackupName;

        private static DirectoryInfo stuffDirInfo;
        private static DirectoryInfo cacheDirInfo;

        /// <summary>
        /// Sets the names of the cache files in the right format based on the solution name and package name.
        /// </summary>
        public static void SetSolutionNameInfoInRightFormat(string rootDir, string solutionName)
        {
            configFilesBackupExtendedPackageName = rootDir + "\\.rdg\\rdg_cache\\";
            globalConfigBackupName = configFilesBackupExtendedPackageName + "global_config_guard.rdg";
            solutionConfigBackupName = configFilesBackupExtendedPackageName + solutionName + "_config_guard.rdg";

            string rdgStuffDirectory = configFilesBackupExtendedPackageName.Substring(0, configFilesBackupExtendedPackageName.LastIndexOf('\\',
                configFilesBackupExtendedPackageName.LastIndexOf('\\') - 1));

            stuffDirInfo = new DirectoryInfo(rdgStuffDirectory);

            string rdgCacheDirectory = configFilesBackupExtendedPackageName.Substring(0, configFilesBackupExtendedPackageName.LastIndexOf('\\'));
            cacheDirInfo = new DirectoryInfo(rdgCacheDirectory);
            cacheDirInfo.Create();
        }

        /// <summary>
        /// Saves the backup of the config file data in json format to the cache file. If the cache file doesn't exist, it will be created.
        /// </summary>
        /// <param name="json">string data that will be saved</param>
        /// <param name="isGlobal">shows if it global config file or not</param>
        public static void UpdateConfigFilesBackup(string json, bool isGlobal)
        {
            if (!Directory.Exists(configFilesBackupExtendedPackageName))
            {
                stuffDirInfo.Create();
                stuffDirInfo.Attributes |= FileAttributes.Hidden;

                cacheDirInfo.Create();
            }

            FileStreamManager.WriteInfoToFile(GetCurrentBackupFileName(isGlobal), json);
        }

        /// <summary>
        /// Gets the backup of the config file data in json format from the cache file. If the cache file doesn't exist or is empty, it returns an empty string.
        /// </summary>
        /// <param name="isGlobal">shows if it global config file or not</param>
        /// <returns>empty string or string with the current file content</returns>
        public static string GetInfoFromBackupFile(bool isGlobal)
        {
            if (globalConfigBackupName == null || solutionConfigBackupName == null)
                return "";

            var currentBackupFileName = GetCurrentBackupFileName(isGlobal);

            if (File.Exists(currentBackupFileName))
            {
                string currentFileContent = FileStreamManager.ReadInfoFromFile(currentBackupFileName);

                if (String.IsNullOrEmpty(currentFileContent))
                    return "";

                return currentFileContent;
            }

            return "";
        }

        /// <summary>
        /// Gets the name of the cache file based on the isGlobal parameter. 
        /// If isGlobal is true, it returns the name of the global config backup file, otherwise - the name of the solution config backup file.
        /// </summary>
        /// <param name="isGlobal">shows if it global config file or not</param>
        /// <returns>globalConfigBackupName or solutionConfigBackupName string</returns>
        private static string GetCurrentBackupFileName(bool isGlobal)
        {
            return isGlobal ? globalConfigBackupName : solutionConfigBackupName;
        }
    }
}