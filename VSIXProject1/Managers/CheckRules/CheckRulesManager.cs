using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VSIXProject1.Comparators;
using VSIXProject1.Data;
using VSIXProject1.Data.ConfigFile;
using VSIXProject1.Data.FrameworkVersion;
using VSIXProject1.Data.Reference;
using VSIXProject1.Managers.CheckRules;

namespace VSIXProject1
{
    public class CheckRulesManager
    {
        static List<ConfigFilePropertyNullError> configPropertyNullErrorList = new List<ConfigFilePropertyNullError>();
        static List<ReferenceMatchError> refsMatchErrorList = new List<ReferenceMatchError>();
        static List<ReferenceError> refsErrorList = new List<ReferenceError>();

        static List<FrameworkVersionComparabilityError> frameworkVersionComparabilityErrorList = new List<FrameworkVersionComparabilityError>();
        static List<MaxFrameworkVersionDeviantValueError> maxFrameworkVersionDeviantValueList = new List<MaxFrameworkVersionDeviantValueError>();

        static List<string> untypedErrorsList = new List<string>();

        static List<ReferenceMatchWarning> refsMatchWarningList = new List<ReferenceMatchWarning>();
        static List<MaxFrameworkVersionConflictWarning> maxFrameworkVersionConflictWarningsList = new List<MaxFrameworkVersionConflictWarning>();
        static List<MaxFrameworkVersionReferenceConflictWarning> maxFrameworkVersionReferenceConflictWarningsList = new List<MaxFrameworkVersionReferenceConflictWarning>();

        static List<RequiredReference> requiredReferencesList = new List<RequiredReference>();
        static Dictionary<string, RequiredMaxFrVersion> requiredMaxFrVersionsDict = new Dictionary<string, RequiredMaxFrVersion>();

        static RefDepGuardErrors refDepGuardErrors; //Реализовать работу с этой структурой вместо верхних трёх-пяти?
        static RefDepGuardWarnings refDepGuardWarnings;
        static RequiredParameters requiredExportParameters;

        static RefDepGuardFindedProblems refDepGuardFindedProblems;

        public static RefDepGuardExportParameters CheckRulesFromConfigFiles(ConfigFilesData configFilesData, ErrorListProvider errorListProvider, Dictionary<string, ProjectState> currentCommitedProjState)
        {
            ConfigFileGlobal configFileGlobal = configFilesData.configFileGlobal;
            ConfigFileSolution configFileSolution = configFilesData.configFileSolution;

            ClearErrorAndWarningLists();

            NotNullChecksSubManager.CheckConfigPropertiesOnNotNull(configFilesData, configPropertyNullErrorList);

            var maxGlobalFrameworkVersionByTypes = GetMaxFrameworkVersionDictionaryByTypes(configFileGlobal?.framework_max_version ?? "-", ErrorLevel.Global);
            var maxSolutionFrameworkVersionByTypes = GetMaxFrameworkVersionDictionaryByTypes(configFileSolution?.framework_max_version ?? "-", ErrorLevel.Solution);

            //CheckMaxFrameworkVersionOneLevelConflict(maxGlobalFrameworkVersionByTypes, ErrorLevel.Global);
            (maxFrameworkVersionConflictWarningsList, maxFrameworkVersionReferenceConflictWarningsList)  = MaxFrameworkRulesChecksSubManager.CheckMaxFrameworkVersionOneLevelConflict(maxSolutionFrameworkVersionByTypes, maxFrameworkVersionConflictWarningsList, maxFrameworkVersionReferenceConflictWarningsList, ErrorLevel.Global);
            //CheckMaxFrameworkVersionOneLevelConflict(maxSolutionFrameworkVersionByTypes, ErrorLevel.Solution);
            (maxFrameworkVersionConflictWarningsList, maxFrameworkVersionReferenceConflictWarningsList) =  MaxFrameworkRulesChecksSubManager.CheckMaxFrameworkVersionOneLevelConflict(maxSolutionFrameworkVersionByTypes, maxFrameworkVersionConflictWarningsList, maxFrameworkVersionReferenceConflictWarningsList, ErrorLevel.Solution);

            List<string> solutionRequiredReferences = configFileSolution?.solution_required_references ?? new List<string>();
            List<string> solutionUnacceptableReferences = configFileSolution?.solution_unacceptable_references ?? new List<string>();

            List<string> globalRequiredReferences = configFileGlobal?.global_required_references ?? new List<string>();
            List<string> globalUnacceptableReferences = configFileGlobal?.global_unacceptable_references ?? new List<string>();

            List<ReferenceAffiliation> unionSolutionAndGlobalReferencesByType = new List<ReferenceAffiliation>
            {
                new ReferenceAffiliation(ErrorLevel.Solution, solutionRequiredReferences, solutionUnacceptableReferences),
                new ReferenceAffiliation(ErrorLevel.Global, globalRequiredReferences, globalUnacceptableReferences)
            };

            requiredReferencesList.AddRange(globalRequiredReferences.ConvertAll(value => new RequiredReference(value, "")));
            requiredReferencesList.AddRange(solutionRequiredReferences.ConvertAll(value => new RequiredReference(value, "")));

            CheckRulesOnMatchConflicts(solutionRequiredReferences, solutionUnacceptableReferences, globalRequiredReferences, globalUnacceptableReferences);

            if (maxGlobalFrameworkVersionByTypes.Count > 0 && maxSolutionFrameworkVersionByTypes.Count > 0)//проверка на противоречие с global
                CheckPrjMaxFrwrkVrsnDifferentLevelsConflicts(maxSolutionFrameworkVersionByTypes, maxGlobalFrameworkVersionByTypes, "", ErrorLevel.Solution, ErrorLevel.Global);

            foreach (KeyValuePair<string, ProjectState> currentProjState in currentCommitedProjState)//для каждого project
            {
                var projName = currentProjState.Key;
                var projReferences = currentProjState.Value.CurrentReferences;
                var projFrameworkVersion = currentProjState.Value.CurrentFrameworkVersion;

                if (configFilesData.configFileSolution?.projects?.ContainsKey(projName) ?? false)
                {
                    ConfigFileProject currentProjectConfigFileSettings = configFileSolution.projects[projName];

                    bool isConsiderRequiredReferences = currentProjectConfigFileSettings.consider_global_and_solution_references?.required ?? true; //Проверка на отключение глобальных и solution рефов для проекта
                    bool isConsiderUnacceptableReferences = currentProjectConfigFileSettings.consider_global_and_solution_references?.unacceptable ?? true;

                    var maxFrameworkVersionByTypes = GetMaxFrameworkVersionDictionaryByTypes(currentProjectConfigFileSettings?.framework_max_version ?? "-", ErrorLevel.Project, projName);
                    (maxFrameworkVersionConflictWarningsList, maxFrameworkVersionReferenceConflictWarningsList) = MaxFrameworkRulesChecksSubManager.CheckMaxFrameworkVersionOneLevelConflict(maxFrameworkVersionByTypes, maxFrameworkVersionConflictWarningsList, maxFrameworkVersionReferenceConflictWarningsList, ErrorLevel.Project, projName);

                    List<string> requiredReferences = currentProjectConfigFileSettings?.required_references ?? new List<string>();
                    List<string> unacceptableReferences = currentProjectConfigFileSettings?.unacceptable_references ?? new List<string>();

                    List<List<string>> configFileProjectAndSolutionReferences = new List<List<string>>
                    {
                        requiredReferences, unacceptableReferences, solutionRequiredReferences, solutionUnacceptableReferences
                    };

                    requiredReferencesList.AddRange(requiredReferences.ConvertAll(value => new RequiredReference(value, projName)));

                    CheckProjectRulesOnMatchConflicts(solutionRequiredReferences, solutionUnacceptableReferences, globalRequiredReferences,
                        globalUnacceptableReferences, requiredReferences, unacceptableReferences, projName);

                    CheckRulesForProjectReferences(projName, projReferences, requiredReferences, true);
                    CheckRulesForProjectReferences(projName, projReferences, unacceptableReferences, false);

                    foreach (ReferenceAffiliation referenceAffiliation in unionSolutionAndGlobalReferencesByType)
                    {
                        if (isConsiderRequiredReferences)//если заявлено
                            //применяем глобальные референсы
                            CheckRulesForSolutionOrGlobalReferences(projName, projReferences, referenceAffiliation.RequiredReferences, referenceAffiliation.ReferenceTypeValue, true, configFileProjectAndSolutionReferences);

                        if (isConsiderUnacceptableReferences)
                            CheckRulesForSolutionOrGlobalReferences(projName, projReferences, referenceAffiliation.UnacceptableReferences, referenceAffiliation.ReferenceTypeValue, false, configFileProjectAndSolutionReferences);
                    }

                    if (maxFrameworkVersionByTypes.Count == 0)
                    {
                        if (maxSolutionFrameworkVersionByTypes.Count > 0)
                        {
                            CheckProjectTargetFrameworkVersion(projFrameworkVersion, maxSolutionFrameworkVersionByTypes, projName, ErrorLevel.Solution, maxGlobalFrameworkVersionByTypes);
                        }
                        else
                        {
                            if (maxGlobalFrameworkVersionByTypes.Count > 0)
                                CheckProjectTargetFrameworkVersion(projFrameworkVersion, maxGlobalFrameworkVersionByTypes, projName, ErrorLevel.Global);
                        }
                    }
                    else//Проверить на противоречие с уровнем solution и global
                    {
                        if (maxSolutionFrameworkVersionByTypes.Count > 0)
                            CheckPrjMaxFrwrkVrsnDifferentLevelsConflicts(maxFrameworkVersionByTypes, maxSolutionFrameworkVersionByTypes, projName, ErrorLevel.Project, ErrorLevel.Solution);

                        if (maxGlobalFrameworkVersionByTypes.Count > 0)
                            CheckPrjMaxFrwrkVrsnDifferentLevelsConflicts(maxFrameworkVersionByTypes, maxGlobalFrameworkVersionByTypes, projName, ErrorLevel.Project, ErrorLevel.Global);

                        CheckProjectTargetFrameworkVersion(projFrameworkVersion, maxFrameworkVersionByTypes, projName, ErrorLevel.Project);
                    }

                    CheckProjectReferencesOnPotentialReferencesFrameworkVersionConflict(projName, projReferences);
                }
                else
                {

                    //Проект есть в solution но его нет в config
                }

                //А что делать если проекта нет в solution, но он есть в config?
                //Рассмотреть в т.ч. случаи когда свойство projects пустое

            }

            refsErrorList.Sort(new ReferenceErrorComparer());
            refsMatchErrorList.Sort(new ReferenceMatchErrorSortComparer());
            configPropertyNullErrorList.Sort(new ConfigFilePropertyNullErrorSortComparer());
            maxFrameworkVersionDeviantValueList.Sort(new MaxFrameworkVersionDeviantValueSortComparer());
            frameworkVersionComparabilityErrorList.Sort(new FrameworkVersionComparabilityErrorSortComparer());

            refDepGuardErrors = new RefDepGuardErrors(
                configPropertyNullErrorList, refsErrorList, refsMatchErrorList,maxFrameworkVersionDeviantValueList, 
                frameworkVersionComparabilityErrorList, untypedErrorsList
                );
            refDepGuardWarnings = new RefDepGuardWarnings(refsMatchWarningList, maxFrameworkVersionConflictWarningsList, maxFrameworkVersionReferenceConflictWarningsList);
            refDepGuardFindedProblems = new RefDepGuardFindedProblems(refDepGuardWarnings, refDepGuardErrors);

            ELPStoreSubManager.StoreErrorListProviderByValues(refDepGuardFindedProblems, configFilesData.solutionName, errorListProvider);

            requiredExportParameters = new RequiredParameters(requiredReferencesList, requiredMaxFrVersionsDict);

            return new RefDepGuardExportParameters(refDepGuardFindedProblems, requiredExportParameters);
        }

        private static void ClearErrorAndWarningLists()
        {
            if (untypedErrorsList != null)
                untypedErrorsList.Clear();

            if (configPropertyNullErrorList != null)
                configPropertyNullErrorList.Clear();

            if (refsErrorList != null)
                refsErrorList.Clear();

            if (refsMatchErrorList != null)
                refsMatchErrorList.Clear();

            if (maxFrameworkVersionConflictWarningsList != null)
                maxFrameworkVersionConflictWarningsList.Clear();

            if (maxFrameworkVersionReferenceConflictWarningsList != null)
                maxFrameworkVersionReferenceConflictWarningsList.Clear();

            if (maxFrameworkVersionDeviantValueList != null)
                maxFrameworkVersionDeviantValueList.Clear();

            if (frameworkVersionComparabilityErrorList != null)
                frameworkVersionComparabilityErrorList.Clear();

            if (refsMatchWarningList != null)
                refsMatchWarningList.Clear();

            if (requiredReferencesList != null)
                requiredReferencesList.Clear();

        }

        //private static void CheckConfigPropertiesOnNotNull(ConfigFilesData configFilesData)
        //{
        //    ConfigFileGlobal configFileGlobal = configFilesData.configFileGlobal;
        //    ConfigFileSolution configFileSolution = configFilesData.configFileSolution;

        //    if (configFileSolution != null)
        //    {
        //        CheckConfigFileSolutionProperties(configFileSolution);

        //        if (configFileSolution.projects != null)
        //        {
        //            foreach (var project in configFileSolution.projects)
        //            {
        //                if (project.Value != null)
        //                    CheckConfigFileProjectProperties(project.Key, project.Value);

        //                else
        //                    configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("project_value", false, project.Key));
        //            }
        //        }
        //        else
        //            configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("projects", false, ""));
        //    }
        //    else
        //        configPropertyNullErrorList.Add(new ConfigFilePropertyNullError(configFilesData.solutionName, false, ""));


        //    if (configFileGlobal != null)
        //        CheckConfigFileGlobalProperties(configFileGlobal);
        //    else
        //        configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("Global", true, ""));
        //}

        //private static void CheckConfigFileSolutionProperties(ConfigFileSolution configFileSolution) //How to make it better? Reflection doesn't work
        //{
        //    if (configFileSolution.name is null)
        //        configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("name", false, ""));

        //    if (configFileSolution.framework_max_version is null)
        //        configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("framework_max_version", false, ""));

        //    if (configFileSolution.solution_required_references is null)
        //        configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("solution_required_references", false, ""));

        //    if (configFileSolution.solution_unacceptable_references is null)
        //        configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("solution_unacceptable_references", false, ""));
        //}

        //private static void CheckConfigFileProjectProperties(string projectKey, ConfigFileProject currentProject)
        //{
        //    if (currentProject.framework_max_version is null)
        //        configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("framework_max_version", false, projectKey));

        //    if (currentProject.consider_global_and_solution_references is null)
        //        configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("consider_global_and_solution_references", false, projectKey));

        //    if (currentProject.required_references is null)
        //        configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("required_references", false, projectKey));

        //    if (currentProject.unacceptable_references is null)
        //        configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("unacceptable_references", false, projectKey));
        //}

        //private static void CheckConfigFileGlobalProperties(ConfigFileGlobal configFileGlobal)
        //{
        //    if (configFileGlobal.name is null)
        //        configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("name", true, ""));

        //    if (configFileGlobal.framework_max_version is null)
        //        configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("framework_max_version", true, ""));

        //    if (configFileGlobal.global_required_references is null)
        //        configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("global_required_references", true, ""));

        //    if (configFileGlobal.global_unacceptable_references is null)
        //        configPropertyNullErrorList.Add(new ConfigFilePropertyNullError("global_unacceptable_references", true, ""));
        //}

        private static Dictionary<string, List<int>> GetMaxFrameworkVersionDictionaryByTypes(string currentMaxFrameworkVersion, ErrorLevel errorLevel, string projName = "")
        {
            if (currentMaxFrameworkVersion == "-")
                return new Dictionary<string, List<int>>();


            if ((currentMaxFrameworkVersion.Contains(';') || currentMaxFrameworkVersion.Contains(':')) && errorLevel == ErrorLevel.Project)
            {
                //Выкинуть ошибку о некорректном формате (На уровне project не допускается перечисление версий фреймворка)

                MaxFrameworkVersionDeviantValueError potentialMaxFrameworkVersionDeviantValueError = new MaxFrameworkVersionDeviantValueError(errorLevel, projName);

                if (!maxFrameworkVersionDeviantValueList.Contains(potentialMaxFrameworkVersionDeviantValueError, new MaxFrameworkVersionDeviantValueContainsComparer()))
                    maxFrameworkVersionDeviantValueList.Add(potentialMaxFrameworkVersionDeviantValueError);

                return new Dictionary<string, List<int>>();
            }


            var currentMaxFrameworkVersionArray = currentMaxFrameworkVersion.Split(';');
            var maxFrameworkDictionary = new Dictionary<string, List<int>>();

            foreach (string maxFrameworkVersionElement in currentMaxFrameworkVersionArray) //Для каждого из ограничений
            {
                string maxFrameworkVersion = maxFrameworkVersionElement;
                if (!maxFrameworkVersion.Contains(':'))
                {
                    maxFrameworkVersion = "all:" + maxFrameworkVersion;
                }

                var maxFrameworkVersionElementSplited = maxFrameworkVersion.Replace(" ", "").Split(':');

                if (String.IsNullOrEmpty(maxFrameworkVersionElementSplited[0])) //Если не указано название типа фреймворка
                {
                    //Выкинуть ошибку о некорректном формате

                    MaxFrameworkVersionDeviantValueError potentialMaxFrameworkVersionDeviantValueError = new MaxFrameworkVersionDeviantValueError(errorLevel, "");

                    if (!maxFrameworkVersionDeviantValueList.Contains(potentialMaxFrameworkVersionDeviantValueError, new MaxFrameworkVersionDeviantValueContainsComparer()))
                        maxFrameworkVersionDeviantValueList.Add(potentialMaxFrameworkVersionDeviantValueError);

                    return new Dictionary<string, List<int>>();
                }

                var maxFrameworkVersionNumbers = maxFrameworkVersionElementSplited[1].Split('.');
                var maxFrameworkVersionNumsList = new List<int>();

                foreach (var maxFrameworkVersionNumber in maxFrameworkVersionNumbers)
                {
                    int maxVersionCurrentNum;
                    if (!Int32.TryParse(maxFrameworkVersionNumber, out maxVersionCurrentNum))//Попытка парсинга очередного числа вресии макс фреймворка
                    {
                        //Ошибка когда найдено некорректное значение max_framework_version в config-файле 
                        MaxFrameworkVersionDeviantValueError potentialMaxFrameworkVersionDeviantValueError = new MaxFrameworkVersionDeviantValueError(errorLevel, projName);
                        if (errorLevel == ErrorLevel.Project)
                        {
                            maxFrameworkVersionDeviantValueList.Add(potentialMaxFrameworkVersionDeviantValueError);
                        }
                        else
                        {
                            if (!maxFrameworkVersionDeviantValueList.Contains(potentialMaxFrameworkVersionDeviantValueError, new MaxFrameworkVersionDeviantValueContainsComparer()))
                                maxFrameworkVersionDeviantValueList.Add(potentialMaxFrameworkVersionDeviantValueError);
                        }

                        return new Dictionary<string, List<int>>();
                    }
                    maxFrameworkVersionNumsList.Add(maxVersionCurrentNum);
                }

                maxFrameworkDictionary.Add(maxFrameworkVersionElementSplited[0], maxFrameworkVersionNumsList);

            }

            return maxFrameworkDictionary;
        }

        private static void CheckMaxFrameworkVersionOneLevelConflict(Dictionary<string, List<int>> currentMaxFrameworkVersion, ErrorLevel ruleLevel, string projName = "-")
        {
            if (currentMaxFrameworkVersion.ContainsKey("all")) //Проверки на противоречия в правилах макс фреймворков одного уровня
            {
                List<int> maxAllTypeFrameworkVersionArray = currentMaxFrameworkVersion["all"];

                foreach (var currentMaxLowLevelFrameworkVersion in currentMaxFrameworkVersion)
                {
                    if (currentMaxLowLevelFrameworkVersion.Key != "all")
                    {
                        List<int> maxCurrentTypeFrameworkVersionArray = currentMaxLowLevelFrameworkVersion.Value;
                        //Возможно это даже ошибка, а не предупреждение
                        CheckMaxFrameworkVersionCurrentConflict(maxAllTypeFrameworkVersionArray, maxCurrentTypeFrameworkVersionArray, projName, ruleLevel, ruleLevel);
                    }
                }
            }
        }

        private static void CheckMaxFrameworkVersionCurrentConflict(List<int> maxHighLevelFrameworkVersionList, List<int> maxLowLevelFrameworkVersionList, string projName, ErrorLevel lowRuleLevel, ErrorLevel highRuleLevel, string refName = "")
        {
            var maxHighLevelFrameworkVersionArrayLength = maxHighLevelFrameworkVersionList.Count;
            var maxLowLevelFrameworkVersionArrayLength = maxLowLevelFrameworkVersionList.Count;

            var minLengthValue = Math.Min(maxLowLevelFrameworkVersionArrayLength, maxHighLevelFrameworkVersionArrayLength);

            for (int i = 0; i < minLengthValue; i++)
            {
                int currentLowLevelFrameworkVersionNum = maxLowLevelFrameworkVersionList[i];
                int currentHighLevelFrameworkVersionNum = maxHighLevelFrameworkVersionList[i];

                if (currentHighLevelFrameworkVersionNum < currentLowLevelFrameworkVersionNum)
                {
                    var maxHighLevelFrameworkVersionString = GetFrameworkVersionString(maxHighLevelFrameworkVersionList.ConvertAll(num => num.ToString()));
                    var maxLowLevelFrameworkVersionString = GetFrameworkVersionString(maxLowLevelFrameworkVersionList.ConvertAll(num => num.ToString()));

                    if (lowRuleLevel != ErrorLevel.Undefined)
                        AddNewMaxFrameworkVersionConflictWarning(maxHighLevelFrameworkVersionString, maxLowLevelFrameworkVersionString, projName, lowRuleLevel, highRuleLevel);
                    else
                        AddNewMaxFrameworkVersionOnReferenceConflictWarning(maxHighLevelFrameworkVersionString, maxLowLevelFrameworkVersionString, projName, refName);

                    return;
                }
                else
                {
                    if (currentHighLevelFrameworkVersionNum > currentLowLevelFrameworkVersionNum)
                        return;
                }
            }

            if (maxHighLevelFrameworkVersionArrayLength < maxLowLevelFrameworkVersionArrayLength)
            {
                for (int i = 0; i < maxLowLevelFrameworkVersionArrayLength; i++)
                {
                    int currentLowLevelFrameworkVersionNum = maxLowLevelFrameworkVersionList[i];

                    if (currentLowLevelFrameworkVersionNum > 0)
                    {
                        //Warning о противоречии между рефами
                        var maxHighLevelFrameworkVersionString = GetFrameworkVersionString(maxHighLevelFrameworkVersionList.ConvertAll(num => num.ToString()));
                        var maxLowLevelFrameworkVersionString = GetFrameworkVersionString(maxLowLevelFrameworkVersionList.ConvertAll(num => num.ToString()));

                        if (lowRuleLevel != ErrorLevel.Undefined)
                            AddNewMaxFrameworkVersionConflictWarning(maxHighLevelFrameworkVersionString, maxLowLevelFrameworkVersionString, projName, lowRuleLevel, highRuleLevel);
                        else
                            AddNewMaxFrameworkVersionOnReferenceConflictWarning(maxHighLevelFrameworkVersionString, maxLowLevelFrameworkVersionString, projName, refName);

                        break;
                    }
                }
            }
        }

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

        private static void AddNewMaxFrameworkVersionConflictWarning(string maxHighLevelFrameworkVersionString, string maxLowLevelFrameworkVersionString, string projName, ErrorLevel lowRuleLevel, ErrorLevel highRuleLevel)
        {

            //Warning о противоречии между рефами
            var potentialMaxFrameworkVersionConflictWarning = new MaxFrameworkVersionConflictWarning(highRuleLevel, lowRuleLevel, maxHighLevelFrameworkVersionString, maxLowLevelFrameworkVersionString, projName);

            if (lowRuleLevel == ErrorLevel.Project)
            {
                maxFrameworkVersionConflictWarningsList.Add(potentialMaxFrameworkVersionConflictWarning);
                return;
            }

            if (!maxFrameworkVersionConflictWarningsList.Contains(potentialMaxFrameworkVersionConflictWarning, new MaxFrameworkVersionConflictWarningContainsComparer()))
                maxFrameworkVersionConflictWarningsList.Add(potentialMaxFrameworkVersionConflictWarning);
        }

        private static void AddNewMaxFrameworkVersionOnReferenceConflictWarning(string maxProjNameFrameworkVersionString, string maxRefNameFrameworkVersionString, string projName, string refName)
        {
            maxFrameworkVersionReferenceConflictWarningsList.Add(
                new MaxFrameworkVersionReferenceConflictWarning(projName, maxProjNameFrameworkVersionString, refName, maxRefNameFrameworkVersionString));
        }

        private static void CheckRulesOnMatchConflicts(List<string> solutionRequiredReferences, List<string> solutionUnacceptableReferences, List<string> globalRequiredReferences, List<string> globalUnacceptableReferences)
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

        private static void CheckProjectRulesOnMatchConflicts(List<string> solutionRequiredReferences, List<string> solutionUnacceptableReferences, List<string> globalRequiredReferences, List<string> globalUnacceptableReferences, List<string> requiredReferences, List<string> unacceptableReferences, string projName)
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

        private static void AddReferenceMatchWarningsToList(ErrorLevel highReferenceLevel, ErrorLevel lowReferenceLevel, string projName, bool isReferenceStraight, List<List<string>> currentIntersect)
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

        private static void CheckPrjMaxFrwrkVrsnDifferentLevelsConflicts(Dictionary<string, List<int>> maxLowLevelFrameworkVersion, Dictionary<string, List<int>> maxHighLevelFrameworkVersion, string projName, ErrorLevel lowRuleLevel, ErrorLevel highRuleLevel)
        {
            foreach (var currentMaxLowLevelFrameworkVersion in maxLowLevelFrameworkVersion)
            {
                var currentMaxLowLevelFrameworkVersionType = currentMaxLowLevelFrameworkVersion.Key;
                List<int> maxLowLevelFrameworkVersionArray = currentMaxLowLevelFrameworkVersion.Value;
                List<int> maxHighLevelFrameworkVersionArray = new List<int>();

                if (maxHighLevelFrameworkVersion.ContainsKey(currentMaxLowLevelFrameworkVersionType)) //Если типы версий фреймворков совпадают
                {
                    maxHighLevelFrameworkVersionArray = maxHighLevelFrameworkVersion[currentMaxLowLevelFrameworkVersionType]; //То проверить соотв. версии
                    CheckMaxFrameworkVersionCurrentConflict(maxHighLevelFrameworkVersionArray, maxLowLevelFrameworkVersionArray, projName, lowRuleLevel, highRuleLevel);
                }
                else
                {
                    if (maxHighLevelFrameworkVersion.ContainsKey("all")) //Если сверху супертип "all", то сравнить с ним
                    {
                        maxHighLevelFrameworkVersionArray = maxHighLevelFrameworkVersion["all"];
                        CheckMaxFrameworkVersionCurrentConflict(maxHighLevelFrameworkVersionArray, maxLowLevelFrameworkVersionArray, projName, lowRuleLevel, highRuleLevel);
                    }
                    else
                    {
                        if (maxLowLevelFrameworkVersion.ContainsKey("all")) //если снизу супертип "all", то сравнить все вышестоящие с ним
                        {
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

        private static void CheckRulesForProjectReferences(string projName, List<string> projReferences, List<string> configFileReferences, bool isReferenceRequired)
        {
            if (configFileReferences != null)
            {
                foreach (string fileReference in configFileReferences) //Объединить с ProjectReferences?
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

        private static void CheckRulesForSolutionOrGlobalReferences(string projName, List<string> projReferences, List<string> currentReferences, ErrorLevel referenceLevel, bool isReferenceRequired, List<List<string>> generalReferences)
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

        private static bool IsRuleConflict(string currentReference, ErrorLevel referenceType, List<List<string>> generalReferences)//Перебрать для каждого solution и Global рефа все нижестоящие на предмет противоречий
        {
            for (int i = 0; i < generalReferences.Count; i++)
            {
                if (referenceType != ErrorLevel.Global && i > 1) //generalReferences содержит все Project и Solution рефы, которые могут конфликтовать с текущим рефом (i 0 и 1 - project рефы, 2 и 3 - solution рефы)
                    break;

                if (generalReferences[i].Contains(currentReference))
                    return true;
            }

            return false;
        }

        private static void CheckProjectTargetFrameworkVersion(string currentProjectSupportedFrameworks, Dictionary<string, List<int>> maxFrameworkVersion, string projName, ErrorLevel errorLevel, Dictionary<string, List<int>> reserveMaxFrameworkVersion = null)
        {
            //В случае если строка идёт из TargetFrameworks (Maui и пр.) нужно предварительное деление по ";"
            //Нужно проверить каждый из 
            var currentProjectSupportedFrameworksArray = currentProjectSupportedFrameworks.Split(';');

            foreach (string currentProjectFramework in currentProjectSupportedFrameworksArray)
            {
                //Предварительный сплит на тире!!! Пример: net5.0-windows1.2

                var currentProjFrameworkArray = currentProjectFramework.Split('-');

                //Формирование списка из цифр версии фреймворка и определение его типа
                var currentProjFrameworkVersionArray = currentProjFrameworkArray[0].Split('.'); //Не все TargetFramework содержат точки! Пример: net45 - Не должно быть проблемой
                var currentProjFrameworkVersionArrayLength = currentProjFrameworkVersionArray.Length;

                var currentProjFrameworkMatch = Regex.Match(currentProjFrameworkVersionArray[0], @"^([a-zA-Z]+)(\d+)$");
                var currentProjFrameworkType = "-";

                if (currentProjFrameworkMatch.Success)
                {
                    currentProjFrameworkType = currentProjFrameworkMatch.Groups[1].Value;
                    currentProjFrameworkVersionArray[0] = currentProjFrameworkMatch.Groups[2].Value;

                    switch (currentProjFrameworkType)
                    {
                        case "v": currentProjFrameworkType = "netf"; break; //В случае если встретился старый .net framework проект с TargetFrameworkVersion
                        case "net": currentProjFrameworkType = currentProjFrameworkVersionArrayLength < 2 ? "netf" : "net"; break;
                            //Т.к. .NET и .NET Framework имеют одно название типа, то для фреймворка в проге условно введён тип "netf"!
                    }
                }

                List<int> currentMaxFrameworkVersionNums = new List<int>();

                if (maxFrameworkVersion.ContainsKey(currentProjFrameworkType))
                {
                    currentMaxFrameworkVersionNums = maxFrameworkVersion[currentProjFrameworkType];
                }
                else
                { //Если не нашлось правила для типа из TargetFramework
                    if (maxFrameworkVersion.ContainsKey("all"))//Проверить на наличие супертипа "all"
                        currentMaxFrameworkVersionNums = maxFrameworkVersion["all"];
                    else //Если и его нет, то попытаться найти ограничение на уровне выше
                    {
                        if (errorLevel == ErrorLevel.Solution && reserveMaxFrameworkVersion != null) //Сделать на уровне Solution предупреждение о том, что не нашлось ни одного подходящего типа Framework ни для одного проекта?
                            CheckProjectTargetFrameworkVersion(currentProjectFramework, reserveMaxFrameworkVersion, projName, ErrorLevel.Global);

                        return;//равносильно "-"
                    }
                }

                var maxFrameworkVersionArrayLength = currentMaxFrameworkVersionNums.Count;
                var maxFrameworkVersionString = GetFrameworkVersionString(currentMaxFrameworkVersionNums.ConvertAll(num => num.ToString()));

                //Загрузка данных об ограничениях на max_fr_version для текущего проекта
                if (!requiredMaxFrVersionsDict.ContainsKey(projName))
                    requiredMaxFrVersionsDict.Add(projName, new RequiredMaxFrVersion(maxFrameworkVersionString, errorLevel));
                else
                    requiredMaxFrVersionsDict[projName] = new RequiredMaxFrVersion(maxFrameworkVersionString, errorLevel);

                var minLengthValue = Math.Min(maxFrameworkVersionArrayLength, currentProjFrameworkVersionArrayLength);

                int i = 0;
                for (i = 0; i < minLengthValue; i++)
                {
                    int currentProjCurrentNum;
                    int maxVersionCurrentNum = Convert.ToInt32(currentMaxFrameworkVersionNums[i]);
                    if (!Int32.TryParse(currentProjFrameworkVersionArray[i], out currentProjCurrentNum))
                    {
                        //предупреждение без типа о том, что не удалось спарсить название проекта и проверка версии фреймворка не получилась

                        untypedErrorsList.Add(projName);

                        return;
                    }

                    if (currentProjCurrentNum > maxVersionCurrentNum)
                    {
                        //Ошибка, когда "TargetFramework" оказался больше чем максимально допустимый

                        var currentProjFrameworkVersionString = GetFrameworkVersionString(currentProjFrameworkVersionArray.ToList());


                        var currentFrameworkVersionComparabilityError =
                            new FrameworkVersionComparabilityError(errorLevel, currentProjFrameworkVersionString, maxFrameworkVersionString, projName);

                        if (!frameworkVersionComparabilityErrorList.Contains(currentFrameworkVersionComparabilityError, new FrameworkVersionComparabilityErrorContainsComparer()))
                            frameworkVersionComparabilityErrorList.Add(currentFrameworkVersionComparabilityError);

                        i = 0;
                        break;
                    }
                    else
                    {
                        if (currentProjCurrentNum < maxVersionCurrentNum)
                        {
                            i = 0;
                            break;
                        }

                    }
                }

                if (currentProjFrameworkVersionArrayLength > maxFrameworkVersionArrayLength && i != 0) //Если в текущей версии есть ещё не рассмотренные цифры
                {
                    for (int j = minLengthValue; j < currentProjFrameworkVersionArrayLength; j++)
                    {
                        int currentProjVersionCurrentNum;

                        if (!Int32.TryParse(currentProjFrameworkVersionArray[j], out currentProjVersionCurrentNum))
                        {
                            untypedErrorsList.Add(projName);

                            break;
                        }

                        if (currentProjVersionCurrentNum > 0)
                        {
                            var currentProjFrameworkVersionString = GetFrameworkVersionString(currentProjFrameworkVersionArray.ToList());

                            var currentFrameworkVersionComparabilityError =
                                new FrameworkVersionComparabilityError(errorLevel, currentProjFrameworkVersionString, maxFrameworkVersionString, projName);

                            if (!frameworkVersionComparabilityErrorList.Contains(currentFrameworkVersionComparabilityError, new FrameworkVersionComparabilityErrorContainsComparer()))
                                frameworkVersionComparabilityErrorList.Add(currentFrameworkVersionComparabilityError);

                            break;
                        }
                    }
                }
            }
        }

        private static void CheckProjectReferencesOnPotentialReferencesFrameworkVersionConflict(string projName, List<string> projReferences)
        {
            if (requiredMaxFrVersionsDict.ContainsKey(projName))
            {
                List<int> currentProjMaxFrVersionNums = requiredMaxFrVersionsDict[projName].VersionText
                    .Split('.')
                    .ToList()
                    .ConvertAll(value => Convert.ToInt32(value));
                foreach (var projReference in projReferences)
                {

                    if (requiredMaxFrVersionsDict.ContainsKey(projReference))
                    {
                        List<int> currentRefMaxVersionNums = requiredMaxFrVersionsDict[projReference].VersionText
                            .Split('.')
                            .ToList()
                            .ConvertAll(value => Convert.ToInt32(value));

                        CheckMaxFrameworkVersionCurrentConflict(currentProjMaxFrVersionNums, currentRefMaxVersionNums, projName, ErrorLevel.Undefined, ErrorLevel.Undefined, projReference);

                    }
                }
            }
        }
        //private static void StoreErrorListProviderByValues(string solutionName, ErrorListProvider errorListProvider)
        //{
        //    if (errorListProvider != null)
        //        errorListProvider.Tasks.Clear();

        //    if (untypedErrorsList.Count > 0)
        //    {
        //        foreach (var projName in untypedErrorsList)
        //        {
        //            string currentText = "RefDepGuard warning: Не получилось произвести проверку версии 'TargetFramework' для проекта '" + projName + "', так как программе не удалось получить из .csproj файла корректное значение для этого свойства. Проверьте, что проект имеет корректную версию 'TargetFramework'";

        //            StoreErrorTask(errorListProvider, currentText, solutionName + ".csproj", true);
        //        }
        //    }

        //    foreach (MaxFrameworkVersionDeviantValueError maxFrameworkVersionDeviantValue in maxFrameworkVersionDeviantValueList)
        //    {
        //        string documentName = solutionName + "_config_guard.rdg";
        //        string relevantProjectName = "";
        //        string globalPrefix = "";

        //        switch (maxFrameworkVersionDeviantValue.ErrorLevel)
        //        {
        //            case ErrorLevel.Global: documentName = "global_config_guard.rdg"; globalPrefix = "глобального "; break;
        //            case ErrorLevel.Solution: relevantProjectName = " уровня Solution"; break;
        //            case ErrorLevel.Project: relevantProjectName = " проекта '" + maxFrameworkVersionDeviantValue.ErrorRelevantProjectName + "'"; break;
        //        }

        //        string errorText = "RefDepGuard framework_max_version deviant value error: параметр 'framework_max_version' " + globalPrefix + "Config-файла " + relevantProjectName + " содержит некорректную запись своего значения. Проверьте его на предмет отсутствия синтаксических ошибок и соответствия шаблону файла конфигурации";

        //        StoreErrorTask(errorListProvider, errorText, documentName, false);
        //    }

        //    foreach (FrameworkVersionComparabilityError frameworkVersionComparabilityError in frameworkVersionComparabilityErrorList)
        //    {
        //        string documentName = solutionName + "_config_guard.rdg";
        //        string ruleLevel = "";


        //        switch (frameworkVersionComparabilityError.ErrorLevel)
        //        {
        //            case ErrorLevel.Global: documentName = "global_config_guard.rdg"; ruleLevel = "ограничение глобального уровня"; break;
        //            case ErrorLevel.Solution: ruleLevel = "ограничение уровня решения"; break;
        //            case ErrorLevel.Project: ruleLevel = "ограничение уровня проекта"; break;
        //        }

        //        string errorText = "RefDepGuard framework version comparability error: 'TargetFrameworkVersion' проекта '" + frameworkVersionComparabilityError.ErrorRelevantProjectName + "' имеет версию '" + frameworkVersionComparabilityError.TargetFrameworkVersion
        //            + "', в то время как максимально допустимой для него версией является '" + frameworkVersionComparabilityError.MaxFrameworkVersion + "' (" + ruleLevel + "). Измените версию проекта или модифицируйте конфигурацию Config-файла";

        //        StoreErrorTask(errorListProvider, errorText, documentName, false);
        //    }

        //    foreach (ConfigFilePropertyNullError configFilePropertyNullError in configPropertyNullErrorList)
        //    {
        //        string documentName = solutionName + "_config_guard.rdg";
        //        string relevantProjectName = "";

        //        if (configFilePropertyNullError.IsGlobal)
        //            documentName = "global_config_guard.rdg";

        //        if (configFilePropertyNullError.ErrorRelevantProjectName != "")
        //            relevantProjectName = " для проекта '" + configFilePropertyNullError.ErrorRelevantProjectName + "'";

        //        string errorText = "RefDepGuard Null property error: Config-файл не содержит свойство '" + configFilePropertyNullError.PropertyName + "'" + relevantProjectName + ". Проверьте его на предмет отсутствия синтаксических ошибок и соответствия шаблону файла конфигурации";

        //        StoreErrorTask(errorListProvider, errorText, documentName, false);
        //    }

        //    foreach (ReferenceMatchError referenceMatchError in refsMatchErrorList)
        //    {
        //        string projectName = "";
        //        string referenceLevelText = "";
        //        string documentName = solutionName + "_config_guard.rdg";
        //        string matchErrorDescription = "";

        //        if (referenceMatchError.IsProjNameMatchError)
        //            matchErrorDescription = " совпадает с именем проекта";
        //        else
        //            matchErrorDescription = " одновременно заявлен как обязательный и недопустимый";

        //        if (referenceMatchError.ProjectName != "")
        //            projectName = "' проекта '" + referenceMatchError.ProjectName;


        //        switch (referenceMatchError.ReferenceLevelValue)
        //        {
        //            case ErrorLevel.Solution: referenceLevelText = "уровня Solution"; break;
        //            case ErrorLevel.Global: referenceLevelText = "глобального уровня"; documentName = "global_config_guard.rdg"; break;
        //            case ErrorLevel.Project: break;
        //        }

        //        string errorText = "RefDepGuard Match error: референс '" + referenceMatchError.ReferenceName + projectName + "' " + referenceLevelText + matchErrorDescription + ". Устраните противоречие в правиле";

        //        StoreErrorTask(errorListProvider, errorText, documentName, false);
        //    }


        //    foreach (ReferenceError error in refsErrorList)
        //    {
        //        string referenceTypeText = "";
        //        string referenceLevelText = "";
        //        string documentName = error.ErrorRelevantProjectName + ".csproj";
        //        string actionForUser = "";

        //        if (error.IsReferenceRequired)
        //        {
        //            referenceTypeText = "Отсутсвует обязательный";
        //            actionForUser = "Добавьте";
        //        }
        //        else
        //        {
        //            referenceTypeText = "Присутствует недопустимый";
        //            actionForUser = "Удалите";
        //        }

        //        switch (error.CurrentReferenceLevel)
        //        {
        //            case ErrorLevel.Solution: referenceLevelText = "уровня Solution"; break;
        //            case ErrorLevel.Global: referenceLevelText = "глобального уровня"; break;
        //            case ErrorLevel.Project: break;
        //        }

        //        string errorText = "RefDepGuard Reference error: " + referenceTypeText + " референс " + referenceLevelText + " '" + error.ReferenceName + "' для проекта '" + error.ErrorRelevantProjectName + "'. " + actionForUser + " его через обозреватель решений";

        //        StoreErrorTask(errorListProvider, errorText, documentName, false);
        //    }

        //    foreach (MaxFrameworkVersionConflictWarning maxFrameworkVersionConflictValue in maxFrameworkVersionConflictWarningsList)
        //    {
        //        string documentName = solutionName + "_config_guard.rdg";
        //        string highErrorLevelText = "";
        //        string lowErrorLevelText = "";

        //        if (maxFrameworkVersionConflictValue.HighErrorLevel == maxFrameworkVersionConflictValue.LowErrorLevel)
        //            highErrorLevelText = ", указанное в супертипе 'all' на том же уровне";

        //        else
        //        {
        //            switch (maxFrameworkVersionConflictValue.HighErrorLevel)
        //            {
        //                case ErrorLevel.Global: highErrorLevelText = "глобального уровня"; break;
        //                case ErrorLevel.Solution: highErrorLevelText = "уровня Solution"; break;
        //            }
        //        }

        //        switch (maxFrameworkVersionConflictValue.LowErrorLevel)
        //        {
        //            case ErrorLevel.Solution: lowErrorLevelText = "уровня Solution"; break;
        //            case ErrorLevel.Project: lowErrorLevelText = "в проекте '" + maxFrameworkVersionConflictValue.ErrorRelevantProjectName + "'"; break;
        //        }

        //        string errorText = "RefDepGuard framework_max_version conflict warning: значение '" + maxFrameworkVersionConflictValue.LowLevelMaxFrameVersion
        //            + "' параметра 'framework_max_version' " + lowErrorLevelText + " превосходит значение '" + maxFrameworkVersionConflictValue.HighLevelMaxFrameVersion
        //            + "' одноимённого параметра " + highErrorLevelText + ". Устраните противоречие";

        //        StoreErrorTask(errorListProvider, errorText, documentName, true);
        //    }

        //    foreach (MaxFrameworkVersionReferenceConflictWarning maxFrameworkVersionReferenceConflictWarning in maxFrameworkVersionReferenceConflictWarningsList)
        //    {
        //        string documentName = solutionName + "_config_guard.rdg";

        //        string errorText = "RefDepGuard framework_max_version reference conflict warning: значение '" + maxFrameworkVersionReferenceConflictWarning.ProjFrameworkVersion
        //            + "' параметра 'framework_max_version' проекта " + maxFrameworkVersionReferenceConflictWarning.ProjName + " приводит к потенциальному конфликту версий TargetFramework" +
        //            ", так как имеется референс на проект, имеющий большее значение значение параметра 'framework_max_version' (проект: " + maxFrameworkVersionReferenceConflictWarning.RefName
        //            + ", Версия: " + maxFrameworkVersionReferenceConflictWarning.RefFrameworkVersion + "). Устраните противоречие";

        //        StoreErrorTask(errorListProvider, errorText, documentName, true);
        //    }

        //    foreach (ReferenceMatchWarning referenceMatchWarning in refsMatchWarningList)
        //    {
        //        string documentName = solutionName + "_config_guard.rdg";
        //        string projectName = "";
        //        string highReferenceLevelText = "";
        //        string lowReferenceLevelText = "";
        //        string referenceTypeText = "";
        //        string warningDescription = "";
        //        string warningAction = "";


        //        if (referenceMatchWarning.ProjectName != "")
        //        {
        //            projectName = "' проекта '" + referenceMatchWarning.ProjectName;
        //        }

        //        if (referenceMatchWarning.LowReferenceLevel == ErrorLevel.Solution)
        //        {
        //            lowReferenceLevelText = "уровня Solution";
        //        }

        //        switch (referenceMatchWarning.HighReferenceLevel)
        //        {
        //            case ErrorLevel.Solution: highReferenceLevelText = "уровня Solution"; break;
        //            case ErrorLevel.Global: highReferenceLevelText = "глобального уровня"; documentName = "global_config_guard.rdg"; break;
        //            case ErrorLevel.Project: break;
        //        }

        //        if (referenceMatchWarning.IsReferenceStraight)
        //        {
        //            warningDescription = " дубирует правило с одноимённым референсом ";
        //            warningAction = ". Устраните дублирование правила";

        //            if (referenceMatchWarning.IsHighLevelReq)
        //                referenceTypeText = " является обязательным и";
        //            else
        //                referenceTypeText = " является недопустимым и";
        //        }
        //        else //В противном случае рассматривается cross match errors, а значит они имеют тип рефа, противиположный более "верхнему" праивлу
        //        {
        //            warningDescription = " противоречит правилу с одноимённым референсом ";
        //            warningAction = ". Устраните противоречие в правиле";

        //            if (referenceMatchWarning.IsHighLevelReq)
        //                referenceTypeText = " является недопустимым и";
        //            else
        //                referenceTypeText = " является обязательным и";
        //        }

        //        string errorText = "RefDepGuard Match Warning: референс '" + referenceMatchWarning.ReferenceName + projectName + "' " + lowReferenceLevelText + referenceTypeText + warningDescription + highReferenceLevelText + warningAction;

        //        StoreErrorTask(errorListProvider, errorText, documentName, true);
        //    }
        //}

        //private static void StoreErrorTask(ErrorListProvider errorListProvider, string currentText, string currentDocument, bool isWarning)
        //{
        //    TaskErrorCategory currentTask = TaskErrorCategory.Error;

        //    if (isWarning)
        //        currentTask = TaskErrorCategory.Warning;

        //    ErrorTask errorTask = new ErrorTask
        //    {
        //        Category = TaskCategory.User,
        //        ErrorCategory = currentTask,
        //        Document = currentDocument,
        //        Text = currentText
        //    };

        //    errorListProvider.Tasks.Add(errorTask);
        //}
    }
}
