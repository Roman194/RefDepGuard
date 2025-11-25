using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject1.Comparators;
using VSIXProject1.Data.FrameworkVersion;

namespace VSIXProject1.Managers.CheckRules
{
    public class MaxFrameworkRulesChecksSubManager
    {
        static List<MaxFrameworkVersionConflictWarning> maxFrameworkVersionConflictWarningsList;
        static List<MaxFrameworkVersionReferenceConflictWarning> maxFrameworkVersionReferenceConflictWarningsList;

        public static Tuple<List<MaxFrameworkVersionConflictWarning>, List<MaxFrameworkVersionReferenceConflictWarning>> CheckMaxFrameworkVersionOneLevelConflict(Dictionary<string, List<int>> currentMaxFrameworkVersion, 
            List<MaxFrameworkVersionConflictWarning> currentMaxFrameworkVersionConflictWarningsList, 
            List<MaxFrameworkVersionReferenceConflictWarning> currentMaxFrameworkVersionReferenceConflictWarningsList, 
            ErrorLevel ruleLevel, string projName = "-")
        {
            maxFrameworkVersionConflictWarningsList = currentMaxFrameworkVersionConflictWarningsList;
            maxFrameworkVersionReferenceConflictWarningsList = currentMaxFrameworkVersionReferenceConflictWarningsList;

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

            return new Tuple<List<MaxFrameworkVersionConflictWarning>,List<MaxFrameworkVersionReferenceConflictWarning>>(maxFrameworkVersionConflictWarningsList, maxFrameworkVersionReferenceConflictWarningsList);
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

        private static string GetFrameworkVersionString(List<string> targetFrameworkVersionArray)//???
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

        //public static void CheckPrjMaxFrwrkVrsnDifferentLevelsConflicts(Dictionary<string, List<int>> maxLowLevelFrameworkVersion, Dictionary<string, List<int>> maxHighLevelFrameworkVersion, string projName, ErrorLevel lowRuleLevel, ErrorLevel highRuleLevel)
        //{
        //    foreach (var currentMaxLowLevelFrameworkVersion in maxLowLevelFrameworkVersion)
        //    {
        //        var currentMaxLowLevelFrameworkVersionType = currentMaxLowLevelFrameworkVersion.Key;
        //        List<int> maxLowLevelFrameworkVersionArray = currentMaxLowLevelFrameworkVersion.Value;
        //        List<int> maxHighLevelFrameworkVersionArray = new List<int>();

        //        if (maxHighLevelFrameworkVersion.ContainsKey(currentMaxLowLevelFrameworkVersionType)) //Если типы версий фреймворков совпадают
        //        {
        //            maxHighLevelFrameworkVersionArray = maxHighLevelFrameworkVersion[currentMaxLowLevelFrameworkVersionType]; //То проверить соотв. версии
        //            CheckMaxFrameworkVersionCurrentConflict(maxHighLevelFrameworkVersionArray, maxLowLevelFrameworkVersionArray, projName, lowRuleLevel, highRuleLevel);
        //        }
        //        else
        //        {
        //            if (maxHighLevelFrameworkVersion.ContainsKey("all")) //Если сверху супертип "all", то сравнить с ним
        //            {
        //                maxHighLevelFrameworkVersionArray = maxHighLevelFrameworkVersion["all"];
        //                CheckMaxFrameworkVersionCurrentConflict(maxHighLevelFrameworkVersionArray, maxLowLevelFrameworkVersionArray, projName, lowRuleLevel, highRuleLevel);
        //            }
        //            else
        //            {
        //                if (maxLowLevelFrameworkVersion.ContainsKey("all")) //если снизу супертип "all", то сравнить все вышестоящие с ним
        //                {
        //                    foreach (var currentMaxHighLevelFrameworkVersion in maxHighLevelFrameworkVersion)
        //                    {
        //                        maxHighLevelFrameworkVersionArray = currentMaxHighLevelFrameworkVersion.Value;
        //                        CheckMaxFrameworkVersionCurrentConflict(maxHighLevelFrameworkVersionArray, maxLowLevelFrameworkVersionArray, projName, lowRuleLevel, highRuleLevel);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
