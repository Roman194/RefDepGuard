using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.CheckRules.Models.ConfigFile
{
    /// <summary>
    /// It's a model that shows a service info to the detection of the current config file and its rollback
    /// </summary>
    public class ConfigFileServiceInfo
    {
        public bool IsGlobal;
        public string CurrentConfigGuardFile;
        public string CurrentConfigGuardRollbackFile;

        /// <param name="isGlobal">shows if its a global or solution config file</param>
        /// <param name="currentConfigGuardFile">curr config file string</param>
        /// <param name="currentConfigGuardRollbackFile">curr rollback config file string</param>
        public ConfigFileServiceInfo(bool isGlobal, string currentConfigGuardFile, string currentConfigGuardRollbackFile)
        {
            IsGlobal = isGlobal;
            CurrentConfigGuardFile = currentConfigGuardFile;
            CurrentConfigGuardRollbackFile = currentConfigGuardRollbackFile;
        }
    }
}