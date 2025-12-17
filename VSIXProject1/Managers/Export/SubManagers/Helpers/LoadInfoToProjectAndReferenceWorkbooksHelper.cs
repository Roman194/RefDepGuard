using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using VSIXProject1.Comparators;
using VSIXProject1.Data;
using VSIXProject1.Data.FrameworkVersion;
using VSIXProject1.Data.Reference;

namespace VSIXProject1.Managers.Export.SubManagers
{
    public class LoadInfoToProjectAndReferenceWorkbooksHelper
    {
        public static void LoadInfoToProjectsWorkbook(Application excel, string solutionName, string currentDateTime, Dictionary<string, ProjectState> commitedProjectsState, RefDepGuardExportParameters refDepGuardExportParameters)
        {
            RefDepGuardErrors refDepGuardErrors = refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardErrors;
            Dictionary<string, RequiredMaxFrVersion> requiredMaxFrVersions = refDepGuardExportParameters.RequiredParametersData.MaxRequiredFrameworkVersion;

            List<ReferenceError> refsErrorList = refDepGuardErrors.RefsErrorList;
            List<MaxFrameworkVersionDeviantValueError> maxFrVersionDeviantValuesList = refDepGuardErrors.MaxFrameworkVersionDeviantValueList;
            List<FrameworkVersionComparabilityError> frameworkVersionComparabilityErrorsList = refDepGuardErrors.FrameworkVersionComparabilityErrorList;


            Worksheet projectsTable = (Worksheet)excel.Worksheets[1];
            projectsTable.Name = "Выборка по проектам";

            //Загрузка и стилизация шапки таблицы
            projectsTable.Cells[2, 2] = "Solution: \"" + solutionName + "\"";
            projectsTable.Cells[3, 2] = currentDateTime;
            projectsTable.Cells[2, 2].Font.Bold = projectsTable.Cells[3, 2].Font.Bold = true;

            projectsTable.Cells[4, 2] = "№";

            projectsTable.Cells[4, 3] = "Проект";

            projectsTable.Cells[4, 4] = "Целевая рабочая\nсреда";
            projectsTable.Cells[4, 5] = "Макс. допустимая\nверсия";

            projectsTable.Cells[4, 6] = "Всего референсов";

            projectsTable.Cells[5, 6] = projectsTable.Cells[5, 8] = projectsTable.Cells[5, 10] = "Кол-во";
            projectsTable.Cells[5, 7] = projectsTable.Cells[5, 9] = projectsTable.Cells[5, 11] = "Названия";

            projectsTable.Cells[4, 8] = "Не обнаружено обязательных референсов";

            projectsTable.Cells[4, 10] = "Обнаружено недопустимых референсов";

            projectsTable.Columns[4].ColumnWidth = 17;
            projectsTable.Columns[5].ColumnWidth = 17;
            projectsTable.Columns[7].ColumnWidth = 35;
            projectsTable.Columns[9].ColumnWidth = 35;
            projectsTable.Columns[11].ColumnWidth = 35;

            Range unionRangeSolutionName = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[2, 11]];
            Range unionRangeGenerateTime = projectsTable.Range[projectsTable.Cells[3, 2], projectsTable.Cells[3, 11]];
            Range unionRangeSolutionNameAndGenerateTime = projectsTable.Range[unionRangeSolutionName, unionRangeGenerateTime];
            Range unionRangeNumTitle = projectsTable.Range[projectsTable.Cells[4, 2], projectsTable.Cells[5, 2]];
            Range unionRangeProjectNameTitle = projectsTable.Range[projectsTable.Cells[4, 3], projectsTable.Cells[5, 3]];
            Range unionRangeTargetFrameworkTitle = projectsTable.Range[projectsTable.Cells[4, 4], projectsTable.Cells[5, 4]];
            Range unionRangeMaxFrVersionTitle = projectsTable.Range[projectsTable.Cells[4, 5], projectsTable.Cells[5, 5]];
            Range unionRangeReferencesCountTitle = projectsTable.Range[projectsTable.Cells[4, 6], projectsTable.Cells[4, 7]];
            Range unionRangeRequiredRefsErrors = projectsTable.Range[projectsTable.Cells[4, 8], projectsTable.Cells[4, 9]];
            Range unionRangeUnacceptableRefsErrorsTitle = projectsTable.Range[projectsTable.Cells[4, 10], projectsTable.Cells[4, 11]];

            Range unionRangeTableTitle = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[5, 11]];

            unionRangeSolutionName.Merge();
            unionRangeGenerateTime.Merge();
            unionRangeNumTitle.Merge();
            unionRangeProjectNameTitle.Merge();
            unionRangeTargetFrameworkTitle.Merge();
            unionRangeMaxFrVersionTitle.Merge();
            unionRangeReferencesCountTitle.Merge();
            unionRangeRequiredRefsErrors.Merge();
            unionRangeUnacceptableRefsErrorsTitle.Merge();

            unionRangeTableTitle.HorizontalAlignment = XlVAlign.xlVAlignCenter;

            //Загрузка данных в саму таблицу
            int i = 0;
            foreach (var currentProject in commitedProjectsState)
            {
                string currentProjectName = currentProject.Key;
                List<string> currentPorjectRefs = currentProject.Value.CurrentReferences;
                string targetFramework = currentProject.Value.CurrentFrameworkVersions;
                List<string> requiredRefsErrors = refsErrorList
                    .Where(value => value.IsReferenceRequired && value.ErrorRelevantProjectName == currentProjectName)
                    .Select(value => value.ReferenceName)
                    .ToList();

                List<string> unacceptableRefsErrors = refsErrorList
                    .Where(value => !value.IsReferenceRequired && value.ErrorRelevantProjectName == currentProjectName)
                    .Select(value => value.ReferenceName)
                    .ToList();
                int requiredRefsErrorsCount = requiredRefsErrors.Count();
                int unacceptableRefsErrorsCount = unacceptableRefsErrors.Count();

                if (i == 0)
                    projectsTable.Cells[6, 2] = "1";
                else
                    projectsTable.Cells[6 + i, 2].FormulaLocal = $"=B{i + 5} + 1";

                projectsTable.Cells[6 + i, 3] = currentProjectName;

                projectsTable.Cells[6 + i, 4] = targetFramework;

                Range currentMaxFrVersionCellRange = projectsTable.Range[projectsTable.Cells[6 + i, 5], projectsTable.Cells[6 + i, 5]];
                currentMaxFrVersionCellRange.NumberFormat = "@";

                //Проверить при ошибке на уровнях выше Project
                if (maxFrVersionDeviantValuesList.Contains(new MaxFrameworkVersionDeviantValueError(ErrorLevel.Global, currentProjectName), new MaxFrameworkVersionDeviantValueExportContainsComparer()))
                    projectsTable.Cells[6 + i, 5] = "?";
                else
                {
                    if (requiredMaxFrVersions.ContainsKey(currentProjectName))
                    {
                        var currentMaxFrVersionRule = requiredMaxFrVersions[currentProjectName];
                        var ruleLevelString = "";
                        switch (currentMaxFrVersionRule.ErrorLevel)
                        {
                            case ErrorLevel.Global: ruleLevelString = "[G]"; break;
                            case ErrorLevel.Solution: ruleLevelString = "[S]"; break;
                        }
                        projectsTable.Cells[6 + i, 5] = currentMaxFrVersionRule.VersionText + ruleLevelString;

                        if (frameworkVersionComparabilityErrorsList.Contains(new FrameworkVersionComparabilityError(ErrorLevel.Global, "", "", currentProjectName), new FrameworkVersionComparabilityErrorExportContainsComparer()))
                            projectsTable.Cells[6 + i, 5].Font.Color = 0x062CCE;
                    }
                    else
                        projectsTable.Cells[6 + i, 5] = "-";

                }

                projectsTable.Cells[6 + i, 6] = currentPorjectRefs.Count;
                projectsTable.Cells[6 + i, 7] = GetProjectsString(currentPorjectRefs);

                projectsTable.Cells[6 + i, 8] = requiredRefsErrorsCount;

                if (requiredRefsErrorsCount > 0)
                {
                    projectsTable.Cells[6 + i, 8].Interior.Color = projectsTable.Cells[6 + i, 9].Interior.Color = 0xCEC7FF;//На самом деле это #FFC7CE, просто Interop зачем-то "разворачивает" это значение
                    projectsTable.Cells[6 + i, 8].Font.Color = projectsTable.Cells[6 + i, 9].Font.Color = 0x062CCE;
                }

                projectsTable.Cells[6 + i, 9] = GetProjectsString(requiredRefsErrors);

                projectsTable.Cells[6 + i, 10] = unacceptableRefsErrorsCount;
                if (unacceptableRefsErrorsCount > 0)
                {
                    projectsTable.Cells[6 + i, 10].Interior.Color = projectsTable.Cells[6 + i, 11].Interior.Color = 0xCEC7FF;
                    projectsTable.Cells[6 + i, 10].Font.Color = projectsTable.Cells[6 + i, 11].Font.Color = 0x062CCE;
                }

                var unacceptableRefsErrorsProjectString = GetProjectsString(unacceptableRefsErrors);
                projectsTable.Cells[6 + i, 11] = unacceptableRefsErrorsProjectString;

                //Смена формата ячейки для того, чтобы при большом количестве рефов содержимое не выходило за пределы ячейки
                if (unacceptableRefsErrorsProjectString.Length > 15)
                {
                    Range currentCellRange = projectsTable.Range[projectsTable.Cells[6 + i, 11], projectsTable.Cells[6 + i, 11]];
                    currentCellRange.HorizontalAlignment = XlHAlign.xlHAlignFill;
                }

                i++;
            }

            //Работа с границами
            int projectsCount = commitedProjectsState.Count;

            Range unionRangeAllTable = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[projectsCount + 5, 11]];
            Range unionRangeNum = projectsTable.Range[projectsTable.Cells[6, 2], projectsTable.Cells[projectsCount + 5, 2]];
            Range unionRangeNumWithTitle = projectsTable.Range[projectsTable.Cells[4, 2], unionRangeNum];
            Range unionRangeProjectName = projectsTable.Range[projectsTable.Cells[6, 3], projectsTable.Cells[projectsCount + 5, 3]];
            Range unionRangeTargetFramework = projectsTable.Range[projectsTable.Cells[6, 4], projectsTable.Cells[projectsCount + 5, 4]];
            Range unionRangeMaxFrVersionWithTitle = projectsTable.Range[projectsTable.Cells[4, 5], projectsTable.Cells[projectsCount + 5, 5]];
            Range unionRangeReferencesCount = projectsTable.Range[projectsTable.Cells[6, 6], projectsTable.Cells[projectsCount + 5, 6]];
            Range unionRangeRequiredRefsErrorsCount = projectsTable.Range[projectsTable.Cells[6, 8], projectsTable.Cells[projectsCount + 5, 8]];
            Range unionRangeUnacceptableRefsErrorsCount = projectsTable.Range[projectsTable.Cells[6, 10], projectsTable.Cells[projectsCount + 5, 10]];

            unionRangeAllTable.Borders.Color = ColorTranslator.ToOle(Color.Black);
            unionRangeAllTable.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);
            unionRangeSolutionNameAndGenerateTime.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);
            unionRangeTableTitle.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);
            unionRangeNumWithTitle.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);

            //Работа с центровкой числовых столбцов
            unionRangeNum.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            unionRangeNum.EntireColumn.AutoFit();
            unionRangeProjectName.EntireColumn.AutoFit();
            unionRangeMaxFrVersionWithTitle.HorizontalAlignment = XlVAlign.xlVAlignCenter;

            unionRangeReferencesCount.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            unionRangeReferencesCount.EntireColumn.AutoFit();

            unionRangeRequiredRefsErrorsCount.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            unionRangeRequiredRefsErrorsCount.EntireColumn.AutoFit();

            unionRangeUnacceptableRefsErrorsCount.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            unionRangeUnacceptableRefsErrorsCount.EntireColumn.AutoFit();
        }

        public static void LoadInfoToReferencesBook(Application excel, string solutionName, string currentDateTime, Dictionary<string, ProjectState> commitedProjectsState, RefDepGuardExportParameters refDepGuardExportParameters)
        {
            List<ReferenceError> refsErrorList = refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardErrors.RefsErrorList;
            List<RequiredReference> requiredReferences = refDepGuardExportParameters.RequiredParametersData.RequiredReferences;

            Worksheet projectsTable = (Worksheet)excel.Worksheets[2];
            projectsTable.Name = "Выборка по референсам";

            projectsTable.Cells[2, 2] = "Solution: \"" + solutionName + "\"";
            projectsTable.Cells[3, 2] = currentDateTime;
            projectsTable.Cells[2, 2].Font.Bold = projectsTable.Cells[3, 2].Font.Bold = true;

            projectsTable.Cells[4, 2] = "№";
            projectsTable.Cells[4, 3] = "Референс";
            projectsTable.Cells[4, 4] = "Проект";
            projectsTable.Cells[4, 5] = "Тип референса";

            Range unionRangeSolutionName = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[2, 5]];
            Range unionRangeGenerateTime = projectsTable.Range[projectsTable.Cells[3, 2], projectsTable.Cells[3, 5]];
            Range unionRangeSolutionWithTime = projectsTable.Range[unionRangeSolutionName, unionRangeGenerateTime];

            Range unionRangeTableTitle = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[4, 5]];

            unionRangeSolutionName.Merge();
            unionRangeGenerateTime.Merge();

            unionRangeTableTitle.HorizontalAlignment = XlVAlign.xlVAlignCenter;

            int i = 0;
            foreach (var currentProject in commitedProjectsState)
            {
                string projectName = currentProject.Key;
                foreach (var projectReference in currentProject.Value.CurrentReferences)
                {
                    if (i == 0)
                        projectsTable.Cells[5, 2] = "1";
                    else
                        projectsTable.Cells[5 + i, 2].FormulaLocal = $"=B{i + 4} + 1";

                    projectsTable.Cells[5 + i, 3] = projectReference;
                    projectsTable.Cells[5 + i, 4] = projectName;

                    projectsTable.Cells[5 + i, 5] = "-";

                    ReferenceError referenceError = refsErrorList
                        .Where(value => value.ErrorRelevantProjectName == projectName && value.ReferenceName == projectReference && value.IsReferenceRequired == false)
                        .FirstOrDefault(); //Должно найтись не более одного такого значения

                    if (referenceError != null)
                    {
                        projectsTable.Cells[5 + i, 5] = "Недопустимый";
                        projectsTable.Cells[5 + i, 5].Interior.Color = 0xCEC7FF;
                        projectsTable.Cells[5 + i, 5].Font.Color = 0x062CCE;

                    }

                    RequiredReference requiredReference = requiredReferences
                        .Where(value => value.ReferenceName == projectReference && (value.RelevantProject == projectName || value.RelevantProject == ""))
                        .FirstOrDefault();

                    if (requiredReference != null)
                    {
                        projectsTable.Cells[5 + i, 5] = "Обязательный";
                        projectsTable.Cells[5 + i, 5].Interior.Color = 0xCEEFC6;
                        projectsTable.Cells[5 + i, 5].Font.Color = 0x006100;
                    }

                    i++;
                }
            }

            Range unionRangeAllTable = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[i + 4, 5]];
            Range unionRangeNumWithTitle = projectsTable.Range[projectsTable.Cells[4, 2], projectsTable.Cells[i + 4, 2]];

            unionRangeAllTable.Borders.Color = ColorTranslator.ToOle(Color.Black);
            unionRangeAllTable.EntireColumn.AutoFit();
            unionRangeAllTable.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);

            unionRangeNumWithTitle.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            unionRangeNumWithTitle.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);

            unionRangeTableTitle.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);
            unionRangeSolutionWithTime.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);
        }

        private static string GetProjectsString(List<String> projectNames)
        {
            string projectString = "";
            int projectsCount = projectNames.Count;

            int j = 0;
            foreach (string currentRefError in projectNames)
            {
                if (j < projectsCount - 1)
                    projectString += currentRefError + ", ";
                else
                    projectString += currentRefError;

                j++;
            }

            if (projectString == "")
                projectString = "-";

            return projectString;
        }
    }
}
