using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject1.Models;

namespace VSIXProject1.Data.ConfigFile
{
    public class ConfigFilesData
    {
        public ConfigFileSolutionDTO configFileSolution;
        public ConfigFileGlobalDTO configFileGlobal;
        public FileParseError ParseError;

        public string solutionName;
        public string packageExtendedName;


        public ConfigFilesData(ConfigFileSolutionDTO configFileSolution, ConfigFileGlobalDTO configFileGlobal, FileParseError parseError, string solutionName, string packageExtendedName)
        {
            this.configFileSolution = configFileSolution;
            this.configFileGlobal = configFileGlobal;
            ParseError = parseError;
            this.solutionName = solutionName; //Nam-ы парсятся не из конфиг-файла
            this.packageExtendedName = packageExtendedName;
        }
    }
}
