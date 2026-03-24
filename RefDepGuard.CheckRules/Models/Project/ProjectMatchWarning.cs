using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.CheckRules.Models.Project
{
    /// <summary>
    /// Shows a warning about project match between projects inside config file and a real solution
    /// </summary>
    public class ProjectMatchWarning
    {
        public string ProjName;
        public bool IsNoProjectInConfigFile;

        /// <param name="projName">project name string</param>
        /// <param name="isNoProjectInConfigFile">shows if project is not found in the config file or in the solution</param>
        public ProjectMatchWarning(string projName, bool isNoProjectInConfigFile)
        {
            ProjName = projName;
            IsNoProjectInConfigFile = isNoProjectInConfigFile;
        }
    }
}