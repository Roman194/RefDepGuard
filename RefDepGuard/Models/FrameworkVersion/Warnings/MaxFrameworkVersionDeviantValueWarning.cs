
namespace RefDepGuard.Models.FrameworkVersion
{
    /// <summary>
    /// Shows a warning about max_fr_ver parameter deviant value
    /// </summary>
    public class MaxFrameworkVersionDeviantValueWarning
    {
        public ProblemLevel WarningLevel;
        public string WarningRelevantProjectName;
        public string DeviantValue;

        /// <param name="warningLevel">relevant warning level</param>
        /// <param name="warningRelevantProjectName">rel proj name string</param>
        /// <param name="deviantValue">deviant value string</param>
        public MaxFrameworkVersionDeviantValueWarning(ProblemLevel warningLevel, string warningRelevantProjectName, string deviantValue)
        {
            WarningLevel = warningLevel;
            WarningRelevantProjectName = warningRelevantProjectName;
            DeviantValue = deviantValue;
        }
    }
}