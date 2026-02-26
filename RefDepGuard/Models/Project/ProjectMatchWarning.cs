
namespace RefDepGuard.Models
{
    public class ProjectMatchWarning
    {
        public string ProjName;
        public bool IsNoProjectInConfigFile;

        public ProjectMatchWarning(string projName,  bool isNoProjectInConfigFile)
        {
            ProjName = projName;
            IsNoProjectInConfigFile = isNoProjectInConfigFile;
        }
    }
}
