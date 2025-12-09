using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject1.Data;
using VSIXProject1.Data.Reference;

namespace VSIXProject1
{
    public class ExportManager
    {
        public static string LoadReferencesDataToReport(
            Application excel, string solutionName, string solutionAddress, string reportType, 
            Dictionary<string, ProjectState> commitedProjectsState, RefDepGuardExportParameters refDepGuardExportParameters
            )
        {
            string  loadError = "";
            string currentDateTime = DateTimeManager.GetCurrentDateTimeInRightFormat();

            RefDepGuardErrors refDepGuardErrors = refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardErrors;
            RefDepGuardWarnings refDepGuardWarnings = refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardWarnings;

            try
            {
                string currentReportDirectory = solutionAddress + "\\reports\\"+ reportType +"\\" + currentDateTime;
                Directory.CreateDirectory(currentReportDirectory);

                switch (reportType)
                {
                    case "table_type":
                        XLSXSubManager.LoadReferencesDataToTableReport(excel, solutionName, solutionAddress, currentReportDirectory, currentDateTime, commitedProjectsState,
                        refDepGuardErrors, refDepGuardExportParameters.RequiredParametersData); 
                        break;

                    case "graph_type":
                        HTMLSubManager.LoadReferencesDataToGraphicReport(solutionName, solutionAddress, currentReportDirectory, commitedProjectsState, refDepGuardErrors,
                            refDepGuardWarnings, refDepGuardExportParameters.RequiredParametersData);
                        break;
                }
            }
            catch (Exception ex)
            {
                loadError = ex.Message;
            }

            return loadError;
        }
    }
}
