using Microsoft.Office.Interop.Excel;
using RefDepGuard.Applied.Models.ConfigFile;
using RefDepGuard.Applied.Models.FrameworkVersion.Errors;
using RefDepGuard.Applied.Models.FrameworkVersion.Warnings;
using RefDepGuard.Applied.Models.FrameworkVersion.Warnings.Conflicts;
using RefDepGuard.Applied.Models.Problem;
using RefDepGuard.Applied.Models.Project;
using RefDepGuard.Applied.Models.RefDepGuard;
using RefDepGuard.Applied.Models.Reference.Errors;
using RefDepGuard.Applied.Models.Reference.Warnings;
using RefDepGuard.StringResources;
using System;
using System.Drawing;

namespace RefDepGuard.Managers.Export.SubManagers
{
    /// <summary>
    /// This class is responsible for loading data about errors and warnings found during the checks to the relevant Excel workbooks.
    /// </summary>
    public class LoadInfoToProblemWorkbooksHelper
    {
        /// <summary>
        /// The method for loading data about errors found during the checks to the relevant Excel workbook. 
        /// It populates the workbook with detailed information about each error, including the project and reference involved, the type and level of the error, 
        /// a description, and recommended actions for resolving the issue. 
        /// If no errors are found, it displays a message indicating that no problems were detected. 
        /// This method ensures that all relevant information is clearly presented to the user in an organized manner.
        /// </summary>
        /// <param name="excel">Application (excel.interop) interface value</param>
        /// <param name="solutionName">solution name string</param>
        /// <param name="currentDateTime">current DateTime of report generation in string format</param>
        /// <param name="refDepGuardErrors">RefDepGuardErrors value</param>
        public static void LoadInfoToRefRepGuardErrors(Application excel, string solutionName, string currentDateTime, RefDepGuardErrors refDepGuardErrors)
        {
            Worksheet projectsTable = (Worksheet)excel.Worksheets[3];
            projectsTable.Name = "RefDepGuard errors";

            Range unionRangeSolutionWithTime, unionRangeTableTitle;

            (projectsTable, unionRangeSolutionWithTime, unionRangeTableTitle) = SetProblemsTableHat(projectsTable, solutionName, currentDateTime, true);

            int i = 0;

            //For each of every type of errors
            //Foreach must go in the order, specified in RefDepGuard Errors and Warnings models, to provide correct order of errors display in the report!
            foreach (ReferenceError currentError in refDepGuardErrors.RefsErrorList)
            {
                string currentErrorText = (currentError.IsReferenceRequired ? Resource.No_Required_String : Resource.Unacceptable_String) + Resource.Reference_String;
                string currentOfferedAction = currentError.IsReferenceRequired ? Resource.Action_Add_String : Resource.Action_Remove_String;

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    currentError.ErrorRelevantProjectName, currentError.ReferenceName, "Reference", currentError.CurrentRuleLevel.ToString(), currentErrorText, 
                    currentOfferedAction, currentError.ErrorRelevantProjectName + ".csproj", i);
            }

            foreach (ReferenceMatchError currentMatchError in refDepGuardErrors.RefsMatchErrorList)
            {
                string errorRelevantProjectName = (currentMatchError.ProjectName != "") ? currentMatchError.ProjectName : "-";
                string currentProblemText = currentMatchError.IsProjNameMatchError ?
                    Resource.One_Proj_Match_Error_Description : Resource.Claim_Match_Error_Description;
                string currentDocName = (currentMatchError.RuleLevel == ProblemLevel.Global) ? "global_config_guard.rdg" : solutionName + "_config_guard.rdg";

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    errorRelevantProjectName, currentMatchError.ReferenceName, "Match", currentMatchError.RuleLevel.ToString(), currentProblemText, 
                    Resource.Reference_Match_Error_Action, currentDocName, i);
            }

            foreach (ConfigFilePropertyNullError currentNullError in refDepGuardErrors.ConfigPropertyNullErrorList)
            {
                string errorRelevantProjectName = (currentNullError.ErrorRelevantProjectName != "") ? currentNullError.ErrorRelevantProjectName : "-";
                string currentErrorLevel = currentNullError.IsGlobal ? "Global" : (errorRelevantProjectName != "-" ? "Project" : "Solution");
                string currentErrorText = Resource.Config_File_Property_Null_Error_Message + currentNullError.PropertyName + "'";
                string currentAction = Resource.Config_File_Property_Null_Error_Action;
                string documentName = (currentErrorLevel == "Global") ? "global_config_guard.rdg" : solutionName + "_config_guard.rdg";

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    errorRelevantProjectName, "-", "Null property", currentErrorLevel, currentErrorText, currentAction, documentName, i);
            }

            foreach (MaxFrameworkVersionDeviantValueError maxFrameworkVersionDeviantValue in refDepGuardErrors.MaxFrameworkVersionDeviantValueList)
            {
                string errorRelevantProjectName = (maxFrameworkVersionDeviantValue.ErrorRelevantProjectName != "") ? 
                    maxFrameworkVersionDeviantValue.ErrorRelevantProjectName : "-";
                string currentErrorLevel = "Global";
                string errorType = maxFrameworkVersionDeviantValue.IsProjectTypeCopyError ?
                    Resource.Project_Type_Copy_Error : Resource.Invalid_Project_Type_Error;
                string currentAction = Resource.Config_File_Property_Null_Error_Action;
                string currentDocumentName = "";

                switch (maxFrameworkVersionDeviantValue.ErrorLevel)
                {
                    case ProblemLevel.Solution: currentErrorLevel = "Solution"; break;
                    case ProblemLevel.Project: currentErrorLevel = "Project"; break;
                }
                currentDocumentName = (currentErrorLevel == "Global") ? "global_config_guard.rdg" : solutionName + "_config_guard.rdg";

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    errorRelevantProjectName, "-", "framework_max_version deviant value", currentErrorLevel, 
                    Resource.Max_Framework_Version_Deviant_Value_Error_Message + errorType, currentAction, currentDocumentName, i);
            }

            foreach (MaxFrameworkVersionIllegalTemplateUsageError maxFrameworkVersionIllegalTemplateUsageError in refDepGuardErrors.MaxFrameworkVersionIllegalTemplateUsageErrorList)
            {
                string errorDescr = maxFrameworkVersionIllegalTemplateUsageError.IsIllegalTFMUsageError ?
                    Resource.Illegal_TFM_Usage_Error_Message : Resource.Incorrect_TFM_Usage_Error_Message;

                string currentErrorText = Resource.Max_Framework_Version_Deviant_Value_Error_Message + Resource.Of_A_Project_String + 
                    maxFrameworkVersionIllegalTemplateUsageError.ProjName + errorDescr;

                string errorOrderSol = maxFrameworkVersionIllegalTemplateUsageError.IsIllegalTFMUsageError ?
                    Resource.Illegal_TFM_Usage_Error_Action : Resource.Incorrect_TFM_Usage_Error_Action;

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    maxFrameworkVersionIllegalTemplateUsageError.ProjName, "-", "framework_max_version illegal template usage", "Project", currentErrorText, 
                    errorOrderSol, solutionName + "_config_guard.rdg", i);
            }

            foreach (FrameworkVersionComparabilityError frameworkVersionComparabilityError in refDepGuardErrors.FrameworkVersionComparabilityErrorList)
            {
                string currentErrorLevel = "Global";
                string currentTFMText = (frameworkVersionComparabilityError.ErrorRelevantTFM != "") ?
                    ("(" + Resource.For_string + "TFM '" + frameworkVersionComparabilityError.ErrorRelevantTFM + "')") : "";
                string currentErrorText = Resource.Parameter_Value + "'TargetFrameworkVersion'" +  Resource.Have_Version_String + 
                    frameworkVersionComparabilityError.TargetFrameworkVersion +
                    "'"+ currentTFMText + Resource.Framework_Version_Comparability_Error_Message + frameworkVersionComparabilityError.MaxFrameworkVersion + "'";
                string documentName = "";

                switch (frameworkVersionComparabilityError.ErrorLevel)
                {
                    case ProblemLevel.Solution: currentErrorLevel = "Solution"; break;
                    case ProblemLevel.Project: currentErrorLevel = "Project"; break;
                }
                documentName = (currentErrorLevel == "Global") ? "global_config_guard.rdg" : solutionName + "_config_guard.rdg";

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    frameworkVersionComparabilityError.ErrorRelevantProjectName, "-", "Framework comparability version", currentErrorLevel, currentErrorText,
                    Resource.Framework_Version_Comparability_Error_Action, documentName, i);
            }

            if (i == 0)
            {//if there are no errors, set message about it in the table
                projectsTable = SetMessageOnZeroFindedWorkbookProblems(projectsTable, true);
                i = 1;
            }

            projectsTable = SetProblemsFullTableStyle(projectsTable, i, unionRangeTableTitle, unionRangeSolutionWithTime, true);
        }

        /// <summary>
        /// The main method for loading data about warnings found during the checks to the relevant Excel workbook.
        /// </summary>
        /// <param name="excel">Application (excel.interop) interface value</param>
        /// <param name="solutionName">solution name string</param>
        /// <param name="currentDateTime">current DateTime of report generation in string format</param>
        /// <param name="refDepGuardWarnings">RefDepGuardWarnings value</param>
        public static void LoadInfoToRefDepGuardWarnings(Application excel, string solutionName, string currentDateTime, RefDepGuardWarnings refDepGuardWarnings){

            Worksheet projectsTable = (Worksheet)excel.Worksheets[4];
            projectsTable.Name = "RefDepGuard warnings";

            Range unionRangeSolutionWithTime, unionRangeTableTitle;

            (projectsTable, unionRangeSolutionWithTime, unionRangeTableTitle) = SetProblemsTableHat(projectsTable, solutionName, currentDateTime, false);

            int i = 0;

            //For each of every type of warnings
            //Foreach must go in the order, specified in RefDepGuard Errors and Warnings models, to provide correct order of errors display in the report!
            foreach (ReferenceMatchWarning referenceMatchWarning in refDepGuardWarnings.RefsMatchWarningList)
            {
                string relevantProject = referenceMatchWarning.ProjectName == "" ? "-" : referenceMatchWarning.ProjectName;
                string currentErrorLevels = "";
                string highReferenceLevelText = "";
                string lowReferenceLevelText = "";
                string referenceTypeText = referenceMatchWarning.IsReferenceStraight ?
                    (referenceMatchWarning.IsHighLevelReq ? Resource.Req_Reference_Type : Resource.Unaccept_Reference_Type) :
                    (referenceMatchWarning.IsHighLevelReq ? Resource.Unaccept_Reference_Type : Resource.Req_Reference_Type); //В противном случае рассматривается cross match errors, а значит они имеют тип рефа, противиположный более "верхнему" правилу
                string warningDescription = referenceMatchWarning.IsReferenceStraight ?
                    Resource.Reference_Match_Warn_Duplicate_Message : Resource.Reference_Match_Warn_Contrad_Message;
                string warningAction = referenceMatchWarning.IsReferenceStraight ? 
                    Resource.Reference_Match_Duplicate_Warn_Action : Resource.Reference_Match_Contrad_Warn_Action;
                string documentName = solutionName + "_config_guard.rdg";

                switch (referenceMatchWarning.HighReferenceLevel)
                {
                    case ProblemLevel.Global: currentErrorLevels += "Global"; highReferenceLevelText = Resource.Global_Level; break;
                    case ProblemLevel.Solution: currentErrorLevels += "Solution"; highReferenceLevelText = Resource.Solution_Level; break;
                }

                currentErrorLevels += " / ";

                switch (referenceMatchWarning.LowReferenceLevel)
                {
                    case ProblemLevel.Solution: currentErrorLevels += "Solution"; lowReferenceLevelText = Resource.Solution_Level; break;
                    case ProblemLevel.Project: currentErrorLevels += "Project"; break;
                }

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    relevantProject, referenceMatchWarning.ReferenceName, "Reference Match", currentErrorLevels, Resource.Reference_String + "'" + 
                    referenceMatchWarning.ReferenceName + "' " + lowReferenceLevelText + referenceTypeText + warningDescription + highReferenceLevelText, 
                    warningAction, documentName, i);
            }

            foreach (var currentProjectNotFoundWarning in refDepGuardWarnings.ProjectNotFoundWarningList)
            {
                string relevantProject = currentProjectNotFoundWarning.ProjName != "" ? currentProjectNotFoundWarning.ProjName : "-";
                string warningLevel = "Global";
                string documentName = (currentProjectNotFoundWarning.WarningLevel == ProblemLevel.Global) ? 
                    "global_config_guard.rdg" : solutionName + "_config_guard.rdg";
                
                switch (currentProjectNotFoundWarning.WarningLevel)
                {
                    case ProblemLevel.Solution: warningLevel = "Solution"; break;
                    case ProblemLevel.Project: warningLevel = "Project"; break;
                }

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    relevantProject, currentProjectNotFoundWarning.ReferenceName, "Project not found", warningLevel, 
                    Resource.Project_Not_Found_Warning_Message, Resource.Project_Not_Found_Warning_Action, documentName, i);
            }

            foreach (ProjectMatchWarning currentProjectMatchWarning in refDepGuardWarnings.ProjectMatchWarningList)
            {
                string placeWhereProjectNotFound = currentProjectMatchWarning.IsNoProjectInConfigFile ? Resource.Config_File_String_2 : Resource.Solution_String;

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    currentProjectMatchWarning.ProjName, "-", "Project match", "-", Resource.Project_Match_Warning_Message + placeWhereProjectNotFound, 
                    Resource.Project_Match_Warning_Action, solutionName + "_config_guard.rdg", i);
            }

            foreach (MaxFrameworkVersionDeviantValueWarning maxFrameworkVersionDeviantValue in refDepGuardWarnings.MaxFrameworkVersionDeviantValueWarningList)
            {
                string relevantProject = maxFrameworkVersionDeviantValue.WarningRelevantProjectName != "" ? 
                    maxFrameworkVersionDeviantValue.WarningRelevantProjectName : "-";
                string warningLevel = "Global";
                string currentWarningText = Resource.Max_Framework_Version_Deviant_Value_Error_Message + Resource.Contains_Value_String + 
                    maxFrameworkVersionDeviantValue.DeviantValue + Resource.Max_Fr_Version_Deviant_Value_Warn_Message;
                string documentName = (maxFrameworkVersionDeviantValue.WarningLevel == ProblemLevel.Global) ? 
                    "global_config_guard.rdg" : solutionName + "_config_guard.rdg";

                switch (maxFrameworkVersionDeviantValue.WarningLevel)
                {
                    case ProblemLevel.Solution: warningLevel = "Solution"; break;
                    case ProblemLevel.Project: warningLevel = "Project"; break;
                }

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    relevantProject, "-", "framework_max_version deviant value", warningLevel, currentWarningText, Resource.Max_Fr_Version_Deviant_Value_Warn_Action
                    , documentName, i);
            }

            foreach (MaxFrameworkVersionConflictWarning maxFrameworkVersionConflictValue in refDepGuardWarnings.MaxFrameworkVersionConflictWarningsList)
            {
                string warningRelevantProjectName = "-";
                string currentWarningLevels = "";
                string highErrorLevelText = "";
                string lowErrorLevelText = "";
                string currentWarningText = "";

                if (maxFrameworkVersionConflictValue.LowWarnLevel == ProblemLevel.Project)
                    warningRelevantProjectName = maxFrameworkVersionConflictValue.WarningRelevantProjectName;

                switch (maxFrameworkVersionConflictValue.HighWarnLevel)
                {
                    case ProblemLevel.Global: currentWarningLevels += "Global"; highErrorLevelText = Resource.Global_Level; break;
                    case ProblemLevel.Solution: currentWarningLevels += "Solution"; highErrorLevelText = Resource.Solution_Level; break;
                }

                if (maxFrameworkVersionConflictValue.HighWarnLevel == maxFrameworkVersionConflictValue.LowWarnLevel)
                    highErrorLevelText = Resource.Supertype_All_Level;

                currentWarningLevels += " / ";

                switch (maxFrameworkVersionConflictValue.LowWarnLevel)
                {
                    case ProblemLevel.Global: currentWarningLevels += "Global"; break;
                    case ProblemLevel.Solution: currentWarningLevels += "Solution"; lowErrorLevelText = Resource.Solution_Level; break;
                    case ProblemLevel.Project: currentWarningLevels += "Project"; lowErrorLevelText = Resource.In_A_Cons_Project_String; break;
                }

                currentWarningText = Resource.Value_String + maxFrameworkVersionConflictValue.LowLevelMaxFrameVersion
                    + Resource.Of_The_Fr_Max_Version_String + "\r\n" + lowErrorLevelText + Resource.Exceed_String + Resource.Value_String + 
                    maxFrameworkVersionConflictValue.HighLevelMaxFrameVersion + Resource.Eponymous_Parameter_String + highErrorLevelText;

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    warningRelevantProjectName, "-", "framework_max_version conflict", currentWarningLevels, currentWarningText, Resource.Reference_Match_Error_Action, 
                    solutionName + "_config_guard.rdg", i);

            }

            foreach (MaxFrameworkVersionReferenceConflictWarning maxFrameworkVersionReferenceConflictWarning in refDepGuardWarnings.MaxFrameworkVersionReferenceConflictWarningsList)
            {
                string warningCause = (maxFrameworkVersionReferenceConflictWarning.IsOneProjectsTypeConflict) ?
                    Resource.Max_Fr_Version_Conflict_Bigger_Value_Error_Cause :
                    Resource.Max_Fr_Version_Reference_Conflict_Incomparable_Error_Cause;

                string currentWarningText = Resource.Value_String + maxFrameworkVersionReferenceConflictWarning.ProjFrameworkVersion + "' "
                    + Resource.Of_The_Fr_Max_Version_String + "\r\n" + Resource.In_A_Cons_Project_String + Resource.Max_Fr_Version_Deviant_Value_Warn_Message + 
                    warningCause + "(" + Resource.Project_String + maxFrameworkVersionReferenceConflictWarning.RefName
                    + "'," + Resource.Version_String + maxFrameworkVersionReferenceConflictWarning.RefFrameworkVersion + ")";

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    maxFrameworkVersionReferenceConflictWarning.ProjName, maxFrameworkVersionReferenceConflictWarning.RefName, "framework_max_version reference conflict",
                    "-", currentWarningText, Resource.Reference_Match_Error_Action, solutionName + "_config_guard.rdg", i);
            }

            foreach (MaxFrameworkVersionTFMNotFoundWarning maxFrameworkVersionTFMNotFoundWarning in refDepGuardWarnings.MaxFrameworkVersionTFMNotFoundWarningList)
            {
                string currentProjName = maxFrameworkVersionTFMNotFoundWarning.ProjName;
                string warningLevel = "Global";
                string currentWarningText = Resource.TFM_Not_Found_Warn_Message + maxFrameworkVersionTFMNotFoundWarning.TFMName;
                string currentAction = Resource.TFM_Not_Found_Warn_Action;
                string documentName = maxFrameworkVersionTFMNotFoundWarning.WarningLevel == ProblemLevel.Global ? 
                    "global_config_guard.rdg" : solutionName + "_config_guard.rdg";

                switch (maxFrameworkVersionTFMNotFoundWarning.WarningLevel)
                {
                    case ProblemLevel.Solution: warningLevel = "Solution"; break;
                    case ProblemLevel.Project: warningLevel = "Project"; break;
                }

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    currentProjName != "" ? currentProjName : "-", "-", "framework_max_version TFM not found", warningLevel, currentWarningText, currentAction, 
                    documentName, i);
            }

            foreach (MaxFrameworkIllegalTemplateUsageWarning maxFrameworkIllegalTemplateUsageWarning in refDepGuardWarnings.MaxFrameworkIllegalTemplateUsageWarningList)
            {
                string warningLevel = maxFrameworkIllegalTemplateUsageWarning.ProblemLevelInfo == ProblemLevel.Global ? "Global" : "Solution";
                string warningText = Resource.Illegal_Template_Usage_Warn_Message;
                string currentAction = Resource.Illegal_Template_Usage_Warn_Action;

                string documentName = (maxFrameworkIllegalTemplateUsageWarning.ProblemLevelInfo == ProblemLevel.Global) ?
                    "global_config_guard.rdg" : solutionName + "_config_guard.rdg";

                (projectsTable, i) = SetCurrentRowElements(projectsTable,"-", "-", "framework_max_version illegal template usage", warningLevel, warningText, currentAction,
                    documentName, i);
            }

            foreach (ProjectNameSemanticWarning projNameSemaWarning in refDepGuardWarnings.ProjectNameSemanticWarningList)
            {
                string warningText = Resource.In_The_Proj_Name_String + projNameSemaWarning.ProjectName + Resource.Proj_Name_Semantic_Warn_Message + 
                    projNameSemaWarning.ExpectedSema + Resource.Proj_Name_Semantic_Warn_Message_1 + projNameSemaWarning.FindedSema + "')";
                string currentAction = Resource.Proj_Name_Semantic_Warn_Action;

                string documentName = "global_config_guard.rdg";

                (projectsTable, i) = SetCurrentRowElements(projectsTable, "-", "-", "Project name semantic warning", "Project", warningText, currentAction,
                    documentName, i);
            }

            foreach (string projName in refDepGuardWarnings.UntypedWarningsList)
            {
                string currentWarningText = Resource.Untyped_Warn_Message;
                string currentAction = Resource.Untyped_Warn_Action;

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    projName, "-", "untyped", "-", currentWarningText, currentAction, solutionName + ".csproj", i);

            }

            foreach (var projKeyValuePair in refDepGuardWarnings.DetectedNDuplicatedTransitRefsDict.Item1)
            {
                string currentText = Resource.Transit_Refs_Message;

                foreach (var refName in projKeyValuePair.Value)
                {
                    currentText += "'" + refName + "', ";
                }

                currentText = currentText.Remove(currentText.Length - 2);

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    projKeyValuePair.Key, "-", "Transit references", "-", currentText, "-", solutionName + ".csproj", i);
            }

            foreach (var projKeyValuePair in refDepGuardWarnings.DetectedNDuplicatedTransitRefsDict.Item2)
            {
                var projName = projKeyValuePair.Key;

                string currentText = Resource.Transit_Refs_Duplicate_Warn_Message;

                foreach (var refName in projKeyValuePair.Value)
                {
                    currentText += "'" + refName + "', ";
                }

                currentText = currentText.Remove(currentText.Length - 2);

                (projectsTable, i) = SetCurrentRowElements(projectsTable,
                    projName, "-", "Transit references duplicate ", "-", currentText, "-", solutionName + ".csproj", i);
            }

            if (i == 0)
            {
                projectsTable = SetMessageOnZeroFindedWorkbookProblems(projectsTable, false);
                i = 1;
            }

            projectsTable = SetProblemsFullTableStyle(projectsTable, i, unionRangeTableTitle, unionRangeSolutionWithTime, false);
        }

        /// <summary>
        /// Sets the hat of the table in the workbook with the main information about the solution and generation time, and also with the titles of columns.
        /// </summary>
        /// <param name="projectsTable">Worksheet value</param>
        /// <param name="solutionName">Solution name string</param>
        /// <param name="currentDateTime">Current DateTime of report generation in string format</param>
        /// <param name="isErrorsTable">Shows if it's an error table or not</param>
        /// <returns>Worksheet and Ranges of current project table<returns>
        private static Tuple<Worksheet, Range, Range> SetProblemsTableHat(Worksheet projectsTable, string solutionName, string currentDateTime, bool isErrorsTable)
        {
            projectsTable.Cells[2, 2] = "Solution: \"" + solutionName + "\"";
            projectsTable.Cells[3, 2] = currentDateTime;
            projectsTable.Cells[2, 2].Font.Bold = projectsTable.Cells[3, 2].Font.Bold = true;

            projectsTable.Cells[4, 2] = "№";
            projectsTable.Cells[4, 3] = Resource.Project_Column_Title;
            projectsTable.Cells[4, 4] = Resource.Reference_Column_Title;
            projectsTable.Cells[4, 5] = (isErrorsTable)? Resource.Error_Type_Title : Resource.Warning_Type_Title;
            projectsTable.Cells[4, 6] = (isErrorsTable)? Resource.Error_Level_Title: Resource.Warning_Level_Title;
            projectsTable.Cells[4, 7] = Resource.Description_Title;
            projectsTable.Cells[4, 8] = Resource.Necessary_Action_Title;
            projectsTable.Cells[4, 9] = Resource.Action_File_Title;

            Range unionRangeSolutionName = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[2, 9]];
            Range unionRangeGenerateTime = projectsTable.Range[projectsTable.Cells[3, 2], projectsTable.Cells[3, 9]];
            Range unionRangeSolutionWithTime = projectsTable.Range[unionRangeSolutionName, unionRangeGenerateTime];
            Range unionRangeTableTitle = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[4, 9]];

            unionRangeSolutionName.Merge();
            unionRangeGenerateTime.Merge();

            unionRangeTableTitle.HorizontalAlignment = XlVAlign.xlVAlignCenter;

            return new Tuple<Worksheet, Range, Range>(projectsTable, unionRangeSolutionWithTime, unionRangeTableTitle);
        }

        /// <summary>
        /// Sets the elements of the current row in the table with the information about the current error/warning. 
        /// Also sets the formula for the first column with numeration of errors/warnings.
        /// </summary>
        /// <param name="projectsTable">Worksheet value</param>
        /// <param name="relevantProject">relevant project string</param>
        /// <param name="relevantReference">relevant reference string</param>
        /// <param name="problemType">problem type string</param>
        /// <param name="problemLevel">problem level string</param>
        /// <param name="description">description string</param>
        /// <param name="offeredAction">offerde action string</param>
        /// <param name="relevantDocumentName">relevant doc name string</param>
        /// <param name="i">current row int index</param>
        /// <returns>workcsheet witn row index</returns>
        private static Tuple<Worksheet, int> SetCurrentRowElements(Worksheet projectsTable, string relevantProject, string relevantReference, string problemType, string problemLevel,
            string description, string offeredAction, string relevantDocumentName, int i)
        {
            if (i == 0)
                projectsTable.Cells[5, 2] = "1";
            else
                projectsTable.Cells[5 + i, 2].FormulaLocal = $"=B{i + 4} + 1";

            projectsTable.Cells[5 + i, 3] = relevantProject;
            projectsTable.Cells[5 + i, 4] = relevantReference;
            projectsTable.Cells[5 + i, 5] = problemType;
            projectsTable.Cells[5 + i, 6] = problemLevel;
            projectsTable.Cells[5 + i, 7] = description;
            projectsTable.Cells[5 + i, 8] = offeredAction;
            projectsTable.Cells[5 + i, 9] = relevantDocumentName;

            i++;

            return new Tuple<Worksheet, int>(projectsTable, i);
        }

        /// <summary>
        /// Sets the message about zero finded errors/warnings in the table, if there are no errors/warnings to display.
        /// </summary>
        /// <param name="projectsTable">Worksheet value</param>
        /// <param name="isErrorsTable">shows if it's an error table or not</param>
        /// <returns>Worksheet of the projectsTable</returns>
        private static Worksheet SetMessageOnZeroFindedWorkbookProblems(Worksheet projectsTable, bool isErrorsTable)
        {
            projectsTable.Cells[5, 2] = isErrorsTable ? Resource.On_Zero_Finded_Errors_Table_Report : Resource.On_Zero_Finded_Warnings_Table_Report;

            Range unionRangeOnEmptyText = projectsTable.Range[projectsTable.Cells[5, 2], projectsTable.Cells[5, 9]];
            unionRangeOnEmptyText.Merge();
            unionRangeOnEmptyText.HorizontalAlignment = XlVAlign.xlVAlignCenter;

            return projectsTable;
        }

        /// <summary>
        /// Sets the style of the table with errors/warnings in the workbook, after it was populated with all the relevant data.
        /// </summary>
        /// <param name="projectsTable">Worksheet value</param>
        /// <param name="i">current row int index</param>
        /// <param name="unionRangeTableTitle">unionRangeTableTitle value</param>
        /// <param name="unionRangeSolutionWithTime">unionRangeSolutionWithTime value</param>
        /// <param name="isErrorsTable">Shows if it's an error table or not</param>
        /// <returns>Worksheet of the projectsTable</returns>
        private static Worksheet SetProblemsFullTableStyle(Worksheet projectsTable, int i, Range unionRangeTableTitle, Range unionRangeSolutionWithTime, bool isErrorsTable)
        {
            Range unionRangeAllTable = projectsTable.Range[projectsTable.Cells[2, 2], projectsTable.Cells[i + 4, 9]];
            Range unionRangeNumWithTitle = projectsTable.Range[projectsTable.Cells[4, 2], projectsTable.Cells[i + 4, 2]];

            unionRangeAllTable.Font.Name = "Calibri";
            unionRangeAllTable.Borders.Color = ColorTranslator.ToOle(Color.Black);
            unionRangeAllTable.EntireColumn.AutoFit();
            unionRangeAllTable.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);

            unionRangeNumWithTitle.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            unionRangeNumWithTitle.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);

            unionRangeTableTitle.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);
            unionRangeSolutionWithTime.BorderAround2(XlLineStyle.xlContinuous, XlBorderWeight.xlMedium, XlColorIndex.xlColorIndexAutomatic);

            projectsTable.Columns[7].ColumnWidth = 50;
            projectsTable.Columns[8].ColumnWidth = 38;
            
            return projectsTable;
        }
    }
}