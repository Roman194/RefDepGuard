using Microsoft.Office.Interop.Excel;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Drawing;
using RefDepGuard.Data;
using RefDepGuard.Data.ConfigFile;
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

            foreach (MaxFrameworkVersionDeviantValueError maxFrameworkVersionDeviantValue in refDepGuardErrors.MaxFrameworkVersionDeviantValueList)
            {
                if (i == 0)
                    projectsTable.Cells[5, 2] = "1";
                else
                    projectsTable.Cells[5 + i, 2].FormulaLocal = $"=B{i + 4} + 1";

                string errorRelevantProjectName = maxFrameworkVersionDeviantValue.ErrorRelevantProjectName;
                string errorType = maxFrameworkVersionDeviantValue.IsProjectTypeCopyError ? "\r\nсодержит один и тот же тип проекта в\r\nшаблоне более одного раза" : " содержит некорректную запись\r\nсвоего значения";

                if (errorRelevantProjectName == "")
                    errorRelevantProjectName = "-";

                projectsTable.Cells[5 + i, 3] = errorRelevantProjectName;
                projectsTable.Cells[5 + i, 4] = "-";

                projectsTable.Cells[5 + i, 5] = "framework_max_version deviant value";

                string currentErrorLevel = "Global";
                if (maxFrameworkVersionDeviantValue.ErrorLevel != ProblemLevel.Global)
                {
                    if (errorRelevantProjectName != "-")
                        currentErrorLevel = "Project";
                    else
                        currentErrorLevel = "Solution";
                }

                projectsTable.Cells[5 + i, 6] = currentErrorLevel;

                projectsTable.Cells[5 + i, 7] = "Параметр 'framework_max_version'" + errorType;
                projectsTable.Cells[5 + i, 8] = "Проверьте его на предмет отсутствия \r\nсинтаксических ошибок и соответствия \r\nшаблону файла конфигурации";

                if (currentErrorLevel == "Global")
                    projectsTable.Cells[5 + i, 9] = "global_config_guard.rdg";
                else
                    projectsTable.Cells[5 + i, 9] = solutionName + "_config_guard.rdg";

                i++;
            }

            foreach (MaxFrameworkVersionIllegalTemplateUsageError maxFrameworkVersionIllegalTemplateUsageError in refDepGuardErrors.MaxFrameworkVersionIllegalTemplateUsageErrorList)
            {
                if (i == 0)
                    projectsTable.Cells[5, 2] = "1";
                else
                    projectsTable.Cells[5 + i, 2].FormulaLocal = $"=B{i + 4} + 1";

                projectsTable.Cells[5 + i, 3] = maxFrameworkVersionIllegalTemplateUsageError.ProjName;
                projectsTable.Cells[5 + i, 4] = "-";
                projectsTable.Cells[5 + i, 5] = "framework_max_version illegal template usage";
                projectsTable.Cells[5 + i, 6] = "Project";
                projectsTable.Cells[5 + i, 7] = "в параметре 'framework_max_version'\r\nпроекта '" +
                    maxFrameworkVersionIllegalTemplateUsageError.ProjName + "' обнаружено использование шаблона задания нескольких ограничений, что недопустимо для этого проекта (в проекте заявлен только один тип TFM)";
                projectsTable.Cells[5 + i, 8] = "Исправьте значение параметра на допустимое";
                projectsTable.Cells[5 + i, 9] = solutionName + "_config_guard.rdg";

                i++;
            }

            foreach (FrameworkVersionComparabilityError frameworkVersionComparabilityError in refDepGuardErrors.FrameworkVersionComparabilityErrorList)
            {
                if (i == 0)
                    projectsTable.Cells[5, 2] = "1";
                else
                    projectsTable.Cells[5 + i, 2].FormulaLocal = $"=B{i + 4} + 1";

                projectsTable.Cells[5 + i, 3] = frameworkVersionComparabilityError.ErrorRelevantProjectName;
                projectsTable.Cells[5 + i, 4] = "-";

                projectsTable.Cells[5 + i, 5] = "Framework comparability version";

                string currentErrorLevel = "Global";
                switch (frameworkVersionComparabilityError.ErrorLevel)
                {
                    case ProblemLevel.Solution: currentErrorLevel = "Solution"; break;
                    case ProblemLevel.Project: currentErrorLevel = "Project"; break;
                }

                projectsTable.Cells[5 + i, 6] = currentErrorLevel;

                projectsTable.Cells[5 + i, 7] = "Параметр 'TargetFrameworkVersion'\r\nимеет версию '" + frameworkVersionComparabilityError.TargetFrameworkVersion + "', в то время как\r\nмаксимально допустимой для него\r\nверсией является '" + frameworkVersionComparabilityError.MaxFrameworkVersion + "'";
                projectsTable.Cells[5 + i, 8] = "Измените версию проекта или модифицируйте конфигурацию Config-\r\nфайла";

                if (currentErrorLevel == "Global")
                    projectsTable.Cells[5 + i, 9] = "global_config_guard.rdg";
                else
                    projectsTable.Cells[5 + i, 9] = solutionName + "_config_guard.rdg";

                i++;
            }

            foreach (ConfigFilePropertyNullError currentNullError in refDepGuardErrors.ConfigPropertyNullErrorList)
            {
                if (i == 0)
                    projectsTable.Cells[5, 2] = "1";
                else
                    projectsTable.Cells[5 + i, 2].FormulaLocal = $"=B{i + 4} + 1";

                string errorRelevantProjectName = currentNullError.ErrorRelevantProjectName;
                if (errorRelevantProjectName == "")
                    errorRelevantProjectName = "-";

                projectsTable.Cells[5 + i, 3] = errorRelevantProjectName;
                projectsTable.Cells[5 + i, 4] = "-";

                projectsTable.Cells[5 + i, 5] = "Null property";

                string currentErrorLevel = "Global";
                if (!currentNullError.IsGlobal)
                {
                    if (errorRelevantProjectName != "-")
                        currentErrorLevel = "Project";
                    else
                        currentErrorLevel = "Solution";
                }

                projectsTable.Cells[5 + i, 6] = currentErrorLevel;

                projectsTable.Cells[5 + i, 7] = "Config-файл не содержит свойство \r\n'" + currentNullError.PropertyName + "'";
                projectsTable.Cells[5 + i, 8] = "Проверьте его на предмет отсутствия \r\nсинтаксических ошибок и соответствия \r\nшаблону файла конфигурации";

                if (currentErrorLevel == "Global")
                    projectsTable.Cells[5 + i, 9] = "global_config_guard.rdg";
                else
                    projectsTable.Cells[5 + i, 9] = solutionName + "_config_guard.rdg";

                i++;

            }

            foreach (ReferenceMatchError currentMatchError in refDepGuardErrors.RefsMatchErrorList)
            {
                if (i == 0)
                    projectsTable.Cells[5, 2] = "1";
                else
                    projectsTable.Cells[5 + i, 2].FormulaLocal = $"=B{i + 4} + 1";

                string errorRelevantProjectName = currentMatchError.ProjectName;
                if (errorRelevantProjectName == "")
                    errorRelevantProjectName = "-";

                projectsTable.Cells[5 + i, 3] = errorRelevantProjectName;
                projectsTable.Cells[5 + i, 4] = currentMatchError.ReferenceName;

                projectsTable.Cells[5 + i, 5] = "Match";

                projectsTable.Cells[5 + i, 6] = currentMatchError.ReferenceLevelValue.ToString();

                if (!currentMatchError.IsProjNameMatchError)
                    projectsTable.Cells[5 + i, 7] = "Референс одновременно заявлен как \r\nобязательный и недопустимый";
                else
                    projectsTable.Cells[5 + i, 7] = "Референс совпадает с именем проекта";

                projectsTable.Cells[5 + i, 8] = "Устраните противоречие в правиле";

                if (currentMatchError.ReferenceLevelValue == ProblemLevel.Global)
                    projectsTable.Cells[5 + i, 9] = "global_config_guard.rdg";
                else
                    projectsTable.Cells[5 + i, 9] = solutionName + "_config_guard.rdg";

                i++;
            }

            foreach (ReferenceError currentError in refDepGuardErrors.RefsErrorList)
            {
                if (i == 0)
                    projectsTable.Cells[5, 2] = "1";
                else
                    projectsTable.Cells[5 + i, 2].FormulaLocal = $"=B{i + 4} + 1";

                projectsTable.Cells[5 + i, 3] = currentError.ErrorRelevantProjectName;
                projectsTable.Cells[5 + i, 4] = currentError.ReferenceName;

                projectsTable.Cells[5 + i, 5] = "Reference";

                projectsTable.Cells[5 + i, 6] = currentError.CurrentReferenceLevel.ToString();

                if (!currentError.IsReferenceRequired)
                {
                    projectsTable.Cells[5 + i, 7] = "Присутствует недопустимый референс";
                    projectsTable.Cells[5 + i, 8] = "Удалить через обозреватель решений";
                }
                else
                {
                    projectsTable.Cells[5 + i, 7] = "Отсутствует обязательный референс";
                    projectsTable.Cells[5 + i, 8] = "Добавить через обозреватель решений";
                }

                projectsTable.Cells[5 + i, 9] = currentError.ErrorRelevantProjectName + ".csproj";

                i++;
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

            foreach (var projKeyValuePair in refDepGuardWarnings.DetectedTransitRefsDict)
            {
                if (i == 0)
                    projectsTable.Cells[5, 2] = "1";
                else
                    projectsTable.Cells[5 + i, 2].FormulaLocal = $"=B{i + 4} + 1";

                projectsTable.Cells[5 + i, 3] = projKeyValuePair.Key;
                projectsTable.Cells[5 + i, 4] = "-";
                projectsTable.Cells[5 + i, 5] = "Transit references warning";
                projectsTable.Cells[5 + i, 6] = "-";

                string currentText = "У данного проекта обнаружены следующие\r\nтранзитивные референсы: ";

                foreach (var refName in projKeyValuePair.Value)
                {
                    currentText += "'" + refName + "', ";
                }

                currentText = currentText.Remove(currentText.Length - 2);

                projectsTable.Cells[5 + i, 7] = currentText;
                projectsTable.Cells[5 + i, 8] = "-";
                projectsTable.Cells[5 + i, 9] = solutionName + ".csproj";

                i++;
            }

            foreach (string projName in refDepGuardWarnings.UntypedWarningsList)
            {
                if (i == 0)
                    projectsTable.Cells[5, 2] = "1";
                else
                    projectsTable.Cells[5 + i, 2].FormulaLocal = $"=B{i + 4} + 1";

                projectsTable.Cells[5 + i, 3] = projName;
                projectsTable.Cells[5 + i, 4] = "-";
                projectsTable.Cells[5 + i, 5] = "untyped";
                projectsTable.Cells[5 + i, 6] = "-";
                projectsTable.Cells[5 + i, 7] = "Не получилось произвести проверку версии 'TargetFramework' для рассматриваемого проекта,\r\n так как программе не удалось получить из .csproj файла корректное\r\n значение для этого свойства";
                projectsTable.Cells[5 + i, 8] = "Проверьте, что проект имеет корректную версию 'TargetFramework'";
                projectsTable.Cells[5 + i, 9] = solutionName + ".csproj";

                i++;
            }

            foreach (ProjectMatchWarning currentProjectMatchWarning in refDepGuardWarnings.ProjectMatchWarningList)
            {
                if (i == 0)
                    projectsTable.Cells[5, 2] = "1";
                else
                    projectsTable.Cells[5 + i, 2].FormulaLocal = $"=B{i + 4} + 1";

                projectsTable.Cells[5 + i, 3] = currentProjectMatchWarning.ProjName;
                projectsTable.Cells[5 + i, 4] = "-";
                projectsTable.Cells[5 + i, 5] = "Project match";
                projectsTable.Cells[5 + i, 6] = "-";

                string placeWhereProjectNotFound = "solution";

                if (currentProjectMatchWarning.IsNoProjectInConfigFile)
                    placeWhereProjectNotFound = "config-\r\nфайле";

                projectsTable.Cells[5 + i, 7] = "Рассматриваемый проект не обнаружен в " + placeWhereProjectNotFound;
                projectsTable.Cells[5 + i, 8] = "Проверьте проект на корректность\r\nнаписания его имени в config-файле";
                projectsTable.Cells[5 + i, 9] = solutionName + "_config_guard.rdg";

                i++;
            }

            foreach (MaxFrameworkVersionDeviantValueWarning maxFrameworkVersionDeviantValue in refDepGuardWarnings.MaxFrameworkVersionDeviantValueWarningList)
            {
                if (i == 0)
                    projectsTable.Cells[5, 2] = "1";
                else
                    projectsTable.Cells[5 + i, 2].FormulaLocal = $"=B{i + 4} + 1";

                projectsTable.Cells[5 + i, 3] = maxFrameworkVersionDeviantValue.WarningRelevantProjectName != "" ? maxFrameworkVersionDeviantValue.WarningRelevantProjectName : "-";
                projectsTable.Cells[5 + i, 4] = "-";
                projectsTable.Cells[5 + i, 5] = "framework_max_version deviant value";

                string warningLevel = "";

                switch (maxFrameworkVersionDeviantValue.WarningLevel)
                {
                    case ProblemLevel.Global: warningLevel = "Global";  break;
                    case ProblemLevel.Solution: warningLevel = "Solution"; break;
                    case ProblemLevel.Project: warningLevel = "Project"; break;
                }

                projectsTable.Cells[5 + i, 6] = warningLevel;
                projectsTable.Cells[5 + i, 7] = "Параметр 'framework_max_version' содержит\r\nзначение '" + maxFrameworkVersionDeviantValue.DeviantValue + "', а должен содержать значение с точкой (формата 'x.x')";
                projectsTable.Cells[5 + i, 8] = "Приведите значение к корректному\r\nформату";
                projectsTable.Cells[5 + i, 9] = maxFrameworkVersionDeviantValue.WarningLevel == ProblemLevel.Global ? "global_config_guard.rdg" : solutionName + "_config_guard.rdg";

                i++;
            }

            foreach (var currentProjectNotFoundWarning in refDepGuardWarnings.ProjectNotFoundWarningList)
            {
                if (i == 0)
                    projectsTable.Cells[5, 2] = "1";
                else
                    projectsTable.Cells[5 + i, 2].FormulaLocal = $"=B{i + 4} + 1";

                projectsTable.Cells[5 + i, 3] = currentProjectNotFoundWarning.ProjName != "" ? currentProjectNotFoundWarning.ProjName : "-";
                projectsTable.Cells[5 + i, 4] = currentProjectNotFoundWarning.ReferenceName;
                projectsTable.Cells[5 + i, 5] = "Project not found";

                string warningLevel = "";
                string documentName = solutionName + "_config_guard.rdg";
                switch (currentProjectNotFoundWarning.WarningLevel)
                {
                    case ProblemLevel.Global: warningLevel = "Global"; documentName = "global_config_guard.rdg";  break;
                    case ProblemLevel.Solution: warningLevel = "Solution"; break;
                    case ProblemLevel.Project: warningLevel = "Project"; break;
                }

                projectsTable.Cells[5 + i, 6] = warningLevel;
                projectsTable.Cells[5 + i, 7] = "Данный проект указан в референс-правиле, но не\r\nобнаружен в Solution";
                projectsTable.Cells[5 + i, 8] = "Проверьте правило на корректность\r\nнаписания имени проекта";
                projectsTable.Cells[5 + i, 9] = documentName;

                i++;
            }

                foreach (MaxFrameworkVersionConflictWarning maxFrameworkVersionConflictValue in refDepGuardWarnings.MaxFrameworkVersionConflictWarningsList)
            {
                string currentErrorLevels = "";
                string highErrorLevelText = "";
                string lowErrorLevelText = "";

                if (i == 0)
                    projectsTable.Cells[5, 2] = "1";
                else
                    projectsTable.Cells[5 + i, 2].FormulaLocal = $"=B{i + 4} + 1";

                string errorRelevantProjectName = "-";
                if (maxFrameworkVersionConflictValue.LowWarnLevel == ProblemLevel.Project)
                    errorRelevantProjectName = maxFrameworkVersionConflictValue.WarningRelevantProjectName;

                projectsTable.Cells[5 + i, 3] = errorRelevantProjectName;
                projectsTable.Cells[5 + i, 4] = "-";

                projectsTable.Cells[5 + i, 5] = "framework_max_version conflict";

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
                    case ProblemLevel.Project: 
                        currentErrorLevels += "Project"; 
                        lowErrorLevelText = "в рассматриваемом проекте "; 
                        break;
                }
                
                projectsTable.Cells[5 + i, 6] = currentErrorLevels;
                projectsTable.Cells[5 + i, 7] = "Значение '" + maxFrameworkVersionConflictValue.LowLevelMaxFrameVersion
                    + "' параметра 'framework_max_version'\r\n" + lowErrorLevelText + " превосходит значение '" + maxFrameworkVersionConflictValue.HighLevelMaxFrameVersion
                    + "' одноимённого параметра" + highErrorLevelText;
                projectsTable.Cells[5 + i, 8] = "Устраните противоречие";
                projectsTable.Cells[5 + i, 9] = solutionName + "_config_guard.rdg";

                i++;

            }

            foreach (MaxFrameworkVersionReferenceConflictWarning maxFrameworkVersionReferenceConflictWarning in refDepGuardWarnings.MaxFrameworkVersionReferenceConflictWarningsList)
            {
                if (i == 0)
                    projectsTable.Cells[5, 2] = "1";
                else
                    projectsTable.Cells[5 + i, 2].FormulaLocal = $"=B{i + 4} + 1";

                string errorCause = (maxFrameworkVersionReferenceConflictWarning.IsOneProjectsTypeConflict) ?
                    "большее значение значение параметра 'framework_max_version' " :
                    "несовместимое значение параметра 'framework_max_version' для проекта типа 'netstandard' ";

                projectsTable.Cells[5 + i, 3] = maxFrameworkVersionReferenceConflictWarning.ProjName;
                projectsTable.Cells[5 + i, 4] = maxFrameworkVersionReferenceConflictWarning.RefName;
                projectsTable.Cells[5 + i, 5] = "framework_max_version reference conflict";
                projectsTable.Cells[5 + i, 6] = "-";
                projectsTable.Cells[5 + i, 7] = "Значение '" + maxFrameworkVersionReferenceConflictWarning.ProjFrameworkVersion
                    + "' параметра 'framework_max_version'\r\nрассматриваемого проекта приводит к\r\nпотенциальному конфликту версий TargetFramework" +
                    ",\r\nтак как имеется референс на проект, имеющий\r\n"+ errorCause + "(проект: " + maxFrameworkVersionReferenceConflictWarning.RefName
                    + ", Версия: " + maxFrameworkVersionReferenceConflictWarning.RefFrameworkVersion + ")";
                projectsTable.Cells[5 + i, 8] = "Устраните противоречие";
                projectsTable.Cells[5 + i, 9] = solutionName + "_config_guard.rdg";

                i++;
            }

            foreach (MaxFrameworkVersionTFMNotFoundWarning maxFrameworkVersionTFMNotFoundWarning in refDepGuardWarnings.MaxFrameworkVersionTFMNotFoundWarningList)
            {
                string currentProjName = maxFrameworkVersionTFMNotFoundWarning.ProjName;

                if (i == 0)
                    projectsTable.Cells[5, 2] = "1";
                else
                    projectsTable.Cells[5 + i, 2].FormulaLocal = $"=B{i + 4} + 1";

                projectsTable.Cells[5 + i, 3] = currentProjName != "" ? currentProjName : "-";
                projectsTable.Cells[5 + i, 4] = "-";
                projectsTable.Cells[5 + i, 5] = "framework_max_version TFM not found";
                
                string warningLevel = "";

                switch (maxFrameworkVersionTFMNotFoundWarning.WarningLevel)
                {
                    case ProblemLevel.Global: warningLevel = "Global"; break;
                    case ProblemLevel.Solution: warningLevel = "Solution"; break;
                    case ProblemLevel.Project: warningLevel = "Project"; break;
                }

                projectsTable.Cells[5 + i, 6] = warningLevel;
                projectsTable.Cells[5 + i, 7] = "Не найден TargetFramework, имеющий значение\r\n'" + maxFrameworkVersionTFMNotFoundWarning.TFMName;
                projectsTable.Cells[5 + i, 8] = "Проверьте указанную в\r\n'max_framework_version' строку на предмет соответствия существующим TFM";
                projectsTable.Cells[5 + i, 9] = maxFrameworkVersionTFMNotFoundWarning.WarningLevel == ProblemLevel.Global ? "global_config_guard.rdg" : solutionName + "_config_guard.rdg";

                i++;
            }

            foreach (ReferenceMatchWarning referenceMatchWarning in refDepGuardWarnings.RefsMatchWarningList)
            {
                string documentName = solutionName + "_config_guard.rdg";
                string currentErrorLevels = "";
                string highReferenceLevelText = "";
                string lowReferenceLevelText = "";
                string referenceTypeText = "";
                string warningDescription = "";
                string warningAction = "";

                if (i == 0)
                    projectsTable.Cells[5, 2] = "1";
                else
                    projectsTable.Cells[5 + i, 2].FormulaLocal = $"=B{i + 4} + 1";

                projectsTable.Cells[5 + i, 3] = referenceMatchWarning.ProjectName == ""? "-": referenceMatchWarning.ProjectName;
                projectsTable.Cells[5 + i, 4] = referenceMatchWarning.ReferenceName;

                projectsTable.Cells[5 + i, 5] = "Reference Match";

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

                if (referenceMatchWarning.IsReferenceStraight)
                {
                    warningDescription = " дубирует правило с одноимённым референсом ";
                    warningAction = "Устраните дублирование правила";

                    if (referenceMatchWarning.IsHighLevelReq)
                        referenceTypeText = " является\r\nобязательным и";
                    else
                        referenceTypeText = " является\r\nнедопустимым и";
                }
                else //В противном случае рассматривается cross match errors, а значит они имеют тип рефа, противиположный более "верхнему" правилу
                {
                    warningDescription = " противоречит правилу с одноимённым референсом ";
                    warningAction = "Устраните противоречие в правиле";

                    if (referenceMatchWarning.IsHighLevelReq)
                        referenceTypeText = "является\r\nнедопустимым и";
                    else
                        referenceTypeText = "является\r\nобязательным и";
                }

                projectsTable.Cells[5 + i, 6] = currentErrorLevels;
                projectsTable.Cells[5 + i, 7] = "Референс '" + referenceMatchWarning.ReferenceName + "' " + lowReferenceLevelText + referenceTypeText + warningDescription + highReferenceLevelText;
                projectsTable.Cells[5 + i, 8] = warningAction;
                projectsTable.Cells[5 + i, 9] = documentName;

                i++;
            }

            if(i == 0)
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
