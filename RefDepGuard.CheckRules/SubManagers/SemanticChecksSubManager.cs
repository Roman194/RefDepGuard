using RefDepGuard.Applied.Models.Project;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RefDepGuard.CheckRules.SubManagers
{
    public class SemanticChecksSubManager
    {
        private static List<ProjectNameSemanticWarning> projectNameSemanticWarningsList = new List<ProjectNameSemanticWarning>();

        public static void ClearSemanticCheckLists()
        {
            if (projectNameSemanticWarningsList != null)
                projectNameSemanticWarningsList.Clear();
        }

        public static void CheckProjectNamesSemantic(List<string> projectNames)
        {
            var findedSemasHashSet = new HashSet<string>();

            foreach(var projName in projectNames)
            {
                foreach (var otherProjName in projectNames)
                {
                    if (otherProjName != projName)
                    {
                        var projNameSemasArray = projName.Split('.');
                        var otherProjNameSemasArray = otherProjName.Split('.');

                        for(int i = 0; i < Math.Min(projNameSemasArray.Length, otherProjNameSemasArray.Length); i++)
                        {
                            var currentProjNameSema = projNameSemasArray[i];
                            var currentOtherProjNameSema = otherProjNameSemasArray[i];

                            if (currentProjNameSema == currentOtherProjNameSema)
                            {
                                //Если есть какая-то совпавшая сёма у 2-х+ проектов, то это уже правило
                                findedSemasHashSet.Add(currentProjNameSema);
                                continue;
                            }
                                    
                            if (currentProjNameSema.Length < 3 || currentOtherProjNameSema.Length < 3)
                                continue;

                            //Если текущая конфигурация является отклонением
                            if (!findedSemasHashSet.Contains(currentProjNameSema) && findedSemasHashSet.Contains(currentOtherProjNameSema))
                            {
                                int differSymbolsCount = 0;

                                //Отличающиеся неповторяющиеся символы
                                if (currentProjNameSema.Length >= currentOtherProjNameSema.Length)
                                    differSymbolsCount = currentProjNameSema.Except(currentOtherProjNameSema).ToList().Count;
                                else
                                    differSymbolsCount = currentOtherProjNameSema.Except(currentProjNameSema).ToList().Count;

                                //Составляем словари с количеством использованных символов в строках
                                var currentProjNameSemaDict = new Dictionary<char, int>();
                                var currentOtherProjNameSemaDict = new Dictionary<char, int>();

                                foreach (var item in currentProjNameSema)
                                {
                                    if (currentProjNameSemaDict.Keys.Contains(item))
                                        currentProjNameSemaDict[item] = (currentProjNameSemaDict[item] + 1);
                                    else
                                        currentProjNameSemaDict.Add(item, 1);
                                }

                                foreach (var item in currentOtherProjNameSema)
                                {
                                    if (currentOtherProjNameSemaDict.Keys.Contains(item))
                                        currentOtherProjNameSemaDict[item] = (currentOtherProjNameSemaDict[item] + 1);
                                    else
                                        currentOtherProjNameSemaDict.Add(item, 1);
                                }

                                //Отличающиеся символы, входящие в строки в других местах (несколько вхождений символа в строке)
                                foreach(var currentChar in currentProjNameSemaDict.Keys)
                                {
                                    if (currentOtherProjNameSemaDict.ContainsKey(currentChar))
                                    {
                                        if (currentProjNameSemaDict[currentChar] != currentOtherProjNameSemaDict[currentChar])
                                            differSymbolsCount += Math.Abs(currentOtherProjNameSemaDict[currentChar] - currentProjNameSemaDict[currentChar]);
                                    }
                                }

                                //Если отличается больше 2-х символов, то считается не опечаткой, а заплан-ым отличием + не имеет смысла проверять глубже по иерархии
                                if (differSymbolsCount > 2 || (differSymbolsCount == 2 && (currentProjNameSema.Length < 5 || currentOtherProjNameSema.Length < 5)))
                                    break;

                                //Здесь у нас случаи, когда отличаются 1-2 символа для сём с 3+ символами
                                //Это случай для генерации предупреждения. Глубже по иерархии тоже не идём, чтобы не создавать множество предупреждений на 1-м проекте

                                //Если для проекта ещё не было добавлено предупреждение
                                var projNameSemanticWarning = projectNameSemanticWarningsList.Find(ell => ell.ProjectName == projName);

                                //Если текущая конфигурация является отклонением и существует правило, то это семантическая ошибка
                                if (projNameSemanticWarning == null)
                                    projectNameSemanticWarningsList.Add(new ProjectNameSemanticWarning(projName, currentOtherProjNameSema, currentProjNameSema));

                                break;
                            }
                        }
                    }
                }
            }
        }

        public static List<ProjectNameSemanticWarning> GetProjectNamesSemanticWarningList()
        {
            return projectNameSemanticWarningsList;
        }
    }
}