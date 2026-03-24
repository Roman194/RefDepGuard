using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.CheckRules.Models.ConfigFile
{
    /// <summary>
    /// This model shows an error when there is a missing property inside one of the config file DTO parameter values (null value)
    /// </summary>
    public class ConfigFilePropertyNullError
    {
        public string PropertyName;
        public bool IsGlobal;
        public string ErrorRelevantProjectName;

        /// <param name="propertyName">a string name of the property with null eror</param>
        /// <param name="isGlobal">shows if the error in the global or solution config file</param>
        /// <param name="errorRelevantProjectName">error relevant project name (if this is a project level error)</param>
        public ConfigFilePropertyNullError(string propertyName, bool isGlobal, string errorRelevantProjectName)
        {
            PropertyName = propertyName;
            IsGlobal = isGlobal;
            ErrorRelevantProjectName = errorRelevantProjectName;
        }
    }
}