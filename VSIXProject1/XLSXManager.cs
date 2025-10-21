using Microsoft.Office.Interop.Excel;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Excel = Microsoft.Office.Interop.Excel;

namespace VSIXProject1
{
    public class XLSXManager
    {

        public static bool LoadReferencesDataToCurrentReport(Application excel, string solutionName, string solutionAddress, Dictionary<string, List<string>> commitedProjectsState, List<ReferenceError> refsErrorList)
        {
            bool isLoadSuccessful = true;
            string currentDateTime = GetCurrentDateTimeInRightFormat();
            Workbook exportWorkbook = excel.Workbooks.Add(Type.Missing);

            loadInfoToProjectsWorkbook(excel, solutionName, currentDateTime, commitedProjectsState, refsErrorList);
            loadInfoToReferencesBook(excel, solutionName, currentDateTime, commitedProjectsState, refsErrorList);

            try
            {
                excel.Application.ActiveWorkbook.SaveAs(solutionAddress + "\\" + solutionName + "_references_report_1.xlsx", Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            }
            catch (COMException)
            {
                isLoadSuccessful = false;
            }

            exportWorkbook.Close(false, Type.Missing, Type.Missing);

            return isLoadSuccessful;
        }

        private static void loadInfoToProjectsWorkbook(Application excel, string solutionName, string currentDateTime, Dictionary<string, List<string>> commitedProjectsState, List<ReferenceError> refsErrorList)
        {
            Worksheet projectsTable = (Worksheet)excel.Worksheets[1];
            projectsTable.Name = "Выборка по проектам";

            //Загрузка и стилизация шапки таблицы
            projectsTable.Cells[2, 2] = "Solution: \"" + solutionName + "\"";
            projectsTable.Cells[3, 2] = currentDateTime;
            projectsTable.Cells[2, 2].Font.Bold = projectsTable.Cells[3, 2].Font.Bold = true;

            projectsTable.Cells[4, 2] = "№";

            projectsTable.Cells[4, 3] = "Проект";

            projectsTable.Cells[4, 4] = "Всего референсов";

            projectsTable.Cells[5, 4] = projectsTable.Cells[5, 6] = projectsTable.Cells[5, 8] = "Кол-во";
            projectsTable.Cells[5, 5] = projectsTable.Cells[5, 7] = projectsTable.Cells[5, 9] = "Названия";

            projectsTable.Cells[4, 6] = "Не обнаружено обязательных референсов";

            projectsTable.Cells[4, 8] = "Обнаружено недопустимых референсов";

            projectsTable.Columns[5].ColumnWidth = 35;
            projectsTable.Columns[7].ColumnWidth = 35;
            projectsTable.Columns[9].ColumnWidth = 35;

            Range unionRangeSolutionName = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[2, 9]];
            Range unionRangeGenerateTime = projectsTable.Range[projectsTable.Cells[3, 2], projectsTable.Cells[3, 9]];
            Range unionRangeSolutionNameAndGenerateTime = projectsTable.Range[unionRangeSolutionName, unionRangeGenerateTime];
            Range unionRangeNumTitle = projectsTable.Range[projectsTable.Cells[4, 2], projectsTable.Cells[5, 2]];
            Range unionRangeProjectNameTitle = projectsTable.Range[projectsTable.Cells[4, 3], projectsTable.Cells[5, 3]];
            Range unionRangeReferencesCountTitle = projectsTable.Range[projectsTable.Cells[4, 4], projectsTable.Cells[4, 5]];
            Range unionRangeRequiredRefsErrors = projectsTable.Range[projectsTable.Cells[4, 6], projectsTable.Cells[4, 7]];
            Range unionRangeUnacceptableRefsErrorsTitle = projectsTable.Range[projectsTable.Cells[4, 8], projectsTable.Cells[4, 9]];

            Range unionRangeTableTitle = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[5, 9]];

            unionRangeSolutionName.Merge();
            unionRangeGenerateTime.Merge();
            unionRangeNumTitle.Merge();
            unionRangeProjectNameTitle.Merge();
            unionRangeReferencesCountTitle.Merge();
            unionRangeRequiredRefsErrors.Merge();
            unionRangeUnacceptableRefsErrorsTitle.Merge();

            unionRangeTableTitle.HorizontalAlignment = XlVAlign.xlVAlignCenter;

            //Загрузка данных в саму таблицу
            int i = 0;
            foreach (var currentProject in commitedProjectsState)
            {
                string currentPorjectName = currentProject.Key;
                List<string> currentPorjectRefs = currentProject.Value;
                List<string> requiredRefsErrors = refsErrorList
                    .Where(value => value.IsReferenceRequired && value.ErrorRelevantProjectName == currentPorjectName)
                    .Select(value => value.ReferenceName)
                    .ToList();

                List<string> unacceptableRefsErrors = refsErrorList
                    .Where(value => !value.IsReferenceRequired && value.ErrorRelevantProjectName == currentPorjectName)
                    .Select(value => value.ReferenceName)
                    .ToList();
                int requiredRefsErrorsCount = requiredRefsErrors.Count();
                int unacceptableRefsErrorsCount = unacceptableRefsErrors.Count();

                if (i == 0)
                    projectsTable.Cells[6, 2] = "1";
                else
                    projectsTable.Cells[6 + i, 2].FormulaLocal = $"=B{i + 5} + 1";

                projectsTable.Cells[6 + i, 3] = currentPorjectName;

                projectsTable.Cells[6 + i, 4] = currentPorjectRefs.Count;
                projectsTable.Cells[6 + i, 5] = GetProjectsString(currentPorjectRefs);

                projectsTable.Cells[6 + i, 6] = requiredRefsErrorsCount;

                if (requiredRefsErrorsCount > 0)
                {
                    projectsTable.Cells[6 + i, 6].Interior.Color = projectsTable.Cells[6 + i, 7].Interior.Color = 0xCEC7FF;//На самом деле это #FFC7CE, просто Interop зачем-то "разворачивает" это значение
                    projectsTable.Cells[6 + i, 6].Font.Color = projectsTable.Cells[6 + i, 7].Font.Color = 0x062CCE;
                }

                projectsTable.Cells[6 + i, 7] = GetProjectsString(requiredRefsErrors);

                projectsTable.Cells[6 + i, 8] = unacceptableRefsErrorsCount;
                if (unacceptableRefsErrorsCount > 0)
                {
                    projectsTable.Cells[6 + i, 8].Interior.Color = projectsTable.Cells[6 + i, 9].Interior.Color = 0xCEC7FF;
                    projectsTable.Cells[6 + i, 8].Font.Color = projectsTable.Cells[6 + i, 9].Font.Color = 0x062CCE;
                }

                var unacceptableRefsErrorsProjectString = GetProjectsString(unacceptableRefsErrors);
                projectsTable.Cells[6 + i, 9] = unacceptableRefsErrorsProjectString;

                if(unacceptableRefsErrorsProjectString != "-") //Смена формата ячейки для того, чтобы при большом количестве рефов содержимое не выходило за пределы ячейки
                {
                    Range currentCellRange = projectsTable.Range[projectsTable.Cells[6 + i, 9], projectsTable.Cells[6 + i, 9]];
                    currentCellRange.HorizontalAlignment = XlHAlign.xlHAlignFill;
                }

                i++;
            }

            //Работа с границами
            int projectsCount = commitedProjectsState.Count;

            Range unionRangeAllTable = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[projectsCount + 5, 9]];
            Range unionRangeNum = projectsTable.Range[projectsTable.Cells[6, 2], projectsTable.Cells[projectsCount + 5, 2]];
            Range unionRangeNumWithTitle = projectsTable.Range[projectsTable.Cells[4, 2], unionRangeNum];
            Range unionRangeProjectName = projectsTable.Range[projectsTable.Cells[6, 3], projectsTable.Cells[projectsCount + 5, 3]];
            Range unionRangeReferencesCount = projectsTable.Range[projectsTable.Cells[6, 4], projectsTable.Cells[projectsCount + 5, 4]];
            Range unionRangeRequiredRefsErrorsCount = projectsTable.Range[projectsTable.Cells[6, 6], projectsTable.Cells[projectsCount + 5, 6]];
            Range unionRangeUnacceptableRefsErrorsCount = projectsTable.Range[projectsTable.Cells[6, 8], projectsTable.Cells[projectsCount + 5, 8]];
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

            unionRangeReferencesCount.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            unionRangeReferencesCount.EntireColumn.AutoFit();

            unionRangeRequiredRefsErrorsCount.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            unionRangeRequiredRefsErrorsCount.EntireColumn.AutoFit();

            unionRangeUnacceptableRefsErrorsCount.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            unionRangeUnacceptableRefsErrorsCount.EntireColumn.AutoFit();

            //unionRangeUnacceptableRefsErrors.HorizontalAlignment = XlHAlign.xlHAlignFill;
        }

        private static void loadInfoToReferencesBook(Application excel, string solutionName, string currentDateTime, Dictionary<string, List<string>> commitedProjectsState, List<ReferenceError> refsErrorList)
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
                foreach(var projectReference in currentProject.Value)
                {
                    if (i == 0)
                        projectsTable.Cells[5, 2] = "1";
                    else
                        projectsTable.Cells[5 + i, 2].FormulaLocal = $"=B{i + 4} + 1";

                    projectsTable.Cells[5 + i, 3] = projectReference;
                    projectsTable.Cells[5 + i, 4] = projectName;

                    ReferenceError referenceError = refsErrorList
                        .Where(value => value.ErrorRelevantProjectName == projectName && value.ReferenceName == projectReference)
                        .FirstOrDefault(); //Должно найтись не более одного такого значения
                    
                    if(referenceError != null)
                    {
                        if (referenceError.IsReferenceRequired)
                        {
                            projectsTable.Cells[5 + i, 5] = "Обязательный";
                            projectsTable.Cells[5 + i, 5].Interior.Color = 0xCEEFC6;
                            projectsTable.Cells[5 + i, 5].Font.Color = 0x006100;
                        }
                        else
                        {
                            projectsTable.Cells[5 + i, 5] = "Недопустимый";
                            projectsTable.Cells[5 + i, 5].Interior.Color = 0xCEC7FF;
                            projectsTable.Cells[5 + i, 5].Font.Color = 0x062CCE;
                        }  
                    }else
                        projectsTable.Cells[5 + i, 5] = "-";

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

        private static string GetCurrentDateTimeInRightFormat()
        {
            DateTime currentDateTime = DateTime.Now;
            return currentDateTime.Day + "." + currentDateTime.Month + "." + currentDateTime.Year + "-" + currentDateTime.Hour + "." + currentDateTime.Minute + "." 
                + currentDateTime.Second;
        }
    }
}
