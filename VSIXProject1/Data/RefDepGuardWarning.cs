using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject1.Data.FrameworkVersion;
using VSIXProject1.Data.Reference;

namespace VSIXProject1.Data
{
    public class RefDepGuardWarning
    {
        public List<ReferenceMatchWarning> RefsMatchWarningList;
        public List<MaxFrameworkVersionConflictWarning> MaxFrameworkVersionConflictWarningsList;
        public List<MaxFrameworkVersionReferenceConflictWarning> MaxFrameworkVersionReferenceConflictWarningsList;

        public RefDepGuardWarning(List<ReferenceMatchWarning> refsMatchWarningList, List<MaxFrameworkVersionConflictWarning> maxFrameworkVersionConflictWarningsList, List<MaxFrameworkVersionReferenceConflictWarning> maxFrameworkVersionReferenceConflictWarningsList)
        {
            RefsMatchWarningList = refsMatchWarningList;
            MaxFrameworkVersionConflictWarningsList = maxFrameworkVersionConflictWarningsList;
            MaxFrameworkVersionReferenceConflictWarningsList = maxFrameworkVersionReferenceConflictWarningsList;
        }
    }
}
