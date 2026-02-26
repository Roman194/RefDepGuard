using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RefDepGuard.Data;

namespace RefDepGuard
{
    public class CurrentStateManager
    {
        public static Dictionary<string, ProjectState> GetCurrentProjectState(DTE dte)
        {
            return GetCurrentRequiredState(dte, false);
        }

        public static Dictionary<string, List<string>> GetCurrentReferencesState(DTE dte)
        {
            Dictionary<string, List<string>> currentReferences = 
                GetCurrentRequiredState(dte, true).ToDictionary(
                    project => project.Key, 
                    project => project.Value.CurrentReferences
                );  

            return currentReferences;
        }

        private static Dictionary<string, ProjectState> GetCurrentRequiredState(DTE dte, bool isOnlyRefsNeeded)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Dictionary<string, ProjectState> commitedProjState = new Dictionary<string, ProjectState>();
            EnvDTE.Solution solution = dte.Solution;

            foreach (EnvDTE.Project project in solution.Projects)
            {
                if (project.FullName != null && project.FullName.Length != 0)
                {
                    string projectFrameworkVersions = "";
                    Dictionary<string, List<int>> projectFrameworkNumVersions = new Dictionary<string, List<int>>();

                    if (!isOnlyRefsNeeded)
                    {
                        projectFrameworkVersions = MSBuildManager.GetTargetFrameworkForProject(project.FullName);
                        projectFrameworkNumVersions = ConvertTargetFrameworkToTransferFormat(projectFrameworkVersions);
                    }

                    VSLangProj.VSProject vSProject = project.Object as VSLangProj.VSProject;

                    if (vSProject != null)
                    {
                        var refsList = new List<string>();

                        foreach (VSLangProj.Reference vRef in vSProject.References)
                        {
                            if (vRef.SourceProject != null)
                                refsList.Add(vRef.Name);
                        }

                        commitedProjState.Add(vSProject.Project.Name, new ProjectState(projectFrameworkNumVersions, projectFrameworkVersions, refsList));
                    }
                }
            }

            return commitedProjState;
        }

        private static Dictionary<string, List<int>> ConvertTargetFrameworkToTransferFormat(string targetFrameworkString)
        {
            Dictionary<string, List<int>> currentTargetFrameworksDict = new Dictionary<string, List<int>>();
            

            if (String.IsNullOrEmpty(targetFrameworkString)) //Если не получилось спарсить строку с таргетами, то возвращаем пустой словарь
                return currentTargetFrameworksDict;
            

            //В случае если строка идёт из TargetFrameworks (Maui и пр.) нужно предварительное деление по ";"
            //Нужно проверить каждый из таргетов на предмет противоречия макс версии.
            //Если версии таргетов и их макс ограничения совпадают (как в Maui), то у них будет одна общая ошибка (при превышении макс версии)
            var currentProjectSupportedFrameworksArray = targetFrameworkString.Split(';');

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
                    //Т.к. у нового стиля netf версия записывается без точек и обычный split на неё не действует
                    if (currentProjFrameworkType == "netf" && currentProjFrameworkVersionArrayLength < 2)
                        currentProjFrameworkVersionArray = SplitStrByEachNum(currentProjFrameworkVersionArray[0]);

                }

                List<int> currentProjFrameworkVersionList = ConvertTargetFrameworkVersionToIntNums(currentProjFrameworkVersionArray);

                // НА текущий момент, если встретилась ошибка парсинга цифр TargetFramework, то вернётся словарь с уже записанными версиями (если такие имеются) 
                if (currentProjFrameworkVersionList.Count == 0) 
                    return currentTargetFrameworksDict;

                if (currentTargetFrameworksDict.ContainsKey(currentProjFrameworkType))
                { //Если уже есть какая-то TF-версия для этого типа проекта, то нужно их сравнить и закоммитить MAX-ую из них
                    List<int> commitedTargetFrameworkVersionList = currentTargetFrameworksDict[currentProjFrameworkType];
                    for(int i = 0; i < currentProjFrameworkVersionList.Count; i++)
                    {
                        int currentProjTargetFrameworkNum = currentProjFrameworkVersionList[i];
                        int commitedTargetFrameworkNum = commitedTargetFrameworkVersionList[i];

                        if (currentProjTargetFrameworkNum > commitedTargetFrameworkNum)
                        {
                            currentTargetFrameworksDict[currentProjFrameworkType] = currentProjFrameworkVersionList;
                            break;
                        }
                    }
                }
                else
                {
                    currentTargetFrameworksDict.Add(currentProjFrameworkType, currentProjFrameworkVersionList);
                }
            }

            return currentTargetFrameworksDict;
        }

        private static string[] SplitStrByEachNum(string currentString)
        {
            int currentStringLength = currentString.Length;
            string[] resultString = new string[currentStringLength];

            for (int i = 0; i < currentStringLength; i++)
                resultString[i] = currentString[i].ToString();

            return resultString;
        }

        private static List<int> ConvertTargetFrameworkVersionToIntNums(string[] targetFrameworkVersionsArray)
        {
            List<int> targetFrameworkVersionsNums = new List<int>();
            for (int i = 0; i < targetFrameworkVersionsArray.Length; i++)
            {
                int currentVersionNum = 0;
                if (!Int32.TryParse(targetFrameworkVersionsArray[i], out currentVersionNum))
                {
                    return new List<int>();
                }
                targetFrameworkVersionsNums.Add(currentVersionNum);
            }

            return targetFrameworkVersionsNums; 
        }

    }
}
