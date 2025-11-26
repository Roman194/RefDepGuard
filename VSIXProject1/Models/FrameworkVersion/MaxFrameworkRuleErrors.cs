using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject1.Data.FrameworkVersion;

namespace VSIXProject1.Models.FrameworkVersion
{
    public class MaxFrameworkRuleErrors
    {
        public List<FrameworkVersionComparabilityError> FrameworkVersionComparabilityErrorList;
        public List<string> UntypedErrorsList;

        public MaxFrameworkRuleErrors(List<FrameworkVersionComparabilityError> frameworkVersionComparabilityErrorList, List<string> untypedErrorsList)
        {
            FrameworkVersionComparabilityErrorList = frameworkVersionComparabilityErrorList;
            UntypedErrorsList = untypedErrorsList;
        }
    }
}
