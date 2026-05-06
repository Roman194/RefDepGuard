using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using RefDepGuard.Managers.Export.SubManagers;
using RefDepGuard.Applied.Models.ConfigFile;
using RefDepGuard.Applied.Models.Project;
using RefDepGuard.Applied.Models.RefDepGuard;

namespace RefDepGuard
{
    /// <summary>
    /// This class is responsible for managing the table exports based on the checking rules data of the solution. 
    /// It contains methods for loading data into Excel workbooks and saving them as reports. 
    /// </summary>
    public class XLSXSubManager
    {
        /// <summary>
        /// The main method. Creates a new Excel workbook, populates it with data about projects and references, as well as any errors or warnings found during the 
        /// checks, and then saves the workbook to a specified directory. 
        /// This class serves as a central point for handling the export of reference data in a tabular format.
        /// </summary>
        /// <param name="excel">Application (excel.interop) interface value</param>
        /// <param name="configFilesData">ConfigFilesData value</param>
        /// <param name="currentReportDirectory">current report directory string</param>
        /// <param name="currentDateTime">current DateTime of report generation in string format</param>
        /// <param name="commitedProjectsState">committed projects state dict</param>
        /// <param name="refDepGuardExportParameters">RefDepGuardExportParameters values</param>
        public static void LoadReferencesDataToTableReport(Application excel, ConfigFilesData configFilesData, string currentReportDirectory, string currentDateTime, 
            Dictionary<string, ProjectState> commitedProjectsState, RefDepGuardExportParameters refDepGuardExportParameters)
        {
            Workbook exportWorkbook = excel.Workbooks.Add(Type.Missing);

            //On some devices Excel creates 1 sheet by default, on others - 3 sheets.
            //We need to have 4 sheets to load all the data, so if there are less than 4 sheets, we need to add the missing sheets manually
            while (exportWorkbook.Worksheets.Count < 4)
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
