using System;
using System.Collections.Generic;
using System.Linq;
using RefDepGuard.Data;
using RefDepGuard.Data.Reference;
using RefDepGuard.Models.Reference;

namespace RefDepGuard.Managers.CheckRules.SubManagers
{
    /// <summary>
    /// This class is responsible for checking the rules related to references in the configuration files. 
    /// It checks for conflicts between required and unacceptable references at both the solution and global levels, as well as at the project level.
    /// </summary>
    public class RefsRuleChecksSubManager
    {
        private static List<ReferenceMatchError> refsMatchErrorList = new List<ReferenceMatchError>();
        private static List<ReferenceError> refsErrorList = new List<ReferenceError>();

        private static List<ReferenceMatchWarning> refsMatchWarningList = new List<ReferenceMatchWarning>();
        private static List<ProjectNotFoundWarning> projectNotFoundWarningsList = new List<ProjectNotFoundWarning>();

        /// <summary>
        /// Clears the lists of reference errors and warnings. This method can be called before performing a new check.
        /// </summary>
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

        /// <summary>
        /// Checks for conflicts between required and unacceptable references at both the solution and global levels.
        /// </summary>
        /// <param name="solutionRequiredReferences">list of strings of solution required references</param>
        /// <param name="solutionUnacceptableReferences">list of strings of solution unacceptable references</param>
        /// <param name="globalRequiredReferences">list of strings of global required references</param>
        /// <param name="globalUnacceptableReferences">list of strings of global unacceptable references</param>
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

            //If there are intersect references between required and unacceptable refs on the same level, it's match error
            //(if intersects are empty then nothing will be added).
            AddReferenceMatchErrorsToList(ProblemLevel.Solution, "", false, solutionReferencesIntersect);
            AddReferenceMatchErrorsToList(ProblemLevel.Global, "", false, globalReferencesIntersect);

            //If there are no match errors, but there are intersects between solution and global refs, it's match warning.
            if (solutionReferencesIntersect.Count == 0 && globalReferencesIntersect.Count == 0)
            {
                AddReferenceMatchWarningsToList(ProblemLevel.Global, ProblemLevel.Solution, "", false, solutionCrossLevelIntersects);
                AddReferenceMatchWarningsToList(ProblemLevel.Global, ProblemLevel.Solution, "", true, solutionStraightLevelIntersects);
            }
        }

        /// <summary>
        /// Checks for conflicts between required and unacceptable references at the project level, as well as conflicts between project references and 
        /// solution/global references.
        /// </summary>
        /// <param name="solutionRequiredReferences">list of strings of solution required references</param>
        /// <param name="solutionUnacceptableReferences">list of strings of solution unacceptable references</param>
        /// <param name="globalRequiredReferences">list of strings of global required references</param>
        /// <param name="globalUnacceptableReferences">list of strings of global unacceptable references</param>
        /// <param name="requiredReferences">list of strings of project required references</param>
        /// <param name="unacceptableReferences">list of strings of project unacceptable references</param>
        /// <param name="projName">relevant project name</param>
        /// <param name="isRequiredHighLevelRefsConsidered">shows whether required global/solution rules is considering or not</param>
        /// <param name="isUnacceptableHighLevelRefsConsidered">shows whether unacceptable global/solution rules is considering or not</param>
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

            if(!isRequiredHighLevelRefsConsidered)//If required global/solution refs is not considering, then we just clear relevant intersects
            {
                projectGlobalCrossLevelIntersects[1].Clear();
                projectSolutionCrossLevelIntesects[1].Clear();
                projectGlobalStraightLevelIntersects[1].Clear();
                projectSolutionStraightLevelIntersects[1].Clear();
            }

            if (!isUnacceptableHighLevelRefsConsidered)//Same for unacceptable refs
            {
                projectGlobalCrossLevelIntersects[0].Clear();
                projectSolutionCrossLevelIntesects[0].Clear();
                projectGlobalStraightLevelIntersects[0].Clear();
                projectSolutionStraightLevelIntersects[0].Clear();
            }

            //Still if there are intersect references between required and unacceptable refs on the project level, it's match error
            AddReferenceMatchErrorsToList(ProblemLevel.Project, projName, false, projectReferencesIntersect);

            //If there are no match errors on the project level, but there are intersects between project refs and solution/global refs, it's match warning.
            if ((isRequiredHighLevelRefsConsidered || isUnacceptableHighLevelRefsConsidered) && projectReferencesIntersect.Count == 0)
            {
                if (refsMatchErrorList.Find(value => value.RuleLevel == ProblemLevel.Global) == null)
                {
                    AddReferenceMatchWarningsToList(ProblemLevel.Global, ProblemLevel.Project, projName, false, projectGlobalCrossLevelIntersects);
                    AddReferenceMatchWarningsToList(ProblemLevel.Global, ProblemLevel.Project, projName, true, projectGlobalStraightLevelIntersects);
                }

                if (refsMatchErrorList.Find(value => value.RuleLevel == ProblemLevel.Solution) == null)
                {
                    AddReferenceMatchWarningsToList(ProblemLevel.Solution, ProblemLevel.Project, projName, false, projectSolutionCrossLevelIntesects);
                    AddReferenceMatchWarningsToList(ProblemLevel.Solution, ProblemLevel.Project, projName, true, projectSolutionStraightLevelIntersects);
                }
            }
        }

        /// <summary>
        /// Checks if the references listed in the project configuration file actually exist in the solution. 
        /// If any reference is found to be not existing, it removes from the list of references and a warning about it is added (project not found warning).
        /// </summary>
        /// <param name="currentRequiredReferences">current project required references</param>
        /// <param name="currentUnacceptableReferences">current project unacceptable references</param>
        /// <param name="currentCommitedProjState">dictionary of the commited projects state</param>
        /// <param name="ruleLevel">current rule level</param>
        /// <param name="projName">current project name (if there is project level)</param>
        /// <returns></returns>
        public static Tuple<List<string>, List<string>> CheckReferencesOnProjectExisting(
            List<string> currentRequiredReferences, List<string> currentUnacceptableReferences, Dictionary<string, ProjectState> currentCommitedProjState, 
            ProblemLevel ruleLevel, string projName = "")
        {
            currentRequiredReferences = CheckReferencesListOnProjectExisting(currentRequiredReferences, currentCommitedProjState, ruleLevel, projName);
            currentUnacceptableReferences = CheckReferencesListOnProjectExisting(currentUnacceptableReferences, currentCommitedProjState, ruleLevel, projName);

            return new Tuple<List<string>, List<string>>(currentRequiredReferences, currentUnacceptableReferences);
        }

        /// <summary>
        /// Checks for conflicts between config files rules and real references of the project. 
        /// If any conflict is found, it adds an error to the list of reference errors.
        /// </summary>
        /// <param name="projName">relevant project name</param>
        /// <param name="projReferences">list of strings with current project references</param>
        /// <param name="configFileReferences">list of strings with current project config file refs</param>
        /// <param name="isReferenceRequired">indicates if ref is required</param>
        public static void CheckRulesForProjectReferences(string projName, List<string> projReferences, List<string> configFileReferences, bool isReferenceRequired)
        {
            if (configFileReferences != null)
            {
                foreach (string fileReference in configFileReferences)//for each refernce from config file
                {
                    if ((isReferenceRequired && !projReferences.Contains(fileReference)) ||
                        (!isReferenceRequired && projReferences.Contains(fileReference)))
                    {//if there is a conflict between config file rules and real project refs
                        if (fileReference == projName) //if reference is the same as project name, it's match error of "slef-locking"
                        {
                            refsMatchWarningList.RemoveAll(value => value.ProjectName == projName); //We delete all match warnings for this project

                            refsMatchErrorList.Add( //and adds match error
                                new ReferenceMatchError(ProblemLevel.Project, fileReference, projName, true)
                                );

                            continue;//still continue checking other refs
                        }

                        //if there is already match error with the same ref name, we don't add reference error, because it's more important to fix match error
                        //than reference error, so we just skip this ref and continue checking other refs
                        if (refsMatchErrorList.Find(error => error.ReferenceName == fileReference) != null)
                            continue;

                        //in all other conflict cases we add reference error
                        refsErrorList.Add(
                            new ReferenceError(fileReference, projName, isReferenceRequired, ProblemLevel.Project)
                            );
                    }
                }
            }
        }

        /// <summary>
        /// Checks for conflicts between config files rules on the solution/global and project levels.
        /// </summary>
        /// <param name="projName">current project name string</param>
        /// <param name="projReferences">list of strings of current project refs</param>
        /// <param name="currentReferences">list of strings of required or unacceptable refs</param>
        /// <param name="referenceLevel">current rule level</param>
        /// <param name="isReferenceRequired">shows if ref is required or not</param>
        /// <param name="generalReferences">lists of strings with config file solution/global refs</param>
        public static void CheckRulesForSolutionOrGlobalReferences(
            string projName, List<string> projReferences, List<string> currentReferences, ProblemLevel referenceLevel,
            bool isReferenceRequired, List<List<string>> generalReferences
            )
        {
            if (currentReferences != null)
            {
                foreach (string currentReference in currentReferences)//for each reference from solution/global refs
                {
                    if ((isReferenceRequired && !projReferences.Contains(currentReference)) ||
                        (!isReferenceRequired && projReferences.Contains(currentReference)))
                    {//if there is a conflict between config file rules and real project refs
                        if (refsMatchErrorList.Find(error => error.ReferenceName == currentReference) != null)
                            continue;

                        if (IsRuleConflict(currentReference, referenceLevel, generalReferences))
                            continue;

                        if (isReferenceRequired && currentReference == projName)
                            continue;

                        //if there is no relevant match error, no "self-lock" and no rule conflict, we add reference error
                        refsErrorList.Add(
                            new ReferenceError(currentReference, projName, isReferenceRequired, referenceLevel)
                            );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the list of reference errors that were found during the checks. 
        /// This method can be called after performing the checks to retrieve the errors and display them to the user.
        /// </summary>
        /// <returns>ReferenceRuleErrors which contains of refsErrorList and refsMatchErrorList</returns>
        public static ReferenceRuleErrors GetReferenceErrors()
        {
            return new ReferenceRuleErrors(refsErrorList, refsMatchErrorList);
        }

        /// <summary>
        /// Gets the list of reference warnings that were found during the checks.
        /// </summary>
        /// <returns>ReferenceRuleWarnings which contains of refsMatchWarningList and projectNotFoundWarningsList</returns>
        public static ReferenceRuleWarnings GetReferenceWarnings()
        {
            return new ReferenceRuleWarnings(refsMatchWarningList, projectNotFoundWarningsList);
        }

        /// <summary>
        /// Checks if the references listed in the project configuration file actually exist in the solution. 
        /// If any reference is found to be not existing, it removes from the list of references and a warning about it is added (project not found warning).
        /// </summary>
        /// <param name="currentReferencesList">list of strings of the current required/unacceptable references</param>
        /// <param name="currentCommitedProjState">commited project state dicitonary</param>
        /// <param name="warningLevel">current rule level</param>
        /// <param name="projName">relevant project name string</param>
        /// <returns>currentReferencesList</returns>
        private static List<string> CheckReferencesListOnProjectExisting(
            List<string> currentReferencesList, Dictionary<string, ProjectState> currentCommitedProjState, ProblemLevel warningLevel, string projName)
        {
            var incorrecltlyReferingRefs = new List<string>();

            foreach (string reference in currentReferencesList)//for each reference from project config file refs
            {
                if (!currentCommitedProjState.ContainsKey(reference))//if there is no project with the same name as reference
                {
                    incorrecltlyReferingRefs.Add(reference);//remember it
                }
            }

            foreach(string incorrRef in incorrecltlyReferingRefs)//for each reference that is incorrecltly refering (to not existing project)
            {
                currentReferencesList.Remove(incorrRef);//remove it from current refs list

                //and add relevant warning
                var currentPNFWarningInstance = new ProjectNotFoundWarning(incorrRef, warningLevel, projName);

                if(projectNotFoundWarningsList.Find(warning => 
                    warning.ReferenceName == incorrRef && warning.WarningLevel == warningLevel && warning.ProjName == projName) == null
                )
                    projectNotFoundWarningsList.Add(currentPNFWarningInstance);
            }

            return currentReferencesList;
        }

        /// <summary>
        /// Adds reference match errors to their list. 
        /// This method is used to add errors when there are conflicts between required and unacceptable references at the same level.
        /// </summary>
        /// <param name="referenceLevel">current ref level</param>
        /// <param name="projName">relevant proj name</param>
        /// <param name="isProjectNameMatchError">if it's "self-locking"</param>
        /// <param name="currentIntersect">list of string with current intersect (union refs)</param>
        private static void AddReferenceMatchErrorsToList(ProblemLevel referenceLevel, string projName, bool isProjectNameMatchError, List<string> currentIntersect)
        {
            refsMatchErrorList.AddRange(
                currentIntersect.ConvertAll(currentReference =>
                    new ReferenceMatchError(referenceLevel, currentReference, projName, isProjectNameMatchError)
                )
            );
        }

        /// <summary>
        /// Adds reference match warnings to their list. 
        /// This method is used to add warnings when there are conflicts between required and unacceptable references at different levels.
        /// </summary>
        /// <param name="highReferenceLevel">"higher" rule level</param>
        /// <param name="lowReferenceLevel">"lower" rule level</param>
        /// <param name="projName">relevant project name</param>
        /// <param name="isReferenceStraight">shows whether is straight match warning</param>
        /// <param name="currentIntersect">list of string with current intersect (union refs)</param>
        private static void AddReferenceMatchWarningsToList(
            ProblemLevel highReferenceLevel, ProblemLevel lowReferenceLevel, string projName, bool isReferenceStraight, List<List<string>> currentIntersect
            )
        {
            bool isHighLevelReq = false;
            foreach (List<string> currentCrossLevelIntersect in currentIntersect)//for each intersect between higher and lower level refs
            {
                refsMatchWarningList.AddRange(
                    currentCrossLevelIntersect.ConvertAll(currentReference =>
                        new ReferenceMatchWarning(highReferenceLevel, lowReferenceLevel, currentReference, projName, isReferenceStraight, isHighLevelReq)
                    )
                );

                isHighLevelReq = !isHighLevelReq;
            }
        }

        /// <summary>
        /// Checks if there is a conflict between the current reference and the references of the lower levels.
        /// </summary>
        /// <param name="currentReference">current ref string</param>
        /// <param name="referenceLevel">current ref level</param>
        /// <param name="generalReferences">contains all project/solution possible conflict refs (0, 1 - project refs; 2, 3 - solution refs)</param>
        /// <returns>whether is this current ref has conflicts with lower levels rules</returns>
        private static bool IsRuleConflict(string currentReference, ProblemLevel referenceLevel, List<List<string>> generalReferences)
        {
            for (int i = 0; i < generalReferences.Count; i++)
            {
                if (referenceLevel != ProblemLevel.Global && i > 1)
                    break;

                //generalReferences содержит все Project и Solution рефы, которые могут конфликтовать с текущим рефом (0 и 1 - project рефы, 2 и 3 - solution рефы)
                if (generalReferences[i].Contains(currentReference))
                    return true;
            }

            return false;
        }
    }
}