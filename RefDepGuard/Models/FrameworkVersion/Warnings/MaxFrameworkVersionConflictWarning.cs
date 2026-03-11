
namespace RefDepGuard.Data.FrameworkVersion
{
    /// <summary>
    /// Shows a warning when max_fr_ver parameters on a differnet levels have a conflicts
    /// </summary>
    public class MaxFrameworkVersionConflictWarning
    {
        public ProblemLevel HighWarnLevel;
        public ProblemLevel LowWarnLevel;
        public string HighLevelMaxFrameVersion;
        public string LowLevelMaxFrameVersion;
        public string WarningRelevantProjectName;

        /// <param name="highWarnLevel">relevant "higher" max_fr_ver level</param>
        /// <param name="lowWarnLevel">relevant "lower" max_fr_ver level</param>
        /// <param name="highLevelMaxFrameVersion">"higher" level max_fr_ver string</param>
        /// <param name="lowLevelMaxFrameVersion">"lower" level max_fr_ver string</param>
        /// <param name="warningRelevantProjectName">relevant proj name string (if "lower" is a project level)</param>
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