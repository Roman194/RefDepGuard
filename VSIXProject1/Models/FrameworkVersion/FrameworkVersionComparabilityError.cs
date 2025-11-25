using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1.Data.FrameworkVersion
{
    public class FrameworkVersionComparabilityError
    {
        public ErrorLevel ErrorLevel;
        public string TargetFrameworkVersion;
        public string MaxFrameworkVersion;
        public string ErrorRelevantProjectName;
        
        public FrameworkVersionComparabilityError(ErrorLevel errorLevel,  string targetFrameworkVersion, string maxFrameworkVersion, string errorRelevantProjectName)
        {
            ErrorLevel = errorLevel;
            TargetFrameworkVersion = targetFrameworkVersion;
            MaxFrameworkVersion = maxFrameworkVersion;
            ErrorRelevantProjectName = errorRelevantProjectName;
        }
    }
}
