using Microsoft.Office.Interop.Excel;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Excel = Microsoft.Office.Interop.Excel;

namespace VSIXProject1
{
    public class XLSXManager
    {

        public static void LoadReferencesDataToCurrentReport(Application excel, string solutionName, string solutionAddress, Dictionary<string, List<string>> commitedProjectsState, List<ReferenceError> refsErrorList)
        {
            Workbook exportWorkbook = excel.Workbooks.Add(Type.Missing);

            Worksheet projectsTable = (Worksheet) excel.Worksheets[1];
            projectsTable.Name = "Выборка по проектам";

            projectsTable.Cells[2, 2] = "Solution: \""+ solutionName  +"\"";
            projectsTable.Cells[3, 2] = "24.09.2025-13.38.33";
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
            Range unionRangeNumTitle = projectsTable.Range[projectsTable.Cells[4, 2], projectsTable.Cells[5, 2]];
            Range unionRangeProjectNameTitle = projectsTable.Range[projectsTable.Cells[4, 3], projectsTable.Cells[5, 3]];
            Range unionRangeReferencesCountTitle = projectsTable.Range[projectsTable.Cells[4, 4], projectsTable.Cells[4, 5]];
            Range unionRangeRequiredRefsErrors = projectsTable.Range[projectsTable.Cells[4, 6], projectsTable.Cells[4, 7]];
            Range unionRangeUnacceptableRefsErrorsTitle = projectsTable.Range[projectsTable.Cells[4, 8], projectsTable.Cells[4, 9]];
            //Range unionReferenecesTitles = projectsTable.Range[projectsTable.Cells[4, 4], projectsTable.Cells[4, 9]];

            Range unionRangeTableTitle = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[5, 9]];

            unionRangeSolutionName.Merge();
            unionRangeGenerateTime.Merge();

            Range unionRangeSolutionNameAndGenerateTime = projectsTable.Range[unionRangeSolutionName, unionRangeGenerateTime];

            unionRangeNumTitle.Merge();
            unionRangeProjectNameTitle.Merge();
            unionRangeReferencesCountTitle.Merge();
            //unionRangeReferencesCountTitle.EntireColumn.AutoFit();
            unionRangeRequiredRefsErrors.Merge();
            //unionRangeRequiredRefsErrors.EntireColumn.AutoFit();
            unionRangeUnacceptableRefsErrorsTitle.Merge();
            //unionRangeUnacceptableRefsErrorsTitle.EntireColumn.AutoFit();

            //unionReferenecesTitles.EntireColumn.AutoFit();

            unionRangeTableTitle.HorizontalAlignment = XlVAlign.xlVAlignCenter;

            int i = 0;
            foreach(var currentProject in commitedProjectsState)
            {
                string currentPorjectName = currentProject.Key;
                List<string> currentPorjectRefs = currentProject.Value;
                string currentProjectRefsOutputString = "";
                int currentProjectRefsCount = currentPorjectRefs.Count;
                int lastCurrentProjectRefsElementIndex = currentProjectRefsCount - 1;
                List<string> requiredRefsErrors = refsErrorList
                    .Where(value => value.IsReferenceRequired && value.ErrorRelevantProjectName == currentPorjectName)
                    .Select(value => value.ReferenceName)
                    .ToList();

                List<string> unacceptableRefsErrors = refsErrorList
                    .Where(value => !value.IsReferenceRequired && value.ErrorRelevantProjectName == currentPorjectName)
                    .Select(value => value.ReferenceName)
                    .ToList();

                if (i == 0)
                    projectsTable.Cells[6, 2] = "1";
                else
                    projectsTable.Cells[6 + i, 2].FormulaLocal = $"=B{i + 5} + 1";

                projectsTable.Cells[6 + i, 3] = currentPorjectName;
                projectsTable.Cells[6 + i, 4] = currentProjectRefsCount;

                int j = 0;
                foreach (string currentRef in currentPorjectRefs)
                {
                    if (j < lastCurrentProjectRefsElementIndex)
                        currentProjectRefsOutputString += currentRef + ", ";
                    else
                        currentProjectRefsOutputString += currentRef;

                    j++;
                }

                if (currentProjectRefsOutputString == "")
                    currentProjectRefsOutputString = "-";

                projectsTable.Cells[6 + i, 5] = currentProjectRefsOutputString;

                int requiredRefsErrorsCount = requiredRefsErrors.Count();
                int unacceptableRefsErrorsCount = unacceptableRefsErrors.Count();
                string currentProjectRequiredRefsErrorsOutputString = "";
                string currentProjectUnacceptableRefsErrorsOutputString = "";

                projectsTable.Cells[6 + i, 6] = requiredRefsErrorsCount;
                
                if(requiredRefsErrorsCount > 0)
                {
                    projectsTable.Cells[6 + i, 6].Interior.Color = projectsTable.Cells[6 + i, 7].Interior.Color = 0xCEC7FF;
                    projectsTable.Cells[6 + i, 6].Font.Color = projectsTable.Cells[6 + i, 7].Font.Color = 0x062CCE;
                }
                
                j = 0;
                foreach (string currentRefError in requiredRefsErrors)
                {
                    if (j < requiredRefsErrorsCount - 1)
                        currentProjectRequiredRefsErrorsOutputString += currentRefError + ", ";
                    else
                        currentProjectRequiredRefsErrorsOutputString += currentRefError;

                    j++;
                }

                if (currentProjectRequiredRefsErrorsOutputString == "")
                    currentProjectRequiredRefsErrorsOutputString = "-";

                projectsTable.Cells[6 + i, 7] = currentProjectRequiredRefsErrorsOutputString;

                projectsTable.Cells[6 + i, 8] = unacceptableRefsErrorsCount;
                if (unacceptableRefsErrorsCount > 0)
                {
                    projectsTable.Cells[6 + i, 8].Interior.Color = projectsTable.Cells[6 + i, 9].Interior.Color = 0xCEC7FF;
                    projectsTable.Cells[6 + i, 8].Font.Color = projectsTable.Cells[6 + i, 9].Font.Color = 0x062CCE;
                }
                    
                j = 0;
                foreach (string currentRefError in unacceptableRefsErrors)
                {
                    if (j < unacceptableRefsErrorsCount - 1)
                        currentProjectUnacceptableRefsErrorsOutputString += currentRefError + ", ";
                    else
                        currentProjectUnacceptableRefsErrorsOutputString += currentRefError;

                    j++;
                }

                if (currentProjectUnacceptableRefsErrorsOutputString == "")
                    currentProjectUnacceptableRefsErrorsOutputString = "-";

                projectsTable.Cells[6 + i, 9] = currentProjectUnacceptableRefsErrorsOutputString;

                i++;
            }

            int projectsCount = commitedProjectsState.Count;

            Range unionRangeAllTable = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[projectsCount + 5, 9]];
            Range unionRangeNum = projectsTable.Range[projectsTable.Cells[6, 2], projectsTable.Cells[projectsCount + 5, 2]];
            Range unionRangeProjectName = projectsTable.Range[projectsTable.Cells[6, 3], projectsTable.Cells[projectsCount + 5, 3]];
            Range unionRangeReferencesCount = projectsTable.Range[projectsTable.Cells[6, 4], projectsTable.Cells[projectsCount + 5, 4]];
            Range unionRangeRequiredRefsErrorsCount = projectsTable.Range[projectsTable.Cells[6, 6], projectsTable.Cells[projectsCount + 5, 6]];
            Range unionRangeUnacceptableRefsErrorsCount = projectsTable.Range[projectsTable.Cells[6, 8], projectsTable.Cells[projectsCount + 5, 8]];

            unionRangeAllTable.Borders.Color = ColorTranslator.ToOle(Color.Black);
            //unionRangeAllTable.Borders.Weight = XlBorderWeight.xlMedium;
            unionRangeAllTable.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);
            unionRangeSolutionNameAndGenerateTime.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);
            unionRangeTableTitle.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);

            unionRangeNum.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            unionRangeNum.EntireColumn.AutoFit();

            Range unionRangeNumWithTitle = projectsTable.Range[projectsTable.Cells[4, 2], unionRangeNum];
            unionRangeNumWithTitle.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);

            unionRangeProjectName.EntireColumn.AutoFit();

            unionRangeReferencesCount.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            unionRangeReferencesCount.EntireColumn.AutoFit();

            unionRangeRequiredRefsErrorsCount.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            unionRangeRequiredRefsErrorsCount.EntireColumn.AutoFit();

            unionRangeUnacceptableRefsErrorsCount.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            unionRangeUnacceptableRefsErrorsCount.EntireColumn.AutoFit();

            //Обработать ситуации когда нет доступа к файлу (занят другим процессом)
            
            excel.Application.ActiveWorkbook.SaveAs(solutionAddress + "\\" + solutionName +"_references_report_1.xlsx", Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

            exportWorkbook.Close(false, Type.Missing, Type.Missing);
        }
    }
}
