using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject1.Data.FrameworkVersion;
using VSIXProject1.Data.Reference;

namespace VSIXProject1.Data
{
    public class RefDepGuardWarnings
    {
        public List<ReferenceMatchWarning> RefsMatchWarningList;
        public List<MaxFrameworkVersionConflictWarning> MaxFrameworkVersionConflictWarningsList;
        public List<MaxFrameworkVersionReferenceConflictWarning> MaxFrameworkVersionReferenceConflictWarningsList;
        public List<string> UntypedWarningsList;

        public RefDepGuardWarnings(List<ReferenceMatchWarning> refsMatchWarningList, List<MaxFrameworkVersionConflictWarning> maxFrameworkVersionConflictWarningsList,
            List<MaxFrameworkVersionReferenceConflictWarning> maxFrameworkVersionReferenceConflictWarningsList, List<string> untypedWarningsList)
        {
            RefsMatchWarningList = refsMatchWarningList;
            MaxFrameworkVersionConflictWarningsList = maxFrameworkVersionConflictWarningsList;
            MaxFrameworkVersionReferenceConflictWarningsList = maxFrameworkVersionReferenceConflictWarningsList;
            UntypedWarningsList = untypedWarningsList;
        }

        public bool IsEmpty()
        {
            if (RefsMatchWarningList.Count == 0 && MaxFrameworkVersionConflictWarningsList.Count == 0 
                && MaxFrameworkVersionReferenceConflictWarningsList.Count == 0 && UntypedWarningsList.Count == 0)
                return true;
            else 
                return false;
        }
    }
}
