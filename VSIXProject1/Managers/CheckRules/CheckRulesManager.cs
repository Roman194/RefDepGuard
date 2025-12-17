using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using VSIXProject1.Comparators;
using VSIXProject1.Data;
using VSIXProject1.Data.ConfigFile;
using VSIXProject1.Data.FrameworkVersion;
using VSIXProject1.Data.Reference;
using VSIXProject1.Managers.CheckRules;
using VSIXProject1.Managers.CheckRules.SubManagers;
using VSIXProject1.Models;

namespace VSIXProject1
{
    public class CheckRulesManager
    {
        static List<MaxFrameworkVersionDeviantValueError> maxFrameworkVersionDeviantValueList = new List<MaxFrameworkVersionDeviantValueError>();
        static List<RequiredReference> requiredReferencesList = new List<RequiredReference>();

        static RefDepGuardErrors refDepGuardErrors;
        static RefDepGuardWarnings refDepGuardWarnings;
        static RequiredParameters requiredExportParameters;
        static RefDepGuardFindedProblems refDepGuardFindedProblems;

        public static Tuple<RefDepGuardExportParameters, ConfigFilesData> CheckRulesFromConfigFiles(
            ConfigFilesData configFilesData, ErrorListProvider errorListProvider, Dictionary<string, ProjectState> currentCommitedProjState, IVsUIShell uIShell
            )
        {
            ConfigFileGlobal configFileGlobal = configFilesData.configFileGlobal;
            ConfigFileSolution configFileSolution = configFilesData.configFileSolution;
            string solutionName = configFilesData.solutionName;

            ClearErrorAndWarningLists();

            var configPropertyNullErrorList = NotNullChecksSubManager.CheckConfigPropertiesOnNotNull(configFilesData);

            var maxGlobalFrameworkVersionByTypes = GetMaxFrameworkVersionDictionaryByTypes(configFileGlobal?.framework_max_version ?? "-", ErrorLevel.Global);
            var maxSolutionFrameworkVersionByTypes = GetMaxFrameworkVersionDictionaryByTypes(configFileSolution?.framework_max_version ?? "-", ErrorLevel.Solution);

            MaxFrameworkRuleChecksSubManager.CheckMaxFrameworkVersionOneLevelConflict(maxSolutionFrameworkVersionByTypes, ErrorLevel.Global);
            MaxFrameworkRuleChecksSubManager.CheckMaxFrameworkVersionOneLevelConflict(maxSolutionFrameworkVersionByTypes, ErrorLevel.Solution);

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

            RefsRuleChecksSubManager.CheckRulesOnMatchConflicts(
                solutionRequiredReferences, solutionUnacceptableReferences, globalRequiredReferences, globalUnacceptableReferences
                );

            if (maxGlobalFrameworkVersionByTypes.Count > 0 && maxSolutionFrameworkVersionByTypes.Count > 0)//проверка на противоречие с global
                MaxFrameworkRuleChecksSubManager.CheckPrjMaxFrwrkVrsnDifferentLevelsConflicts(
                    maxSolutionFrameworkVersionByTypes, maxGlobalFrameworkVersionByTypes, "", ErrorLevel.Solution, ErrorLevel.Global
                    );

            //Проверка на наличие незафиксированных в конфиге и уже удалённых в solution проектов
            var projectMatchWarningList = new List<ProjectMatchWarning>();
            (configFilesData, projectMatchWarningList) = CheckProjectsMatchSubManager.CheckAndUpdateProjectsOnMatch(configFilesData, currentCommitedProjState, uIShell);

            foreach (KeyValuePair<string, ProjectState> currentProjState in currentCommitedProjState)//для каждого project
            {
                var projName = currentProjState.Key;
                var projReferences = currentProjState.Value.CurrentReferences;
                var projFrameworkVersions = currentProjState.Value.CurrentFrameworkVersions;

                if (configFilesData.configFileSolution?.projects?.ContainsKey(projName) ?? false)//Эта проверка требуется, так как п-ль мог запретить автомат. добавление недостающих проектов
                {
                    ConfigFileProject currentProjectConfigFileSettings = configFileSolution.projects[projName];

                    bool isConsiderRequiredReferences = currentProjectConfigFileSettings.consider_global_and_solution_references?.required ?? true; //Проверка на отключение глобальных и solution рефов для проекта
                    bool isConsiderUnacceptableReferences = currentProjectConfigFileSettings.consider_global_and_solution_references?.unacceptable ?? true;

                    var maxFrameworkVersionByTypes = GetMaxFrameworkVersionDictionaryByTypes(currentProjectConfigFileSettings?.framework_max_version ?? "-", ErrorLevel.Project, projName);
                    MaxFrameworkRuleChecksSubManager.CheckMaxFrameworkVersionOneLevelConflict(maxFrameworkVersionByTypes, ErrorLevel.Project, projName);

                    List<string> requiredReferences = currentProjectConfigFileSettings?.required_references ?? new List<string>();
                    List<string> unacceptableReferences = currentProjectConfigFileSettings?.unacceptable_references ?? new List<string>();

                    List<List<string>> configFileProjectAndSolutionReferences = new List<List<string>>
                    {
                        requiredReferences, unacceptableReferences, solutionRequiredReferences, solutionUnacceptableReferences
                    };

                    requiredReferencesList.AddRange(requiredReferences.ConvertAll(value => new RequiredReference(value, projName)));

                    RefsRuleChecksSubManager.CheckProjectRulesOnMatchConflicts(
                        solutionRequiredReferences, solutionUnacceptableReferences, globalRequiredReferences,
                        globalUnacceptableReferences, requiredReferences, unacceptableReferences, projName);

                    RefsRuleChecksSubManager.CheckRulesForProjectReferences(projName, projReferences, requiredReferences, true);
                    RefsRuleChecksSubManager.CheckRulesForProjectReferences(projName, projReferences, unacceptableReferences, false);

                    foreach (ReferenceAffiliation referenceAffiliation in unionSolutionAndGlobalReferencesByType)
                    {
                        if (isConsiderRequiredReferences)//если заявлено
                            //применяем глобальные референсы
                            RefsRuleChecksSubManager.CheckRulesForSolutionOrGlobalReferences(
                                projName, projReferences, referenceAffiliation.RequiredReferences, referenceAffiliation.ReferenceTypeValue, 
                                true, configFileProjectAndSolutionReferences);

                        if (isConsiderUnacceptableReferences)
                            RefsRuleChecksSubManager.CheckRulesForSolutionOrGlobalReferences(
                                projName, projReferences, referenceAffiliation.UnacceptableReferences, referenceAffiliation.ReferenceTypeValue, 
                                false, configFileProjectAndSolutionReferences);
                    }

                    if (maxFrameworkVersionByTypes.Count == 0)
                    {
                        if (maxSolutionFrameworkVersionByTypes.Count > 0)
                        {
                            MaxFrameworkRuleChecksSubManager.CheckProjectTargetFrameworkVersion(
                                projFrameworkVersions, maxSolutionFrameworkVersionByTypes, projName, ErrorLevel.Solution, maxGlobalFrameworkVersionByTypes
                                );
                        }
                        else
                        {
                            if (maxGlobalFrameworkVersionByTypes.Count > 0)
                                MaxFrameworkRuleChecksSubManager.CheckProjectTargetFrameworkVersion(
                                    projFrameworkVersions, maxGlobalFrameworkVersionByTypes, projName, ErrorLevel.Global
                                    );
                        }
                    }
                    else//Проверить на противоречие с уровнем solution и global
                    {
                        if (maxSolutionFrameworkVersionByTypes.Count > 0)
                            MaxFrameworkRuleChecksSubManager.CheckPrjMaxFrwrkVrsnDifferentLevelsConflicts(
                                maxFrameworkVersionByTypes, maxSolutionFrameworkVersionByTypes, projName, ErrorLevel.Project, ErrorLevel.Solution
                                );

                        if (maxGlobalFrameworkVersionByTypes.Count > 0)
                            MaxFrameworkRuleChecksSubManager.CheckPrjMaxFrwrkVrsnDifferentLevelsConflicts(
                                maxFrameworkVersionByTypes, maxGlobalFrameworkVersionByTypes, projName, ErrorLevel.Project, ErrorLevel.Global
                                );

                        MaxFrameworkRuleChecksSubManager.CheckProjectTargetFrameworkVersion(
                            projFrameworkVersions, maxFrameworkVersionByTypes, projName, ErrorLevel.Project
                            );
                    }

                    MaxFrameworkRuleChecksSubManager.CheckProjectReferencesOnPotentialReferencesFrameworkVersionConflict(projName, projReferences);
                }
            }

            var refsMatchWarningList = RefsRuleChecksSubManager.GetReferenceWarnings();//На текущий момент только тип RefsMatchWarning 
            var refsRuleCheckErrors = RefsRuleChecksSubManager.GetReferenceErrors();

            var maxFrameworkVersionWarnings = MaxFrameworkRuleChecksSubManager.GetMaxFrameworkVersionWarnings();
            var maxFrameworkRuleErrors = MaxFrameworkRuleChecksSubManager.GetMaxFrameworkRuleErrors();
            var requiredMaxFrVersionsDict = MaxFrameworkRuleChecksSubManager.GetRequiredMaxFrVersionsDict();

            refsRuleCheckErrors.RefsErrorList.Sort(new ReferenceErrorSortComparer());
            refsRuleCheckErrors.RefsMatchErrorList.Sort(new ReferenceMatchErrorSortComparer());
            configPropertyNullErrorList.Sort(new ConfigFilePropertyNullErrorSortComparer());
            maxFrameworkVersionDeviantValueList.Sort(new MaxFrameworkVersionDeviantValueSortComparer());
            maxFrameworkRuleErrors.FrameworkVersionComparabilityErrorList.Sort(new FrameworkVersionComparabilityErrorSortComparer());

            refDepGuardErrors = new RefDepGuardErrors(
                configPropertyNullErrorList, refsRuleCheckErrors.RefsErrorList, refsRuleCheckErrors.RefsMatchErrorList, maxFrameworkVersionDeviantValueList,
                maxFrameworkRuleErrors.FrameworkVersionComparabilityErrorList
                );
            refDepGuardWarnings = new RefDepGuardWarnings(
                refsMatchWarningList, maxFrameworkVersionWarnings.MaxFrameworkVersionConflictWarningsList, 
                maxFrameworkVersionWarnings.MaxFrameworkVersionReferenceConflictWarningsList, projectMatchWarningList, 
                maxFrameworkRuleErrors.UntypedWarningsList);

            refDepGuardFindedProblems = new RefDepGuardFindedProblems(refDepGuardWarnings, refDepGuardErrors);

            ELPStoreManager.StoreErrorListProviderByValues(refDepGuardFindedProblems, configFilesData.solutionName, errorListProvider);

            requiredExportParameters = new RequiredParameters(requiredReferencesList, requiredMaxFrVersionsDict);

            return new Tuple<RefDepGuardExportParameters, ConfigFilesData>(
                new RefDepGuardExportParameters(refDepGuardFindedProblems, requiredExportParameters), 
                configFilesData
            );
        }

        private static void ClearErrorAndWarningLists()
        {
            if (maxFrameworkVersionDeviantValueList != null)
                maxFrameworkVersionDeviantValueList.Clear();

            if (requiredReferencesList != null)
                requiredReferencesList.Clear();

            NotNullChecksSubManager.ClearConfigPropertyNullErrorList();
            RefsRuleChecksSubManager.ClearRefsErrorsAndWarnings();
            MaxFrameworkRuleChecksSubManager.ClearErrorAndWarningLists();
            CheckProjectsMatchSubManager.ClearErrorLists();
        }

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

    }
}
