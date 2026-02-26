using System;
using System.IO;
using RefDepGuard.Managers.Applied;

namespace RefDepGuard.Managers.Import
{
    public class CacheManager
    {
        private static string configFilesBackupExtendedPackageName;
        private static string globalConfigBackupName;
        private static string solutionConfigBackupName;

        private static DirectoryInfo stuffDirInfo;
        private static DirectoryInfo cacheDirInfo;

        static CacheManager()
        {
            configFilesBackupExtendedPackageName = SolutionNameManager.GetPackageName() + "\\.rdg\\rdg_cache\\";
            globalConfigBackupName = configFilesBackupExtendedPackageName + "global_config_guard.rdg";
            solutionConfigBackupName = configFilesBackupExtendedPackageName + SolutionNameManager.GetSolutionName() + "_config_guard.rdg";

            string rdgStuffDirectory = configFilesBackupExtendedPackageName.Substring(0, configFilesBackupExtendedPackageName.LastIndexOf('\\',
                configFilesBackupExtendedPackageName.LastIndexOf('\\') - 1));

            stuffDirInfo = new DirectoryInfo(rdgStuffDirectory);

            string rdgCacheDirectory = configFilesBackupExtendedPackageName.Substring(0, configFilesBackupExtendedPackageName.LastIndexOf('\\'));
            cacheDirInfo = new DirectoryInfo(rdgCacheDirectory);
            cacheDirInfo.Create();
        }

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

        public static string GetInfoFromBackupFile(bool isGlobal)
        {
            if (globalConfigBackupName == null || solutionConfigBackupName == null)
                return "";

            var currentBackupFileName = GetCurrentBackupFileName(isGlobal);

            if (File.Exists(currentBackupFileName))
            {
                string currentFileContent  = FileStreamManager.ReadInfoFromFile(currentBackupFileName);

                if (String.IsNullOrEmpty(currentFileContent))
                    return "";

                return currentFileContent;
            }

            return "";
        }

        private static string GetCurrentBackupFileName(bool isGlobal)
        {
            return isGlobal ? globalConfigBackupName : solutionConfigBackupName;
        }
    }
}
