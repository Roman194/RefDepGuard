using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1.Data.FrameworkVersion
{
    public class MaxFrameworkVersionDeviantValueError
    {
        public ErrorLevel ErrorLevel;
        public string ErrorRelevantProjectName;
        public bool IsProjectTypeCopyError;
        
        public MaxFrameworkVersionDeviantValueError(ErrorLevel errorLevel, string errorRelevantProjectName, bool isProjectTypeCopyError)
        {
            ErrorLevel = errorLevel;
            ErrorRelevantProjectName = errorRelevantProjectName;
            IsProjectTypeCopyError = isProjectTypeCopyError;
        }
    }
}
