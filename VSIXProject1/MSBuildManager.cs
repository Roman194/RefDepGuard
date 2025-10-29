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

                string [] targetFrameworks = currentProject.GetPropertyValue("TargetFrameworks").Split(';');

                if (!string.IsNullOrEmpty(targetFrameworks[0])) //На текущий момент работа с несколькими таргетами для одного проекта не рассматривается!
                {
                    return targetFrameworks[0];
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
