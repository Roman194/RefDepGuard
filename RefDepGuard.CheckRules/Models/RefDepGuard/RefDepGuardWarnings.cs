using RefDepGuard.CheckRules.Models.FrameworkVersion.Warnings;
using RefDepGuard.CheckRules.Models.FrameworkVersion.Warnings.Conflicts;
using RefDepGuard.CheckRules.Models.Project;
using RefDepGuard.CheckRules.Models.Reference.Warnings;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.CheckRules.Models.RefDepGuard
{
    /// <summary>
    /// It's an abstraction of all lists of the possible extention warnings. Shows all finded warnings after the rules check
    /// </summary>
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

        /// <param name="refsMatchWarningList">list of ReferenceMatchWarning values</param>
        /// <param name="projectNotFoundWarningList">list of ProjectNotFoundWarning values</param>
        /// <param name="projectMatchWarningList">list of ProjectMatchWarning values</param>
        /// <param name="maxFrameworkVersionDeviantValueWarningList">list of MaxFrameworkVersionDeviantValueWarning values</param>
        /// <param name="maxFrameworkVersionConflictWarningsList">list of MaxFrameworkVersionConflictWarning values</param>
        /// <param name="maxFrameworkVersionReferenceConflictWarningsList">list of MaxFrameworkVersionReferenceConflictWarning values</param>
        /// <param name="maxFrameworkVersionTFMNotFoundWarningList">list of MaxFrameworkVersionTFMNotFoundWarning values</param>
        /// <param name="untypedWarningsList">list of string values</param>
        /// <param name="detectedTransitRefsDict">dictionary of list of string values</param>
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

        /// <summary>
        /// Shows if there is no any warnings inside extention after the rule check
        /// </summary>
        /// <returns>a bool result on the is empty question</returns>
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

        public int Count()
        {
            return RefsMatchWarningList.Count + ProjectNotFoundWarningList.Count + ProjectMatchWarningList.Count + MaxFrameworkVersionDeviantValueWarningList.Count +
                MaxFrameworkVersionConflictWarningsList.Count + MaxFrameworkVersionReferenceConflictWarningsList.Count + MaxFrameworkVersionTFMNotFoundWarningList.Count +
                UntypedWarningsList.Count + DetectedTransitRefsDict.Count;
        }
    }
}