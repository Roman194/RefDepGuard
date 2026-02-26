
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
