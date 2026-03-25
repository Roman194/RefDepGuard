using RefDepGuard.Applied.Models.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using RefDepGuard.Applied.Models.ConfigFile;
using RefDepGuard.CheckRules.Models;

namespace RefDepGuard.CheckRules.SubManagers
{
    /// <summary>
    /// This class is responsible for checking if the properties of the configuration files are not null. 
    /// It checks both the global and solution configuration files, as well as the projects within the solution configuration file. 
    /// If any property is found to be null, it adds an error to the list of ConfigFilePropertyNullError, which is used to inform the user about missing or 
    /// incomplete configuration data.
    /// </summary>
    public class NotNullChecksSubManager
    {
        private static List<ConfigFilePropertyNullError> configPropertyNullErrorList = new List<ConfigFilePropertyNullError>();

        /// <summary>
        /// Clears the list of ConfigFilePropertyNullError. This method can be called before performing a new check.
        /// </summary>
        public static void ClearConfigPropertyNullErrorList()
        {
            if (configPropertyNullErrorList != null)
                configPropertyNullErrorList.Clear();
        }

        /// <summary>
        /// The main method of the SubManager. Checks if the properties of the configuration files are not null.
        /// </summary>
        /// <param name="configFilesData">ConfigFilesData current value</param>
        /// <returns></returns>
        public static List<ConfigFilePropertyNullError> CheckConfigPropertiesOnNotNull(ConfigFilesData configFilesData)
        {
            ConfigFileGlobalDTO configFileGlobal = configFilesData.ConfigFileGlobal;
            ConfigFileSolutionDTO configFileSolution = configFilesData.ConfigFileSolution;

            if (configFileSolution != null)//If solution config file contains some data
            {
                CheckConfigFileSolutionProperties(configFileSolution);//Check solution config file properties on not null

                if (configFileSolution.projects != null)//If this file contains some projects
                {
                    foreach (var project in configFileSolution.projects)//for each project
                    {
                        if (project.Value != null)
                            CheckConfigFileProjectProperties(project.Key, project.Value);//Check project config file properties on not null
                        //If there is something missing, add relevant error to the list of ConfigFilePropertyNullError
                        else
                            configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("project_value", false, project.Key));
                    }
                }
                else
                    configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("projects", false, ""));
            }
            else
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError(configFilesData.SolutionName, false, ""));


            if (configFileGlobal != null) //If global config file contains some data
                CheckConfigFileGlobalProperties(configFileGlobal); //Check global config file properties on not null
            else
                configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("Global", true, ""));

            return configPropertyNullErrorList;
        }

        /// <summary>
        /// Checks if the properties of the solution configuration file are not null. If any property is found to be null, it adds an error to the list of 
        /// ConfigFilePropertyNullError,
        /// </summary>
        /// <param name="configFileSolution">current ConfigFileSolutionDTO value</param>
        private static void CheckConfigFileSolutionProperties(ConfigFileSolutionDTO configFileSolution)
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

        /// <summary>
        /// Checks if the properties of a project in the solution configuration file are not null. If any property is found to be null, it adds an error
        /// </summary>
        /// <param name="projectKey">current project name string</param>
        /// <param name="currentProject">currtent project DTO value</param>
        private static void CheckConfigFileProjectProperties(string projectKey, ConfigFileProjectDTO currentProject)
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

        /// <summary>
        /// Checks if the properties of the global configuration file are not null. If any property is found to be null, it adds an error
        /// </summary>
        /// <param name="configFileGlobal">DTO of global config file value</param>
        private static void CheckConfigFileGlobalProperties(ConfigFileGlobalDTO configFileGlobal)
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