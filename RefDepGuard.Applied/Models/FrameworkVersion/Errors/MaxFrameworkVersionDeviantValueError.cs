using RefDepGuard.Applied.Models.Problem;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.Applied.Models.FrameworkVersion.Errors
{
    /// <summary>
    /// Shows an error about deviant value inside max_fr_ver config file parameter
    /// </summary>
    public class MaxFrameworkVersionDeviantValueError
    {
        public ProblemLevel ErrorLevel;
        public string ErrorRelevantProjectName;
        public bool IsProjectTypeCopyError;

        /// <param name="errorLevel">relevant error level</param>
        /// <param name="errorRelevantProjectName">error relevant proj name string (for a project error level))</param>
        /// <param name="isProjectTypeCopyError">shows if this is "self-locking" error</param>
        public MaxFrameworkVersionDeviantValueError(ProblemLevel errorLevel, string errorRelevantProjectName, bool isProjectTypeCopyError)
        {
            ErrorLevel = errorLevel;
            ErrorRelevantProjectName = errorRelevantProjectName;
            IsProjectTypeCopyError = isProjectTypeCopyError;
        }
    }
}