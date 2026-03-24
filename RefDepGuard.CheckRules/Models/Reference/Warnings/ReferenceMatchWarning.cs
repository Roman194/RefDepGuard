using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.CheckRules.Models.Reference.Warnings
{
    /// <summary>
    /// Shows a warning when different level refs rule is not match with each other
    /// </summary>
    public class ReferenceMatchWarning
    {
        public ProblemLevel HighReferenceLevel;
        public ProblemLevel LowReferenceLevel;
        public string ReferenceName;
        public string ProjectName;
        public bool IsReferenceStraight;
        public bool IsHighLevelReq;

        /// <param name="highReferenceLevel">"higher" level ref rule</param>
        /// <param name="lowReferenceLevel">"lower" level ref rule</param>
        /// <param name="referenceName">reference name string</param>
        /// <param name="projectName">project name string</param>
        /// <param name="isReferenceStaright">shows if the rules id duplicates or conflicts to each other</param>
        /// <param name="isHighLevelReq">shows if the "higher" level rule is requierd or unacceptable</param>
        public ReferenceMatchWarning(ProblemLevel highReferenceLevel, ProblemLevel lowReferenceLevel, string referenceName, string projectName, bool isReferenceStaright, bool isHighLevelReq)
        {
            HighReferenceLevel = highReferenceLevel;
            LowReferenceLevel = lowReferenceLevel;
            ReferenceName = referenceName;
            ProjectName = projectName;
            IsReferenceStraight = isReferenceStaright;
            IsHighLevelReq = isHighLevelReq;
        }
    }
}