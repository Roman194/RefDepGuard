using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.Applied.Models.FrameworkVersion.Errors
{
    /// <summary>
    /// Shows an error of the illegal template usage inside config files (use of the template on the proj level where there is only one TFM or usage of the TFM that not in the
    /// project TFM-s)
    /// </summary>
    public class MaxFrameworkVersionIllegalTemplateUsageError
    {
        public string ProjName;
        public bool IsIllegalTFMUsageError;

        /// <param name="projName">project name string</param>
        /// <param name="isIllegalTFMUsageError">shows if its an illegal TFM usage or illegal template usage at all</param>
        public MaxFrameworkVersionIllegalTemplateUsageError(string projName, bool isIllegalTFMUsageError)
        {
            ProjName = projName;
            IsIllegalTFMUsageError = isIllegalTFMUsageError;
        }
    }
}