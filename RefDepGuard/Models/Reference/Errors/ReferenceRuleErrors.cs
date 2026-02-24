using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
