using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.CheckRules.Models.Reference
{
    /// <summary>
    /// Shows a required refs abstraction
    /// </summary>
    public class RequiredReference
    {
        public string ReferenceName;
        public string RelevantProject;

        /// <param name="referenceName">reference name string</param>
        /// <param name="relevantProject">project name string</param>
        public RequiredReference(string referenceName, string relevantProject)
        {
            ReferenceName = referenceName;
            RelevantProject = relevantProject;
        }
    }
}