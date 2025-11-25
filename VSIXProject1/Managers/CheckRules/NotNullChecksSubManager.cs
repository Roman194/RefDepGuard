using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject1.Data.ConfigFile;
using VSIXProject1.Data.Reference;

namespace VSIXProject1.Managers.CheckRules
{
    public class NotNullChecksSubManager
    {
        public static void CheckConfigPropertiesOnNotNull(ConfigFilesData configFilesData, List<ConfigFilePropertyNullError> configPropertyNullErrorList)
        {
            ConfigFileGlobal configFileGlobal = configFilesData.configFileGlobal;
            ConfigFileSolution configFileSolution = configFilesData.configFileSolution;

            if (configFileSolution != null)
            {
                CheckConfigFileSolutionProperties(configFileSolution, configPropertyNullErrorList);

                if (configFileSolution.projects != null)
                {
                    foreach (var project in configFileSolution.projects)
                    {
                        if (project.Value != null)
                            CheckConfigFileProjectProperties(project.Key, project.Value, configPropertyNullErrorList);

                        else
                            configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("project_value", false, project.Key));
                    }
                }
                else
                    configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("projects", false, ""));
            }
            else
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError(configFilesData.solutionName, false, ""));


            if (configFileGlobal != null)
                CheckConfigFileGlobalProperties(configFileGlobal, configPropertyNullErrorList);
            else
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("Global", true, ""));
        }

        private static void CheckConfigFileSolutionProperties(ConfigFileSolution configFileSolution, List<ConfigFilePropertyNullError> configPropertyNullErrorList) 
            //How to make it better? Reflection doesn't work
        {
            if (configFileSolution.name is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("name", false, ""));

            if (configFileSolution.framework_max_version is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("framework_max_version", false, ""));

            if (configFileSolution.solution_required_references is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("solution_required_references", false, ""));

            if (configFileSolution.solution_unacceptable_references is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("solution_unacceptable_references", false, ""));
        }

        private static void CheckConfigFileProjectProperties(string projectKey, ConfigFileProject currentProject, List<ConfigFilePropertyNullError> configPropertyNullErrorList)
        {
            if (currentProject.framework_max_version is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("framework_max_version", false, projectKey));

            if (currentProject.consider_global_and_solution_references is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("consider_global_and_solution_references", false, projectKey));

            if (currentProject.required_references is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("required_references", false, projectKey));

            if (currentProject.unacceptable_references is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("unacceptable_references", false, projectKey));
        }

        private static void CheckConfigFileGlobalProperties(ConfigFileGlobal configFileGlobal, List<ConfigFilePropertyNullError> configPropertyNullErrorList)
        {
            if (configFileGlobal.name is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("name", true, ""));

            if (configFileGlobal.framework_max_version is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("framework_max_version", true, ""));

            if (configFileGlobal.global_required_references is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("global_required_references", true, ""));

            if (configFileGlobal.global_unacceptable_references is null)
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("global_unacceptable_references", true, ""));
        }
    }
}
