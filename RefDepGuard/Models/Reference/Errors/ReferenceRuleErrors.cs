using System.Collections.Generic;

namespace RefDepGuard.Models.Reference
{
    public class ReferenceRuleErrors
    {
        public List<ReferenceError> RefsErrorList;
        public List<ReferenceMatchError> RefsMatchErrorList;

        public ReferenceRuleErrors(List<ReferenceError> refsErrorList, List<ReferenceMatchError> refsMatchErrorList) {
            RefsErrorList = refsErrorList;
            RefsMatchErrorList = refsMatchErrorList;
        }
    }
}
