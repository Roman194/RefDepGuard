
namespace RefDepGuard.Data.Reference
{
    public class RequiredReference
    {
        public string ReferenceName;
        public string RelevantProject;

        public RequiredReference(string referenceName, string relevantProject)
        {
            ReferenceName = referenceName;
            RelevantProject = relevantProject;
        }
    }
}
