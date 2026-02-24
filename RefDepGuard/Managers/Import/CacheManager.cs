using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RefDepGuard.Managers.Applied;
using RefDepGuard.Models;

namespace RefDepGuard.Managers.Import
{
    public class CacheManager
    {
        private static string configFilesBackupExtendedPackageName;
        private static string globalConfigBackupName;
        private static string solutionConfigBackupName;

        public static void UpdateConfigFilesBackup(string json, bool isGlobal)
        {
            configFilesBackupExtendedPackageName = SolutionNameManager.GetPackageName() + "\\.rdg\\rdg_cache\\";
            globalConfigBackupName = configFilesBackupExtendedPackageName + "global_config_guard.rdg";
            solutionConfigBackupName = configFilesBackupExtendedPackageName + SolutionNameManager.GetSolutionName() + "_config_guard.rdg";

            string rdgStuffDirectory = configFilesBackupExtendedPackageName.Substring(0, configFilesBackupExtendedPackageName.LastIndexOf('\\',
                configFilesBackupExtendedPackageName.LastIndexOf('\\') - 1));

            var dirInfo = new DirectoryInfo(rdgStuffDirectory);
            dirInfo.Create();
            dirInfo.Attributes |= FileAttributes.Hidden;

            string rdgCacheDirectory = configFilesBackupExtendedPackageName.Substring(0, configFilesBackupExtendedPackageName.LastIndexOf('\\'));
            var dirCacheInfo = new DirectoryInfo(rdgCacheDirectory);
            dirCacheInfo.Create();

            using (FileStream fileStream = File.Create(isGlobal? globalConfigBackupName : solutionConfigBackupName))
            {
                StreamWriter streamWriter = new StreamWriter(fileStream);

                streamWriter.Write(json);

                streamWriter.Flush();
                fileStream.Flush();

                streamWriter.Close();
            }
        }

        public static string GetInfoFromBackupFile(bool isGlobal)
        {
            if (globalConfigBackupName == null || solutionConfigBackupName == null)
                return "";

            var currentBackupFileName = isGlobal ? globalConfigBackupName : solutionConfigBackupName;

            if (File.Exists(currentBackupFileName))
            {
                using (FileStream fileStream = new FileStream(currentBackupFileName, FileMode.Open))
                {
                    StreamReader sr = new StreamReader(fileStream);

                    string currentFileContent = sr.ReadToEnd();
                    if (String.IsNullOrEmpty(currentFileContent))
                        return "";

                    return currentFileContent;
                }
            }

            return "";
        }


    }
}
