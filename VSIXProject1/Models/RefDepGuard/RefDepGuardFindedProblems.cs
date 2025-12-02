using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSIXProject1.Data.Reference;

namespace VSIXProject1.Data
{
    public class RefDepGuardFindedProblems
    {
        public RefDepGuardWarnings RefDepGuardWarnings;
        public RefDepGuardErrors RefDepGuardErrors;
        public RefDepGuardFindedProblems(RefDepGuardWarnings refDepGuardWarnings, RefDepGuardErrors refDepGuardErrors)
        {
            RefDepGuardErrors = refDepGuardErrors;
            RefDepGuardWarnings = refDepGuardWarnings;
        }

        public bool IsEmpty()
        {
            if (RefDepGuardWarnings.IsEmpty() && RefDepGuardErrors.IsEmpty())
                return true;
            else
                return false;
        }
    }
}
