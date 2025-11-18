using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using VSIXProject1.Comparators;
using VSIXProject1.Data;
using VSIXProject1.Data.FrameworkVersion;
using VSIXProject1.Data.Reference;

namespace VSIXProject1
{
    public class XLSXManager
    {

        public static bool LoadReferencesDataToTableReport(Application excel, string solutionName, string solutionAddress, Dictionary<string, ProjectState> commitedProjectsState, RefDepGuardErrors refDepGuardErrors, RequiredExportParameters requiredExportParameters)
        {
            bool isLoadSuccessful = true;
            string currentDateTime = DateTimeManager.GetCurrentDateTimeInRightFormat();
            Workbook exportWorkbook = excel.Workbooks.Add(Type.Missing);

            LoadInfoToProjectsWorkbook(excel, solutionName, currentDateTime, commitedProjectsState, refDepGuardErrors, requiredExportParameters.MaxRequiredFrameworkVersion);
            LoadInfoToReferencesBook(excel, solutionName, currentDateTime, commitedProjectsState, refDepGuardErrors.RefsErrorList, requiredExportParameters.RequiredReferences);
            LoadInfoToRefRepGuardErrors(excel, solutionName, currentDateTime, refDepGuardErrors);

            try
            {
                string currentReportDirectory = solutionAddress + "\\reports\\table_type\\" + currentDateTime;
                Directory.CreateDirectory(currentReportDirectory);

                excel.Application.ActiveWorkbook.SaveAs(currentReportDirectory + "\\" + solutionName + "_references_report.xlsx", Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            }
            catch (COMException ex)
            {
                isLoadSuccessful = false;
            }

            exportWorkbook.Close(false, Type.Missing, Type.Missing);

            return isLoadSuccessful;
        }

        private static void LoadInfoToProjectsWorkbook(Application excel, string solutionName, string currentDateTime, Dictionary<string, ProjectState> commitedProjectsState, RefDepGuardErrors refDepGuardErrors, Dictionary<string, RequiredMaxFrVersion> requiredMaxFrVersions)
        {
            List<ReferenceError> refsErrorList = refDepGuardErrors.RefsErrorList;
            List<MaxFrameworkVersionDeviantValue> maxFrVersionDeviantValuesList = refDepGuardErrors.MaxFrameworkVersionDeviantValueList;
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
                string targetFramework = currentProject.Value.CurrentFrameworkVersion;
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

                if (maxFrVersionDeviantValuesList.Contains(new MaxFrameworkVersionDeviantValue(ErrorLevel.Global, currentProjectName), new MaxFrameworkVersionDeviantValueExportContainsComparer()))
                    projectsTable.Cells[6 + i, 5] = "?";
                else
                {
                    if (requiredMaxFrVersions.ContainsKey(currentProjectName)) {
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

                if(unacceptableRefsErrorsProjectString != "-") //Смена формата ячейки для того, чтобы при большом количестве рефов содержимое не выходило за пределы ячейки
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
            //Range unionRangeUnacceptableRefsErrors = projectsTable.Range[projectsTable.Cells[6, 9], projectsTable.Cells[projectsCount + 5, 9]];

            unionRangeAllTable.Borders.Color = ColorTranslator.ToOle(Color.Black);
            //unionRangeAllTable.Borders.Weight = XlBorderWeight.xlMedium;
            unionRangeAllTable.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);
            unionRangeSolutionNameAndGenerateTime.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);
            unionRangeTableTitle.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);
            unionRangeNumWithTitle.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);

            //Работа с центровкой числовых столбцов
            unionRangeNum.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            unionRangeNum.EntireColumn.AutoFit();

            unionRangeProjectName.EntireColumn.AutoFit();
            //unionRangeTargetFramework.EntireColumn.AutoFit();

            unionRangeMaxFrVersionWithTitle.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            //unionRangeMaxFrVersionWithTitle.EntireColumn.AutoFit();

            unionRangeReferencesCount.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            unionRangeReferencesCount.EntireColumn.AutoFit();

            unionRangeRequiredRefsErrorsCount.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            unionRangeRequiredRefsErrorsCount.EntireColumn.AutoFit();

            unionRangeUnacceptableRefsErrorsCount.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            unionRangeUnacceptableRefsErrorsCount.EntireColumn.AutoFit();

            //unionRangeUnacceptableRefsErrors.HorizontalAlignment = XlHAlign.xlHAlignFill;
        }

        private static void LoadInfoToReferencesBook(Application excel, string solutionName, string currentDateTime, Dictionary<string, ProjectState> commitedProjectsState, List<ReferenceError> refsErrorList, List<RequiredReference> requiredReferences)
        {
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
                foreach(var projectReference in currentProject.Value.CurrentReferences)
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
                    
                    if(referenceError!= null)
                    {
                        projectsTable.Cells[5 + i, 5] = "Недопустимый";
                        projectsTable.Cells[5 + i, 5].Interior.Color = 0xCEC7FF;
                        projectsTable.Cells[5 + i, 5].Font.Color = 0x062CCE;
                         
                    }

                    RequiredReference requiredReference = requiredReferences
                        .Where(value => value.ReferenceName == projectReference && (value.RelevantProject == projectName || value.RelevantProject == ""))
                        .FirstOrDefault();

                    if(requiredReference != null)
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

        private static void LoadInfoToRefRepGuardErrors(Application excel, string solutionName, string currentDateTime, RefDepGuardErrors refDepGuardErrors)
        {
            Worksheet projectsTable = (Worksheet)excel.Worksheets[3];
            projectsTable.Name = "RefDepGuard errors";

            projectsTable.Cells[2, 2] = "Solution: \"" + solutionName + "\"";
            projectsTable.Cells[3, 2] = currentDateTime;
            projectsTable.Cells[2, 2].Font.Bold = projectsTable.Cells[3, 2].Font.Bold = true;

            projectsTable.Cells[4, 2] = "№";
            projectsTable.Cells[4, 3] = "Проект";
            projectsTable.Cells[4, 4] = "Референс";
            projectsTable.Cells[4, 5] = "Тип ошибки";
            projectsTable.Cells[4, 6] = "Уровень ошибки";
            projectsTable.Cells[4, 7] = "Описание";
            projectsTable.Cells[4, 8] = "Необходимое действие";
            projectsTable.Cells[4, 9] = "Файл действия";

            Range unionRangeSolutionName = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[2, 9]];
            Range unionRangeGenerateTime = projectsTable.Range[projectsTable.Cells[3, 2], projectsTable.Cells[3, 9]];
            Range unionRangeSolutionWithTime = projectsTable.Range[unionRangeSolutionName, unionRangeGenerateTime];

            Range unionRangeTableTitle = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[4, 9]];

            unionRangeSolutionName.Merge();
            unionRangeGenerateTime.Merge();

            unionRangeTableTitle.HorizontalAlignment = XlVAlign.xlVAlignCenter;

            int i = 0;

            foreach(MaxFrameworkVersionDeviantValue maxFrameworkVersionDeviantValue in refDepGuardErrors.MaxFrameworkVersionDeviantValueList)
            {
                if (i == 0)
                    projectsTable.Cells[5, 2] = "1";
                else
                    projectsTable.Cells[5 + i, 2].FormulaLocal = $"=B{i + 4} + 1";

                string errorRelevantProjectName = maxFrameworkVersionDeviantValue.ErrorRelevantProjectName;
                if (errorRelevantProjectName == "")
                    errorRelevantProjectName = "-";

                projectsTable.Cells[5 + i, 3] = errorRelevantProjectName;
                projectsTable.Cells[5 + i, 4] = "-";

                projectsTable.Cells[5 + i, 5] = "framework_max_version deviant value";

                string currentErrorLevel = "Global";
                if (maxFrameworkVersionDeviantValue.ErrorLevel != ErrorLevel.Global)
                {
                    if (errorRelevantProjectName != "-")
                        currentErrorLevel = "Project";
                    else
                        currentErrorLevel = "Solution";
                }

                projectsTable.Cells[5 + i, 6] = currentErrorLevel;

                projectsTable.Cells[5 + i, 7] = "параметр 'framework_max_version' содержит некорректную запись\r\nсвоего значения";
                projectsTable.Cells[5 + i, 8] = "Проверьте его на предмет отсутствия \r\nсинтаксических ошибок и соответствия \r\nшаблону файла конфигурации";

                if (currentErrorLevel == "Global")
                    projectsTable.Cells[5 + i, 9] = "global_config_guard.rdg";
                else
                    projectsTable.Cells[5 + i, 9] = solutionName + "_config_guard.rdg";

                i++;
            }

            foreach(FrameworkVersionComparabilityError frameworkVersionComparabilityError in refDepGuardErrors.FrameworkVersionComparabilityErrorList)
            {
                if (i == 0)
                    projectsTable.Cells[5, 2] = "1";
                else
                    projectsTable.Cells[5 + i, 2].FormulaLocal = $"=B{i + 4} + 1";

                projectsTable.Cells[5 + i, 3] = frameworkVersionComparabilityError.ErrorRelevantProjectName;
                projectsTable.Cells[5 + i, 4] = "-";

                projectsTable.Cells[5 + i, 5] = "Framework comparability version";

                string currentErrorLevel = "Global";
                switch(frameworkVersionComparabilityError.ErrorLevel)
                {
                    case ErrorLevel.Solution: currentErrorLevel = "Solution"; break;
                    case ErrorLevel.Project: currentErrorLevel = "Project"; break;
                }

                projectsTable.Cells[5 + i, 6] = currentErrorLevel;

                projectsTable.Cells[5 + i, 7] = "параметр 'TargetFrameworkVersion'\r\nимеет версию'" + frameworkVersionComparabilityError.TargetFrameworkVersion + "', в то время как\r\nмаксимально допустимой для него\r\nверсией является '" + frameworkVersionComparabilityError.MaxFrameworkVersion  +"'";
                projectsTable.Cells[5 + i, 8] = "Измените версию проекта или модифицируйте конфигурацию Config-\r\nфайла";

                if (currentErrorLevel == "Global")
                    projectsTable.Cells[5 + i, 9] = "global_config_guard.rdg";
                else
                    projectsTable.Cells[5 + i, 9] = solutionName + "_config_guard.rdg";

                i++;
            }

            foreach (ConfigFilePropertyNullError currentNullError in refDepGuardErrors.ConfigPropertyNullErrorList)
            {
                if (i == 0)
                    projectsTable.Cells[5, 2] = "1";
                else
                    projectsTable.Cells[5 + i, 2].FormulaLocal = $"=B{i + 4} + 1";

                string errorRelevantProjectName = currentNullError.ErrorRelevantProjectName;
                if (errorRelevantProjectName == "")
                    errorRelevantProjectName = "-";

                projectsTable.Cells[5 + i, 3] = errorRelevantProjectName;
                projectsTable.Cells[5 + i, 4] = "-";

                projectsTable.Cells[5 + i, 5] = "Null property";

                string currentErrorLevel = "Global";
                if (!currentNullError.IsGlobal)
                {
                    if (errorRelevantProjectName != "-")
                        currentErrorLevel = "Project";
                    else
                        currentErrorLevel = "Solution";
                }

                projectsTable.Cells[5 + i, 6] = currentErrorLevel;

                projectsTable.Cells[5 + i, 7] = "Config-файл не содержит свойство \r\n'" + currentNullError.PropertyName + "'";
                projectsTable.Cells[5 + i, 8] = "Проверьте его на предмет отсутствия \r\nсинтаксических ошибок и соответствия \r\nшаблону файла конфигурации";
                
                if(currentErrorLevel == "Global")
                    projectsTable.Cells[5 + i, 9] = "global_config_guard.rdg";
                else
                    projectsTable.Cells[5 + i, 9] = solutionName + "_config_guard.rdg";

                i++;

            }

            foreach(ReferenceMatchError currentMatchError in refDepGuardErrors.RefsMatchErrorList)
            {
                if (i == 0)
                    projectsTable.Cells[5, 2] = "1";
                else
                    projectsTable.Cells[5 + i, 2].FormulaLocal = $"=B{i + 4} + 1";

                string errorRelevantProjectName = currentMatchError.ProjectName;
                if (errorRelevantProjectName == "")
                    errorRelevantProjectName = "-";

                projectsTable.Cells[5 + i, 3] = errorRelevantProjectName;
                projectsTable.Cells[5 + i, 4] = currentMatchError.ReferenceName;

                projectsTable.Cells[5 + i, 5] = "Match";

                projectsTable.Cells[5 + i, 6] = currentMatchError.ReferenceLevelValue.ToString();

                if (!currentMatchError.IsProjNameMatchError)
                    projectsTable.Cells[5 + i, 7] = "Референс одновременно заявлен как \r\nобязательный и недопустимый";
                else
                    projectsTable.Cells[5 + i, 7] = "Референс совпадает с именем проекта";
                    
                projectsTable.Cells[5 + i, 8] = "Устраните противоречие в правиле";

                if (currentMatchError.ReferenceLevelValue == ErrorLevel.Global)
                    projectsTable.Cells[5 + i, 9] = "global_config_guard.rdg";
                else
                    projectsTable.Cells[5 + i, 9] = solutionName + "_config_guard.rdg";

                i++;
            }

            foreach (ReferenceError currentError in refDepGuardErrors.RefsErrorList)
            {
                if (i == 0)
                    projectsTable.Cells[5, 2] = "1";
                else
                    projectsTable.Cells[5 + i, 2].FormulaLocal = $"=B{i + 4} + 1";

                projectsTable.Cells[5 + i, 3] = currentError.ErrorRelevantProjectName;
                projectsTable.Cells[5 + i, 4] = currentError.ReferenceName;

                projectsTable.Cells[5 + i, 5] = "Reference";

                projectsTable.Cells[5 + i, 6] = currentError.CurrentReferenceLevel.ToString();

                if (!currentError.IsReferenceRequired)
                {
                    projectsTable.Cells[5 + i, 7] = "Присутствует недопустимый референс";
                    projectsTable.Cells[5 + i, 8] = "Удалить через обозреватель решений";
                }
                else
                {
                    projectsTable.Cells[5 + i, 7] = "Отсутствует обязательный референс";
                    projectsTable.Cells[5 + i, 8] = "Добавить через обозреватель решений";
                }

                projectsTable.Cells[5 + i, 9] = currentError.ErrorRelevantProjectName + ".csproj";

                i++;
            }

            Range unionRangeAllTable = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[i + 4, 9]];
            Range unionRangeNumWithTitle = projectsTable.Range[projectsTable.Cells[4, 2], projectsTable.Cells[i + 4, 2]];

            unionRangeAllTable.Borders.Color = ColorTranslator.ToOle(Color.Black);
            unionRangeAllTable.EntireColumn.AutoFit();
            unionRangeAllTable.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);

            unionRangeNumWithTitle.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            unionRangeNumWithTitle.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);

            unionRangeTableTitle.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);
            unionRangeSolutionWithTime.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);
        }
    }
}
