using Microsoft.Office.Interop.Excel;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace VSIXProject1
{
    public class XLSXManager
    {

        public static void LoadReferencesDataToCurrentReport(Excel.Application excel, string solutionName, string solutionAddress, Dictionary<string, List<string>> commitedProjectsState, List<ReferenceError> refsErrorList)
        {
            Workbook exportWorkbook = excel.Workbooks.Add(Type.Missing);

            Worksheet projectsTable = (Worksheet) excel.Worksheets[1];
            projectsTable.Name = "Выборка по проектам";

            projectsTable.Cells[4, 2] = "№";

            projectsTable.Cells[4, 3] = "Проект";

            projectsTable.Cells[4, 4] = "Всего референсов";

            projectsTable.Cells[4, 6] = "Не обнаружено обязательных референсов";

            projectsTable.Cells[4, 8] = "Обнаружено недопустимых референсов";

            Excel.Range range = projectsTable.Cells[4, 3];
            Excel.Range range2 = projectsTable.Cells[5, 3]; //Как объединять ячейки?
            Excel.Range union_range = projectsTable.Range[range, range2];

            excel.Application.ActiveWorkbook.SaveAs(solutionAddress + "\\report.xlsx", Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);




        }
    }
}
