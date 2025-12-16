using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VSIXProject1.Comparators;
using VSIXProject1.Data.FrameworkVersion;
using VSIXProject1.Models.FrameworkVersion;

namespace VSIXProject1.Managers.CheckRules
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
        }

        public static void CheckMaxFrameworkVersionOneLevelConflict(Dictionary<string, List<int>> currentMaxFrameworkVersion, ErrorLevel ruleLevel, string projName = "-")
        {
            if (currentMaxFrameworkVersion.ContainsKey("all")) //Проверки на противоречия в правилах макс фреймворков одного уровня
            {
                List<int> maxAllTypeFrameworkVersionArray = currentMaxFrameworkVersion["all"];

                foreach (var currentMaxLowLevelFrameworkVersion in currentMaxFrameworkVersion)
                {
                    if (currentMaxLowLevelFrameworkVersion.Key != "all")
                    {
                        List<int> maxCurrentTypeFrameworkVersionArray = currentMaxLowLevelFrameworkVersion.Value;
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

        public static void CheckPrjMaxFrwrkVrsnDifferentLevelsConflicts(
            Dictionary<string, List<int>> maxLowLevelFrameworkVersion, Dictionary<string, List<int>> maxHighLevelFrameworkVersion, string projName, 
            ErrorLevel lowRuleLevel, ErrorLevel highRuleLevel)
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

        public static void CheckProjectTargetFrameworkVersion(
            string currentProjectSupportedFrameworks,  Dictionary<string, List<int>> maxFrameworkVersion, 
            string projName, ErrorLevel errorLevel, Dictionary<string, List<int>> reserveMaxFrameworkVersion = null)
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
                        case "v": 
                            currentProjFrameworkType = "netf"; 
                            break; //В случае если встретился старый .net framework проект с TargetFrameworkVersion
                        case "net": 
                            currentProjFrameworkType = currentProjFrameworkVersionArrayLength < 2 ? "netf" : "net";   
                            break;
                            //Т.к. .NET и .NET Framework имеют одно название типа, то для фреймворка в проге условно введён тип "netf"!
                    }

                    if (currentProjFrameworkType == "netf" && currentProjFrameworkVersionArrayLength < 2)//Т.к. у нового стиля netf версия записывается без точек и обычный split на неё не действует
                        currentProjFrameworkVersionArray = SplitStrByEachNum(currentProjFrameworkVersionArray[0]);
                    
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
                            CheckProjectTargetFrameworkVersion(currentProjectFramework,reserveMaxFrameworkVersion, projName, ErrorLevel.Global);

                        return;//равносильно "-"
                    }
                }

                var maxFrameworkVersionArrayLength = currentMaxFrameworkVersionNums.Count;
                var maxFrameworkVersionString = GetFrameworkVersionString(currentMaxFrameworkVersionNums.ConvertAll(num => num.ToString()));

                //Загрузка данных об ограничениях на max_fr_version для текущего проекта
                if (!requiredMaxFrVersionsDict.ContainsKey(projName))//Имеет ли смысл делать это каждый раз при проверке правил? - Да, т.к. TargetFramework мог измениться между коммитами
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
                        untypedWarningsList.Add(projName);
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
                            untypedWarningsList.Add(projName);
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

        public static void CheckProjectReferencesOnPotentialReferencesFrameworkVersionConflict(string projName, List<string> projReferences)
        {
            if (requiredMaxFrVersionsDict.ContainsKey(projName)) //При проверке потенциального конфликта TargetFramework на текущий момент тип фреймворка не учитывается!
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

        private static string[] SplitStrByEachNum(string currentString)
        {
            int currentStringLength = currentString.Length;
            string[] resultString = new string[currentStringLength];

            for (int i = 0; i < currentStringLength; i++)
                resultString[i] = currentString[i].ToString();
            
            return resultString;
        }

        public static MaxFrameworkVersionWarnings GetMaxFrameworkVersionWarnings()
        {
            return new MaxFrameworkVersionWarnings(maxFrameworkVersionConflictWarningsList, maxFrameworkVersionReferenceConflictWarningsList);
        }

        public static MaxFrameworkRuleErrors GetMaxFrameworkRuleErrors()
        {
            return new MaxFrameworkRuleErrors(frameworkVersionComparabilityErrorList, untypedWarningsList);
        }

        public static Dictionary<string, RequiredMaxFrVersion> GetRequiredMaxFrVersionsDict()
        {
            return requiredMaxFrVersionsDict;
        }
    }
}
