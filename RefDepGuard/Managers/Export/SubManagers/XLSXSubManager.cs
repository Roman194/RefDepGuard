using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using RefDepGuard.Data;
using RefDepGuard.Data.ConfigFile;
using RefDepGuard.Managers.Export.SubManagers;

namespace RefDepGuard
{
    public class XLSXSubManager
    {
        public static void LoadReferencesDataToTableReport(Application excel, ConfigFilesData configFilesData, string currentReportDirectory, string currentDateTime, 
            Dictionary<string, ProjectState> commitedProjectsState, RefDepGuardExportParameters refDepGuardExportParameters)
        {
            Workbook exportWorkbook = excel.Workbooks.Add(Type.Missing);

            while(exportWorkbook.Worksheets.Count < 4) //На некоторых ПК эксель по дефолту создаёт 1 лист, а не 4. Тогда нужно создать нехватающие листы вручную
            {
                exportWorkbook.Worksheets.Add();
            }
            LoadInfoToProjectAndReferenceWorkbooksHelper.LoadInfoToProjectsWorkbook(excel, configFilesData.SolutionName, currentDateTime, commitedProjectsState, 
                refDepGuardExportParameters);
            LoadInfoToProjectAndReferenceWorkbooksHelper.LoadInfoToReferencesBook(excel, configFilesData.SolutionName, currentDateTime, commitedProjectsState, 
                refDepGuardExportParameters);

            LoadInfoToProblemWorkbooksHelper.LoadInfoToRefRepGuardErrors(excel, configFilesData.SolutionName, currentDateTime, 
                refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardErrors);
            LoadInfoToProblemWorkbooksHelper.LoadInfoToRefDepGuardWarnings(excel, configFilesData.SolutionName, currentDateTime,
                refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardWarnings);

            excel.Application.ActiveWorkbook.SaveAs(currentReportDirectory + "\\" + configFilesData.SolutionName + "_references_report.xlsx", Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing, 
                Type.Missing, Type.Missing);

            exportWorkbook.Close(false, Type.Missing, Type.Missing);
        }
    }
}
