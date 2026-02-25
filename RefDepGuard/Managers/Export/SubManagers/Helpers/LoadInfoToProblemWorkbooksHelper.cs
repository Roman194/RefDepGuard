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
    public class LoadInfoToProblemWorkbooksHelper
    {
        public static void LoadInfoToRefRepGuardErrors(Application excel, string solutionName, string currentDateTime, RefDepGuardErrors refDepGuardErrors)
        {
            Worksheet projectsTable = (Worksheet)excel.Worksheets[3];
            projectsTable.Name = "RefDepGuard errors";

            Range unionRangeSolutionWithTime, unionRangeTableTitle;

            (projectsTable, unionRangeSolutionWithTime, unionRangeTableTitle) = SetProblemsTableHat(projectsTable, solutionName, currentDateTime, true);

            int i = 0;

            //Форичи должны идти в порядке, указанном в моделях RefDepGuard Errors и Warnings!

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
                    "Референс одновременно заявлен как \r\nобязательный и недопустимый";
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
                string currentErrorLevel = "Global"; //???
                string errorType = maxFrameworkVersionDeviantValue.IsProjectTypeCopyError ? 
                    "\r\nсодержит один и тот же тип проекта в\r\nшаблоне более одного раза" : 
                    " содержит некорректную запись\r\nсвоего значения";
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
                string currentErrorText = "в параметре 'framework_max_version'\r\nпроекта '" + maxFrameworkVersionIllegalTemplateUsageError.ProjName + 
                    "' обнаружено использование шаблона задания нескольких ограничений, что недопустимо для этого проекта (в проекте заявлен только один тип TFM)";

                (projectsTable, i) = SetCurrentRowElements(projectsTable, 
                    maxFrameworkVersionIllegalTemplateUsageError.ProjName, "-", "framework_max_version illegal template usage", "Project", currentErrorText, 
                    "Исправьте значение параметра на допустимое", solutionName + "_config_guard.rdg", i);
            }

            foreach (FrameworkVersionComparabilityError frameworkVersionComparabilityError in refDepGuardErrors.FrameworkVersionComparabilityErrorList)
            {
                string currentErrorLevel = "Global";
                string currentErrorText = "Параметр 'TargetFrameworkVersion'\r\nимеет версию '" + frameworkVersionComparabilityError.TargetFrameworkVersion + 
                    "', в то время как\r\nмаксимально допустимой для него\r\nверсией является '" + frameworkVersionComparabilityError.MaxFrameworkVersion + "'";
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
            {
                projectsTable = SetMessageOnZeroFindedWorkbookProblems(projectsTable, true);
                i = 1;
            }

            projectsTable = SetProblemsFullTableStyle(projectsTable, i, unionRangeTableTitle, unionRangeSolutionWithTime, true);
        }

        public static void LoadInfoToRefDepGuardWarnings(Application excel, string solutionName, string currentDateTime, RefDepGuardWarnings refDepGuardWarnings){

            Worksheet projectsTable = (Worksheet)excel.Worksheets[4];
            projectsTable.Name = "RefDepGuard warnings";

            Range unionRangeSolutionWithTime, unionRangeTableTitle;

            (projectsTable, unionRangeSolutionWithTime, unionRangeTableTitle) = SetProblemsTableHat(projectsTable, solutionName, currentDateTime, false);

            int i = 0;

            foreach (ReferenceMatchWarning referenceMatchWarning in refDepGuardWarnings.RefsMatchWarningList)
            {
                string relevantProject = referenceMatchWarning.ProjectName == "" ? "-" : referenceMatchWarning.ProjectName;
                string currentErrorLevels = "";
                string highReferenceLevelText = "";
                string lowReferenceLevelText = "";
                string referenceTypeText = referenceMatchWarning.IsReferenceStraight ? 
                    (referenceMatchWarning.IsHighLevelReq ? " является\r\nобязательным и" : " является\r\nнедопустимым и") : 
                    (referenceMatchWarning.IsHighLevelReq ? "является\r\nнедопустимым и" : "является\r\nобязательным и"); //В противном случае рассматривается cross match errors, а значит они имеют тип рефа, противиположный более "верхнему" правилу
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
                string currentErrorText = "Параметр 'framework_max_version' содержит\r\nзначение '" + maxFrameworkVersionDeviantValue.DeviantValue + 
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
                    "несовместимое значение параметра 'framework_max_version' для проекта типа 'netstandard' ";

                string currentErrorText = "Значение '" + maxFrameworkVersionReferenceConflictWarning.ProjFrameworkVersion
                    + "' параметра 'framework_max_version'\r\nрассматриваемого проекта приводит к\r\nпотенциальному конфликту версий TargetFramework" +
                    ",\r\nтак как имеется референс на проект, имеющий\r\n" + errorCause + "(проект: " + maxFrameworkVersionReferenceConflictWarning.RefName
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

        private static Worksheet SetMessageOnZeroFindedWorkbookProblems(Worksheet projectsTable, bool isErrorsTable)
        {
            projectsTable.Cells[5, 2] = (isErrorsTable ? "Ошибки" : "Предупреждения") + " на момент экспорта не обнаружены";

            Range unionRangeOnEmptyText = projectsTable.Range[projectsTable.Cells[5, 2], projectsTable.Cells[5, 9]];
            unionRangeOnEmptyText.Merge();
            unionRangeOnEmptyText.HorizontalAlignment = XlVAlign.xlVAlignCenter;

            return projectsTable;
        }

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

            if (!isErrorsTable)
            {
                projectsTable.Columns[7].ColumnWidth = 50;
                projectsTable.Columns[8].ColumnWidth = 38;
            }
            
            return projectsTable;
        }
    }
}
