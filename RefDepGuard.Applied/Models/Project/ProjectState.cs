using System.Collections.Generic;

namespace RefDepGuard.Applied.Models.Project
{
    /// <summary>
    /// It's a model that shows the current state of the project, which is needed to compare with the config file parameters 
    /// and make a decision about the project compliance with the config file rules.
    /// It contains the current framework versions of the project and the current references of the project.
    /// </summary>
    public class ProjectState
    {
        public Dictionary<string, List<int>> CurrentFrameworkVersions;
        public string CurrentFrameworkVersionsString;
        public List<string> CurrentReferences;

        public ProjectState(Dictionary<string, List<int>> currentFrameworkVersions, string currentFrameworkVersionsString, List<string> currentReferences)
        {
            CurrentFrameworkVersions = currentFrameworkVersions;
            CurrentFrameworkVersionsString = currentFrameworkVersionsString;
            CurrentReferences = currentReferences;
        }
    }
}