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
using VSIXProject1.Models.FrameworkVersion;

namespace VSIXProject1
{
    public class CheckRulesManager
    {
        private static List<MaxFrameworkVersionDeviantValueError> maxFrameworkVersionDeviantValueErrorList = new List<MaxFrameworkVersionDeviantValueError>();
        private static List<MaxFrameworkVersionDeviantValueWarning> maxFrameworkVersionDeviantValueWarningList = new List<MaxFrameworkVersionDeviantValueWarning>();

        private static List<RequiredReference> requiredReferencesList = new List<RequiredReference>();

        private static RefDepGuardErrors refDepGuardErrors;
        private static RefDepGuardWarnings refDepGuardWarnings;
        private static RequiredParameters requiredExportParameters;
        private static RefDepGuardFindedProblems refDepGuardFindedProblems;

        public static Tuple<RefDepGuardExportParameters, ConfigFilesData> CheckRulesFromConfigFiles(
            ConfigFilesData configFilesData, ErrorListProvider errorListProvider, Dictionary<string, ProjectState> currentCommitedProjState, IVsUIShell uIShell
            )
        {
            ConfigFileGlobal configFileGlobal = configFilesData.configFileGlobal;
            ConfigFileSolution configFileSolution = configFilesData.configFileSolution;
            string solutionName = configFilesData.solutionName;
            FileParseError parseErrors = configFilesData.ParseError;

            ClearErrorAndWarningLists();

            var configPropertyNullErrorList = NotNullChecksSubManager.CheckConfigPropertiesOnNotNull(configFilesData);

            var maxGlobalFrameworkVersionByTypes = GetMaxFrameworkVersionDictionaryByTypes(configFileGlobal?.framework_max_version ?? "-", ErrorLevel.Global);
            var maxSolutionFrameworkVersionByTypes = GetMaxFrameworkVersionDictionaryByTypes(configFileSolution?.framework_max_version ?? "-", ErrorLevel.Solution);

            MaxFrameworkRuleChecksSubManager.CheckMaxFrameworkVersionOneLevelConflict(maxGlobalFrameworkVersionByTypes, ErrorLevel.Global);
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

            if (maxGlobalFrameworkVersionByTypes.Count > 0 && maxSolutionFrameworkVersionByTypes.Count > 0)//проверка на противоречие с global
                MaxFrameworkRuleChecksSubManager.CheckPrjMaxFrwrkVrsnDifferentLevelsConflicts(
                    maxSolutionFrameworkVersionByTypes, maxGlobalFrameworkVersionByTypes, "-", ErrorLevel.Solution, ErrorLevel.Global
                    );

            //Проверка на наличие незафиксированных в конфиге и уже удалённых в solution проектов
            var projectMatchWarningList = new List<ProjectMatchWarning>();
            (configFilesData, projectMatchWarningList) = CheckProjectsMatchSubManager.CheckAndUpdateProjectsOnMatch(configFilesData, currentCommitedProjState, uIShell);

            (globalRequiredReferences, globalUnacceptableReferences) = 
                RefsRuleChecksSubManager.CheckReferencesOnProjectExisting(globalRequiredReferences, globalUnacceptableReferences, currentCommitedProjState, ErrorLevel.Global);

            (solutionRequiredReferences, solutionUnacceptableReferences) = 
                RefsRuleChecksSubManager.CheckReferencesOnProjectExisting(solutionRequiredReferences, solutionUnacceptableReferences, currentCommitedProjState, ErrorLevel.Solution);


            RefsRuleChecksSubManager.CheckRulesOnMatchConflicts(
                solutionRequiredReferences, solutionUnacceptableReferences, globalRequiredReferences, globalUnacceptableReferences
                );

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

                    Dictionary<string, List<int>> projTypes = currentCommitedProjState[projName].CurrentFrameworkVersions;
                    var maxFrameworkVersionByTypes = GetMaxFrameworkVersionDictionaryByTypes(currentProjectConfigFileSettings?.framework_max_version ?? "-", ErrorLevel.Project, projName, projTypes.Keys.ToList());
                    //На уровне Project не может быть противоречий одного уровня!!!
                    //MaxFrameworkRuleChecksSubManager.CheckMaxFrameworkVersionOneLevelConflict(maxFrameworkVersionByTypes, ErrorLevel.Project, projName);

                    List<string> requiredReferences = currentProjectConfigFileSettings?.required_references ?? new List<string>();
                    List<string> unacceptableReferences = currentProjectConfigFileSettings?.unacceptable_references ?? new List<string>();

                    List<List<string>> configFileProjectAndSolutionReferences = new List<List<string>>
                    {
                        requiredReferences, unacceptableReferences, solutionRequiredReferences, solutionUnacceptableReferences
                    };

                    requiredReferencesList.AddRange(requiredReferences.ConvertAll(value => new RequiredReference(value, projName)));

                    (requiredReferences, unacceptableReferences) = 
                        RefsRuleChecksSubManager.CheckReferencesOnProjectExisting(requiredReferences, unacceptableReferences, currentCommitedProjState, ErrorLevel.Project, projName);

                    RefsRuleChecksSubManager.CheckProjectRulesOnMatchConflicts(
                        solutionRequiredReferences, solutionUnacceptableReferences, globalRequiredReferences, globalUnacceptableReferences, requiredReferences, 
                        unacceptableReferences, projName, isConsiderRequiredReferences, isConsiderUnacceptableReferences);

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
                }
            }

            //Для корректной проверки конфликтов рефов по макс версиям требуется предварительно собрать инфу обо всех проектах, их макс версиях и конфликтов между макс версиями
            //Поэтому проверка вынесена в отдельный цикл
            foreach (KeyValuePair<string, ProjectState> currentProjState in currentCommitedProjState)
            {
                var projName = currentProjState.Key;
                var projReferences = currentProjState.Value.CurrentReferences;

                if (configFilesData.configFileSolution?.projects?.ContainsKey(projName) ?? false)
                {
                    MaxFrameworkRuleChecksSubManager.CheckProjectReferencesOnPotentialReferencesFrameworkVersionConflict(projName, projReferences);
                }
            }

            var refsRuleChecksWarnings = RefsRuleChecksSubManager.GetReferenceWarnings();
            var refsRuleCheckErrors = RefsRuleChecksSubManager.GetReferenceErrors();

            var maxFrameworkVersionWarnings = MaxFrameworkRuleChecksSubManager.GetMaxFrameworkVersionWarnings();
            var maxFrameworkRuleProblems = MaxFrameworkRuleChecksSubManager.GetMaxFrameworkRuleProblems();
            var requiredMaxFrVersionsDict = MaxFrameworkRuleChecksSubManager.GetRequiredMaxFrVersionsDict();

            refsRuleCheckErrors.RefsErrorList.Sort(new ReferenceErrorSortComparer());//Сортируются только "ошибки"?
            refsRuleCheckErrors.RefsMatchErrorList.Sort(new ReferenceMatchErrorSortComparer());
            configPropertyNullErrorList.Sort(new ConfigFilePropertyNullErrorSortComparer());
            maxFrameworkVersionDeviantValueErrorList.Sort(new MaxFrameworkVersionDeviantValueSortComparer());
            maxFrameworkRuleProblems.FrameworkVersionComparabilityErrorList.Sort(new FrameworkVersionComparabilityErrorSortComparer());

            refDepGuardErrors = new RefDepGuardErrors(
                configPropertyNullErrorList, refsRuleCheckErrors.RefsErrorList, refsRuleCheckErrors.RefsMatchErrorList, maxFrameworkVersionDeviantValueErrorList,
                maxFrameworkRuleProblems.FrameworkVersionComparabilityErrorList
                );
            refDepGuardWarnings = new RefDepGuardWarnings(
                refsRuleChecksWarnings.ReferenceMatchWarningsList, refsRuleChecksWarnings.ProjectNotFoundWarningsList,
                maxFrameworkVersionDeviantValueWarningList,
                maxFrameworkVersionWarnings.MaxFrameworkVersionConflictWarningsList, 
                maxFrameworkVersionWarnings.MaxFrameworkVersionReferenceConflictWarningsList, 
                projectMatchWarningList, maxFrameworkRuleProblems.UntypedWarningsList);

            refDepGuardFindedProblems = new RefDepGuardFindedProblems(refDepGuardWarnings, refDepGuardErrors);

            //Вывод обнаруженных проблем по ограничениям конфиг-файлов
            ELPStoreManager.StoreErrorListProviderByValues(refDepGuardFindedProblems, configFilesData, errorListProvider);

            if(parseErrors != FileParseError.None) //Вывод предупреждений о неудаче парсинга конфиг-файлов
            {
                if(parseErrors == FileParseError.Global || parseErrors == FileParseError.All)
                    ELPStoreManager.ShowUnsuccessfulConfigFileParseWarning(errorListProvider, "глобального файла конфигурации");

                if(parseErrors == FileParseError.Solution || parseErrors == FileParseError.All)
                    ELPStoreManager.ShowUnsuccessfulConfigFileParseWarning(errorListProvider, "файла конфигурации конкретного solution");
            }

            requiredExportParameters = new RequiredParameters(requiredReferencesList, requiredMaxFrVersionsDict);

            return new Tuple<RefDepGuardExportParameters, ConfigFilesData>(
                new RefDepGuardExportParameters(refDepGuardFindedProblems, requiredExportParameters), 
                configFilesData
            );
        }

        private static void ClearErrorAndWarningLists()
        {
            if (maxFrameworkVersionDeviantValueErrorList != null)
                maxFrameworkVersionDeviantValueErrorList.Clear();

            if (maxFrameworkVersionDeviantValueWarningList != null)
                maxFrameworkVersionDeviantValueWarningList.Clear();

            if (requiredReferencesList != null)
                requiredReferencesList.Clear();

            NotNullChecksSubManager.ClearConfigPropertyNullErrorList();
            RefsRuleChecksSubManager.ClearRefsErrorsAndWarnings();
            MaxFrameworkRuleChecksSubManager.ClearErrorAndWarningLists();
            CheckProjectsMatchSubManager.ClearErrorLists();
        }

        private static Dictionary<string, List<int>> GetMaxFrameworkVersionDictionaryByTypes(string currentMaxFrameworkVersion, ErrorLevel errorLevel, string projName = "", List<string> projTypes = null)
        {
            projTypes = projTypes ?? new List<string>();

            if (currentMaxFrameworkVersion == "-")
                return new Dictionary<string, List<int>>();

            if ((currentMaxFrameworkVersion.Contains(';') || currentMaxFrameworkVersion.Contains(':')) && errorLevel == ErrorLevel.Project && projTypes.Count == 1)
            {
                //Выкинуть ошибку о некорректном формате (На уровне project не допускается перечисление версий фреймворка пользователем, если это не позволяет проект)
                MaxFrameworkVersionDeviantValueError potentialMaxFrameworkVersionDeviantValueError = new MaxFrameworkVersionDeviantValueError(errorLevel, projName);

                if (!maxFrameworkVersionDeviantValueErrorList.Contains(potentialMaxFrameworkVersionDeviantValueError, new MaxFrameworkVersionDeviantValueContainsComparer()))
                    maxFrameworkVersionDeviantValueErrorList.Add(potentialMaxFrameworkVersionDeviantValueError);

                return new Dictionary<string, List<int>>();
            }

            if (!currentMaxFrameworkVersion.Contains(':')) //Приведение всех ограничений к шаблону
            {
                if (errorLevel == ErrorLevel.Project) //Если встречается ограничение на проект, то надо подставить тип этого проекта (или несколько типов)!!!
                {
                    if (projTypes.Count == 1) //И получается, что на проектном уровне всё же используется шаблон различных огр-ий, но только если у нас TargetFrameworks!
                        currentMaxFrameworkVersion = projTypes.FirstOrDefault() + ":" + currentMaxFrameworkVersion;
                    else
                    {
                        foreach (var projType in projTypes)
                        {
                            currentMaxFrameworkVersion += projType + ":" + currentMaxFrameworkVersion;

                            if (projTypes.IndexOf(projType) != projTypes.Count - 1)
                                currentMaxFrameworkVersion += "; ";
                        }
                    }
                }

                else
                    currentMaxFrameworkVersion = "all:" + currentMaxFrameworkVersion;
            }

            var currentMaxFrameworkVersionArray = currentMaxFrameworkVersion.Split(';');
            var maxFrameworkDictionary = new Dictionary<string, List<int>>();

            foreach (string maxFrameworkVersion in currentMaxFrameworkVersionArray) //Для каждого из ограничений
            {
                var maxFrameworkVersionElementSplited = maxFrameworkVersion.Replace(" ", "").Split(':');

                //Если не указано название типа фреймворка или/и в правой части нет ничего
                if (String.IsNullOrEmpty(maxFrameworkVersionElementSplited[0]) || String.IsNullOrEmpty(maxFrameworkVersionElementSplited[1]))
                {
                    //Выкинуть ошибку о некорректном формате
                    MaxFrameworkVersionDeviantValueError potentialMaxFrameworkVersionDeviantValueError = new MaxFrameworkVersionDeviantValueError(errorLevel, "");

                    if (!maxFrameworkVersionDeviantValueErrorList.Contains(potentialMaxFrameworkVersionDeviantValueError, new MaxFrameworkVersionDeviantValueContainsComparer()))
                        maxFrameworkVersionDeviantValueErrorList.Add(potentialMaxFrameworkVersionDeviantValueError);

                    return new Dictionary<string, List<int>>();
                }

                //Если при TargetFrameworks п-ль указал тип проекта, которого нет в TF или не супертип all, то выдать ошибку
                if(errorLevel == ErrorLevel.Project && (!projTypes.Contains(maxFrameworkVersionElementSplited[0]) && maxFrameworkVersionElementSplited[0] != "all"))
                { //Надо задать на это + где projTypes == 1 новый вид ошибок?
                    MaxFrameworkVersionDeviantValueError potentialMaxFrameworkVersionDeviantValueError = new MaxFrameworkVersionDeviantValueError(errorLevel, projName);

                    if (!maxFrameworkVersionDeviantValueErrorList.Contains(potentialMaxFrameworkVersionDeviantValueError, new MaxFrameworkVersionDeviantValueContainsComparer()))
                        maxFrameworkVersionDeviantValueErrorList.Add(potentialMaxFrameworkVersionDeviantValueError);

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
                            maxFrameworkVersionDeviantValueErrorList.Add(potentialMaxFrameworkVersionDeviantValueError);
                        }
                        else
                        {
                            if (!maxFrameworkVersionDeviantValueErrorList.Contains(potentialMaxFrameworkVersionDeviantValueError, new MaxFrameworkVersionDeviantValueContainsComparer()))
                                maxFrameworkVersionDeviantValueErrorList.Add(potentialMaxFrameworkVersionDeviantValueError);
                        }

                        return new Dictionary<string, List<int>>();
                    }
                    maxFrameworkVersionNumsList.Add(maxVersionCurrentNum);
                }

                if (maxFrameworkVersionNumsList.Count == 1)//Если числовое ограничение указано в формате без точки
                {
                    //добавить новую framework_max_version deviant value warning и незначащий ноль в конец
                    maxFrameworkVersionNumsList.Add(0);
                    maxFrameworkVersionDeviantValueWarningList.Add(new MaxFrameworkVersionDeviantValueWarning(errorLevel, projName, maxFrameworkVersionElementSplited[1]));
                }

                maxFrameworkDictionary.Add(maxFrameworkVersionElementSplited[0], maxFrameworkVersionNumsList);
            }
            return maxFrameworkDictionary;
        }
    }
}
