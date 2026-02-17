using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1.Models.FrameworkVersion
{
    public class MaxFrameworkVersionDeviantValueWarning
    {
        public ProblemLevel WarningLevel;
        public string WarningRelevantProjectName;
        public string DeviantValue;

        public MaxFrameworkVersionDeviantValueWarning(ProblemLevel warningLevel, string warningRelevantProjectName, string deviantValue)
        {
            WarningLevel = warningLevel;
            WarningRelevantProjectName = warningRelevantProjectName;
            DeviantValue = deviantValue;
        }
    }
}
