using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1.Models.FrameworkVersion
{
    public class MaxFrameworkVersionTFMNotFoundWarning
    {
        public string TFMName;
        public ErrorLevel WarningLevel;
        public string ProjName;

        public MaxFrameworkVersionTFMNotFoundWarning(string tFMName,  ErrorLevel warningLevel, string projName)
        {
            TFMName = tFMName;
            WarningLevel = warningLevel;
            ProjName = projName;
        }
    }
}
