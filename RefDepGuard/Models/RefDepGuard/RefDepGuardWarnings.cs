using System.Collections.Generic;
using RefDepGuard.Data.FrameworkVersion;
using RefDepGuard.Data.Reference;
using RefDepGuard.Models;
using RefDepGuard.Models.FrameworkVersion;
using RefDepGuard.Models.Reference;

namespace RefDepGuard.Data
{
    public class RefDepGuardWarnings
    {
        public List<ReferenceMatchWarning> RefsMatchWarningList;
        public List<ProjectNotFoundWarning> ProjectNotFoundWarningList;
        public List<ProjectMatchWarning> ProjectMatchWarningList;
        public List<MaxFrameworkVersionDeviantValueWarning> MaxFrameworkVersionDeviantValueWarningList;
        public List<MaxFrameworkVersionConflictWarning> MaxFrameworkVersionConflictWarningsList;
        public List<MaxFrameworkVersionReferenceConflictWarning> MaxFrameworkVersionReferenceConflictWarningsList;
        public List<MaxFrameworkVersionTFMNotFoundWarning> MaxFrameworkVersionTFMNotFoundWarningList;
        public List<string> UntypedWarningsList;
        public Dictionary<string, List<string>> DetectedTransitRefsDict;

        public RefDepGuardWarnings(List<ReferenceMatchWarning> refsMatchWarningList, List<ProjectNotFoundWarning> projectNotFoundWarningList,
            List<ProjectMatchWarning> projectMatchWarningList,
            List<MaxFrameworkVersionDeviantValueWarning> maxFrameworkVersionDeviantValueWarningList,
            List<MaxFrameworkVersionConflictWarning> maxFrameworkVersionConflictWarningsList,
            List<MaxFrameworkVersionReferenceConflictWarning> maxFrameworkVersionReferenceConflictWarningsList, 
            List<MaxFrameworkVersionTFMNotFoundWarning> maxFrameworkVersionTFMNotFoundWarningList,
            List<string> untypedWarningsList, Dictionary<string, List<string>> detectedTransitRefsDict)
        {
            RefsMatchWarningList = refsMatchWarningList;
            ProjectNotFoundWarningList = projectNotFoundWarningList;
            ProjectMatchWarningList = projectMatchWarningList;
            MaxFrameworkVersionDeviantValueWarningList = maxFrameworkVersionDeviantValueWarningList;
            MaxFrameworkVersionConflictWarningsList = maxFrameworkVersionConflictWarningsList;
            MaxFrameworkVersionReferenceConflictWarningsList = maxFrameworkVersionReferenceConflictWarningsList;
            MaxFrameworkVersionTFMNotFoundWarningList = maxFrameworkVersionTFMNotFoundWarningList;
            UntypedWarningsList = untypedWarningsList;
            DetectedTransitRefsDict = detectedTransitRefsDict;
        }

        public bool IsEmpty()
        {
            if (RefsMatchWarningList.Count == 0 && ProjectNotFoundWarningList.Count == 0 && ProjectMatchWarningList.Count == 0 && 
                MaxFrameworkVersionDeviantValueWarningList.Count == 0 && MaxFrameworkVersionConflictWarningsList.Count == 0 && 
                MaxFrameworkVersionReferenceConflictWarningsList.Count == 0 && MaxFrameworkVersionTFMNotFoundWarningList.Count == 0 && 
                UntypedWarningsList.Count == 0 && DetectedTransitRefsDict.Count == 0)
                return true;
            else 
                return false;
        }
    }
}
