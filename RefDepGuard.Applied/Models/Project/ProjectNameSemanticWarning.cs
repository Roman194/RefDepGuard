
namespace RefDepGuard.Applied.Models.Project
{
    public class ProjectNameSemanticWarning
    {
        public string ProjectName;
        public string ExpectedSema;
        public string FindedSema;

        public ProjectNameSemanticWarning(string projectName, string expectedSema, string findedSema)
        {
            ProjectName = projectName;
            ExpectedSema = expectedSema;
            FindedSema = findedSema;
        }
    }
}