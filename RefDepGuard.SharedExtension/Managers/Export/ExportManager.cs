using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using RefDepGuard.Applied.Models.Project;
using RefDepGuard.Applied.Models.ConfigFile;
using RefDepGuard.Applied.Models.RefDepGuard;

namespace RefDepGuard
{
    /// <summary>
    /// This class is responsible for managing the export of reports based on the checking rules data of the solution.
    /// </summary>
    public class ExportManager
    {
        private static string currentReportDirectory;

        /// <summary>
        /// Manages of generating the export reports based on the checking rules data of the solution.
        /// </summary>
        /// <param name="excel">Application (excel.interop) interface value</param>
        /// <param name="configFilesData">ConfigFilesData value</param>
        /// <param name="reportType">graph or table report</param>
        /// <param name="commitedProjectsState">committed projects state dict</param>
        /// <param name="refDepGuardExportParameters">RefDepGuardExportParameters values</param>
        /// <returns>loadError text (if the report wasn't generated successfully)</returns>
        public static string LoadReferencesDataToReport(
            Application excel, ConfigFilesData configFilesData, string reportType, 
            Dictionary<string, ProjectState> commitedProjectsState, RefDepGuardExportParameters refDepGuardExportParameters
            )
        {
            string loadError = "";
            string currentDateTime = DateTimeManager.GetCurrentDateTimeInRightFormat();

            RefDepGuardErrors refDepGuardErrors = refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardErrors;
            RefDepGuardWarnings refDepGuardWarnings = refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardWarnings;

            try
            {
                currentReportDirectory = configFilesData.PackageExtendedName + "\\reports\\"+ reportType +"\\" + currentDateTime;
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

        /// <summary>
        /// Opens the directory where the current report is stored. This method can be called after generating a report to quickly access it by the user.
        /// </summary>
        /// <returns>the result of opening the current report directory</returns>
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