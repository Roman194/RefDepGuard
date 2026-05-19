
namespace RefDepGuard.Applied.Models.Project
{
    /// <summary>
    /// Shows the warning about the project name semantic. 
    /// It helps to highlight when the project name doesn't match the expected semantic that is floows from divided semantic rules.
    /// </summary>
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