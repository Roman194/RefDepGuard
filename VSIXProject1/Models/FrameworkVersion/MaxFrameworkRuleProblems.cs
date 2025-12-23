using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject1.Data.FrameworkVersion;

namespace VSIXProject1.Models.FrameworkVersion
{
    public class MaxFrameworkRuleProblems
    {
        public List<FrameworkVersionComparabilityError> FrameworkVersionComparabilityErrorList;
        public List<string> UntypedWarningsList;

        public MaxFrameworkRuleProblems(List<FrameworkVersionComparabilityError> frameworkVersionComparabilityErrorList, List<string> untypedWarningsList)
        {
            FrameworkVersionComparabilityErrorList = frameworkVersionComparabilityErrorList;
            UntypedWarningsList = untypedWarningsList;
        }
    }
}
