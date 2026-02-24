using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefDepGuard.Data.FrameworkVersion
{
    public class MaxFrameworkVersionConflictWarning
    {
        public ProblemLevel HighWarnLevel;
        public ProblemLevel LowWarnLevel;
        public string HighLevelMaxFrameVersion;
        public string LowLevelMaxFrameVersion;
        public string WarningRelevantProjectName;
        
        public MaxFrameworkVersionConflictWarning(ProblemLevel highWarnLevel, ProblemLevel lowWarnLevel, string highLevelMaxFrameVersion, string lowLevelMaxFrameVersion, string warningRelevantProjectName)
        {
            HighWarnLevel = highWarnLevel;
            LowWarnLevel = lowWarnLevel;
            HighLevelMaxFrameVersion = highLevelMaxFrameVersion;
            LowLevelMaxFrameVersion = lowLevelMaxFrameVersion;
            WarningRelevantProjectName = warningRelevantProjectName;
        }
    }
}
