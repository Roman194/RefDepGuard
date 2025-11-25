using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject1.Data.FrameworkVersion;

namespace VSIXProject1.Data.Reference
{
    public class RefDepGuardErrors
    {
        public List<ConfigFilePropertyNullError> ConfigPropertyNullErrorList;
        public List<ReferenceError> RefsErrorList;
        public List<ReferenceMatchError> RefsMatchErrorList;
        public List<MaxFrameworkVersionDeviantValueError> MaxFrameworkVersionDeviantValueList;
        public List<FrameworkVersionComparabilityError> FrameworkVersionComparabilityErrorList;
        public List<string> UntypedErrorsList;

        public RefDepGuardErrors(
            List<ConfigFilePropertyNullError> configPropertyNullErrorList, List<ReferenceError> refsErrorList, 
            List<ReferenceMatchError> refsMatchErrorList, List<MaxFrameworkVersionDeviantValueError> maxFrameworkVersionDeviantValueList,
            List<FrameworkVersionComparabilityError> frameworkVersionComparabilityErrorList, List<string> untypedErrorsList)
        {
            ConfigPropertyNullErrorList = configPropertyNullErrorList;
            RefsErrorList = refsErrorList;
            RefsMatchErrorList = refsMatchErrorList;
            MaxFrameworkVersionDeviantValueList = maxFrameworkVersionDeviantValueList;
            FrameworkVersionComparabilityErrorList = frameworkVersionComparabilityErrorList;
            UntypedErrorsList = untypedErrorsList;
        }
    }
}
