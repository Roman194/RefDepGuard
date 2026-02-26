using RefDepGuard.Data.Reference;

namespace RefDepGuard.Data
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
