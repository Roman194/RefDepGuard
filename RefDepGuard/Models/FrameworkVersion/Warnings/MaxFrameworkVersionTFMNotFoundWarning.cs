
namespace RefDepGuard.Models.FrameworkVersion
{
    /// <summary>
    /// Shows a warning when there is an unfamiliar TFM in max_fr_ver parameter
    /// </summary>
    public class MaxFrameworkVersionTFMNotFoundWarning
    {
        public string TFMName;
        public ProblemLevel WarningLevel;
        public string ProjName;

        /// <param name="tFMName">TFM name string</param>
        /// <param name="warningLevel">relevant warning level</param>
        /// <param name="projName">relevant project name string</param>
        public MaxFrameworkVersionTFMNotFoundWarning(string tFMName,  ProblemLevel warningLevel, string projName)
        {
            TFMName = tFMName;
            WarningLevel = warningLevel;
            ProjName = projName;
        }
    }
}