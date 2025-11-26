using System.Collections.Generic;
using VSIXProject1.Data.ConfigFile;
using VSIXProject1.Data.Reference;

namespace VSIXProject1.Managers.CheckRules
{
    public class NotNullChecksSubManager
    {
        static List<ConfigFilePropertyNullError> configPropertyNullErrorList = new List<ConfigFilePropertyNullError>();

        public static void ClearConfigPropertyNullErrorList()
        {
            if (configPropertyNullErrorList != null)
                configPropertyNullErrorList.Clear();
        }
        public static List<ConfigFilePropertyNullError> CheckConfigPropertiesOnNotNull(ConfigFilesData configFilesData)
        {
            ConfigFileGlobal configFileGlobal = configFilesData.configFileGlobal;
            ConfigFileSolution configFileSolution = configFilesData.configFileSolution;

            if (configFileSolution != null)
            {
                CheckConfigFileSolutionProperties(configFileSolution);

                if (configFileSolution.projects != null)
                {
                    foreach (var project in configFileSolution.projects)
                    {
                        if (project.Value != null)
                            CheckConfigFileProjectProperties(project.Key, project.Value);

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
                CheckConfigFileGlobalProperties(configFileGlobal);
            else
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("Global", true, ""));

            return configPropertyNullErrorList;
        }

        private static void CheckConfigFileSolutionProperties(ConfigFileSolution configFileSolution) 
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

        private static void CheckConfigFileProjectProperties(string projectKey, ConfigFileProject currentProject)
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

        private static void CheckConfigFileGlobalProperties(ConfigFileGlobal configFileGlobal)
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
