using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1.Data.FrameworkVersion
{
    public class MaxFrameworkVersionDeviantValue
    {
        public ErrorLevel ErrorLevel;
        public string ErrorRelevantProjectName;
        
        public MaxFrameworkVersionDeviantValue(ErrorLevel errorLevel, string errorRelevantProjectName)
        {
            ErrorLevel = errorLevel;
            ErrorRelevantProjectName = errorRelevantProjectName;
        }
    }
}
