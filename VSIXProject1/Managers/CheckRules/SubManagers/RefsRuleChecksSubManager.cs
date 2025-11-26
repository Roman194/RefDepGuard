using System;
using System.Collections.Generic;
using System.Linq;
using VSIXProject1.Data.Reference;
using VSIXProject1.Models.Reference;

namespace VSIXProject1.Managers.CheckRules.SubManagers
{
    public class RefsRuleChecksSubManager
    {
        static List<ReferenceMatchError> refsMatchErrorList = new List<ReferenceMatchError>();
        static List<ReferenceError> refsErrorList = new List<ReferenceError>();

        static List<ReferenceMatchWarning> refsMatchWarningList = new List<ReferenceMatchWarning>();

        public static void ClearRefsErrorsAndWarnings()
        {
            if (refsErrorList != null)
                refsErrorList.Clear();

            if (refsMatchErrorList != null)
                refsMatchErrorList.Clear();

            if (refsMatchWarningList != null)
                refsMatchWarningList.Clear();
        }

        public static void CheckRulesOnMatchConflicts(
            List<string> solutionRequiredReferences, List<string> solutionUnacceptableReferences, 
            List<string> globalRequiredReferences, List<string> globalUnacceptableReferences
            )
        {
            List<string> solutionReferencesIntersect = solutionRequiredReferences.Intersect(solutionUnacceptableReferences).ToList();
            List<string> globalReferencesIntersect = globalRequiredReferences.Intersect(globalUnacceptableReferences).ToList();

            List<string> solutionReqAndGlobalUnacceptIntersect = solutionRequiredReferences.Intersect(globalUnacceptableReferences).ToList();
            List<string> solutionReqStraightLevelIntersect = solutionRequiredReferences.Intersect(globalRequiredReferences).ToList();

            List<string> solutionUnacceptAndGlobalReqIntersect = solutionUnacceptableReferences.Intersect(globalRequiredReferences).ToList();
            List<string> solutionUnacceptStraightLevelIntersect = solutionUnacceptableReferences.Intersect(globalUnacceptableReferences).ToList();

            List<List<string>> solutionCrossLevelIntersects = new List<List<string>> { solutionReqAndGlobalUnacceptIntersect, solutionUnacceptAndGlobalReqIntersect };
            List<List<string>> solutionStraightLevelIntersects = new List<List<string>> { solutionUnacceptStraightLevelIntersect, solutionReqStraightLevelIntersect };

            AddReferenceMatchErrorsToList(ErrorLevel.Solution, "", false, solutionReferencesIntersect);
            AddReferenceMatchErrorsToList(ErrorLevel.Global, "", false, globalReferencesIntersect);

            AddReferenceMatchWarningsToList(ErrorLevel.Global, ErrorLevel.Solution, "", false, solutionCrossLevelIntersects);
            AddReferenceMatchWarningsToList(ErrorLevel.Global, ErrorLevel.Solution, "", true, solutionStraightLevelIntersects);
        }

        public static void CheckProjectRulesOnMatchConflicts(
            List<string> solutionRequiredReferences, List<string> solutionUnacceptableReferences, List<string> globalRequiredReferences, 
            List<string> globalUnacceptableReferences, List<string> requiredReferences, List<string> unacceptableReferences, string projName
            )
        {
            List<string> projectReferencesIntersect = requiredReferences.Intersect(unacceptableReferences).ToList();

            List<string> projectReqAndGlobalUnacceptIntersect = requiredReferences.Intersect(globalUnacceptableReferences).ToList();
            List<string> projectReqAndSolutionUnacceptIntersect = requiredReferences.Intersect(solutionUnacceptableReferences).ToList();
            List<string> projectReqGlobalIntersect = requiredReferences.Intersect(globalRequiredReferences).ToList();
            List<string> projectReqSolutionIntersect = requiredReferences.Intersect(solutionRequiredReferences).ToList();

            List<string> projectUnacceptAndGlobalReqIntersect = unacceptableReferences.Intersect(globalRequiredReferences).ToList();
            List<string> projectUnacceptAndSolutionReqIntersect = unacceptableReferences.Intersect(solutionRequiredReferences).ToList();
            List<string> projectUnacceptGlobalIntersect = unacceptableReferences.Intersect(globalUnacceptableReferences).ToList();
            List<string> projectUnacceptSolutionIntersect = unacceptableReferences.Intersect(solutionUnacceptableReferences).ToList();

            List<List<string>> projectGlobalCrossLevelIntersects = new List<List<string>>() { projectReqAndGlobalUnacceptIntersect, projectUnacceptAndGlobalReqIntersect };
            List<List<string>> projectSoluionCrossLevelIntesects = new List<List<string>>() { projectReqAndSolutionUnacceptIntersect, projectUnacceptAndSolutionReqIntersect };
            List<List<string>> projectGlobalStraightLevelIntersects = new List<List<string>>() { projectUnacceptGlobalIntersect, projectReqGlobalIntersect };
            List<List<string>> projectSolutionStraightLevelIntersects = new List<List<string>>() { projectUnacceptSolutionIntersect, projectReqSolutionIntersect };

            AddReferenceMatchErrorsToList(ErrorLevel.Project, projName, false, projectReferencesIntersect);

            AddReferenceMatchWarningsToList(ErrorLevel.Global, ErrorLevel.Project, projName, false, projectGlobalCrossLevelIntersects);
            AddReferenceMatchWarningsToList(ErrorLevel.Solution, ErrorLevel.Project, projName, false, projectSoluionCrossLevelIntesects);

            AddReferenceMatchWarningsToList(ErrorLevel.Global, ErrorLevel.Project, projName, true, projectGlobalStraightLevelIntersects);
            AddReferenceMatchWarningsToList(ErrorLevel.Solution, ErrorLevel.Project, projName, true, projectSolutionStraightLevelIntersects);
        }

        private static void AddReferenceMatchErrorsToList(ErrorLevel referenceLevel, string projName, bool isProjectNameMatchError, List<string> currentIntersect)
        {
            refsMatchErrorList.AddRange(
                currentIntersect.ConvertAll(currentReference =>
                    new ReferenceMatchError(referenceLevel, currentReference, projName, isProjectNameMatchError)
                )
            );
        }

        private static void AddReferenceMatchWarningsToList(
            ErrorLevel highReferenceLevel, ErrorLevel lowReferenceLevel, string projName, bool isReferenceStraight, List<List<string>> currentIntersect
            )
        {
            bool isHighLevelReq = false;
            foreach (List<string> currentCrossLevelIntersect in currentIntersect)
            {
                refsMatchWarningList.AddRange(
                    currentCrossLevelIntersect.ConvertAll(currentReference =>
                        new ReferenceMatchWarning(highReferenceLevel, lowReferenceLevel, currentReference, projName, isReferenceStraight, isHighLevelReq)
                    )
                );

                isHighLevelReq = !isHighLevelReq;
            }
        }

        public static void CheckRulesForProjectReferences(string projName, List<string> projReferences, List<string> configFileReferences, bool isReferenceRequired)
        {
            if (configFileReferences != null)
            {
                foreach (string fileReference in configFileReferences)
                {
                    if ((isReferenceRequired && !projReferences.Contains(fileReference)) ||
                        (!isReferenceRequired && projReferences.Contains(fileReference)))
                    {
                        if (fileReference == projName) //Для Project рефов не допускается совпадение рефа и его проекта. Это "замыкание на себя"
                        {
                            refsMatchErrorList.Add(
                                new ReferenceMatchError(ErrorLevel.Project, fileReference, projName, true)
                                );

                            continue;
                        }

                        //Если реф с таким же названием содежится в MatchError, то пофиг уже на Level: важнеее устранить конфликт рефов, чем вывести по уровню
                        if (refsMatchErrorList.Contains(new ReferenceMatchError(ErrorLevel.Project, fileReference, projName, false), new ReferenceMatchErrorComparer()))
                            continue;

                        refsErrorList.Add(
                            new ReferenceError(fileReference, projName, isReferenceRequired, ErrorLevel.Project)
                            );
                    }
                }
            }
        }

        public static void CheckRulesForSolutionOrGlobalReferences(
            string projName, List<string> projReferences, List<string> currentReferences, ErrorLevel referenceLevel, 
            bool isReferenceRequired, List<List<string>> generalReferences
            )
        {
            if (currentReferences != null)
            {
                foreach (string currentReference in currentReferences)
                {
                    if ((isReferenceRequired && !projReferences.Contains(currentReference)) ||
                        (!isReferenceRequired && projReferences.Contains(currentReference)))
                    {
                        if (refsMatchErrorList.Contains(new ReferenceMatchError(referenceLevel, currentReference, "", false), new ReferenceMatchErrorComparer()))
                            continue;

                        if (IsRuleConflict(currentReference, referenceLevel, generalReferences))
                            continue;

                        if (isReferenceRequired && currentReference == projName)
                            continue;

                        refsErrorList.Add(new ReferenceError(currentReference, projName, isReferenceRequired, referenceLevel));
                    }
                }
            }
        }

        //Перебрать для каждого solution и Global рефа все нижестоящие на предмет противоречий
        private static bool IsRuleConflict(string currentReference, ErrorLevel referenceType, List<List<string>> generalReferences)
        {
            for (int i = 0; i < generalReferences.Count; i++)
            {
                if (referenceType != ErrorLevel.Global && i > 1)
                    break;

                //generalReferences содержит все Project и Solution рефы, которые могут конфликтовать с текущим рефом (0 и 1 - project рефы, 2 и 3 - solution рефы)
                if (generalReferences[i].Contains(currentReference))
                    return true;
            }

            return false;
        }

        public static ReferenceRuleErrors GetReferenceErrors()
        {
            return new ReferenceRuleErrors(refsErrorList, refsMatchErrorList);
        }

        public static List<ReferenceMatchWarning> GetReferenceWarnings()
        {
            return refsMatchWarningList;
        }
    }
}
