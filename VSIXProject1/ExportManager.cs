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

        public static bool LoadReferencesDataToReport(Application excel, string solutionName, string solutionAddress, string reportType, Dictionary<string, ProjectState> commitedProjectsState, RefDepGuardErrors refDepGuardErrors, RefDepGuardWarning refDepGuardWarning, RequiredExportParameters requiredExportParameters)
        {
            bool isLoadSuccessful = true;
            string currentDateTime = DateTimeManager.GetCurrentDateTimeInRightFormat();

            try
            {
                string currentReportDirectory = solutionAddress + "\\reports\\"+ reportType +"\\" + currentDateTime;
                Directory.CreateDirectory(currentReportDirectory);

                switch (reportType)
                {
                    case "table_type":
                        XLSXManager.LoadReferencesDataToTableReport(excel, solutionName, solutionAddress, currentReportDirectory, currentDateTime, commitedProjectsState,
                        refDepGuardErrors, requiredExportParameters); 
                        break;

                    case "graph_type":
                        HTMLManager.LoadReferencesDataToGraphicReport(solutionName, solutionAddress, currentReportDirectory, commitedProjectsState, refDepGuardErrors,
                            refDepGuardWarning, requiredExportParameters);
                        break;
                }
            }
            catch (Exception)
            {
                isLoadSuccessful = false;
            }

            return isLoadSuccessful;
        }
    }
}
