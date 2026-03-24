using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.CheckRules.Models.Reference.Errors
{
    /// <summary>
    /// It's an abstraction on all reference extention errors
    /// </summary>
    public class ReferenceRuleErrors
    {
        public List<ReferenceError> RefsErrorList;
        public List<ReferenceMatchError> RefsMatchErrorList;

        /// <param name="refsErrorList">list of ReferenceError values</param>
        /// <param name="refsMatchErrorList">list of ReferenceMatchError values</param>
        public ReferenceRuleErrors(List<ReferenceError> refsErrorList, List<ReferenceMatchError> refsMatchErrorList)
        {
            RefsErrorList = refsErrorList;
            RefsMatchErrorList = refsMatchErrorList;
        }
    }
}