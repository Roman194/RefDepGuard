using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1.Data
{
    public class ConfigFileServiceInfo
    {
        public bool IsGlobal;
        public string SolutionConfigGuardFile;
        public string SolutionConfigGuardRollbackFile;
        public FileErrorMessage FileErrorMessage;

        public ConfigFileServiceInfo(bool isGlobal, string solutionConfigGuardFile, string solutionConfigGuardRollbackFile, FileErrorMessage fileErrorMessage) {
            IsGlobal = isGlobal;
            SolutionConfigGuardFile = solutionConfigGuardFile;  
            SolutionConfigGuardRollbackFile = solutionConfigGuardRollbackFile;
            FileErrorMessage = fileErrorMessage;
        }
    }
}
