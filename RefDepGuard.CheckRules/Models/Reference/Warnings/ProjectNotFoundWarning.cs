using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.CheckRules.Models.Reference.Warnings
{
    /// <summary>
    /// Shows a warning when a reference from the config file isn't match to any of the exsisting projects inside current solution
    /// </summary>
    public class ProjectNotFoundWarning
    {
        public string ReferenceName;
        public ProblemLevel WarningLevel;
        public string ProjName;

        /// <param name="referenceName">reference name string</param>
        /// <param name="warningLevel">relevant warning level</param>
        /// <param name="projName">project name string (if its a project level)</param>
        public ProjectNotFoundWarning(string referenceName, ProblemLevel warningLevel, string projName)
        {
            ReferenceName = referenceName;
            WarningLevel = warningLevel;
            ProjName = projName;
        }
    }
}