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
        
        public MaxFrameworkVersionDeviantValueError(ErrorLevel errorLevel, string errorRelevantProjectName)
        {
            ErrorLevel = errorLevel;
            ErrorRelevantProjectName = errorRelevantProjectName;
        }
    }
}
