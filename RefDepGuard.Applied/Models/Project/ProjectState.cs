using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.Applied.Models.Project
{
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