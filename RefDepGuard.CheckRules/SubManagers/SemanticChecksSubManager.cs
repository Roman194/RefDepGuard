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

                            //Подсчитываем количество отличающихся символов в сёмах с учётом порядка символов
                            var differSymbolsCount = Enumerable.Range(0, Math.Min(currentProjNameSema.Length, currentOtherProjNameSema.Length))
                                                        .Count(j => currentProjNameSema[j] != currentOtherProjNameSema[j]);

                            //Разница между кол-вом символов идёт как экстра неповторяющиеся символы
                            differSymbolsCount += Math.Abs(currentProjNameSema.Length - currentOtherProjNameSema.Length);

                            //Если отличается больше 2-х символов, то считается не опечаткой, а заплан-ым отличием + не имеет смысла проверять глубже по иерархии
                            if (differSymbolsCount > 2 || (differSymbolsCount == 2 && (currentProjNameSema.Length < 5 || currentOtherProjNameSema.Length < 5))) 
                                break;

                            //Здесь у нас случаи, когда отличаются 1-2 символа для сём с 3+ символами
                            //Это случай для генерации предупреждения. Глубже по иерархии тоже не идём, чтобы не создавать множество предупреждений на 1-м проекте

                            //Если для проекта ещё не было добавлено предупреждение
                            var projNameSemanticWarning = projectNameSemanticWarningsList.Find(ell => ell.ProjectName == projName);

                            //Если текущая конфигурация является отклонением и существует правило, то это семантическая ошибка
                            if(!findedSemasHashSet.Contains(currentProjNameSema) && findedSemasHashSet.Contains(currentOtherProjNameSema) 
                                && projNameSemanticWarning == null)
                                projectNameSemanticWarningsList.Add(new ProjectNameSemanticWarning(projName, currentOtherProjNameSema, currentProjNameSema));

                            break;
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