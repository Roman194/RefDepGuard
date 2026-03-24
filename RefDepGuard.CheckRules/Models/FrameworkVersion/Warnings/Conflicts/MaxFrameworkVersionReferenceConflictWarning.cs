using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.CheckRules.Models.FrameworkVersion.Warnings.Conflicts
{
    /// <summary>
    /// Shows a warning when there are reference conflict between max_fr_ver parameter values
    /// </summary>
    public class MaxFrameworkVersionReferenceConflictWarning
    {
        public string ProjName;
        public string ProjFrameworkVersion;
        public string RefName;
        public string RefFrameworkVersion;
        public bool IsOneProjectsTypeConflict;

        /// <param name="projName">project name string</param>
        /// <param name="projFrameworkVersion">project max_fr_ver value string</param>
        /// <param name="refName">reference name string</param>
        /// <param name="refFrameworkVersion">reference max_fr_ver value string</param>
        /// <param name="isOneProjectsTypeConflict">shows if it's one projects type conflict or not</param>
        public MaxFrameworkVersionReferenceConflictWarning(string projName, string projFrameworkVersion, string refName, string refFrameworkVersion, bool isOneProjectsTypeConflict)
        {
            ProjName = projName;
            ProjFrameworkVersion = projFrameworkVersion;
            RefName = refName;
            RefFrameworkVersion = refFrameworkVersion;
            IsOneProjectsTypeConflict = isOneProjectsTypeConflict;
        }
    }
}