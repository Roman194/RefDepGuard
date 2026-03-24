using Microsoft.Build.Evaluation;

namespace RefDepGuard.Console.Managers
{
    public class MSBuildManager
    {
        public static string GetTargetFrameworkForProject(Project currentProject)
        {
            string targetFramework = currentProject.GetPropertyValue("TargetFramework");

            if (!string.IsNullOrEmpty(targetFramework))
            {
                return targetFramework;
            }

            string targetFrameworks = currentProject.GetPropertyValue("TargetFrameworks");

            if (!string.IsNullOrEmpty(targetFrameworks))
            {
                return targetFrameworks;
            }

            string targetFrameworkVersion = currentProject.GetPropertyValue("TargetFrameworkVersion");

            if (!string.IsNullOrEmpty(targetFrameworkVersion))
            {
                return targetFrameworkVersion;
            }
            
            return "";
        }
    }
}
