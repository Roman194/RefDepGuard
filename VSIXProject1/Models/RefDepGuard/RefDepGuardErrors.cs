using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject1.Data.FrameworkVersion;
using VSIXProject1.Models.FrameworkVersion;

namespace VSIXProject1.Data.Reference
{
    public class RefDepGuardErrors
    {
        public List<ConfigFilePropertyNullError> ConfigPropertyNullErrorList;
        public List<ReferenceError> RefsErrorList;
        public List<ReferenceMatchError> RefsMatchErrorList;
        public List<MaxFrameworkVersionDeviantValueError> MaxFrameworkVersionDeviantValueList;
        public List<MaxFrameworkVersionIllegalTemplateUsageError> MaxFrameworkVersionIllegalTemplateUsageErrorList;
        public List<FrameworkVersionComparabilityError> FrameworkVersionComparabilityErrorList;

        public RefDepGuardErrors(
            List<ConfigFilePropertyNullError> configPropertyNullErrorList, List<ReferenceError> refsErrorList, 
            List<ReferenceMatchError> refsMatchErrorList, List<MaxFrameworkVersionDeviantValueError> maxFrameworkVersionDeviantValueList,
            List<MaxFrameworkVersionIllegalTemplateUsageError> maxFrameworkVersionIllegalTemplateUsageErrors,
            List<FrameworkVersionComparabilityError> frameworkVersionComparabilityErrorList)
        {
            ConfigPropertyNullErrorList = configPropertyNullErrorList;
            RefsErrorList = refsErrorList;
            RefsMatchErrorList = refsMatchErrorList;
            MaxFrameworkVersionDeviantValueList = maxFrameworkVersionDeviantValueList;
            MaxFrameworkVersionIllegalTemplateUsageErrorList = maxFrameworkVersionIllegalTemplateUsageErrors;
            FrameworkVersionComparabilityErrorList = frameworkVersionComparabilityErrorList;
        }

        public bool IsEmpty()
        {
            if (ConfigPropertyNullErrorList.Count < 1 && RefsErrorList.Count < 1 && RefsMatchErrorList.Count < 1 
                && MaxFrameworkVersionDeviantValueList.Count < 1 && MaxFrameworkVersionIllegalTemplateUsageErrorList.Count < 1 &&
                FrameworkVersionComparabilityErrorList.Count < 1)
                return true;
            else
                return false;
        }
    }
}
