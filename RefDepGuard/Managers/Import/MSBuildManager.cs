using Microsoft.Build.Evaluation;

namespace RefDepGuard
{
    /// <summary>
    /// This class is responsible for managing the MSBuild projects of the solution.
    /// </summary>
    public class MSBuildManager
    {
        /// <summary>
        /// Gets the target framework(s) for the given project. 
        /// It first tries to get the "TargetFramework" property, then "TargetFrameworks", and finally "TargetFrameworkVersion".
        /// </summary>
        /// <param name="projectName">project name string which TF is needed</param>
        /// <returns>string with "TargetFramework(-s)" or its version or empty string</returns>
        public static string GetTargetFrameworkForProject(string projectName)
        {
            var projectCollection = new ProjectCollection();
            var currentProject = projectCollection.LoadProject(projectName);

            if (currentProject != null)
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
            }

            return "";
        }
    }
}
