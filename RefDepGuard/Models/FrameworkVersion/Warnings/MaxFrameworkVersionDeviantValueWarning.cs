
namespace RefDepGuard.Models.FrameworkVersion
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
