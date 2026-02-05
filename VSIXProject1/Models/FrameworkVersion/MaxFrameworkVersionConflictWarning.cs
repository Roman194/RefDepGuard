using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1.Data.FrameworkVersion
{
    public class MaxFrameworkVersionConflictWarning
    {
        public ErrorLevel HighErrorLevel;
        public ErrorLevel LowErrorLevel;
        public string HighLevelMaxFrameVersion;
        public string LowLevelMaxFrameVersion;
        public string WarningRelevantProjectName;
        
        public MaxFrameworkVersionConflictWarning(ErrorLevel highErrorLevel, ErrorLevel lowErrorLevel, string highLevelMaxFrameVersion, string lowLevelMaxFrameVersion, string warningRelevantProjectName)
        {
            HighErrorLevel = highErrorLevel;
            LowErrorLevel = lowErrorLevel;
            HighLevelMaxFrameVersion = highLevelMaxFrameVersion;
            LowLevelMaxFrameVersion = lowLevelMaxFrameVersion;
            WarningRelevantProjectName = warningRelevantProjectName;
        }
    }
}
