using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using VSIXProject1.Data;
using VSIXProject1.Data.ConfigFile;
using VSIXProject1.Managers.Export.SubManagers;

namespace VSIXProject1
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
            LoadInfoToProjectAndReferenceWorkbooksHelper.LoadInfoToProjectsWorkbook(excel, configFilesData.solutionName, currentDateTime, commitedProjectsState, 
                refDepGuardExportParameters);
            LoadInfoToProjectAndReferenceWorkbooksHelper.LoadInfoToReferencesBook(excel, configFilesData.solutionName, currentDateTime, commitedProjectsState, 
                refDepGuardExportParameters);

            LoadInfoToProblemWorkbooksHelper.LoadInfoToRefRepGuardErrors(excel, configFilesData.solutionName, currentDateTime, 
                refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardErrors);
            LoadInfoToProblemWorkbooksHelper.LoadInfoToRefDepGuardWarnings(excel, configFilesData.solutionName, currentDateTime,
                refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardWarnings);

            excel.Application.ActiveWorkbook.SaveAs(currentReportDirectory + "\\" + configFilesData.solutionName + "_references_report.xlsx", Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing, 
                Type.Missing, Type.Missing);

            exportWorkbook.Close(false, Type.Missing, Type.Missing);
        }
    }
}
