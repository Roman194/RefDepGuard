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
        public bool isParseError;

        public string solutionName;
        public string packageExtendedName;


        public ConfigFilesData(ConfigFileSolution configFileSolution, ConfigFileGlobal configFileGlobal, bool isParseError, string solutionName, string packageExtendedName)
        {
            this.configFileSolution = configFileSolution;
            this.configFileGlobal = configFileGlobal;
            this.isParseError = isParseError;
            this.solutionName = solutionName; //Nam-ы парсятся не из конфиг-файла
            this.packageExtendedName = packageExtendedName;
        }
    }
}
