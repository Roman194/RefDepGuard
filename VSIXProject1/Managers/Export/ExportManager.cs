using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject1.Data;
using VSIXProject1.Data.ConfigFile;
using VSIXProject1.Data.Reference;

namespace VSIXProject1
{
    public class ExportManager
    {
        private static string currentReportDirectory;

        public static string LoadReferencesDataToReport(
            Application excel, ConfigFilesData configFilesData, string reportType, 
            Dictionary<string, ProjectState> commitedProjectsState, RefDepGuardExportParameters refDepGuardExportParameters
            )
        {
            string  loadError = "";
            string currentDateTime = DateTimeManager.GetCurrentDateTimeInRightFormat();

            RefDepGuardErrors refDepGuardErrors = refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardErrors;
            RefDepGuardWarnings refDepGuardWarnings = refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardWarnings;

            try
            {
                currentReportDirectory = configFilesData.packageExtendedName + "\\reports\\"+ reportType +"\\" + currentDateTime;
                Directory.CreateDirectory(currentReportDirectory);

                switch (reportType)
                {
                    case "table_type":
                        XLSXSubManager.LoadReferencesDataToTableReport(excel, configFilesData, currentReportDirectory, currentDateTime, commitedProjectsState,
                        refDepGuardExportParameters); 
                        break;

                    case "graph_type":
                        HTMLSubManager.LoadReferencesDataToGraphicReport(configFilesData, currentReportDirectory, commitedProjectsState, refDepGuardExportParameters);
                        break;
                }
            }
            catch (Exception ex)
            {
                loadError = ex.Message;
            }

            return loadError;
        }

        public static bool OpenCurrentReportDirectory()
        {
            if(currentReportDirectory != null)
            {
                Process.Start(currentReportDirectory);
                return true;
            }

            return false;
        }
    }
}
