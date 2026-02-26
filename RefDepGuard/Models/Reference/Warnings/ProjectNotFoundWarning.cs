
namespace RefDepGuard.Models.Reference
{
    public class ProjectNotFoundWarning
    {
        public string ReferenceName;
        public ProblemLevel WarningLevel;
        public string ProjName;

        public ProjectNotFoundWarning(string referenceName, ProblemLevel warningLevel, string projName)
        {
            ReferenceName = referenceName;
            WarningLevel = warningLevel;
            ProjName = projName;
        }
    }
}
