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
        public List<MaxFrameworkVersionDeviantValue> MaxFrameworkVersionDeviantValueList;
        public List<FrameworkVersionComparabilityError> FrameworkVersionComparabilityErrorList;

        public RefDepGuardErrors(List<ConfigFilePropertyNullError> configPropertyNullErrorList, List<ReferenceError> refsErrorList, 
            List<ReferenceMatchError> refsMatchErrorList, List<MaxFrameworkVersionDeviantValue> maxFrameworkVersionDeviantValueList,
            List<FrameworkVersionComparabilityError> frameworkVersionComparabilityErrorList)
        {
            ConfigPropertyNullErrorList = configPropertyNullErrorList;
            RefsErrorList = refsErrorList;
            RefsMatchErrorList = refsMatchErrorList;
            MaxFrameworkVersionDeviantValueList = maxFrameworkVersionDeviantValueList;
            FrameworkVersionComparabilityErrorList = frameworkVersionComparabilityErrorList;
        }
    }
}
