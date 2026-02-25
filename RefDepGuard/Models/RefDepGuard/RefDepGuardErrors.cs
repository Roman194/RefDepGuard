using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RefDepGuard.Data.FrameworkVersion;
using RefDepGuard.Models.FrameworkVersion;

namespace RefDepGuard.Data.Reference
{
    public class RefDepGuardErrors
    {
        public List<ReferenceError> RefsErrorList;
        public List<ReferenceMatchError> RefsMatchErrorList;
        public List<ConfigFilePropertyNullError> ConfigPropertyNullErrorList;
        public List<MaxFrameworkVersionDeviantValueError> MaxFrameworkVersionDeviantValueList;
        public List<MaxFrameworkVersionIllegalTemplateUsageError> MaxFrameworkVersionIllegalTemplateUsageErrorList;
        public List<FrameworkVersionComparabilityError> FrameworkVersionComparabilityErrorList;

        public RefDepGuardErrors(
            List<ReferenceError> refsErrorList, List<ReferenceMatchError> refsMatchErrorList,
            List<ConfigFilePropertyNullError> configPropertyNullErrorList, List<MaxFrameworkVersionDeviantValueError> maxFrameworkVersionDeviantValueList,
            List<MaxFrameworkVersionIllegalTemplateUsageError> maxFrameworkVersionIllegalTemplateUsageErrors,
            List<FrameworkVersionComparabilityError> frameworkVersionComparabilityErrorList)
        {
            RefsErrorList = refsErrorList;
            RefsMatchErrorList = refsMatchErrorList;
            ConfigPropertyNullErrorList = configPropertyNullErrorList;
            MaxFrameworkVersionDeviantValueList = maxFrameworkVersionDeviantValueList;
            MaxFrameworkVersionIllegalTemplateUsageErrorList = maxFrameworkVersionIllegalTemplateUsageErrors;
            FrameworkVersionComparabilityErrorList = frameworkVersionComparabilityErrorList;
        }

        public bool IsEmpty()
        {
            if (RefsErrorList.Count < 1 && RefsMatchErrorList.Count < 1 && ConfigPropertyNullErrorList.Count < 1 && MaxFrameworkVersionDeviantValueList.Count < 1 && 
                MaxFrameworkVersionIllegalTemplateUsageErrorList.Count < 1 && FrameworkVersionComparabilityErrorList.Count < 1)
                return true;
            else
                return false;
        }
    }
}
