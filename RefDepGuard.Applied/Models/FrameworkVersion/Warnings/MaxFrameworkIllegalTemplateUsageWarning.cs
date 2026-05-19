using RefDepGuard.Applied.Models.Problem;

namespace RefDepGuard.Applied.Models.FrameworkVersion.Warnings
{
    /// <summary>
    /// Shows a warning about illegal template usage inside max_fr_ver config file parameter
    /// </summary>
    public class MaxFrameworkIllegalTemplateUsageWarning
    {
        public ProblemLevel ProblemLevelInfo;

        public MaxFrameworkIllegalTemplateUsageWarning(ProblemLevel problemLevelInfo)
        {
            ProblemLevelInfo = problemLevelInfo;
        }
    }
}