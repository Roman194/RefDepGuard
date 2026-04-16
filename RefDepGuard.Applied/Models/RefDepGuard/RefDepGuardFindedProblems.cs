using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.Applied.Models.RefDepGuard
{
    /// <summary>
    /// Its an abstraction of the all finded problems (errors / warnings) after the rules check
    /// </summary>
    public class RefDepGuardFindedProblems
    {
        public RefDepGuardWarnings RefDepGuardWarnings;
        public RefDepGuardErrors RefDepGuardErrors;

        /// <param name="refDepGuardWarnings">RefDepGuardWarnings value</param>
        /// <param name="refDepGuardErrors">RefDepGuardErrors value</param>
        public RefDepGuardFindedProblems(RefDepGuardWarnings refDepGuardWarnings, RefDepGuardErrors refDepGuardErrors)
        {
            RefDepGuardErrors = refDepGuardErrors;
            RefDepGuardWarnings = refDepGuardWarnings;
        }

        /// <summary>
        /// Shows if there is no any problems at this moment after rules check 
        /// </summary>
        /// <returns>an answer on the is empty question</returns>
        public bool IsEmpty()
        {
            if (RefDepGuardWarnings.IsEmpty() && RefDepGuardErrors.IsEmpty())
                return true;
            else
                return false;
        }
    }
}