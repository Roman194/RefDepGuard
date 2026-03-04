using RefDepGuard.Data;
using RefDepGuard.Data.FrameworkVersion;
using RefDepGuard.Models;
using RefDepGuard.Models.FrameworkVersion;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RefDepGuard.Managers.CheckRules
{
    public class MaxFrameworkRuleChecksSubManager
    {
        private static List<MaxFrameworkVersionConflictWarning> maxFrameworkVersionConflictWarningsList = new List<MaxFrameworkVersionConflictWarning>();
        private static List<MaxFrameworkVersionReferenceConflictWarning> maxFrameworkVersionReferenceConflictWarningsList = new List<MaxFrameworkVersionReferenceConflictWarning>();

        private static List<string> untypedWarningsList = new List<string>();
        private static List<FrameworkVersionComparabilityError> frameworkVersionComparabilityErrorList = new List<FrameworkVersionComparabilityError>();

        private static Dictionary<string, RequiredMaxFrVersion> requiredMaxFrVersionsDict = new Dictionary<string, RequiredMaxFrVersion>();

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

        public static void CheckMaxFrameworkVersionOneLevelConflict(Dictionary<string, List<int>> currentMaxFrameworkVersion, ProblemLevel ruleLevel)
        {
            if (currentMaxFrameworkVersion.ContainsKey("all")) //Проверки на противоречия в правилах макс фреймворков одного уровня
            {
                List<int> maxAllTypeFrameworkVersionArray = currentMaxFrameworkVersion["all"];

                foreach (var currentMaxLowLevelFrameworkVersion in currentMaxFrameworkVersion)
                {
                    if (currentMaxLowLevelFrameworkVersion.Key != "all")
                    {
                        List<int> maxCurrentTypeFrameworkVersionArray = currentMaxLowLevelFrameworkVersion.Value;
                        CheckMaxFrameworkVersionCurrentConflict(maxAllTypeFrameworkVersionArray, maxCurrentTypeFrameworkVersionArray, "-", ruleLevel, ruleLevel);
                    }
                }
            }
        }

        public static void CheckProjectMaxFrameworkVersionDifferentLevelsConflicts(
            Dictionary<string, List<int>> maxLowLevelFrameworkVersion, Dictionary<string, List<int>> maxHighLevelFrameworkVersion, string projName,
            ProblemLevel lowRuleLevel, ProblemLevel highRuleLevel)
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
                        if (maxLowLevelFrameworkVersion.ContainsKey("all")) //если снизу (solution) супертип "all", то сравнить все вышестоящие с ним
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

        public static void CheckProjectTargetFrameworkVersion(
            Dictionary<string, List<int>> currentProjectSupportedFrameworks, Dictionary<string, List<int>> maxFrameworkVersion,
            string projName, ProblemLevel errorLevel, Dictionary<string, List<int>> reserveMaxFrameworkVersion = null)
        {
            
            if (currentProjectSupportedFrameworks.Count == 0)
            {
                //Если не найдены какие-либо TargetFrameworks, то не проверяем + соотв. warning
                untypedWarningsList.Add(projName);
                return;
            }

            foreach (string currentProjectFramework in currentProjectSupportedFrameworks.Keys) //Для каждого из TragetFrameowrk(-s)
            {

                string currentMaxFrVersionType = currentProjectFramework;
                List<int> currentMaxFrameworkVersionNums = new List<int>();

                List<int> currentProjFrameworkVersionArray = currentProjectSupportedFrameworks[currentProjectFramework];
                int currentProjFrameworkVersionArrayLength = currentProjFrameworkVersionArray.Count;

                if (maxFrameworkVersion.ContainsKey(currentProjectFramework))
                {
                    currentMaxFrameworkVersionNums = maxFrameworkVersion[currentProjectFramework];
                }
                else
                { //Если не нашлось правила для типа из TargetFramework
                    if (maxFrameworkVersion.ContainsKey("all"))
                    { //Проверить на наличие супертипа "all"
                        currentMaxFrameworkVersionNums = maxFrameworkVersion["all"];
                        if (errorLevel != ProblemLevel.Project)
                            currentMaxFrVersionType = "all";
                    }
                    else //Если и его нет, то попытаться найти ограничение на уровне выше
                    {
                        if (errorLevel == ProblemLevel.Solution && reserveMaxFrameworkVersion != null) //Сделать на уровне Solution предупреждение о том, что не нашлось ни одного подходящего типа Framework ни для одного проекта?
                            CheckProjectTargetFrameworkVersion(currentProjectSupportedFrameworks, reserveMaxFrameworkVersion, projName, ProblemLevel.Global);

                        return;//равносильно "-"
                    }
                }

                var maxFrameworkVersionArrayLength = currentMaxFrameworkVersionNums.Count;
                var maxFrameworkVersionString = GetFrameworkVersionString(currentMaxFrameworkVersionNums.ConvertAll(num => num.ToString()));

                var isConflictWarningRelevantForProject = maxFrameworkVersionConflictWarningsList.Find(value =>
                    value.LowWarnLevel == errorLevel && (value.WarningRelevantProjectName == projName || value.WarningRelevantProjectName == "-")
                    ) != null ? true : false;

                //Загрузка данных об ограничениях на max_fr_version для текущего проекта
                if (!requiredMaxFrVersionsDict.ContainsKey(projName))//Имеет ли смысл делать это каждый раз при проверке правил? - Да, т.к. TargetFramework мог измениться между коммитами
                    requiredMaxFrVersionsDict.Add(projName, 
                        new RequiredMaxFrVersion(maxFrameworkVersionString, currentMaxFrameworkVersionNums, errorLevel, currentMaxFrVersionType, isConflictWarningRelevantForProject));
                else
                    requiredMaxFrVersionsDict[projName] = 
                        new RequiredMaxFrVersion(maxFrameworkVersionString, currentMaxFrameworkVersionNums, errorLevel, currentMaxFrVersionType, isConflictWarningRelevantForProject);

                var minLengthValue = Math.Min(maxFrameworkVersionArrayLength, currentProjFrameworkVersionArrayLength);

                int i = 0;
                for (i = 0; i < minLengthValue; i++)
                {
                    int currentProjCurrentNum = currentProjFrameworkVersionArray[i];
                    int maxVersionCurrentNum = Convert.ToInt32(currentMaxFrameworkVersionNums[i]);
                    

                    if (currentProjCurrentNum > maxVersionCurrentNum)
                    {
                        //Ошибка, когда "TargetFramework" оказался больше чем максимально допустимый
                        var currentProjFrameworkVersionString = GetFrameworkVersionString(currentProjFrameworkVersionArray.Select(x => x.ToString()).ToList());

                        if(frameworkVersionComparabilityErrorList.Find(error => 
                                error.ErrorLevel == errorLevel && error.TargetFrameworkVersion == currentProjFrameworkVersionString &&
                                error.MaxFrameworkVersion == maxFrameworkVersionString && error.ErrorRelevantProjectName == projName) == null
                            )
                            frameworkVersionComparabilityErrorList.Add(
                                new FrameworkVersionComparabilityError(errorLevel, currentProjFrameworkVersionString, maxFrameworkVersionString, projName)
                                );

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
                        int currentProjVersionCurrentNum = currentProjFrameworkVersionArray[j];

                        if (currentProjVersionCurrentNum > 0)
                        {
                            var currentProjFrameworkVersionString = GetFrameworkVersionString(currentProjFrameworkVersionArray.Select(x => x.ToString()).ToList());

                            if (frameworkVersionComparabilityErrorList.Find(error =>
                                error.ErrorLevel == errorLevel && error.TargetFrameworkVersion == currentProjFrameworkVersionString &&
                                error.MaxFrameworkVersion == maxFrameworkVersionString && error.ErrorRelevantProjectName == projName) == null
                            )
                                frameworkVersionComparabilityErrorList.Add(
                                    new FrameworkVersionComparabilityError(errorLevel, currentProjFrameworkVersionString, maxFrameworkVersionString, projName)
                                    );

                            break;
                        }
                    }
                }
            }

        }

        public static void CheckProjectReferencesOnPotentialReferencesFrameworkVersionConflict(string projName, List<string> projReferences)
        {
            // Проверка конфликтов рефов по макс версиям не производится, если макс версия одного из текущих проектов "конфликтует" с какой-то глобальной или solution версией
            // или если есть конфликт в макс версии global или solution или между ними
            var projectError = maxFrameworkVersionConflictWarningsList.Find(value => value.WarningRelevantProjectName == projName || value.WarningRelevantProjectName == "-");

            if (requiredMaxFrVersionsDict.ContainsKey(projName) && projectError == null)
            {
                List<int> currentProjMaxFrVersionNums = requiredMaxFrVersionsDict[projName].VersionNums;
                //.Split('.')
                //.ToList()
                //.ConvertAll(value => Convert.ToInt32(value));

                foreach (var projReference in projReferences)
                {
                    var referenceError = maxFrameworkVersionConflictWarningsList.Find(value => value.WarningRelevantProjectName == projReference);

                    if (requiredMaxFrVersionsDict.ContainsKey(projReference) && referenceError == null)
                    {
                        //В случае, если типы проектов не совместимы друг с другом, выдаст предупреждение сама IDE, поэтому имеет смысл проверять на потенциальные
                        //max_framework_version конфликты только проекты одного типа, ограничения на все типы проектов или при совместимых типах проектов, но имеющих
                        //совместимость не для всех версий проектов (например, для net не м.б. потенциальных проблем с max_fr_ver, т.к. каждая версия совместима с каждой netcoreapp/ netf)

                        //При проверке потенциального конфликта TargetFramework на текущий момент рассматриваются только ограничения одного типа!

                        RequiredMaxFrVersion projVersion = requiredMaxFrVersionsDict[projName];
                        RequiredMaxFrVersion refVersion = requiredMaxFrVersionsDict[projReference];

                        if (refVersion.ProjectTypeRule == projVersion.ProjectTypeRule || refVersion.ProjectTypeRule == "all" || projVersion.ProjectTypeRule == "all")
                        {
                            List<int> currentRefMaxVersionNums = refVersion.VersionNums;
                            //.Split('.')
                            //.ToList()
                            //.ConvertAll(value => Convert.ToInt32(value));

                            CheckMaxFrameworkVersionCurrentConflict(currentProjMaxFrVersionNums, currentRefMaxVersionNums, projName, 
                                ProblemLevel.Undefined, ProblemLevel.Undefined, projReference);
                        }
                        else
                        {
                            //Если реф - netstandard, а проект с ним совместим, то проверить потенциальные проблемы по req max versions!
                            if (refVersion.ProjectTypeRule == "netstandard" && TFMSample.PossibleComparableTFMsWithNetStandard().Contains(projVersion.ProjectTypeRule))
                            {
                                if (projVersion.ProjectTypeRule == "net") continue;

                                //Т.к. п-ль может указать абсолютно любую версию, то её нужно привести к одной из Ex версий Netstandard
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

                                if (currMinVersion == "-") //Ни одна версия не поддерживается, есть проблема
                                {
                                    AddNewMaxFrameworkVersionOnReferenceConflictWarning(projVersion.VersionText, nearestExistingNetStdVersion, projName, projReference, false);
                                    continue;
                                }

                                //List<int> currentRefMaxVersionNums = refVersion.VersionText
                                //    .Split('.')
                                //    .ToList()
                                //    .ConvertAll(value => Convert.ToInt32(value));

                                List<int> currMinVersionNums = currMinVersion //Мин версия, которой должен соответствовать проект, чтобы иметь связь для текущей версии netstandard
                                    .Split('.')
                                    .ToList()
                                    .ConvertAll(value => Convert.ToInt32(value));

                                //Так как нужно проверить, больше ли текущая TFM-версия проекта чем минимально допустимая, то TFM-версия рефа передаётся отедльным параметром
                                //(она не участвует в сравнении но д.б. зафиксирована в соотв. предупреждении)
                                CheckMaxFrameworkVersionCurrentConflict(currentProjMaxFrVersionNums, currMinVersionNums, projName, 
                                    ProblemLevel.Undefined, ProblemLevel.Undefined, projReference, nearestExistingNetStdVersionNums); 
                            }
                        }
                    }
                }

            }
        }
        public static MaxFrameworkVersionWarnings GetMaxFrameworkVersionWarnings()
        {
            return new MaxFrameworkVersionWarnings(maxFrameworkVersionConflictWarningsList, maxFrameworkVersionReferenceConflictWarningsList);
        }

        public static MaxFrameworkRuleProblems GetMaxFrameworkRuleProblems()
        {
            return new MaxFrameworkRuleProblems(frameworkVersionComparabilityErrorList, untypedWarningsList);
        }

        public static Dictionary<string, RequiredMaxFrVersion> GetRequiredMaxFrVersionsDict()
        {
            return requiredMaxFrVersionsDict;
        }

        private static void CheckMaxFrameworkVersionCurrentConflict(
            List<int> maxHighLevelFrameworkVersionList, List<int> maxLowLevelFrameworkVersionList, string projName, ProblemLevel lowRuleLevel, ProblemLevel highRuleLevel, 
            string refName = "", List<int> currentRefFrameworkVersionList = null)
        {
            var isOneProjectsTypeConflict = (currentRefFrameworkVersionList != null) ? false : true;

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
                    var currentRefFrameworkVersionString = (!isOneProjectsTypeConflict) ? 
                        GetFrameworkVersionString(currentRefFrameworkVersionList.ConvertAll(num => num.ToString())) : null;

                    if (lowRuleLevel != ProblemLevel.Undefined)
                        AddNewMaxFrameworkVersionConflictWarning(maxHighLevelFrameworkVersionString, maxLowLevelFrameworkVersionString, projName, lowRuleLevel, highRuleLevel);
                    else
                        AddNewMaxFrameworkVersionOnReferenceConflictWarning(maxHighLevelFrameworkVersionString, currentRefFrameworkVersionString ?? maxLowLevelFrameworkVersionString, projName, refName, 
                            isOneProjectsTypeConflict);

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

        private static void AddNewMaxFrameworkVersionConflictWarning(
            string maxHighLevelFrameworkVersionString, string maxLowLevelFrameworkVersionString, string projName, ProblemLevel lowRuleLevel, ProblemLevel highRuleLevel)
        {
            //Warning о противоречии между рефами
            var potentialMaxFrameworkVersionConflictWarning = 
                new MaxFrameworkVersionConflictWarning(highRuleLevel, lowRuleLevel, maxHighLevelFrameworkVersionString, maxLowLevelFrameworkVersionString, projName);

            if (lowRuleLevel == ProblemLevel.Project)
            {
                maxFrameworkVersionConflictWarningsList.Add(potentialMaxFrameworkVersionConflictWarning);
                return;
            }

            if(maxFrameworkVersionConflictWarningsList.Find(error => 
                error.HighWarnLevel == highRuleLevel && error.LowWarnLevel == lowRuleLevel && error.WarningRelevantProjectName == projName) == null
            )
                maxFrameworkVersionConflictWarningsList.Add(potentialMaxFrameworkVersionConflictWarning);
        }

        private static void AddNewMaxFrameworkVersionOnReferenceConflictWarning(
            string maxProjNameFrameworkVersionString, string maxRefNameFrameworkVersionString, string projName, string refName, bool isOneProjectsTypeConflict)
        {
            maxFrameworkVersionReferenceConflictWarningsList.Add(
                new MaxFrameworkVersionReferenceConflictWarning(
                    projName, maxProjNameFrameworkVersionString, refName, maxRefNameFrameworkVersionString, isOneProjectsTypeConflict)
                );
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
    }
}
