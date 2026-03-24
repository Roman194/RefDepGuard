using RefDepGuard.CheckRules.Data;
using RefDepGuard.CheckRules.Models;
using RefDepGuard.CheckRules.Models.FrameworkVersion;
using RefDepGuard.CheckRules.Models.FrameworkVersion.Errors;
using RefDepGuard.CheckRules.Models.FrameworkVersion.Warnings.Conflicts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RefDepGuard.CheckRules.SubManagers
{
    /// <summary>
    /// This class is responsible for checking the rules related to maximum framework versions specified in the configuration file.
    /// </summary>
    public class MaxFrameworkRuleChecksSubManager
    {
        private static List<MaxFrameworkVersionConflictWarning> maxFrameworkVersionConflictWarningsList = new List<MaxFrameworkVersionConflictWarning>();
        private static List<MaxFrameworkVersionReferenceConflictWarning> maxFrameworkVersionReferenceConflictWarningsList = new List<MaxFrameworkVersionReferenceConflictWarning>();

        private static List<string> untypedWarningsList = new List<string>();
        private static List<FrameworkVersionComparabilityError> frameworkVersionComparabilityErrorList = new List<FrameworkVersionComparabilityError>();

        private static Dictionary<string, RequiredMaxFrVersion> requiredMaxFrVersionsDict = new Dictionary<string, RequiredMaxFrVersion>();

        /// <summary>
        /// Clears the lists of warnings and errors related to maximum framework version rules. 
        /// This method can be called before performing a new check to ensure that previous results do not interfere with the current check.
        /// </summary>
        public static void ClearErrorAndWarningLists()
        {
            if (untypedWarningsList != null)
                untypedWarningsList.Clear();

            if (maxFrameworkVersionConflictWarningsList != null)
                maxFrameworkVersionConflictWarningsList.Clear();

            if (maxFrameworkVersionReferenceConflictWarningsList != null)
                maxFrameworkVersionReferenceConflictWarningsList.Clear();

            if (frameworkVersionComparabilityErrorList != null)
                frameworkVersionComparabilityErrorList.Clear();

            if (requiredMaxFrVersionsDict != null)
                requiredMaxFrVersionsDict.Clear();
        }

        /// <summary>
        /// Checks for conflicts in maximum framework version rules at the same level and adds warnings to the list if any conflicts are found.
        /// </summary>
        /// <param name="currentMaxFrameworkVersion">max framework version dictionary</param>
        /// <param name="ruleLevel">current max_fr_ver rules level</param>
        public static void CheckMaxFrameworkVersionOneLevelConflict(Dictionary<string, List<int>> currentMaxFrameworkVersion, ProblemLevel ruleLevel)
        {
            if (currentMaxFrameworkVersion.ContainsKey("all")) //Any conflicts on one level can be only between "all" and other types
            {
                List<int> maxAllTypeFrameworkVersionArray = currentMaxFrameworkVersion["all"];

                foreach (var currentMaxLowLevelFrameworkVersion in currentMaxFrameworkVersion)//So we check all types against "all"
                {
                    if (currentMaxLowLevelFrameworkVersion.Key != "all")
                    {
                        List<int> maxCurrentTypeFrameworkVersionArray = currentMaxLowLevelFrameworkVersion.Value;
                        CheckMaxFrameworkVersionCurrentConflict(maxAllTypeFrameworkVersionArray, maxCurrentTypeFrameworkVersionArray, "-", ruleLevel, ruleLevel);
                    }
                }
            }
        }

        /// <summary>
        /// Checks for conflicts in maximum framework version rules between different levels and adds warnings to the list if any conflicts are found.
        /// </summary>
        /// <param name="maxLowLevelFrameworkVersion">Max framework version dictionary of the lower rule level</param>
        /// <param name="maxHighLevelFrameworkVersion">Max framework version dictionary of the higher rule level</param>
        /// <param name="projName">Current relevant project name</param>
        /// <param name="lowRuleLevel">The lower rule level</param>
        /// <param name="highRuleLevel">The hogher rule level</param>
        public static void CheckProjectMaxFrameworkVersionDifferentLevelsConflicts(
            Dictionary<string, List<int>> maxLowLevelFrameworkVersion, Dictionary<string, List<int>> maxHighLevelFrameworkVersion, string projName,
            ProblemLevel lowRuleLevel, ProblemLevel highRuleLevel)
        {

            foreach (var currentMaxLowLevelFrameworkVersion in maxLowLevelFrameworkVersion)//For each type of framework version in the lower level rules 
            {
                var currentMaxLowLevelFrameworkVersionType = currentMaxLowLevelFrameworkVersion.Key;
                List<int> maxLowLevelFrameworkVersionArray = currentMaxLowLevelFrameworkVersion.Value;
                List<int> maxHighLevelFrameworkVersionArray = new List<int>();

                if (maxHighLevelFrameworkVersion.ContainsKey(currentMaxLowLevelFrameworkVersionType))//If the same type exists in the higher level rules,
                {   //then check the versions for this type
                    maxHighLevelFrameworkVersionArray = maxHighLevelFrameworkVersion[currentMaxLowLevelFrameworkVersionType];
                    CheckMaxFrameworkVersionCurrentConflict(maxHighLevelFrameworkVersionArray, maxLowLevelFrameworkVersionArray, projName, lowRuleLevel, highRuleLevel);
                }
                else
                {
                    if (maxHighLevelFrameworkVersion.ContainsKey("all")) //If the same type doesn't exist in the higher level rules, but "all" exists,
                    {   //then check the versions for "all" type from the higher level rules
                        maxHighLevelFrameworkVersionArray = maxHighLevelFrameworkVersion["all"];
                        CheckMaxFrameworkVersionCurrentConflict(maxHighLevelFrameworkVersionArray, maxLowLevelFrameworkVersionArray, projName, lowRuleLevel, highRuleLevel);
                    }
                    else
                    {
                        if (maxLowLevelFrameworkVersion.ContainsKey("all")) //If "all" exists in the lower level rules,
                        {   //then check the versions for each of high level rules, if they exist
                            foreach (var currentMaxHighLevelFrameworkVersion in maxHighLevelFrameworkVersion)
                            {
                                maxHighLevelFrameworkVersionArray = currentMaxHighLevelFrameworkVersion.Value;
                                CheckMaxFrameworkVersionCurrentConflict(maxHighLevelFrameworkVersionArray, maxLowLevelFrameworkVersionArray, projName, lowRuleLevel, highRuleLevel);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the TargetFramework version(s) of the current project is/are compatible with the maximum framework version rules specified in the configuration file.
        /// </summary>
        /// <param name="currentProjectSupportedFrameworks">current project target framework(-s) value dictionary</param>
        /// <param name="maxFrameworkVersion">max framework version on current rule level dicitionary</param>
        /// <param name="projName">relevant project name</param>
        /// <param name="problemLevel">current rule level</param>
        /// <param name="reserveMaxFrameworkVersion">max framework version on higher then current level dictionary</param>
        public static void CheckProjectTargetFrameworkVersion(
            Dictionary<string, List<int>> currentProjectSupportedFrameworks, Dictionary<string, List<int>> maxFrameworkVersion,
            string projName, ProblemLevel problemLevel, Dictionary<string, List<int>> reserveMaxFrameworkVersion = null)
        {

            if (currentProjectSupportedFrameworks.Count == 0)//If there are no TargetFrameworks, then we don't check and add corresponding warning
            {
                untypedWarningsList.Add(projName);
                return;
            }

            foreach (string currentProjectFramework in currentProjectSupportedFrameworks.Keys) //Foreach TragetFrameowrk(-s)
            {
                string currentMaxFrVersionType = currentProjectFramework;
                List<int> currentMaxFrameworkVersionNums = new List<int>();
                string maxFrameworkVersionString;

                List<int> currentProjFrameworkVersionArray = currentProjectSupportedFrameworks[currentProjectFramework];
                int currentProjFrameworkVersionArrayLength = currentProjFrameworkVersionArray.Count;

                if (maxFrameworkVersion.ContainsKey(currentProjectFramework))//If there is a rule for the same type as in TargetFramework, then we compare with it
                {
                    if (currentProjectFramework == "netstandard") //In case of "netstandard", we compare with nearest existing netstandard version, as the user can specify any version
                        (maxFrameworkVersionString, currentMaxFrameworkVersionNums) = TFMSample.GetNearestExistingNetstandartVersion(maxFrameworkVersion[currentProjectFramework]);
                    else
                        currentMaxFrameworkVersionNums = maxFrameworkVersion[currentProjectFramework];
                }
                else
                {
                    if (maxFrameworkVersion.ContainsKey("all"))//If there is no rule for the same type as in TargetFramework, but there is "all" type rule,
                    {
                        //then we compare with "all"
                        currentMaxFrameworkVersionNums = maxFrameworkVersion["all"];
                        if (problemLevel != ProblemLevel.Project)
                            currentMaxFrVersionType = "all";
                    }
                    else //If there is no even "all" type rule, 
                    {   //then we try to compare with higher level rules (if they exist)
                        if (problemLevel == ProblemLevel.Solution && reserveMaxFrameworkVersion != null) //Сделать на уровне Solution предупреждение о том, что не нашлось ни одного подходящего типа Framework ни для одного проекта?
                            CheckProjectTargetFrameworkVersion(currentProjectSupportedFrameworks, reserveMaxFrameworkVersion, projName, ProblemLevel.Global);

                        //if at this moment there is nothing to compare with, its equal to '-'

                        continue;//continue as we neeeds to consider all TargetFrameworks of the project
                    }
                }

                //Loading data about max_framework_version restrictions for the current project
                var maxFrameworkVersionArrayLength = currentMaxFrameworkVersionNums.Count;
                maxFrameworkVersionString = GetFrameworkVersionString(currentMaxFrameworkVersionNums.ConvertAll(num => num.ToString()));

                var isConflictWarningRelevantForProject = maxFrameworkVersionConflictWarningsList.Find(value =>
                    value.LowWarnLevel == problemLevel && (value.WarningRelevantProjectName == projName || value.WarningRelevantProjectName == "-")
                    ) != null ? true : false;

                if (!requiredMaxFrVersionsDict.ContainsKey(projName))//Имеет ли смысл делать это каждый раз при проверке правил? - Да, т.к. TargetFramework мог измениться между коммитами
                    requiredMaxFrVersionsDict.Add(projName,
                        new RequiredMaxFrVersion(maxFrameworkVersionString, currentMaxFrameworkVersionNums, problemLevel, currentMaxFrVersionType, isConflictWarningRelevantForProject));
                else
                    requiredMaxFrVersionsDict[projName] =
                        new RequiredMaxFrVersion(maxFrameworkVersionString, currentMaxFrameworkVersionNums, problemLevel, currentMaxFrVersionType, isConflictWarningRelevantForProject);

                var minLengthValue = Math.Min(maxFrameworkVersionArrayLength, currentProjFrameworkVersionArrayLength);

                int i = 0;
                for (i = 0; i < minLengthValue; i++)//Foreach number in the version arrays
                {
                    int currentProjCurrentNum = currentProjFrameworkVersionArray[i];
                    int maxVersionCurrentNum = Convert.ToInt32(currentMaxFrameworkVersionNums[i]);


                    if (currentProjCurrentNum > maxVersionCurrentNum)//If current project version is higher than max version,
                    {
                        //then add error to the list ("TargetFramework" is higher than accepted by max_framework_version rule)
                        var currentProjFrameworkVersionString = GetFrameworkVersionString(currentProjFrameworkVersionArray.Select(x => x.ToString()).ToList());

                        if (frameworkVersionComparabilityErrorList.Find(error =>
                                error.ErrorLevel == problemLevel && error.TargetFrameworkVersion == currentProjFrameworkVersionString &&
                                error.MaxFrameworkVersion == maxFrameworkVersionString && error.ErrorRelevantProjectName == projName) == null
                            )
                            frameworkVersionComparabilityErrorList.Add(
                                new FrameworkVersionComparabilityError(problemLevel, currentProjFrameworkVersionString, maxFrameworkVersionString, projName)
                                );

                        i = 0;
                        break;
                    }
                    else
                    {
                        if (currentProjCurrentNum < maxVersionCurrentNum)//If current project version is lower than max version,
                        {
                            //then we don't have a conflict and can stop comparing other numbers in the version arrays
                            i = 0;
                            break;
                        }

                    }
                }

                if (currentProjFrameworkVersionArrayLength > maxFrameworkVersionArrayLength && i != 0) //if there are still some nums in the current project version,
                {
                    //then we compare these nums with 0
                    for (int j = minLengthValue; j < currentProjFrameworkVersionArrayLength; j++)
                    {
                        int currentProjVersionCurrentNum = currentProjFrameworkVersionArray[j];

                        if (currentProjVersionCurrentNum > 0)//if the current one higher, then we have a conflict
                        {
                            var currentProjFrameworkVersionString = GetFrameworkVersionString(currentProjFrameworkVersionArray.Select(x => x.ToString()).ToList());

                            if (frameworkVersionComparabilityErrorList.Find(error =>
                                error.ErrorLevel == problemLevel && error.TargetFrameworkVersion == currentProjFrameworkVersionString &&
                                error.MaxFrameworkVersion == maxFrameworkVersionString && error.ErrorRelevantProjectName == projName) == null
                            )
                                frameworkVersionComparabilityErrorList.Add(
                                    new FrameworkVersionComparabilityError(problemLevel, currentProjFrameworkVersionString, maxFrameworkVersionString, projName)
                                    );

                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks for potential conflicts in maximum framework version rules between the current project and its references.
        /// </summary>
        /// <param name="projName">current project name string</param>
        /// <param name="projReferences">list of references of this project</param>
        public static void CheckProjectReferencesOnPotentialReferencesFrameworkVersionConflict(string projName, List<string> projReferences)
        {
            //Check on project references on potential references framework version conflicts is produced only for projects that don't have any conflicts in max
            //framework version rules with higher level rules or there is no conflicts between global and solution rules
            var projectError = maxFrameworkVersionConflictWarningsList.Find(value => value.WarningRelevantProjectName == projName || value.WarningRelevantProjectName == "-");

            if (requiredMaxFrVersionsDict.ContainsKey(projName) && projectError == null)
            {
                List<int> currentProjMaxFrVersionNums = requiredMaxFrVersionsDict[projName].VersionNums;

                foreach (var projReference in projReferences)//For each reference of the project
                {
                    var referenceError = maxFrameworkVersionConflictWarningsList.Find(value => value.WarningRelevantProjectName == projReference);

                    if (requiredMaxFrVersionsDict.ContainsKey(projReference) && referenceError == null)
                    {
                        //In case when project types aren't compatible with each other there is IDE warnings. So it makes sense to check on potential max_fr_ver
                        //reference conflicts only on one type projects and different compatible projects that have it not for all versions (netstandard)
                        //(if differ compar projects have it on all versions, check have no sense as there are can't be problem cases)

                        RequiredMaxFrVersion projVersion = requiredMaxFrVersionsDict[projName];
                        RequiredMaxFrVersion refVersion = requiredMaxFrVersionsDict[projReference];

                        //if there is a rule for the same type as in TargetFramework, then we compare with it
                        if (refVersion.ProjectTypeRule == projVersion.ProjectTypeRule || refVersion.ProjectTypeRule == "all" || projVersion.ProjectTypeRule == "all")
                        {
                            List<int> currentRefMaxVersionNums = refVersion.VersionNums;

                            CheckMaxFrameworkVersionCurrentConflict(currentProjMaxFrVersionNums, currentRefMaxVersionNums, projName,
                                ProblemLevel.Undefined, ProblemLevel.Undefined, projReference);
                        }
                        else
                        {
                            //if there is "netstandard" reference and current project is compatible with netstandard, then we compare with it
                            if (refVersion.ProjectTypeRule == "netstandard" && TFMSample.PossibleComparableTFMsWithNetStandard().Contains(projVersion.ProjectTypeRule))
                            {
                                if (projVersion.ProjectTypeRule == "net") continue;

                                //Imporatnt: as the user can specify any version, we need to find the nearest existing netstandard version to the current project's
                                //max_framework_version and compare with it!
                                string nearestExistingNetStdVersion;
                                List<int> nearestExistingNetStdVersionNums;
                                (nearestExistingNetStdVersion, nearestExistingNetStdVersionNums) = TFMSample.GetNearestExistingNetstandartVersion(refVersion.VersionNums);

                                var minProjTypeVersions = TFMSample.MinProjTypeVersionsPerNetstandardVersion()[nearestExistingNetStdVersion];

                                string currMinVersion = "";

                                switch (projVersion.ProjectTypeRule)
                                {
                                    case "netcoreapp": currMinVersion = minProjTypeVersions.MinNetcoreappVer; break;
                                    case "netf": currMinVersion = minProjTypeVersions.MinNetfVer; break;
                                    default: currMinVersion = minProjTypeVersions.MinUapVer; break;
                                }

                                if (currMinVersion == "-") //If there are no compatible versions for current project type, then we add conflict warning
                                {
                                    AddNewMaxFrameworkVersionOnReferenceConflictWarning(projVersion.VersionText, nearestExistingNetStdVersion, projName, projReference, false);
                                    continue;
                                }

                                //Min version of the project to have a link with current version of netstandard reference
                                List<int> currMinVersionNums = currMinVersion
                                    .Split('.')
                                    .ToList()
                                    .ConvertAll(value => Convert.ToInt32(value));

                                //As we need to check if the current project's TFM version is higher than the minimum required version for compatibility with the
                                //reference, the TFM version of the reference is passed as a separate parameter
                                //(it doesn't take part in compare, but still should be commited in relevant warning)
                                CheckMaxFrameworkVersionCurrentConflict(currentProjMaxFrVersionNums, currMinVersionNums, projName,
                                    ProblemLevel.Undefined, ProblemLevel.Undefined, projReference, nearestExistingNetStdVersionNums);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the list of warnings related to conflicts in maximum framework version rules. 
        /// These warnings indicate that there are conflicting maximum framework version rules for the same project or for all projects.
        /// </summary>
        /// <returns>MaxFrameworkVersionWarnings with 2 lists of conflict warnings</returns>
        public static MaxFrameworkVersionConflictWarnings GetMaxFrameworkVersionWarnings()
        {
            return new MaxFrameworkVersionConflictWarnings(maxFrameworkVersionConflictWarningsList, maxFrameworkVersionReferenceConflictWarningsList);
        }

        /// <summary>
        /// Gets the list of errors related to incompatibility of the project's TargetFramework version with the max_fr_ver.
        /// </summary>
        /// <returns>MaxFrameworkRuleProblems with 2 lists of comparability errors and uptyped warnings</returns>
        public static MaxFrameworkRuleProblems GetMaxFrameworkRuleProblems()
        {
            return new MaxFrameworkRuleProblems(frameworkVersionComparabilityErrorList, untypedWarningsList);
        }

        /// <summary>
        /// Gets the dictionary of required maximum framework versions for each project, which is used for checking potential max_fr_ver conflicts between projects 
        /// and their references.
        /// </summary>
        /// <returns>Dictionary with RequiredMaxFrVersion for each project</returns>
        public static Dictionary<string, RequiredMaxFrVersion> GetRequiredMaxFrVersionsDict()
        {
            return requiredMaxFrVersionsDict;
        }

        /// <summary>
        /// Checks for conflicts in max_fr_ver rules between two projects. Just consider two max_fr_version in abstraction of place from where these rules come.
        /// It considers that conditionally more higher level should be not bigger than lower level, but if it is, then we have a conflict and add warning about it.
        /// </summary>
        /// <param name="maxHighLevelFrameworkVersionList">Conditionally higher max_fr_ver list of nums</param>
        /// <param name="maxLowLevelFrameworkVersionList">Conditionally lower max_fr_ver list of nums</param>
        /// <param name="projName">relevant project name string</param>
        /// <param name="lowRuleLevel">lower level rule</param>
        /// <param name="highRuleLevel">higher level rule</param>
        /// <param name="refName">relevant reference name (only for max_fr_ver reference conflicts)</param>
        /// <param name="currentRefFrameworkVersionList">Reference max_fr_ver list of nums (only for max_fr_ver reference conflicts)</param>
        private static void CheckMaxFrameworkVersionCurrentConflict(
            List<int> maxHighLevelFrameworkVersionList, List<int> maxLowLevelFrameworkVersionList, string projName, ProblemLevel lowRuleLevel, ProblemLevel highRuleLevel,
            string refName = "", List<int> currentRefFrameworkVersionList = null)
        {
            var isOneProjectsTypeConflict = (currentRefFrameworkVersionList != null) ? false : true;

            var maxHighLevelFrameworkVersionArrayLength = maxHighLevelFrameworkVersionList.Count;
            var maxLowLevelFrameworkVersionArrayLength = maxLowLevelFrameworkVersionList.Count;

            var minLengthValue = Math.Min(maxLowLevelFrameworkVersionArrayLength, maxHighLevelFrameworkVersionArrayLength);

            for (int i = 0; i < minLengthValue; i++)//For each number in the version arrays on the same positions
            {
                int currentLowLevelFrameworkVersionNum = maxLowLevelFrameworkVersionList[i];
                int currentHighLevelFrameworkVersionNum = maxHighLevelFrameworkVersionList[i];

                if (currentHighLevelFrameworkVersionNum < currentLowLevelFrameworkVersionNum)//If the current "higher" number is lower than the current "lower",
                {
                    //then we have a conflict and add warning about it
                    var maxHighLevelFrameworkVersionString = GetFrameworkVersionString(maxHighLevelFrameworkVersionList.ConvertAll(num => num.ToString()));
                    var maxLowLevelFrameworkVersionString = GetFrameworkVersionString(maxLowLevelFrameworkVersionList.ConvertAll(num => num.ToString()));
                    var currentRefFrameworkVersionString = (!isOneProjectsTypeConflict) ?
                        GetFrameworkVersionString(currentRefFrameworkVersionList.ConvertAll(num => num.ToString())) : null;

                    if (lowRuleLevel != ProblemLevel.Undefined)//By ProblemLevel.Undefined we understand which type of conflict we have and which warning should be determined
                        AddNewMaxFrameworkVersionConflictWarning(maxHighLevelFrameworkVersionString, maxLowLevelFrameworkVersionString, projName, lowRuleLevel, highRuleLevel);
                    else
                        AddNewMaxFrameworkVersionOnReferenceConflictWarning(maxHighLevelFrameworkVersionString, currentRefFrameworkVersionString ?? maxLowLevelFrameworkVersionString, projName, refName,
                            isOneProjectsTypeConflict);

                    return;
                }
                else
                {
                    if (currentHighLevelFrameworkVersionNum > currentLowLevelFrameworkVersionNum)//If the current "higher" number is higher than the current "lower",
                        return; //then we don't have a conflict and can stop comparing other numbers in the version arrays
                }
            }

            if (maxHighLevelFrameworkVersionArrayLength < maxLowLevelFrameworkVersionArrayLength)//If there are still some numbers in the "lower" version array,
            {
                //then we compare them with 0 
                for (int i = 0; i < maxLowLevelFrameworkVersionArrayLength; i++)
                {
                    int currentLowLevelFrameworkVersionNum = maxLowLevelFrameworkVersionList[i];

                    if (currentLowLevelFrameworkVersionNum > 0)//If the current "lower" number is higher than 0, then we have a conflict and add warning about it
                    {
                        var maxHighLevelFrameworkVersionString = GetFrameworkVersionString(maxHighLevelFrameworkVersionList.ConvertAll(num => num.ToString()));
                        var maxLowLevelFrameworkVersionString = GetFrameworkVersionString(maxLowLevelFrameworkVersionList.ConvertAll(num => num.ToString()));
                        var currentRefFrameworkVersionString = (!isOneProjectsTypeConflict) ?
                            GetFrameworkVersionString(currentRefFrameworkVersionList.ConvertAll(num => num.ToString())) : null;

                        if (lowRuleLevel != ProblemLevel.Undefined)
                            AddNewMaxFrameworkVersionConflictWarning(maxHighLevelFrameworkVersionString, maxLowLevelFrameworkVersionString, projName, lowRuleLevel,
                                highRuleLevel);
                        else
                            AddNewMaxFrameworkVersionOnReferenceConflictWarning(maxHighLevelFrameworkVersionString, currentRefFrameworkVersionString ?? maxLowLevelFrameworkVersionString, projName, refName,
                                isOneProjectsTypeConflict);

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Adds a new warning about a conflict in maximum framework version rules between two levels to the list of warnings.
        /// </summary>
        /// <param name="maxHighLevelFrameworkVersionString">Conditionally higher max_fr_ver string</param>
        /// <param name="maxLowLevelFrameworkVersionString">Conditionally lower max_fr_ver string</param>
        /// <param name="projName">relevant project name string</param>
        /// <param name="lowRuleLevel">lower level rule</param>
        /// <param name="highRuleLevel">higher level rule</param>
        private static void AddNewMaxFrameworkVersionConflictWarning(
            string maxHighLevelFrameworkVersionString, string maxLowLevelFrameworkVersionString, string projName, ProblemLevel lowRuleLevel, ProblemLevel highRuleLevel)
        {
            var potentialMaxFrameworkVersionConflictWarning =
                new MaxFrameworkVersionConflictWarning(highRuleLevel, lowRuleLevel, maxHighLevelFrameworkVersionString, maxLowLevelFrameworkVersionString, projName);

            if (lowRuleLevel == ProblemLevel.Project)//Always adds warning when project level
            {
                maxFrameworkVersionConflictWarningsList.Add(potentialMaxFrameworkVersionConflictWarning);
                return;
            }

            if (maxFrameworkVersionConflictWarningsList.Find(error =>
                error.HighWarnLevel == highRuleLevel && error.LowWarnLevel == lowRuleLevel && error.WarningRelevantProjectName == projName) == null
            ) //Otherwise adds warning only when it isn't duplicate
                maxFrameworkVersionConflictWarningsList.Add(potentialMaxFrameworkVersionConflictWarning);
        }

        /// <summary>
        /// Adds a new warning about a potential conflict in maximum framework version rules between the project and its reference to the list of warnings.
        /// </summary>
        /// <param name="maxProjNameFrameworkVersionString">project max_fr_ver string</param>
        /// <param name="maxRefNameFrameworkVersionString">reference max_fr_ver string</param>
        /// <param name="projName">relevant project name</param>
        /// <param name="refName">relevant ref name</param>
        /// <param name="isOneProjectsTypeConflict">determines if there are one type projects conflict</param>
        private static void AddNewMaxFrameworkVersionOnReferenceConflictWarning(
            string maxProjNameFrameworkVersionString, string maxRefNameFrameworkVersionString, string projName, string refName, bool isOneProjectsTypeConflict)
        {
            maxFrameworkVersionReferenceConflictWarningsList.Add(
                new MaxFrameworkVersionReferenceConflictWarning(
                    projName, maxProjNameFrameworkVersionString, refName, maxRefNameFrameworkVersionString, isOneProjectsTypeConflict)
                );
        }

        /// <summary>
        /// Converts a list of numbers of a framework version to a string format.
        /// </summary>
        /// <param name="targetFrameworkVersionArray">current max_fr_ver list of strings</param>
        /// <returns>Union string of all max_fr_ver nums through dots</returns>
        private static string GetFrameworkVersionString(List<string> targetFrameworkVersionArray)
        {
            string outputString = "";
            bool isFirstIteration = true;

            foreach (var item in targetFrameworkVersionArray)
            {
                if (isFirstIteration)
                {
                    outputString += item;
                    isFirstIteration = false;
                }
                else
                    outputString += "." + item;
            }

            return outputString;
        }
    }
}