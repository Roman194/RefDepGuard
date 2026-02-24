using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using RefDepGuard.Comparators.ContainsComparators;
using RefDepGuard.Data;
using RefDepGuard.Data.Reference;
using RefDepGuard.Models.Reference;

namespace RefDepGuard.Managers.CheckRules.SubManagers
{
    public class RefsRuleChecksSubManager
    {
        private static List<ReferenceMatchError> refsMatchErrorList = new List<ReferenceMatchError>();
        private static List<ReferenceError> refsErrorList = new List<ReferenceError>();

        private static List<ReferenceMatchWarning> refsMatchWarningList = new List<ReferenceMatchWarning>();
        private static List<ProjectNotFoundWarning> projectNotFoundWarningsList = new List<ProjectNotFoundWarning>();

        public static void ClearRefsErrorsAndWarnings()
        {
            if (refsErrorList != null)
                refsErrorList.Clear();

            if (refsMatchErrorList != null)
                refsMatchErrorList.Clear();

            if (refsMatchWarningList != null)
                refsMatchWarningList.Clear();

            if(projectNotFoundWarningsList != null)
                projectNotFoundWarningsList.Clear();
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

            AddReferenceMatchErrorsToList(ProblemLevel.Solution, "", false, solutionReferencesIntersect);
            AddReferenceMatchErrorsToList(ProblemLevel.Global, "", false, globalReferencesIntersect);

            //Выводим match warning только если нет match error
            if(solutionReferencesIntersect.Count == 0 && globalReferencesIntersect.Count == 0)
            {
                AddReferenceMatchWarningsToList(ProblemLevel.Global, ProblemLevel.Solution, "", false, solutionCrossLevelIntersects);
                AddReferenceMatchWarningsToList(ProblemLevel.Global, ProblemLevel.Solution, "", true, solutionStraightLevelIntersects);
            }
        }

        public static void CheckProjectRulesOnMatchConflicts(
            List<string> solutionRequiredReferences, List<string> solutionUnacceptableReferences, List<string> globalRequiredReferences, 
            List<string> globalUnacceptableReferences, List<string> requiredReferences, List<string> unacceptableReferences, string projName,
            bool isRequiredHighLevelRefsConsidered, bool isUnacceptableHighLevelRefsConsidered
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
            List<List<string>> projectSolutionCrossLevelIntesects = new List<List<string>>() { projectReqAndSolutionUnacceptIntersect, projectUnacceptAndSolutionReqIntersect };
            List<List<string>> projectGlobalStraightLevelIntersects = new List<List<string>>() { projectUnacceptGlobalIntersect, projectReqGlobalIntersect };
            List<List<string>> projectSolutionStraightLevelIntersects = new List<List<string>>() { projectUnacceptSolutionIntersect, projectReqSolutionIntersect };

            if(!isRequiredHighLevelRefsConsidered)
            {
                projectGlobalCrossLevelIntersects[1].Clear();
                projectSolutionCrossLevelIntesects[1].Clear();
                projectGlobalStraightLevelIntersects[1].Clear();
                projectSolutionStraightLevelIntersects[1].Clear();
            }

            if (!isUnacceptableHighLevelRefsConsidered)
            {
                projectGlobalCrossLevelIntersects[0].Clear();
                projectSolutionCrossLevelIntesects[0].Clear();
                projectGlobalStraightLevelIntersects[0].Clear();
                projectSolutionStraightLevelIntersects[0].Clear();
            }

            AddReferenceMatchErrorsToList(ProblemLevel.Project, projName, false, projectReferencesIntersect);
            
            //В зависимости от параметров учёта global и solution правил выводятся те или иные match warning
            if((isRequiredHighLevelRefsConsidered || isUnacceptableHighLevelRefsConsidered) && projectReferencesIntersect.Count == 0)
            {
                if (refsMatchErrorList.Find(value => value.ReferenceLevelValue == ProblemLevel.Global) == null)
                {
                    AddReferenceMatchWarningsToList(ProblemLevel.Global, ProblemLevel.Project, projName, false, projectGlobalCrossLevelIntersects);
                    AddReferenceMatchWarningsToList(ProblemLevel.Global, ProblemLevel.Project, projName, true, projectGlobalStraightLevelIntersects);
                }

                if (refsMatchErrorList.Find(value => value.ReferenceLevelValue == ProblemLevel.Solution) == null)
                {
                    AddReferenceMatchWarningsToList(ProblemLevel.Solution, ProblemLevel.Project, projName, false, projectSolutionCrossLevelIntesects);
                    AddReferenceMatchWarningsToList(ProblemLevel.Solution, ProblemLevel.Project, projName, true, projectSolutionStraightLevelIntersects);
                }
            }
        }

        public static Tuple<List<string>, List<string>> CheckReferencesOnProjectExisting(
            List<string> currentRequiredReferences, List<string> currentUnacceptableReferences, Dictionary<string, ProjectState> currentCommitedProjState, 
            ProblemLevel errorLevel, string projName = "")
        {
            currentRequiredReferences = CheckReferencesListOnProjectExisting(currentRequiredReferences, currentCommitedProjState, errorLevel, projName);
            currentUnacceptableReferences = CheckReferencesListOnProjectExisting(currentUnacceptableReferences, currentCommitedProjState, errorLevel, projName);

            return new Tuple<List<string>, List<string>>(currentRequiredReferences, currentUnacceptableReferences);
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
                            refsMatchWarningList.RemoveAll(value => value.ProjectName == projName); //Удалить все Match Warning для замыкающегося проекта

                            refsMatchErrorList.Add(
                                new ReferenceMatchError(ProblemLevel.Project, fileReference, projName, true)
                                );

                            continue;
                        }

                        //Если реф с таким же названием содежится в MatchError, то пофиг уже на Level: важнеее устранить конфликт рефов, чем вывести по уровню
                        if (refsMatchErrorList.Contains(new ReferenceMatchError(ProblemLevel.Project, fileReference, projName, false), new ReferenceMatchErrorComparer()))
                            continue;

                        refsErrorList.Add(
                            new ReferenceError(fileReference, projName, isReferenceRequired, ProblemLevel.Project)
                            );
                    }
                }
            }
        }

        public static void CheckRulesForSolutionOrGlobalReferences(
            string projName, List<string> projReferences, List<string> currentReferences, ProblemLevel referenceLevel,
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
        
        public static ReferenceRuleErrors GetReferenceErrors()
        {
            return new ReferenceRuleErrors(refsErrorList, refsMatchErrorList);
        }

        public static ReferenceRuleWarnings GetReferenceWarnings()
        {
            return new ReferenceRuleWarnings(refsMatchWarningList, projectNotFoundWarningsList);
        }

        private static List<string> CheckReferencesListOnProjectExisting(
            List<string> currentReferencesList, Dictionary<string, ProjectState> currentCommitedProjState, ProblemLevel errorLevel, string projName)
        {
            var incorrecltlyReferingRefs = new List<string>();

            foreach (string reference in currentReferencesList)
            {
                if (!currentCommitedProjState.ContainsKey(reference))
                {
                    incorrecltlyReferingRefs.Add(reference);
                }
            }

            foreach(string incorrRef in incorrecltlyReferingRefs)
            {
                currentReferencesList.Remove(incorrRef);

                var currentPNFWarningInstance = new ProjectNotFoundWarning(incorrRef, errorLevel, projName);

                if (!projectNotFoundWarningsList.Contains(currentPNFWarningInstance, new ProjectNotFoundContainsComparer()))
                {
                    projectNotFoundWarningsList.Add(currentPNFWarningInstance);
                }
            }

            return currentReferencesList;
        }

        private static void AddReferenceMatchErrorsToList(ProblemLevel referenceLevel, string projName, bool isProjectNameMatchError, List<string> currentIntersect)
        {
            refsMatchErrorList.AddRange(
                currentIntersect.ConvertAll(currentReference =>
                    new ReferenceMatchError(referenceLevel, currentReference, projName, isProjectNameMatchError)
                )
            );
        }

        private static void AddReferenceMatchWarningsToList(
            ProblemLevel highReferenceLevel, ProblemLevel lowReferenceLevel, string projName, bool isReferenceStraight, List<List<string>> currentIntersect
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

        //Перебрать для каждого solution и Global рефа все нижестоящие на предмет противоречий
        private static bool IsRuleConflict(string currentReference, ProblemLevel referenceType, List<List<string>> generalReferences)
        {
            for (int i = 0; i < generalReferences.Count; i++)
            {
                if (referenceType != ProblemLevel.Global && i > 1)
                    break;

                //generalReferences содержит все Project и Solution рефы, которые могут конфликтовать с текущим рефом (0 и 1 - project рефы, 2 и 3 - solution рефы)
                if (generalReferences[i].Contains(currentReference))
                    return true;
            }

            return false;
        }
    }
}
