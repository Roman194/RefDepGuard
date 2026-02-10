using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject1.Data.FrameworkVersion;
using VSIXProject1.Data.Reference;
using VSIXProject1.Models;
using VSIXProject1.Models.FrameworkVersion;
using VSIXProject1.Models.Reference;

namespace VSIXProject1.Data
{
    //Упорядочить!!!
    public class RefDepGuardWarnings
    {
        public List<ReferenceMatchWarning> RefsMatchWarningList;
        public List<ProjectNotFoundWarning> ProjectNotFoundWarningList;
        public List<MaxFrameworkVersionDeviantValueWarning> MaxFrameworkVersionDeviantValueWarningList;
        public List<MaxFrameworkVersionConflictWarning> MaxFrameworkVersionConflictWarningsList;
        public List<MaxFrameworkVersionReferenceConflictWarning> MaxFrameworkVersionReferenceConflictWarningsList;
        public List<MaxFrameworkVersionTFMNotFoundWarning> MaxFrameworkVersionTFMNotFoundWarningList;
        public List<ProjectMatchWarning> ProjectMatchWarningList;
        public List<string> UntypedWarningsList;

        public RefDepGuardWarnings(List<ReferenceMatchWarning> refsMatchWarningList, List<ProjectNotFoundWarning> projectNotFoundWarningList,
            List<MaxFrameworkVersionDeviantValueWarning> maxFrameworkVersionDeviantValueWarningList,
            List<MaxFrameworkVersionConflictWarning> maxFrameworkVersionConflictWarningsList,
            List<MaxFrameworkVersionReferenceConflictWarning> maxFrameworkVersionReferenceConflictWarningsList, 
            List<MaxFrameworkVersionTFMNotFoundWarning> maxFrameworkVersionTFMNotFoundWarningList,
            List<ProjectMatchWarning> projectMatchWarningList, List<string> untypedWarningsList)
        {
            RefsMatchWarningList = refsMatchWarningList;
            ProjectNotFoundWarningList = projectNotFoundWarningList;
            MaxFrameworkVersionDeviantValueWarningList = maxFrameworkVersionDeviantValueWarningList;
            MaxFrameworkVersionConflictWarningsList = maxFrameworkVersionConflictWarningsList;
            MaxFrameworkVersionReferenceConflictWarningsList = maxFrameworkVersionReferenceConflictWarningsList;
            MaxFrameworkVersionTFMNotFoundWarningList = maxFrameworkVersionTFMNotFoundWarningList;
            ProjectMatchWarningList = projectMatchWarningList;
            UntypedWarningsList = untypedWarningsList;
        }

        public bool IsEmpty()
        {
            if (RefsMatchWarningList.Count == 0 && ProjectNotFoundWarningList.Count == 0 && MaxFrameworkVersionDeviantValueWarningList.Count == 0 
                && MaxFrameworkVersionConflictWarningsList.Count == 0 && MaxFrameworkVersionReferenceConflictWarningsList.Count == 0 && 
                MaxFrameworkVersionTFMNotFoundWarningList.Count == 0 && ProjectMatchWarningList.Count == 0 && UntypedWarningsList.Count == 0)
                return true;
            else 
                return false;
        }
    }
}
