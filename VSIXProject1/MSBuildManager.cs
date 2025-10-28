using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Evaluation;

namespace VSIXProject1
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
