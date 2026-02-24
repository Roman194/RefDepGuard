using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefDepGuard.Data.Reference
{
    public class ConfigFilePropertyNullError
    {
        public string PropertyName;
        public bool IsGlobal;
        public string ErrorRelevantProjectName;


        public ConfigFilePropertyNullError(string propertyName, bool isGlobal, string errorRelevantProjectName)
        {
            PropertyName = propertyName;
            IsGlobal = isGlobal;
            ErrorRelevantProjectName = errorRelevantProjectName;
        }
    }
}
