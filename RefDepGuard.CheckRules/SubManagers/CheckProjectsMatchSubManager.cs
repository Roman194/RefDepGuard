using System;
using System.Collections.Generic;
using System.Linq;
using RefDepGuard.Applied.Models.ConfigFile;
using RefDepGuard.Applied.Models.Project;

namespace RefDepGuard.CheckRules.SubManagers
{
    /// <summary>
    /// This class is responsible for checking if the projects in the solution match the projects listed in the configuration file.
    /// </summary>
    public class CheckProjectsMatchSubManager
    {
        private static List<ProjectMatchWarning> projectMatchWarningList = new List<ProjectMatchWarning>();

        /// <summary>
        /// Clears the list of project match warnings. This method can be called before performing a new check.
        /// </summary>
        public static void ClearErrorLists()
        {
            if (projectMatchWarningList != null)
                projectMatchWarningList.Clear();
        }

        /// <summary>
        /// The main method of the SubManager. Checks if the projects in the solution match the projects listed in the configuration file. 
        /// If there are discrepancies (added or removed projects), it prompts the user to either update the configuration file accordingly 
        /// or to ignore the discrepancies and add warnings to the list.
        /// </summary>
        /// <param name="configFilesData">ConfigFilesData commited value</param>
        /// <param name="currentCommitedProjState">Solution projects commited state</param>
        /// <returns>ConfigFilesData value and list of current ProjectMatchWarning</returns>
        /// <see cref="ShowPromptAndSolveDifferProblems"/>

        public static List<ProjectMatchWarning> GetProjectsMatchAfterChecksWarning(
            ConfigFilesData configFilesData, Dictionary<string, ProjectState> currentCommitedProjState)
        {
            List<string> addedProjectsList, removedProjectsList = new List<string>();
            (addedProjectsList, removedProjectsList) = CheckSolutionNConfigFileProjectsOnMatch(configFilesData, currentCommitedProjState);
            
            addedProjectsList.ForEach(addedProj => projectMatchWarningList.Add(new ProjectMatchWarning(addedProj, true)));
            removedProjectsList.ForEach(removedProj => projectMatchWarningList.Add(new ProjectMatchWarning(removedProj, false)));

            return projectMatchWarningList;
        }

        public static Tuple<List<string>, List<string>> CheckSolutionNConfigFileProjectsOnMatch(
            ConfigFilesData configFilesData, Dictionary<string, ProjectState> currentCommitedProjState)
        {
            var addedProjectsList = new List<string>();
            var removedProjectsList = new List<string>();

            foreach (KeyValuePair<string, ProjectState> currentProjState in currentCommitedProjState)//Check on added to solution projects
            {
                var projName = currentProjState.Key;
                var projState = currentProjState.Value.CurrentFrameworkVersions.Keys.ToList();

                if (!configFilesData.ConfigFileSolution?.projects?.ContainsKey(projName) ?? false)
                {
                    addedProjectsList.Add(projName);
                }
            }

            foreach (var dictValue in configFilesData.ConfigFileSolution?.projects) //Check on deleted projects
            {
                var projName = dictValue.Key;
                if (!currentCommitedProjState.ContainsKey(projName))
                {
                    removedProjectsList.Add(projName);
                }
            }

            return new Tuple<List<string>, List<string>>(addedProjectsList, removedProjectsList);
        }
    }
}