using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefDepGuard.Console.Models
{
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
