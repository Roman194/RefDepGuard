using Microsoft.Office.Interop.Excel;
using Microsoft.VisualStudio.VCProjectEngine;
using RefDepGuard.Applied.Models.FrameworkVersion;
using RefDepGuard.Applied.Models.FrameworkVersion.Errors;
using RefDepGuard.Applied.Models.FrameworkVersion.Warnings.Conflicts;
using RefDepGuard.Applied.Models.Problem;
using RefDepGuard.Applied.Models.Project;
using RefDepGuard.Applied.Models.RefDepGuard;
using RefDepGuard.Applied.Models.Reference;
using RefDepGuard.Applied.Models.Reference.Errors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RefDepGuard.Managers.Export.SubManagers
{
    /// <summary>
    /// This class is responsible for loading data about projects and references, as well as any errors or warnings found during the checks, into Excel workbooks.
    /// </summary>
    public class LoadInfoToProjectAndReferenceWorkbooksHelper
    {

        /// <summary>
        /// Loads data about projects, as well as any errors or warnings found during the checks, into the first sheet of the Excel workbook.
        /// </summary>
        /// <param name="excel">Application (excel.interop) interface value</param>
        /// <param name="solutionName">Solution name string</param>
        /// <param name="currentDateTime">current DateTime of report generation in string format</param>
        /// <param name="commitedProjectsState">committed projects state dict</param>
        /// <param name="refDepGuardExportParameters">RefDepGuardExportParameters values</param>
        public static void LoadInfoToProjectsWorkbook(
            Application excel, string solutionName, string currentDateTime, Dictionary<string, ProjectState> commitedProjectsState, 
            RefDepGuardExportParameters refDepGuardExportParameters)
        {
            RefDepGuardErrors refDepGuardErrors = refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardErrors;
            RefDepGuardWarnings refDepGuardWarnings = refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardWarnings;
            Dictionary<string, List<RequiredMaxFrVersion>> requiredMaxFrVersions = refDepGuardExportParameters.RequiredParametersData.MaxRequiredFrameworkVersion;

            List<ReferenceError> refsErrorList = refDepGuardErrors.RefsErrorList;
            List<ReferenceMatchError> refMatchErrors = refDepGuardErrors.RefsMatchErrorList;
            List<MaxFrameworkVersionDeviantValueError> maxFrVersionDeviantValuesList = refDepGuardErrors.MaxFrameworkVersionDeviantValueList;
            List<FrameworkVersionComparabilityError> frameworkVersionComparabilityErrorsList = refDepGuardErrors.FrameworkVersionComparabilityErrorList;
            List<MaxFrameworkVersionConflictWarning> maxFrameworkVersionConflictWarningsList = refDepGuardWarnings.MaxFrameworkVersionConflictWarningsList;

            int widthIndex = 11;
            int heightIndex = 5; //count of rows that need to be counted from the beginning to go beyond the "header" of the table
            int firstRowIndex = heightIndex + 1;

            Worksheet projectsTable = (Worksheet)excel.Worksheets[1];
            projectsTable.Name = "Выборка по проектам";

            //Load and style of the table hat
            projectsTable = SetUnionColumnNames(projectsTable, solutionName, currentDateTime);
            projectsTable.Cells[heightIndex - 1, 3] = "Проект";
            projectsTable.Cells[heightIndex - 1, 4] = "Целевая рабочая\nсреда";
            projectsTable.Cells[heightIndex - 1, 5] = "Макс. допустимая\nверсия";
            projectsTable.Cells[heightIndex - 1, 6] = "Всего референсов";
            projectsTable.Cells[heightIndex, 6] = projectsTable.Cells[heightIndex, 8] = projectsTable.Cells[heightIndex, 10] = "Кол-во";
            projectsTable.Cells[heightIndex, 7] = projectsTable.Cells[heightIndex, 9] = projectsTable.Cells[heightIndex, 11] = "Названия";
            projectsTable.Cells[heightIndex - 1, 8] = "Не обнаружено обязательных референсов";
            projectsTable.Cells[heightIndex - 1, 10] = "Обнаружено недопустимых референсов";

            projectsTable.Columns[4].ColumnWidth = 17;
            projectsTable.Columns[5].ColumnWidth = 17;
            projectsTable.Columns[7].ColumnWidth = 35;
            projectsTable.Columns[9].ColumnWidth = 35;
            projectsTable.Columns[11].ColumnWidth = 35;

            Range unionRangeSolutionNameAndGenerateTime, unionRangeTableTitle;
            (unionRangeSolutionNameAndGenerateTime, unionRangeTableTitle) = SetUnionTableHatRanges(projectsTable, 11, 5);
            Range unionRangeNumTitle = projectsTable.Range[projectsTable.Cells[heightIndex - 1, 2], projectsTable.Cells[heightIndex, 2]];
            Range unionRangeProjectNameTitle = projectsTable.Range[projectsTable.Cells[heightIndex - 1, 3], projectsTable.Cells[heightIndex, 3]];
            Range unionRangeTargetFrameworkTitle = projectsTable.Range[projectsTable.Cells[heightIndex - 1, 4], projectsTable.Cells[heightIndex, 4]];
            Range unionRangeMaxFrVersionTitle = projectsTable.Range[projectsTable.Cells[heightIndex - 1, 5], projectsTable.Cells[heightIndex, 5]];
            Range unionRangeReferencesCountTitle = projectsTable.Range[projectsTable.Cells[heightIndex - 1, 6], projectsTable.Cells[heightIndex - 1, 7]];
            Range unionRangeRequiredRefsErrors = projectsTable.Range[projectsTable.Cells[heightIndex - 1, 8], projectsTable.Cells[heightIndex - 1, 9]];
            Range unionRangeUnacceptableRefsErrorsTitle = projectsTable.Range[projectsTable.Cells[heightIndex - 1, 10], projectsTable.Cells[heightIndex - 1, 11]];

            unionRangeNumTitle.Merge();
            unionRangeProjectNameTitle.Merge();
            unionRangeTargetFrameworkTitle.Merge();
            unionRangeMaxFrVersionTitle.Merge();
            unionRangeReferencesCountTitle.Merge();
            unionRangeRequiredRefsErrors.Merge();
            unionRangeUnacceptableRefsErrorsTitle.Merge();

            //Loads data to the table
            int i = 0;
            foreach (var currentProject in commitedProjectsState)//for each project in the commited projects state dictionary
            {
                string currentProjectName = currentProject.Key;
                List<string> currentPorjectRefs = currentProject.Value.CurrentReferences;
                string targetFramework = currentProject.Value.CurrentFrameworkVersionsString;
                List<string> requiredRefsErrors = refsErrorList
                    .Where(value => value.IsReferenceRequired && value.ErrorRelevantProjectName == currentProjectName)
                    .Select(value => value.ReferenceName)
                    .ToList();

                List<string> unacceptableRefsErrors = refsErrorList
                    .Where(value => !value.IsReferenceRequired && value.ErrorRelevantProjectName == currentProjectName)
                    .Select(value => value.ReferenceName)
                    .ToList();
                int requiredRefsErrorsCount = requiredRefsErrors.Count();
                int unacceptableRefsErrorsCount = unacceptableRefsErrors.Count();

                projectsTable = SetCurrentRowNum(projectsTable, i, heightIndex);
                projectsTable.Cells[firstRowIndex + i, 3] = currentProjectName;
                projectsTable.Cells[firstRowIndex + i, 4] = targetFramework;

                Range currentMaxFrVersionCellRange = projectsTable.Range[projectsTable.Cells[firstRowIndex + i, 5], projectsTable.Cells[firstRowIndex + i, 5]];
                currentMaxFrVersionCellRange.NumberFormat = "@";

                if (requiredMaxFrVersions.ContainsKey(currentProjectName))
                {
                    var projectTFMsVersion = "";
                    foreach (var currentProjectTFM in requiredMaxFrVersions[currentProjectName])
                    {
                        var ruleLevelString = "";
                        switch (currentProjectTFM.ReqLevel)
                        {
                            case ProblemLevel.Global: ruleLevelString = "[G]"; break;
                            case ProblemLevel.Solution: ruleLevelString = "[S]"; break;
                        }
                        projectTFMsVersion += (currentProjectTFM.VersionText + ruleLevelString);

                        if (requiredMaxFrVersions[currentProjectName].Last() != currentProjectTFM)
                            projectTFMsVersion += "; ";
                    }

                    projectsTable.Cells[firstRowIndex + i, 5] = projectTFMsVersion;

                    if (frameworkVersionComparabilityErrorsList.Find(warning => warning.ErrorRelevantProjectName == currentProjectName) != null)
                        projectsTable.Cells[firstRowIndex + i, 5].Font.Color = 0x062CCE;
                    else
                    {
                        if(maxFrameworkVersionConflictWarningsList.Find(warning => warning.WarningRelevantProjectName == currentProjectName) != null)
                            projectsTable.Cells[firstRowIndex + i, 5].Font.Color = 0x00C0FF; //Текст #FFC000
                    }
                }
                else
                {
                    if (maxFrVersionDeviantValuesList.Find(value => value.ErrorRelevantProjectName == currentProjectName) != null ||
                        maxFrVersionDeviantValuesList.Find(value => value.ErrorRelevantProjectName == "") != null)
                        projectsTable.Cells[firstRowIndex + i, 5] = "?";
                    else
                        projectsTable.Cells[firstRowIndex + i, 5] = "-";
                }

                projectsTable.Cells[firstRowIndex + i, 6] = currentPorjectRefs.Count;
                projectsTable.Cells[firstRowIndex + i, 7] = GetProjectsString(currentPorjectRefs);

                projectsTable.Cells[firstRowIndex + i, 8] = requiredRefsErrorsCount;

                if (requiredRefsErrorsCount > 0)
                {
                    //Actually this is not #CEC7FF, this is #FFC7CE, but Interop for some reason "reverses" this value
                    projectsTable.Cells[firstRowIndex + i, 8].Interior.Color = projectsTable.Cells[firstRowIndex + i, 9].Interior.Color = 0xCEC7FF;
                    projectsTable.Cells[firstRowIndex + i, 8].Font.Color = projectsTable.Cells[firstRowIndex + i, 9].Font.Color = 0x062CCE;
                }

                projectsTable.Cells[firstRowIndex + i, 9] = GetProjectsString(requiredRefsErrors);

                projectsTable.Cells[firstRowIndex + i, 10] = unacceptableRefsErrorsCount;
                if (unacceptableRefsErrorsCount > 0)
                {
                    projectsTable.Cells[firstRowIndex + i, 10].Interior.Color = projectsTable.Cells[firstRowIndex + i, 11].Interior.Color = 0xCEC7FF;
                    projectsTable.Cells[firstRowIndex + i, 10].Font.Color = projectsTable.Cells[firstRowIndex + i, 11].Font.Color = 0x062CCE;
                }

                var unacceptableRefsErrorsProjectString = GetProjectsString(unacceptableRefsErrors);
                projectsTable.Cells[firstRowIndex + i, 11] = unacceptableRefsErrorsProjectString;

                //Change cell format to prevent content from overflowing the cell if there are too many refs errors
                if (unacceptableRefsErrorsProjectString.Length > 15)
                {
                    Range currentCellRange = projectsTable.Range[projectsTable.Cells[firstRowIndex + i, 11], projectsTable.Cells[firstRowIndex + i, 11]];
                    currentCellRange.HorizontalAlignment = XlHAlign.xlHAlignFill;
                }

                i++;
            }

            //Working with borders
            projectsTable = SetUnionTableStyle(projectsTable, unionRangeSolutionNameAndGenerateTime, unionRangeTableTitle, i, i, heightIndex, widthIndex, false);

            Range unionRangeProjectName = projectsTable.Range[projectsTable.Cells[firstRowIndex, 3], projectsTable.Cells[i + heightIndex, 3]];
            Range unionRangeTargetFramework = projectsTable.Range[projectsTable.Cells[firstRowIndex, 4], projectsTable.Cells[i + heightIndex, 4]];
            Range unionRangeMaxFrVersionWithTitle = projectsTable.Range[projectsTable.Cells[heightIndex - 1, 5], projectsTable.Cells[i + heightIndex, 5]];
            Range unionRangeReferencesCount = projectsTable.Range[projectsTable.Cells[firstRowIndex, 6], projectsTable.Cells[i + heightIndex, 6]];
            Range unionRangeRequiredRefsErrorsCount = projectsTable.Range[projectsTable.Cells[firstRowIndex, 8], projectsTable.Cells[i + heightIndex, 8]];
            Range unionRangeUnacceptableRefsErrorsCount = projectsTable.Range[projectsTable.Cells[firstRowIndex, 10], projectsTable.Cells[i + heightIndex, 10]];

            //Warking with alignment and auto fit of columns
            unionRangeProjectName.EntireColumn.AutoFit();
            unionRangeMaxFrVersionWithTitle.HorizontalAlignment = XlVAlign.xlVAlignCenter;

            unionRangeReferencesCount.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            unionRangeReferencesCount.EntireColumn.AutoFit();

            unionRangeRequiredRefsErrorsCount.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            unionRangeRequiredRefsErrorsCount.EntireColumn.AutoFit();

            unionRangeUnacceptableRefsErrorsCount.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            unionRangeUnacceptableRefsErrorsCount.EntireColumn.AutoFit();
        }

        /// <summary>
        /// Loads data about references, as well as any errors or warnings found during the checks, into the second sheet of the Excel workbook.
        /// </summary>
        /// <param name="excel">Application (excel.interop) interface value</param>
        /// <param name="solutionName">Solution name string</param>
        /// <param name="currentDateTime">current DateTime of report generation in string format</param>
        /// <param name="commitedProjectsState">committed projects state dict</param>
        /// <param name="refDepGuardExportParameters">RefDepGuardExportParameters values</param>
        public static void LoadInfoToReferencesBook(Application excel, string solutionName, string currentDateTime, Dictionary<string, ProjectState> commitedProjectsState, RefDepGuardExportParameters refDepGuardExportParameters)
        {
            List<ReferenceError> refsErrorList = refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardErrors.RefsErrorList;
            List<ReferenceMatchError> refsMatchErrorList = refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardErrors.RefsMatchErrorList;
            List<RequiredReference> requiredReferences = refDepGuardExportParameters.RequiredParametersData.RequiredReferences;
            List<MaxFrameworkVersionReferenceConflictWarning> maxFrameworkVersionReferenceConflictWarningsList = 
                refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardWarnings.MaxFrameworkVersionReferenceConflictWarningsList;

            bool isPotentialVersionConflict = false;
            int widthIndex = 5;
            int heightIndex = 4; //As long as the names of columns in this table take up one row less, the index value is one less too
            int firstRowIndex = heightIndex + 1;

            Worksheet projectsTable = (Worksheet)excel.Worksheets[2];
            projectsTable.Name = "Выборка по референсам";

            projectsTable = SetUnionColumnNames(projectsTable, solutionName, currentDateTime);

            projectsTable.Cells[heightIndex, 3] = "Референс"; //for this reason heightIndex is call without minus one, as in the projects table
            projectsTable.Cells[heightIndex, 4] = "Проект";
            projectsTable.Cells[heightIndex, 5] = "Тип референса";

            Range unionRangeSolutionWithTime, unionRangeTableTitle;
            (unionRangeSolutionWithTime, unionRangeTableTitle) = SetUnionTableHatRanges(projectsTable, widthIndex, heightIndex);

            int i = 0;
            foreach (var currentProject in commitedProjectsState)//for each project in the commited projects state dictionary
            {
                string projectName = currentProject.Key;
                foreach (var projectReference in currentProject.Value.CurrentReferences)
                {
                    projectsTable = SetCurrentRowNum(projectsTable, i, heightIndex);
                    projectsTable.Cells[firstRowIndex + i, 3] = projectReference;
                    projectsTable.Cells[firstRowIndex + i, 4] = projectName;

                    //The choose of reference type is done according in the priority order.
                    RequiredReference requiredReference = requiredReferences
                        .Where(value => value.ReferenceName == projectReference && (value.RelevantProject == projectName || value.RelevantProject == ""))
                        .FirstOrDefault();//Should be finded no more than one value

                    MaxFrameworkVersionReferenceConflictWarning maxFrameworkVersionReference = maxFrameworkVersionReferenceConflictWarningsList
                        .Where(value => value.RefName == projectReference && value.ProjName == projectName)
                        .FirstOrDefault();

                    ReferenceError referenceError = refsErrorList
                        .Where(value => value.ErrorRelevantProjectName == projectName && value.ReferenceName == projectReference && value.IsReferenceRequired == false)
                        .FirstOrDefault();

                    ReferenceMatchError referenceMatchError = refsMatchErrorList
                        .Where(value => value.ReferenceName == projectReference && (value.ProjectName == projectName || value.ProjectName == ""))
                        .FirstOrDefault();

                    if (referenceMatchError != null)
                        projectsTable = SetReferenceTypeStyle(projectsTable, firstRowIndex, i, "?");
                    else
                    {
                        if (referenceError != null)
                            projectsTable = SetReferenceTypeStyle(projectsTable, firstRowIndex, i, "Недопустимый", 0xCEC7FF, 0x062CCE);
                        else
                        {
                            if (maxFrameworkVersionReference != null)
                            {
                                projectsTable = SetReferenceTypeStyle(projectsTable, firstRowIndex, i, "Потенциальный\r\nконфликт версий", 0x92e3f7, 0x00648b); //фон #f7e392 текст #8b6400
                                isPotentialVersionConflict = true;
                            }
                            else
                            {
                                if (requiredReference != null)
                                    projectsTable = SetReferenceTypeStyle(projectsTable, firstRowIndex, i, "Обязательный", 0xCEEFC6, 0x006100);
                                else
                                    projectsTable = SetReferenceTypeStyle(projectsTable, firstRowIndex, i, "-");
                            }
                        }
                    }

                    i++;
                }
            }

            var j = i; //i - columns without missing required references$ j - with them

            foreach (var refError in refsErrorList) //adds missing required references
            {
                if (refError.IsReferenceRequired)//if it is a reference required error,
                {
                    var refMatchError = refsMatchErrorList.Find(value =>
                        value.ReferenceName == refError.ReferenceName &&
                        (value.ProjectName == refError.ErrorRelevantProjectName || value.ProjectName == "")
                     );

                    //we check if the project and reference of this error are in the solution and if they are,
                    //we added a column in refernce table for them
                    if (refMatchError == null)
                    {
                        projectsTable.Cells[firstRowIndex + j, 2] = "-";
                        projectsTable.Cells[firstRowIndex + j, 3] = refError.ReferenceName;
                        projectsTable.Cells[firstRowIndex + j, 4] = refError.ErrorRelevantProjectName;

                        projectsTable.Cells[firstRowIndex + j, 3].Font.Color = projectsTable.Cells[firstRowIndex + j, 4].Font.Color = 0xA6A6A6;

                        projectsTable = SetReferenceTypeStyle(projectsTable, firstRowIndex, j, "Обязательный", 0xCEEFC6, 0x006100);

                        j++;
                    }
                }
            }

            projectsTable = SetUnionTableStyle(projectsTable, unionRangeSolutionWithTime, unionRangeTableTitle, i, j, heightIndex, widthIndex, true);

            if (isPotentialVersionConflict)
                projectsTable.Columns[5].ColumnWidth = 16;
        }

        /// <summary>
        /// Sets the column names for the projects and references tables, as well as the solution name and report generation time, and styles them.
        /// </summary>
        /// <param name="projectsTable">Worksheet value</param>
        /// <param name="solutionName">Solution name string</param>
        /// <param name="currentDateTime">Current DateTime of report generation in string format</param>
        /// <returns>projectsTable: Worksheet value</returns>
        private static Worksheet SetUnionColumnNames(Worksheet projectsTable, string solutionName, string currentDateTime)
        {
            projectsTable.Cells[2, 2] = "Solution: \"" + solutionName + "\"";
            projectsTable.Cells[3, 2] = currentDateTime;
            projectsTable.Cells[2, 2].Font.Bold = projectsTable.Cells[3, 2].Font.Bold = true;
            projectsTable.Cells[4, 2] = "№";

            return projectsTable;
        }

        /// <summary>
        /// Sets Ranges that are union for the table hats of both tables.
        /// </summary>
        /// <param name="projectsTable">Worksheet value</param>
        /// <param name="widthIndex">int widthIndex of the table</param>
        /// <param name="heightIndex">int heightIndex of the table</param>
        /// <returns>two ranges: solution with time and table title</returns>
        private static Tuple<Range, Range> SetUnionTableHatRanges(Worksheet projectsTable, int widthIndex, int heightIndex)
        {
            Range unionRangeSolutionName = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[2, widthIndex]];
            Range unionRangeGenerateTime = projectsTable.Range[projectsTable.Cells[3, 2], projectsTable.Cells[3, widthIndex]];
            Range unionRangeSolutionWithTime = projectsTable.Range[unionRangeSolutionName, unionRangeGenerateTime];

            Range unionRangeTableTitle = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[heightIndex, widthIndex]];

            unionRangeSolutionName.Merge();
            unionRangeGenerateTime.Merge();

            unionRangeTableTitle.HorizontalAlignment = XlVAlign.xlVAlignCenter;

            return new Tuple<Range, Range>(unionRangeSolutionWithTime, unionRangeTableTitle);
        }

        /// <summary>
        /// Sets the current row number in the first column of the table. 
        /// If it's the first row, sets "1", if not, sets formula to increase the previous row number by 1.
        /// </summary>
        /// <param name="projectsTable">Worksheet value</param>
        /// <param name="i">row index int</param>
        /// <param name="heightIndex">table height int index</param>
        /// <returns>Worksheet value</returns>
        private static Worksheet SetCurrentRowNum(Worksheet projectsTable, int i, int heightIndex)
        {
            if (i == 0)
                projectsTable.Cells[heightIndex + 1, 2] = "1";
            else
                projectsTable.Cells[heightIndex + 1 + i, 2].FormulaLocal = $"=B{i + heightIndex} + 1";

            return projectsTable;
        }

        /// <summary>
        /// Sets the reference type text and style in the current row of the references table based on the reference type 
        /// (required, unacceptable, potential version conflict or none of these).
        /// </summary>
        /// <param name="projectsTable">Worksheet value</param>
        /// <param name="firstRowIndex">first row index int</param>
        /// <param name="i">row index int</param>
        /// <param name="textType">text type string</param>
        /// <param name="interiorColor">interior color int</param>
        /// <param name="fontColor">font color int</param>
        /// <returns></returns>
        private static Worksheet SetReferenceTypeStyle(Worksheet projectsTable, int firstRowIndex, int i, string textType, 
            int interiorColor = 0x0fffff, int fontColor = 0x000000)
        {
            projectsTable.Cells[firstRowIndex + i, 5] = textType;
            if(interiorColor != 0x0fffff)
                projectsTable.Cells[firstRowIndex + i, 5].Interior.Color = interiorColor;
            projectsTable.Cells[firstRowIndex + i, 5].Font.Color = fontColor;

            return projectsTable;
        }

        /// <summary>
        /// Sets the style for the whole table based on the union ranges of the table hat, as well as the range of all the table with data.
        /// </summary>
        /// <param name="projectsTable">Worksheet value</param>
        /// <param name="unionRangeSolutionWithTime">RangeSolutionWithTime</param>
        /// <param name="unionRangeTableTitle">RangeTableTitle</param>
        /// <param name="i">row index int</param>
        /// <param name="extraColumnIndex">extracolumn int index</param>
        /// <param name="widthIndex">width int index</param>
        /// <param name="isReferencesWorkbook">shows if it's references workbook or not</param>
        /// <returns>Worksheet value</returns>
        private static Worksheet SetUnionTableStyle(Worksheet projectsTable, Range unionRangeSolutionWithTime, Range unionRangeTableTitle, int i, int j, int extraColumnIndex, int widthIndex, bool isReferencesWorkbook)
        {
            Range unionRangeTableWithoutMissingReq = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[i + extraColumnIndex, widthIndex]];
            Range unionRangeAllTable = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[j + extraColumnIndex, widthIndex]];
            Range unionRangeNumWithTitle = projectsTable.Range[projectsTable.Cells[4, 2], projectsTable.Cells[j + extraColumnIndex, 2]];

            unionRangeAllTable.Font.Name = "Calibri";
            unionRangeAllTable.Borders.Color = ColorTranslator.ToOle(Color.Black);
            if(isReferencesWorkbook)
                unionRangeAllTable.EntireColumn.AutoFit();
            unionRangeAllTable.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);

            unionRangeTableWithoutMissingReq.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);

            unionRangeNumWithTitle.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            unionRangeNumWithTitle.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);

            unionRangeTableTitle.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);
            unionRangeSolutionWithTime.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);

            return projectsTable;
        }

        /// <summary>
        /// Gets the string with project names from the list of project names. If there are more than 2 project names, they will be separated by comma and space.
        /// </summary>
        /// <param name="projectNames">list of strings of project names</param>
        /// <returns>projects string</returns>
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
    }
}
