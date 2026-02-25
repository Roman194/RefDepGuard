using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using RefDepGuard.Data;
using RefDepGuard.Data.FrameworkVersion;
using RefDepGuard.Data.Reference;

namespace RefDepGuard.Managers.Export.SubManagers
{
    public class LoadInfoToProjectAndReferenceWorkbooksHelper
    {
        //Посмотреть что можно оптимизировать в визуальной настройке по аналогии с ProblemsHelper
        public static void LoadInfoToProjectsWorkbook(Application excel, string solutionName, string currentDateTime, Dictionary<string, ProjectState> commitedProjectsState, RefDepGuardExportParameters refDepGuardExportParameters)
        {
            RefDepGuardErrors refDepGuardErrors = refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardErrors;
            RefDepGuardWarnings refDepGuardWarnings = refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardWarnings;
            Dictionary<string, RequiredMaxFrVersion> requiredMaxFrVersions = refDepGuardExportParameters.RequiredParametersData.MaxRequiredFrameworkVersion;

            List<ReferenceError> refsErrorList = refDepGuardErrors.RefsErrorList;
            List<MaxFrameworkVersionDeviantValueError> maxFrVersionDeviantValuesList = refDepGuardErrors.MaxFrameworkVersionDeviantValueList;
            List<FrameworkVersionComparabilityError> frameworkVersionComparabilityErrorsList = refDepGuardErrors.FrameworkVersionComparabilityErrorList;
            List<MaxFrameworkVersionConflictWarning> maxFrameworkVersionConflictWarningsList = refDepGuardWarnings.MaxFrameworkVersionConflictWarningsList;

            int widthIndex = 11;
            int heightIndex = 5; //кол-во строк, которые нужно отсчитать от начала, чтобы перейти за пределы "шапки" таблицы
            int firstRowIndex = heightIndex + 1;

            Worksheet projectsTable = (Worksheet)excel.Worksheets[1];
            projectsTable.Name = "Выборка по проектам";

            //Загрузка и стилизация шапки таблицы
            projectsTable = SetUnionColumnNames(projectsTable, solutionName, currentDateTime);
            projectsTable.Cells[heightIndex - 1, 3] = "Проект";
            projectsTable.Cells[heightIndex - 1, 4] = "Целевая рабочая\nсреда";
            projectsTable.Cells[heightIndex - 1, 5] = "Макс. допустимая\nверсия";
            projectsTable.Cells[heightIndex - 1, 6] = "Всего референсов";
            projectsTable.Cells[heightIndex, 6] = projectsTable.Cells[heightIndex, 8] = projectsTable.Cells[heightIndex, 10] = "Кол-во";
            projectsTable.Cells[heightIndex, 7] = projectsTable.Cells[heightIndex, 9] = projectsTable.Cells[heightIndex, 11] = "Названия";
            projectsTable.Cells[heightIndex - 1, 8] = "Не обнаружено обязательных референсов";
            projectsTable.Cells[heightIndex - 1, 10] = "Обнаружено недопустимых референсов";

            projectsTable.Columns[4].ColumnWidth = 17;
            projectsTable.Columns[5].ColumnWidth = 17;
            projectsTable.Columns[7].ColumnWidth = 35;
            projectsTable.Columns[9].ColumnWidth = 35;
            projectsTable.Columns[11].ColumnWidth = 35;

            Range unionRangeSolutionNameAndGenerateTime, unionRangeTableTitle;
            (unionRangeSolutionNameAndGenerateTime, unionRangeTableTitle) = SetUnionTableHatRanges(projectsTable, 11, 5);
            Range unionRangeNumTitle = projectsTable.Range[projectsTable.Cells[heightIndex - 1, 2], projectsTable.Cells[heightIndex, 2]];
            Range unionRangeProjectNameTitle = projectsTable.Range[projectsTable.Cells[heightIndex - 1, 3], projectsTable.Cells[heightIndex, 3]];
            Range unionRangeTargetFrameworkTitle = projectsTable.Range[projectsTable.Cells[heightIndex - 1, 4], projectsTable.Cells[heightIndex, 4]];
            Range unionRangeMaxFrVersionTitle = projectsTable.Range[projectsTable.Cells[heightIndex - 1, 5], projectsTable.Cells[heightIndex, 5]];
            Range unionRangeReferencesCountTitle = projectsTable.Range[projectsTable.Cells[heightIndex - 1, 6], projectsTable.Cells[heightIndex - 1, 7]];
            Range unionRangeRequiredRefsErrors = projectsTable.Range[projectsTable.Cells[heightIndex - 1, 8], projectsTable.Cells[heightIndex - 1, 9]];
            Range unionRangeUnacceptableRefsErrorsTitle = projectsTable.Range[projectsTable.Cells[heightIndex - 1, 10], projectsTable.Cells[heightIndex - 1, 11]];

            unionRangeNumTitle.Merge();
            unionRangeProjectNameTitle.Merge();
            unionRangeTargetFrameworkTitle.Merge();
            unionRangeMaxFrVersionTitle.Merge();
            unionRangeReferencesCountTitle.Merge();
            unionRangeRequiredRefsErrors.Merge();
            unionRangeUnacceptableRefsErrorsTitle.Merge();

            //Загрузка данных в саму таблицу
            int i = 0;
            foreach (var currentProject in commitedProjectsState)
            {
                string currentProjectName = currentProject.Key;
                List<string> currentPorjectRefs = currentProject.Value.CurrentReferences;
                string targetFramework = currentProject.Value.CurrentFrameworkVersionsString;
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

                projectsTable = SetCurrentRowNum(projectsTable, i, heightIndex);
                projectsTable.Cells[firstRowIndex + i, 3] = currentProjectName;
                projectsTable.Cells[firstRowIndex + i, 4] = targetFramework;

                Range currentMaxFrVersionCellRange = projectsTable.Range[projectsTable.Cells[firstRowIndex + i, 5], projectsTable.Cells[firstRowIndex + i, 5]];
                currentMaxFrVersionCellRange.NumberFormat = "@";

                //Проверить при ошибке на уровнях выше Project
                if (requiredMaxFrVersions.ContainsKey(currentProjectName))
                {
                    var currentMaxFrVersionRule = requiredMaxFrVersions[currentProjectName];
                    var ruleLevelString = "";
                    switch (currentMaxFrVersionRule.ReqLevel)
                    {
                        case ProblemLevel.Global: ruleLevelString = "[G]"; break;
                        case ProblemLevel.Solution: ruleLevelString = "[S]"; break;
                    }
                    projectsTable.Cells[firstRowIndex + i, 5] = currentMaxFrVersionRule.VersionText + ruleLevelString;

                    if(frameworkVersionComparabilityErrorsList.Find(warning => warning.ErrorRelevantProjectName == currentProjectName) != null)
                        projectsTable.Cells[firstRowIndex + i, 5].Font.Color = 0x062CCE;
                    else
                    {
                        if(maxFrameworkVersionConflictWarningsList.Find(warning => warning.WarningRelevantProjectName == currentProjectName) != null)
                            projectsTable.Cells[firstRowIndex + i, 5].Font.Color = 0x00C0FF; //Текст #FFC000
                    }
                }
                else
                {
                    if (maxFrVersionDeviantValuesList.Find(value => value.ErrorRelevantProjectName == currentProjectName) != null ||
                        maxFrVersionDeviantValuesList.Find(value => value.ErrorRelevantProjectName == "") != null)
                        projectsTable.Cells[firstRowIndex + i, 5] = "?";
                    else
                        projectsTable.Cells[firstRowIndex + i, 5] = "-";
                }

                projectsTable.Cells[firstRowIndex + i, 6] = currentPorjectRefs.Count;
                projectsTable.Cells[firstRowIndex + i, 7] = GetProjectsString(currentPorjectRefs);

                projectsTable.Cells[firstRowIndex + i, 8] = requiredRefsErrorsCount;

                if (requiredRefsErrorsCount > 0)
                {
                    projectsTable.Cells[firstRowIndex + i, 8].Interior.Color = projectsTable.Cells[firstRowIndex + i, 9].Interior.Color = 0xCEC7FF;//На самом деле это #FFC7CE, просто Interop зачем-то "разворачивает" это значение
                    projectsTable.Cells[firstRowIndex + i, 8].Font.Color = projectsTable.Cells[firstRowIndex + i, 9].Font.Color = 0x062CCE;
                }

                projectsTable.Cells[firstRowIndex + i, 9] = GetProjectsString(requiredRefsErrors);

                projectsTable.Cells[firstRowIndex + i, 10] = unacceptableRefsErrorsCount;
                if (unacceptableRefsErrorsCount > 0)
                {
                    projectsTable.Cells[firstRowIndex + i, 10].Interior.Color = projectsTable.Cells[firstRowIndex + i, 11].Interior.Color = 0xCEC7FF;
                    projectsTable.Cells[firstRowIndex + i, 10].Font.Color = projectsTable.Cells[firstRowIndex + i, 11].Font.Color = 0x062CCE;
                }

                var unacceptableRefsErrorsProjectString = GetProjectsString(unacceptableRefsErrors);
                projectsTable.Cells[firstRowIndex + i, 11] = unacceptableRefsErrorsProjectString;

                //Смена формата ячейки для того, чтобы при большом количестве рефов содержимое не выходило за пределы ячейки
                if (unacceptableRefsErrorsProjectString.Length > 15)
                {
                    Range currentCellRange = projectsTable.Range[projectsTable.Cells[firstRowIndex + i, 11], projectsTable.Cells[firstRowIndex + i, 11]];
                    currentCellRange.HorizontalAlignment = XlHAlign.xlHAlignFill;
                }

                i++;
            }

            //Работа с границами
            projectsTable = SetUnionTableStyle(projectsTable, unionRangeSolutionNameAndGenerateTime, unionRangeTableTitle, i, heightIndex, widthIndex, false);

            Range unionRangeProjectName = projectsTable.Range[projectsTable.Cells[firstRowIndex, 3], projectsTable.Cells[i + heightIndex, 3]];
            Range unionRangeTargetFramework = projectsTable.Range[projectsTable.Cells[firstRowIndex, 4], projectsTable.Cells[i + heightIndex, 4]];
            Range unionRangeMaxFrVersionWithTitle = projectsTable.Range[projectsTable.Cells[heightIndex - 1, 5], projectsTable.Cells[i + heightIndex, 5]];
            Range unionRangeReferencesCount = projectsTable.Range[projectsTable.Cells[firstRowIndex, 6], projectsTable.Cells[i + heightIndex, 6]];
            Range unionRangeRequiredRefsErrorsCount = projectsTable.Range[projectsTable.Cells[firstRowIndex, 8], projectsTable.Cells[i + heightIndex, 8]];
            Range unionRangeUnacceptableRefsErrorsCount = projectsTable.Range[projectsTable.Cells[firstRowIndex, 10], projectsTable.Cells[i + heightIndex, 10]];

            //Работа с центровкой числовых столбцов
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
            List<MaxFrameworkVersionReferenceConflictWarning> maxFrameworkVersionReferenceConflictWarningsList = 
                refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardWarnings.MaxFrameworkVersionReferenceConflictWarningsList;

            bool isPotentialVersionConflict = false;
            int widthIndex = 5;
            int heightIndex = 4;
            int firstRowIndex = heightIndex + 1;

            Worksheet projectsTable = (Worksheet)excel.Worksheets[2];
            projectsTable.Name = "Выборка по референсам";

            projectsTable = SetUnionColumnNames(projectsTable, solutionName, currentDateTime);

            projectsTable.Cells[heightIndex, 3] = "Референс"; //А выше подобное heightIndex - 1. Это норм или нет?
            projectsTable.Cells[heightIndex, 4] = "Проект";
            projectsTable.Cells[heightIndex, 5] = "Тип референса";

            Range unionRangeSolutionWithTime, unionRangeTableTitle;
            (unionRangeSolutionWithTime, unionRangeTableTitle) = SetUnionTableHatRanges(projectsTable, widthIndex, heightIndex);

            int i = 0;
            foreach (var currentProject in commitedProjectsState)
            {
                string projectName = currentProject.Key;
                foreach (var projectReference in currentProject.Value.CurrentReferences)
                {
                    projectsTable = SetCurrentRowNum(projectsTable, i, heightIndex);
                    projectsTable.Cells[firstRowIndex + i, 3] = projectReference;
                    projectsTable.Cells[firstRowIndex + i, 4] = projectName;
                    projectsTable.Cells[firstRowIndex + i, 5] = "-";

                    //Выделения типа связи расставлены в порядке обратном порядку приоритезации
                    RequiredReference requiredReference = requiredReferences
                        .Where(value => value.ReferenceName == projectReference && (value.RelevantProject == projectName || value.RelevantProject == ""))
                        .FirstOrDefault(); //Должно найтись не более одного такого значения

                    if (requiredReference != null) //Оптимизировать задание стиля!!!
                    {
                        projectsTable.Cells[firstRowIndex + i, 5] = "Обязательный";
                        projectsTable.Cells[firstRowIndex + i, 5].Interior.Color = 0xCEEFC6;
                        projectsTable.Cells[firstRowIndex + i, 5].Font.Color = 0x006100;
                    }

                    MaxFrameworkVersionReferenceConflictWarning maxFrameworkVersionReference = maxFrameworkVersionReferenceConflictWarningsList
                        .Where(value => value.RefName ==  projectReference && value.ProjName == projectName)
                        .FirstOrDefault();

                    if (maxFrameworkVersionReference != null) //фон #f7e392 текст #8b6400
                    {
                        projectsTable.Cells[firstRowIndex + i, 5] = "Потенциальный\r\nконфликт версий";
                        projectsTable.Cells[firstRowIndex + i, 5].Interior.Color = 0x92e3f7;
                        projectsTable.Cells[firstRowIndex + i, 5].Font.Color = 0x00648b;

                        isPotentialVersionConflict = true;
                    }

                    ReferenceError referenceError = refsErrorList
                        .Where(value => value.ErrorRelevantProjectName == projectName && value.ReferenceName == projectReference && value.IsReferenceRequired == false)
                        .FirstOrDefault();

                    if (referenceError != null)
                    {
                        projectsTable.Cells[firstRowIndex + i, 5] = "Недопустимый";
                        projectsTable.Cells[firstRowIndex + i, 5].Interior.Color = 0xCEC7FF;
                        projectsTable.Cells[firstRowIndex + i, 5].Font.Color = 0x062CCE;
                    }

                    i++;
                }
            }

            projectsTable = SetUnionTableStyle(projectsTable, unionRangeSolutionWithTime, unionRangeTableTitle, i, heightIndex, widthIndex, true);

            if (isPotentialVersionConflict)
                projectsTable.Columns[5].ColumnWidth = 16;
        }

        private static Worksheet SetUnionColumnNames(Worksheet projectsTable, string solutionName, string currentDateTime)
        {
            projectsTable.Cells[2, 2] = "Solution: \"" + solutionName + "\"";
            projectsTable.Cells[3, 2] = currentDateTime;
            projectsTable.Cells[2, 2].Font.Bold = projectsTable.Cells[3, 2].Font.Bold = true;
            projectsTable.Cells[4, 2] = "№";

            return projectsTable;
        }

        private static Tuple<Range, Range> SetUnionTableHatRanges(Worksheet projectsTable, int widthIndex, int heightIndex)
        {
            Range unionRangeSolutionName = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[2, widthIndex]];
            Range unionRangeGenerateTime = projectsTable.Range[projectsTable.Cells[3, 2], projectsTable.Cells[3, widthIndex]];
            Range unionRangeSolutionWithTime = projectsTable.Range[unionRangeSolutionName, unionRangeGenerateTime];

            Range unionRangeTableTitle = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[heightIndex, widthIndex]];

            unionRangeSolutionName.Merge();
            unionRangeGenerateTime.Merge();

            unionRangeTableTitle.HorizontalAlignment = XlVAlign.xlVAlignCenter;

            return new Tuple<Range, Range>(unionRangeSolutionWithTime, unionRangeTableTitle);
        }

        private static Worksheet SetCurrentRowNum(Worksheet projectsTable, int i, int heightIndex)
        {
            if (i == 0)
                projectsTable.Cells[heightIndex + 1, 2] = "1";
            else
                projectsTable.Cells[heightIndex + 1 + i, 2].FormulaLocal = $"=B{i + heightIndex} + 1";

            return projectsTable;
        }

        private static Worksheet SetUnionTableStyle(Worksheet projectsTable, Range unionRangeSolutionWithTime, Range unionRangeTableTitle, int i, int extraColumnIndex, int widthIndex, bool isReferencesWorkbook)
        {
            Range unionRangeAllTable = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[i + extraColumnIndex, widthIndex]];
            Range unionRangeNumWithTitle = projectsTable.Range[projectsTable.Cells[4, 2], projectsTable.Cells[i + extraColumnIndex, 2]];

            unionRangeAllTable.Font.Name = "Calibri";
            unionRangeAllTable.Borders.Color = ColorTranslator.ToOle(Color.Black);
            if(isReferencesWorkbook)
                unionRangeAllTable.EntireColumn.AutoFit();
            unionRangeAllTable.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);

            unionRangeNumWithTitle.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            unionRangeNumWithTitle.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);

            unionRangeTableTitle.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);
            unionRangeSolutionWithTime.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);

            return projectsTable;
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
