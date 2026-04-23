using RefDepGuard.Applied.Models.Problem;

namespace RefDepGuard.Applied.Models.FrameworkVersion.Warnings
{
    public class MaxFrameworkIllegalTemplateUsageWarning
    {
        public ProblemLevel ProblemLevelInfo;

        public MaxFrameworkIllegalTemplateUsageWarning(ProblemLevel problemLevelInfo)
        {
            ProblemLevelInfo = problemLevelInfo;
        }
    }
}