using Microsoft.Build.Evaluation;
using System.Collections.Generic;
using System.Linq;

namespace RefDepGuard
{
    public class MSBuildManager
    {
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
