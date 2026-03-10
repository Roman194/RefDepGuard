using Microsoft.Office.Interop.Excel;
using System;
using System.Drawing;
using RefDepGuard.Data;
using RefDepGuard.Data.FrameworkVersion;
using RefDepGuard.Data.Reference;
using RefDepGuard.Models;
using RefDepGuard.Models.FrameworkVersion;

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
                string currentErrorText = currentError.IsReferenceRequired ? "Отсутствует обязательный референс" : "Присутствует недопустимый референс";
                string currentOfferedAction = currentError.IsReferenceRequired ? "Добавить через обозреватель решений" : "Удалить через обозреватель решений";

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    currentError.ErrorRelevantProjectName, currentError.ReferenceName, "Reference", currentError.CurrentReferenceLevel.ToString(), currentErrorText, 
                    currentOfferedAction, currentError.ErrorRelevantProjectName + ".csproj", i);
            }

            foreach (ReferenceMatchError currentMatchError in refDepGuardErrors.RefsMatchErrorList)
            {
                string errorRelevantProjectName = (currentMatchError.ProjectName != "") ? currentMatchError.ProjectName : "-";
                string currentProblemText = currentMatchError.IsProjNameMatchError ? 
                    "Референс совпадает с именем проекта" :
                    "Референс одновременно заявлен как обязательный и\r\nнедопустимый";
                string currentDocName = (currentMatchError.ReferenceLevelValue == ProblemLevel.Global) ? "global_config_guard.rdg" : solutionName + "_config_guard.rdg";

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    errorRelevantProjectName, currentMatchError.ReferenceName, "Match", currentMatchError.ReferenceLevelValue.ToString(), currentProblemText, 
                    "Устраните противоречие в правиле", currentDocName, i);
            }

            foreach (ConfigFilePropertyNullError currentNullError in refDepGuardErrors.ConfigPropertyNullErrorList)
            {
                string errorRelevantProjectName = (currentNullError.ErrorRelevantProjectName != "") ? currentNullError.ErrorRelevantProjectName : "-";
                string currentErrorLevel = currentNullError.IsGlobal ? "Global" : (errorRelevantProjectName != "-" ? "Project" : "Solution");
                string currentErrorText = "Config-файл не содержит свойство \r\n'" + currentNullError.PropertyName + "'";
                string currentAction = "Проверьте его на предмет отсутствия \r\nсинтаксических ошибок и соответствия \r\nшаблону файла конфигурации";
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
                    " содержит один и тот\r\nже тип проекта в шаблоне более одного раза" :
                    " содержит\r\nнекорректную запись своего значения";
                string currentAction = "Проверьте его на предмет отсутствия \r\nсинтаксических ошибок и соответствия \r\nшаблону файла конфигурации";
                string currentDocumentName = "";

                switch (maxFrameworkVersionDeviantValue.ErrorLevel)
                {
                    case ProblemLevel.Solution: currentErrorLevel = "Solution"; break;
                    case ProblemLevel.Project: currentErrorLevel = "Project"; break;
                }
                currentDocumentName = (currentErrorLevel == "Global") ? "global_config_guard.rdg" : solutionName + "_config_guard.rdg";

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    errorRelevantProjectName, "-", "framework_max_version deviant value", currentErrorLevel, "Параметр 'framework_max_version'" + errorType, 
                    currentAction, currentDocumentName, i);
            }

            foreach (MaxFrameworkVersionIllegalTemplateUsageError maxFrameworkVersionIllegalTemplateUsageError in refDepGuardErrors.MaxFrameworkVersionIllegalTemplateUsageErrorList)
            {
                string errorDescr = maxFrameworkVersionIllegalTemplateUsageError.IsIllegalTFMUsageError ?
                    "а попытка задать\r\nограничение для TFM, не обнаруженного в текущем проекте" :
                    "о использование\r\nшаблона задания нескольких ограничений, что недопустимо для этого проекта (в проекте заявлен только один тип TFM)";

                string currentErrorText = "в параметре 'framework_max_version'\r\nпроекта '" + maxFrameworkVersionIllegalTemplateUsageError.ProjName + 
                    "' обнаружен" + errorDescr;

                string errorOrderSol = maxFrameworkVersionIllegalTemplateUsageError.IsIllegalTFMUsageError ? 
                    "Проверьте указанную строку\r\nmax_framework_version на предмет соответствия релевантных проекту TFM" : 
                    "Исправьте значение параметра на допустимое";

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    maxFrameworkVersionIllegalTemplateUsageError.ProjName, "-", "framework_max_version illegal template usage", "Project", currentErrorText, 
                    errorOrderSol, solutionName + "_config_guard.rdg", i);
            }

            foreach (FrameworkVersionComparabilityError frameworkVersionComparabilityError in refDepGuardErrors.FrameworkVersionComparabilityErrorList)
            {
                string currentErrorLevel = "Global";
                string currentErrorText = "Параметр 'TargetFrameworkVersion' имеет версию '" + frameworkVersionComparabilityError.TargetFrameworkVersion +
                    "', в\r\nто время как максимально допустимой для него версией является '" + frameworkVersionComparabilityError.MaxFrameworkVersion + "'";
                string documentName = "";

                switch (frameworkVersionComparabilityError.ErrorLevel)
                {
                    case ProblemLevel.Solution: currentErrorLevel = "Solution"; break;
                    case ProblemLevel.Project: currentErrorLevel = "Project"; break;
                }
                documentName = (currentErrorLevel == "Global") ? "global_config_guard.rdg" : solutionName + "_config_guard.rdg";

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    frameworkVersionComparabilityError.ErrorRelevantProjectName, "-", "Framework comparability version", currentErrorLevel, currentErrorText, 
                    "Измените версию проекта или модифицируйте конфигурацию Config-\r\nфайла", documentName, i);
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
                    (referenceMatchWarning.IsHighLevelReq ? " является\r\nобязательным и" : " является\r\nнедопустимым и") : 
                    (referenceMatchWarning.IsHighLevelReq ? " является\r\nнедопустимым и" : " является\r\nобязательным и"); //В противном случае рассматривается cross match errors, а значит они имеют тип рефа, противиположный более "верхнему" правилу
                string warningDescription = referenceMatchWarning.IsReferenceStraight ? 
                    " дубирует правило с одноимённым референсом " : " противоречит правилу с одноимённым референсом ";
                string warningAction = referenceMatchWarning.IsReferenceStraight ? "Устраните дублирование правила" : "Устраните противоречие в правиле";
                string documentName = solutionName + "_config_guard.rdg";

                switch (referenceMatchWarning.HighReferenceLevel)
                {
                    case ProblemLevel.Global: currentErrorLevels += "Global"; highReferenceLevelText = "глобального уровня"; break;
                    case ProblemLevel.Solution: currentErrorLevels += "Solution"; highReferenceLevelText = "уровня Solution"; break;
                }

                currentErrorLevels += " / ";

                switch (referenceMatchWarning.LowReferenceLevel)
                {
                    case ProblemLevel.Solution: currentErrorLevels += "Solution"; lowReferenceLevelText = "уровня Solution"; break;
                    case ProblemLevel.Project: currentErrorLevels += "Project"; break;
                }

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    relevantProject, referenceMatchWarning.ReferenceName, "Reference Match", currentErrorLevels, "Референс '" + referenceMatchWarning.ReferenceName + "' " + 
                    lowReferenceLevelText + referenceTypeText + warningDescription + highReferenceLevelText, warningAction, documentName, i);
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
                    "Данный проект указан в референс-правиле, но не\r\nобнаружен в Solution", "Проверьте правило на корректность\r\nнаписания имени проекта", documentName, i);
            }

            foreach (ProjectMatchWarning currentProjectMatchWarning in refDepGuardWarnings.ProjectMatchWarningList)
            {
                string placeWhereProjectNotFound = currentProjectMatchWarning.IsNoProjectInConfigFile ? "config-\r\nфайле" : "solution";

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    currentProjectMatchWarning.ProjName, "-", "Project match", "-", "Рассматриваемый проект не обнаружен в " + placeWhereProjectNotFound, 
                    "Проверьте проект на корректность\r\nнаписания его имени в config-файле", solutionName + "_config_guard.rdg", i);
            }

            foreach (MaxFrameworkVersionDeviantValueWarning maxFrameworkVersionDeviantValue in refDepGuardWarnings.MaxFrameworkVersionDeviantValueWarningList)
            {
                string relevantProject = maxFrameworkVersionDeviantValue.WarningRelevantProjectName != "" ? 
                    maxFrameworkVersionDeviantValue.WarningRelevantProjectName : "-";
                string warningLevel = "Global";
                string currentErrorText = "Параметр 'framework_max_version' содержит значение\r\n'" + maxFrameworkVersionDeviantValue.DeviantValue + 
                    "', а должен содержать значение с точкой (формата 'x.x')";
                string documentName = (maxFrameworkVersionDeviantValue.WarningLevel == ProblemLevel.Global) ? 
                    "global_config_guard.rdg" : solutionName + "_config_guard.rdg";

                switch (maxFrameworkVersionDeviantValue.WarningLevel)
                {
                    case ProblemLevel.Solution: warningLevel = "Solution"; break;
                    case ProblemLevel.Project: warningLevel = "Project"; break;
                }

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    relevantProject, "-", "framework_max_version deviant value", warningLevel, currentErrorText, "Приведите значение к корректному\r\nформату", documentName, i);
            }

            foreach (MaxFrameworkVersionConflictWarning maxFrameworkVersionConflictValue in refDepGuardWarnings.MaxFrameworkVersionConflictWarningsList)
            {
                string errorRelevantProjectName = "-";
                string currentErrorLevels = "";
                string highErrorLevelText = "";
                string lowErrorLevelText = "";
                string currentErrorText = "";

                if (maxFrameworkVersionConflictValue.LowWarnLevel == ProblemLevel.Project)
                    errorRelevantProjectName = maxFrameworkVersionConflictValue.WarningRelevantProjectName;

                switch (maxFrameworkVersionConflictValue.HighWarnLevel)
                {
                    case ProblemLevel.Global: currentErrorLevels += "Global"; highErrorLevelText = " глобального уровня"; break;
                    case ProblemLevel.Solution: currentErrorLevels += "Solution"; highErrorLevelText = " уровня Solution"; break;
                }

                if (maxFrameworkVersionConflictValue.HighWarnLevel == maxFrameworkVersionConflictValue.LowWarnLevel)
                    highErrorLevelText = ", указанное в супертипе 'all' на том же уровне";

                currentErrorLevels += " / ";

                switch (maxFrameworkVersionConflictValue.LowWarnLevel)
                {
                    case ProblemLevel.Global: currentErrorLevels += "Global"; break;
                    case ProblemLevel.Solution: currentErrorLevels += "Solution"; lowErrorLevelText = "уровня Solution"; break;
                    case ProblemLevel.Project: currentErrorLevels += "Project"; lowErrorLevelText = "в рассматриваемом проекте "; break;
                }

                currentErrorText = "Значение '" + maxFrameworkVersionConflictValue.LowLevelMaxFrameVersion
                    + "' параметра 'framework_max_version'\r\n" + lowErrorLevelText + " превосходит значение '" + maxFrameworkVersionConflictValue.HighLevelMaxFrameVersion
                    + "' одноимённого параметра" + highErrorLevelText;

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    errorRelevantProjectName, "-", "framework_max_version conflict", currentErrorLevels, currentErrorText, "Устраните противоречие", 
                    solutionName + "_config_guard.rdg", i);

            }

            foreach (MaxFrameworkVersionReferenceConflictWarning maxFrameworkVersionReferenceConflictWarning in refDepGuardWarnings.MaxFrameworkVersionReferenceConflictWarningsList)
            {
                string errorCause = (maxFrameworkVersionReferenceConflictWarning.IsOneProjectsTypeConflict) ?
                    "большее значение значение параметра 'framework_max_version' " :
                    "несовместимое значение параметра 'framework_max_version' для текущего значения проекта типа 'netstandard' ";

                string currentErrorText = "Значение '" + maxFrameworkVersionReferenceConflictWarning.ProjFrameworkVersion
                    + "' параметра 'framework_max_version'\r\nрассматриваемого проекта приводит к потенциальному конфликту версий TargetFramework" +
                    ",так как имеется референс на проект, имеющий " + errorCause + "(проект: " + maxFrameworkVersionReferenceConflictWarning.RefName
                    + ", Версия: " + maxFrameworkVersionReferenceConflictWarning.RefFrameworkVersion + ")";

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    maxFrameworkVersionReferenceConflictWarning.ProjName, maxFrameworkVersionReferenceConflictWarning.RefName, "framework_max_version reference conflict",
                    "-", currentErrorText, "Устраните противоречие", solutionName + "_config_guard.rdg", i);
            }

            foreach (MaxFrameworkVersionTFMNotFoundWarning maxFrameworkVersionTFMNotFoundWarning in refDepGuardWarnings.MaxFrameworkVersionTFMNotFoundWarningList)
            {
                string currentProjName = maxFrameworkVersionTFMNotFoundWarning.ProjName;
                string warningLevel = "Global";
                string currentErrorText = "Не найден TargetFramework, имеющий значение\r\n'" + maxFrameworkVersionTFMNotFoundWarning.TFMName;
                string currentAction = "Проверьте указанную в\r\n'max_framework_version' строку на предмет соответствия существующим TFM";
                string documentName = maxFrameworkVersionTFMNotFoundWarning.WarningLevel == ProblemLevel.Global ? 
                    "global_config_guard.rdg" : solutionName + "_config_guard.rdg";

                switch (maxFrameworkVersionTFMNotFoundWarning.WarningLevel)
                {
                    case ProblemLevel.Solution: warningLevel = "Solution"; break;
                    case ProblemLevel.Project: warningLevel = "Project"; break;
                }

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    currentProjName != "" ? currentProjName : "-", "-", "framework_max_version TFM not found", warningLevel, currentErrorText, currentAction, 
                    documentName, i);
            }

            foreach (string projName in refDepGuardWarnings.UntypedWarningsList)
            {
                string currentErrorText = "Не получилось произвести проверку версии 'TargetFramework' для рассматриваемого проекта,\r\n так как программе не удалось получить из .csproj файла корректное\r\n значение для этого свойства";
                string currentAction = "Проверьте, что проект имеет корректную версию 'TargetFramework'";

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    projName, "-", "untyped", "-", currentErrorText, currentAction, solutionName + ".csproj", i);

            }

            foreach (var projKeyValuePair in refDepGuardWarnings.DetectedTransitRefsDict)
            {
                string currentText = "У данного проекта обнаружены следующие\r\nтранзитивные референсы: ";

                foreach (var refName in projKeyValuePair.Value)
                {
                    currentText += "'" + refName + "', ";
                }

                currentText = currentText.Remove(currentText.Length - 2);

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    projKeyValuePair.Key, "-", "Transit references warning", "-", currentText, "-", solutionName + ".csproj", i);
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
            projectsTable.Cells[4, 3] = "Проект";
            projectsTable.Cells[4, 4] = "Референс";
            projectsTable.Cells[4, 5] = (isErrorsTable)? "Тип ошибки": "Тип предупреждения";
            projectsTable.Cells[4, 6] = (isErrorsTable)? "Уровень ошибки": "Уровни предупреждения";
            projectsTable.Cells[4, 7] = "Описание";
            projectsTable.Cells[4, 8] = "Необходимое действие";
            projectsTable.Cells[4, 9] = "Файл действия";

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
            projectsTable.Cells[5, 2] = (isErrorsTable ? "Ошибки" : "Предупреждения") + " на момент экспорта не обнаружены";

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
