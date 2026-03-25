using RefDepGuard.CheckRules.Models.Reference.Errors;
using RefDepGuard.CheckRules.Models.FrameworkVersion.Errors;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.CheckRules.Models.RefDepGuard
{
    /// <summary>
    /// It's an abstraction with lists of all possible extention errors
    /// </summary>
    public class RefDepGuardErrors
    {
        public List<ReferenceError> RefsErrorList;
        public List<ReferenceMatchError> RefsMatchErrorList;
        public List<ConfigFilePropertyNullError> ConfigPropertyNullErrorList;
        public List<MaxFrameworkVersionDeviantValueError> MaxFrameworkVersionDeviantValueList;
        public List<MaxFrameworkVersionIllegalTemplateUsageError> MaxFrameworkVersionIllegalTemplateUsageErrorList;
        public List<FrameworkVersionComparabilityError> FrameworkVersionComparabilityErrorList;


        /// <param name="refsErrorList">list of ReferenceError values</param>
        /// <param name="refsMatchErrorList">list of ReferenceMatchError values</param>
        /// <param name="configPropertyNullErrorList">list of ConfigFilePropertyNullError values</param>
        /// <param name="maxFrameworkVersionDeviantValueList">list of MaxFrameworkVersionDeviantValueError values</param>
        /// <param name="maxFrameworkVersionIllegalTemplateUsageErrors">list of MaxFrameworkVersionIllegalTemplateUsageError values</param>
        /// <param name="frameworkVersionComparabilityErrorList">list of FrameworkVersionComparatibilityError values</param>
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

        /// <summary>
        /// Shows if there is no errors inside extention at this moment
        /// </summary>
        /// <returns>answer on is empty question</returns>
        public bool IsEmpty()
        {
            if (RefsErrorList.Count < 1 && RefsMatchErrorList.Count < 1 && ConfigPropertyNullErrorList.Count < 1 && MaxFrameworkVersionDeviantValueList.Count < 1 &&
                MaxFrameworkVersionIllegalTemplateUsageErrorList.Count < 1 && FrameworkVersionComparabilityErrorList.Count < 1)
                return true;
            else
                return false;
        }

        public int Count()
        {
            return RefsErrorList.Count + RefsMatchErrorList.Count + ConfigPropertyNullErrorList.Count + MaxFrameworkVersionDeviantValueList.Count +
                MaxFrameworkVersionIllegalTemplateUsageErrorList.Count + FrameworkVersionComparabilityErrorList.Count;
        }
    }
}