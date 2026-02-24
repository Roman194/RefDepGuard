using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefDepGuard.Data
{
    public class ConfigFileServiceInfo
    {
        public bool IsGlobal;
        public string SolutionConfigGuardFile;
        public string SolutionConfigGuardRollbackFile;

        public ConfigFileServiceInfo(bool isGlobal, string solutionConfigGuardFile, string solutionConfigGuardRollbackFile) {
            IsGlobal = isGlobal;
            SolutionConfigGuardFile = solutionConfigGuardFile;  
            SolutionConfigGuardRollbackFile = solutionConfigGuardRollbackFile;
        }
    }
}
