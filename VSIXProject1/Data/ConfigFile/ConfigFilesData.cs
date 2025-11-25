using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1.Data.ConfigFile
{
    public class ConfigFilesData
    {
        public ConfigFileSolution configFileSolution;
        public ConfigFileGlobal configFileGlobal;

        public string solutionName;
        public string packageExtendedName;

        public ConfigFilesData(ConfigFileSolution configFileSolution, ConfigFileGlobal configFileGlobal, string solutionName, string packageExtendedName)
        {
            this.configFileSolution = configFileSolution;
            this.configFileGlobal = configFileGlobal;
            this.solutionName = solutionName;
            this.packageExtendedName = packageExtendedName;
        }
    }
}
