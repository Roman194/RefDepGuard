using RefDepGuard.Applied.Models.ConfigFile;
using RefDepGuard.Applied.Models.FrameworkVersion.Errors;
using RefDepGuard.Applied.Models.FrameworkVersion.Warnings;
using RefDepGuard.Applied.Models.FrameworkVersion.Warnings.Conflicts;
using RefDepGuard.Applied.Models.Problem;
using RefDepGuard.Applied.Models.RefDepGuard;
using RefDepGuard.Applied.Models.Reference.Errors;
using RefDepGuard.Applied.Models.Reference.Warnings;
using System.Collections.Generic;

namespace RefDepGuard.Applied
{
    public class ProblemsStringStoreManager
    {
        public static List<ProblemString> ConvertCurrentErrorsToStringFormat(
            RefDepGuardErrors refDepGuardErrors, ConfigFilesData configFilesData, bool isLoadToConsole)
        {
            var problemsStringList = new List<ProblemString>();
            var outputPlacePrefix = isLoadToConsole ? "    - " : "RefDepGuard ";
            var outputPlaceTransfer = isLoadToConsole ? "\r\n" : " ";

            foreach (ReferenceError error in refDepGuardErrors.RefsErrorList)
            {
                string referenceTypeText = error.IsReferenceRequired ? "Отсутсвует обязательный" : "Присутствует недопустимый";
                string actionForUser = error.IsReferenceRequired ? "Добавьте" : "Удалите";
                string referenceLevelText = "";
                string documentName = error.ErrorRelevantProjectName + ".csproj";

                switch (error.CurrentRuleLevel)
                {
                    case ProblemLevel.Solution: referenceLevelText = "уровня Solution "; break;
                    case ProblemLevel.Global: referenceLevelText = "глобального уровня "; break;
                }

                string errorText = outputPlacePrefix + "Reference error: " + referenceTypeText + " референс " + referenceLevelText + "'" + error.ReferenceName +
                    "' для проекта '" + error.ErrorRelevantProjectName + "'." + outputPlaceTransfer + actionForUser + " его через обозреватель решений"; 
                
                problemsStringList.Add(new ProblemString(errorText, documentName));
            }

            foreach (ReferenceMatchError referenceMatchError in refDepGuardErrors.RefsMatchErrorList)
            {
                string projectName = (referenceMatchError.ProjectName != "") ? "' проекта '" + referenceMatchError.ProjectName : "";
                string referenceLevelText = "";
                string matchErrorDescription = referenceMatchError.IsProjNameMatchError ?
                    " совпадает с именем проекта" : " одновременно заявлен как обязательный и недопустимый";
                string documentName = (referenceMatchError.RuleLevel == ProblemLevel.Global) ?
                    "global_config_guard.rdg" : configFilesData.SolutionName + "_config_guard.rdg";

                switch (referenceMatchError.RuleLevel)
                {
                    case ProblemLevel.Solution: referenceLevelText = " уровня Solution"; break;
                    case ProblemLevel.Global: referenceLevelText = " глобального уровня"; break;
                }

                string errorText = outputPlacePrefix + "Match error: референс '" + referenceMatchError.ReferenceName + projectName + "'" + referenceLevelText +
                    matchErrorDescription + "." + outputPlaceTransfer + "Устраните противоречие в правиле";

                problemsStringList.Add(new ProblemString(errorText, documentName));
            }

            foreach (ConfigFilePropertyNullError configFilePropertyNullError in refDepGuardErrors.ConfigPropertyNullErrorList)
            {
                string relevantProjectName = (configFilePropertyNullError.ErrorRelevantProjectName != "") ?
                    " для проекта '" + configFilePropertyNullError.ErrorRelevantProjectName + "'" : "";
                string errorText = outputPlacePrefix + "Null property error: Config-файл не содержит свойство '" + configFilePropertyNullError.PropertyName + "'" +
                    relevantProjectName + "." + outputPlaceTransfer + "Проверьте его на предмет отсутствия синтаксических ошибок и соответствия шаблону файла конфигурации";
                string documentName = configFilePropertyNullError.IsGlobal ? "global_config_guard.rdg" : configFilesData.SolutionName + "_config_guard.rdg";

                problemsStringList.Add(new ProblemString(errorText, documentName));
            }

            foreach (MaxFrameworkVersionDeviantValueError maxFrameworkVersionDeviantValue in refDepGuardErrors.MaxFrameworkVersionDeviantValueList)
            {
                string relevantProjectName = "";
                string globalPrefix = "";
                string errorType = maxFrameworkVersionDeviantValue.IsProjectTypeCopyError ?
                    " содержит один и тот же тип проекта в шаблоне более одного раза" : " содержит некорректную запись своего значения";
                string documentName = (maxFrameworkVersionDeviantValue.ErrorLevel == ProblemLevel.Global) ?
                    "global_config_guard.rdg" : configFilesData.SolutionName + "_config_guard.rdg";

                switch (maxFrameworkVersionDeviantValue.ErrorLevel)
                {
                    case ProblemLevel.Global: globalPrefix = "глобального "; break;
                    case ProblemLevel.Solution: relevantProjectName = "уровня Solution"; break;
                    case ProblemLevel.Project: relevantProjectName = "проекта '" + maxFrameworkVersionDeviantValue.ErrorRelevantProjectName + "'"; break;
                }
                string errorText = outputPlacePrefix + "framework_max_version deviant value error: параметр 'framework_max_version' " + globalPrefix + "Config-файла " +
                    relevantProjectName + errorType + "." + outputPlaceTransfer + "Проверьте его на предмет отсутствия синтаксических ошибок и соответствия шаблону файла конфигурации";

                problemsStringList.Add(new ProblemString(errorText, documentName));
            }

            foreach (MaxFrameworkVersionIllegalTemplateUsageError maxFrameworkVersionIllegalTemplateUsageError in refDepGuardErrors.MaxFrameworkVersionIllegalTemplateUsageErrorList)
            {
                string errorDescr = maxFrameworkVersionIllegalTemplateUsageError.IsIllegalTFMUsageError ?
                    ("а попытка задать ограничение для TFM, не обнаруженного в текущем проекте." + outputPlaceTransfer + 
                    "Проверьте указанную строку max_framework_version на предмет соответствия релевантных проекту TFM") :
                    ("о использование шаблона задания нескольких ограничений, что недопустимо для этого проекта (в проекте заявлен только один тип TFM)." 
                    + outputPlaceTransfer + "Исправьте значение параметра на допустимое");

                string errorText = outputPlacePrefix + "framework_max_version illegal template usage error: в параметре 'framework_max_version' проекта '" +
                    maxFrameworkVersionIllegalTemplateUsageError.ProjName +
                    "' обнаружен" + errorDescr;

                string documentName = configFilesData.SolutionName + "_config_guard.rdg";

                problemsStringList.Add(new ProblemString(errorText, documentName));
            }

            foreach (FrameworkVersionComparabilityError frameworkVersionComparabilityError in refDepGuardErrors.FrameworkVersionComparabilityErrorList)
            {
                string ruleLevel = "ограничение глобального уровня";
                string currentTFMText = (frameworkVersionComparabilityError.ErrorRelevantTFM != "") ?
                    (" для TFM '" + frameworkVersionComparabilityError.ErrorRelevantTFM + "'") : "";
                string documentName = (frameworkVersionComparabilityError.ErrorLevel == ProblemLevel.Global) ?
                    configFilesData.SolutionName + "_config_guard.rdg" : "global_config_guard.rdg";

                switch (frameworkVersionComparabilityError.ErrorLevel)
                {
                    case ProblemLevel.Solution: ruleLevel = "ограничение уровня решения"; break;
                    case ProblemLevel.Project: ruleLevel = "ограничение уровня проекта"; break;
                }
                string errorText = outputPlacePrefix + "Framework version comparability error: 'TargetFrameworkVersion' проекта '" + frameworkVersionComparabilityError.ErrorRelevantProjectName +
                    "'"+ currentTFMText +" имеет версию '" + frameworkVersionComparabilityError.TargetFrameworkVersion + "', в то время как максимально допустимой для него версией является '" +
                    frameworkVersionComparabilityError.MaxFrameworkVersion + "' (" + ruleLevel + ")." + outputPlaceTransfer + 
                    "Измените версию проекта или модифицируйте конфигурацию Config-файла";

                problemsStringList.Add(new ProblemString(errorText, documentName));
            }

            return problemsStringList;
        }


        public static List<ProblemString> ConvertCurrentWarningsToStringFormat(
            RefDepGuardWarnings refDepGuardWarnings, ConfigFilesData configFilesData, bool isLoadToConsole)
        {
            var problemsStringList = new List<ProblemString>();
            var outputPlacePrefix = isLoadToConsole ? "    - " : "RefDepGuard ";
            var outputPlaceTransfer = isLoadToConsole ? "\r\n" : " ";

            foreach (ReferenceMatchWarning referenceMatchWarning in refDepGuardWarnings.RefsMatchWarningList)
            {
                string projectName = (referenceMatchWarning.ProjectName != "") ? "' проекта '" + referenceMatchWarning.ProjectName : "";
                string highReferenceLevelText = "";
                string lowReferenceLevelText = (referenceMatchWarning.LowReferenceLevel == ProblemLevel.Solution) ? "уровня Solution" : "";
                string referenceTypeText = referenceMatchWarning.IsReferenceStraight ?
                    (referenceMatchWarning.IsHighLevelReq ? " является обязательным и" : " является недопустимым и") :
                    (referenceMatchWarning.IsHighLevelReq ? " является недопустимым и" : " является обязательным и"); //В противном случае рассматривается cross match errors, а значит они имеют тип рефа, противиположный более "верхнему" правилу
                string warningDescription = referenceMatchWarning.IsReferenceStraight ?
                    " дубирует правило с одноимённым референсом " : " противоречит правилу с одноимённым референсом ";
                string warningAction = referenceMatchWarning.IsReferenceStraight ? ("." + outputPlaceTransfer + "Устраните дублирование правила") : 
                    ("." + outputPlaceTransfer + "Устраните противоречие в правиле");
                string documentName = (referenceMatchWarning.HighReferenceLevel == ProblemLevel.Global) ?
                    "global_config_guard.rdg" : configFilesData.SolutionName + "_config_guard.rdg";

                switch (referenceMatchWarning.HighReferenceLevel)
                {
                    case ProblemLevel.Solution: highReferenceLevelText = "уровня Solution"; break;
                    case ProblemLevel.Global: highReferenceLevelText = "глобального уровня"; break;
                }
                string warningText = outputPlacePrefix + "Match Warning: референс '" + referenceMatchWarning.ReferenceName + projectName + "' " + lowReferenceLevelText + 
                    referenceTypeText + warningDescription + highReferenceLevelText + warningAction;

                problemsStringList.Add(new ProblemString(warningText, documentName));
            }

            foreach (var currentProjectNotFoundWarning in refDepGuardWarnings.ProjectNotFoundWarningList)
            {
                string ruleLevel = "уровня Solution";
                string documentName = (currentProjectNotFoundWarning.WarningLevel == ProblemLevel.Global) ?
                    "global_config_guard.rdg" : configFilesData.SolutionName + "_config_guard.rdg";

                switch (currentProjectNotFoundWarning.WarningLevel)
                {
                    case ProblemLevel.Global: ruleLevel = "глобального уровня"; break;
                    case ProblemLevel.Project: ruleLevel = "в проекте '" + currentProjectNotFoundWarning.ProjName + "' "; break;
                }
                string warningText = outputPlacePrefix + "Project not found warning: проект '" + currentProjectNotFoundWarning.ReferenceName + 
                    "', указанный в референс-правиле " + ruleLevel + ", не обнаружен в solution" + "." + outputPlaceTransfer + 
                    "Проверьте правило на корректность написания в config-файле";

                problemsStringList.Add(new ProblemString(warningText, documentName));
            }

            foreach (var currentProjectMatchWarning in refDepGuardWarnings.ProjectMatchWarningList)
            {
                string placeWhereProjectNotFound = currentProjectMatchWarning.IsNoProjectInConfigFile ? "config-файле" : "solution";

                string warningText = outputPlacePrefix + "Project match warning: проект '" + currentProjectMatchWarning.ProjName + "' не обнаружен в " + 
                    placeWhereProjectNotFound + "." + outputPlaceTransfer + "Проверьте проект на корректность написания его имени в config-файле";

                problemsStringList.Add(new ProblemString(warningText, configFilesData.SolutionName + "_config_guard.rdg"));
            }

            foreach (MaxFrameworkVersionDeviantValueWarning maxFrameworkVersionDeviantValue in refDepGuardWarnings.MaxFrameworkVersionDeviantValueWarningList)
            {
                string relevantProjectName = "глобального Config-файла";
                string warningText = "";
                string documentName = (maxFrameworkVersionDeviantValue.WarningLevel == ProblemLevel.Global) ?
                    "global_config_guard.rdg" : configFilesData.SolutionName + "_config_guard.rdg";

                switch (maxFrameworkVersionDeviantValue.WarningLevel)
                {
                    case ProblemLevel.Solution: relevantProjectName = "Config-файла уровня Solution"; break;
                    case ProblemLevel.Project: relevantProjectName = "проекта '" + maxFrameworkVersionDeviantValue.WarningRelevantProjectName + "'"; break;
                }
                warningText = outputPlacePrefix + "framework_max_version deviant value warning: параметр 'framework_max_version' " + relevantProjectName + 
                    " содержит значение '" + maxFrameworkVersionDeviantValue.DeviantValue +
                    "', а должен содержать значение с точкой (формата 'x.x')." + outputPlaceTransfer + "Приведите значение к корректному формату";

                problemsStringList.Add(new ProblemString(warningText, documentName));
            }

            foreach (MaxFrameworkVersionConflictWarning maxFrameworkVersionConflictValue in refDepGuardWarnings.MaxFrameworkVersionConflictWarningsList)
            { //Выделить в error случаи, когда рассматриваюся версии одного уровня? (случаи с all)
                string documentName = configFilesData.SolutionName + "_config_guard.rdg";
                string highErrorLevelText = "";
                string lowErrorLevelText = "";

                if (maxFrameworkVersionConflictValue.HighWarnLevel == maxFrameworkVersionConflictValue.LowWarnLevel)
                    highErrorLevelText = ", указанное в супертипе 'all' на том же уровне";
                else
                {
                    switch (maxFrameworkVersionConflictValue.HighWarnLevel)
                    {
                        case ProblemLevel.Global: highErrorLevelText = "глобального уровня"; break;
                        case ProblemLevel.Solution: highErrorLevelText = "уровня Solution"; break;
                    }
                }

                switch (maxFrameworkVersionConflictValue.LowWarnLevel)
                {
                    case ProblemLevel.Solution: lowErrorLevelText = "уровня Solution"; break;
                    case ProblemLevel.Project: lowErrorLevelText = "в проекте '" + maxFrameworkVersionConflictValue.WarningRelevantProjectName + "'"; break;
                }

                string warningText = outputPlacePrefix + "framework_max_version conflict warning: значение '" + maxFrameworkVersionConflictValue.LowLevelMaxFrameVersion
                    + "' параметра 'framework_max_version' " + lowErrorLevelText + " превосходит значение '" + maxFrameworkVersionConflictValue.HighLevelMaxFrameVersion
                    + "' одноимённого параметра " + highErrorLevelText + "." + outputPlaceTransfer + "Устраните противоречие";

                problemsStringList.Add(new ProblemString(warningText, documentName));
            }

            foreach (MaxFrameworkVersionReferenceConflictWarning maxFrameworkVersionReferenceConflictWarning in refDepGuardWarnings.MaxFrameworkVersionReferenceConflictWarningsList)
            {
                string errorCause = (maxFrameworkVersionReferenceConflictWarning.IsOneProjectsTypeConflict) ?
                    "большее значение значение параметра 'framework_max_version' " :
                    "несовместимое значение параметра 'framework_max_version' для текущего значения проекта типа 'netstandard' ";
                string warningText = outputPlacePrefix + "framework_max_version reference conflict warning: значение '" + 
                    maxFrameworkVersionReferenceConflictWarning.ProjFrameworkVersion + "' параметра 'framework_max_version' проекта " + 
                    maxFrameworkVersionReferenceConflictWarning.ProjName + " приводит к потенциальному конфликту версий TargetFramework" +
                    ", так как имеется референс на проект, имеющий " + errorCause + "(проект: " + maxFrameworkVersionReferenceConflictWarning.RefName
                    + ", Версия: " + maxFrameworkVersionReferenceConflictWarning.RefFrameworkVersion + ")." + outputPlaceTransfer + "Устраните противоречие";
                string documentName = configFilesData.SolutionName + "_config_guard.rdg";

                problemsStringList.Add(new ProblemString(warningText, documentName));
            }

            foreach (MaxFrameworkVersionTFMNotFoundWarning maxFrameworkVersionTFMNotFoundWarning in refDepGuardWarnings.MaxFrameworkVersionTFMNotFoundWarningList)
            {
                string warningLevel = "глобальный уровень";
                string documentName = (maxFrameworkVersionTFMNotFoundWarning.WarningLevel == ProblemLevel.Global) ?
                    "global_config_guard.rdg" : configFilesData.SolutionName + "_config_guard.rdg";

                switch (maxFrameworkVersionTFMNotFoundWarning.WarningLevel)
                {
                    case ProblemLevel.Solution: warningLevel = "уровень решения"; break;
                    case ProblemLevel.Project: warningLevel = "уровень проекта '" + maxFrameworkVersionTFMNotFoundWarning.ProjName + "'"; break;
                }
                string warningText = outputPlacePrefix + "framework_max_version TFM not found warning: Не найден TargetFramework, имеющий значение '" +
                    maxFrameworkVersionTFMNotFoundWarning.TFMName + 
                    "' (" + warningLevel + ")." + outputPlaceTransfer + "Проверьте указанную строку max_framework_version на предмет соответствия существующим TFM";

                problemsStringList.Add(new ProblemString(warningText, documentName));
            }

            foreach(MaxFrameworkIllegalTemplateUsageWarning maxFrameworkIllegalTemplateUsageWarning in refDepGuardWarnings.MaxFrameworkIllegalTemplateUsageWarningList)
            {
                string warningLevel = maxFrameworkIllegalTemplateUsageWarning.ProblemLevelInfo == ProblemLevel.Global ? "глобального уровня" : "уровня решения";

                string documentName = (maxFrameworkIllegalTemplateUsageWarning.ProblemLevelInfo == ProblemLevel.Global) ?
                    "global_config_guard.rdg" : configFilesData.SolutionName + "_config_guard.rdg";

                string warningText = outputPlacePrefix + "framework_max_version illegal template usage warning: в параметре 'framework_max_version' " + warningLevel +
                    " указан TFM, который не встречается ни в одном из TargetFramework проектов этого решения" + outputPlaceTransfer +
                    "Проверьте указанную строку max_framework_version на предмет соответствия релевантных решению TFM";

                problemsStringList.Add(new ProblemString(warningText, documentName));
            }

            foreach (var projName in refDepGuardWarnings.UntypedWarningsList)
            {
                string currentText = outputPlacePrefix + "Warning: Не получилось произвести проверку версии 'TargetFramework' для проекта '" + projName +
                    "', так как программе не удалось получить из .csproj файла корректное значение для этого свойства." +
                    outputPlaceTransfer + "Проверьте, что проект имеет корректную версию 'TargetFramework'";

                problemsStringList.Add(new ProblemString(currentText, ""));
            }

            foreach (var projKeyValuePair in refDepGuardWarnings.DetectedTransitRefsDict)
            {
                string projName = projKeyValuePair.Key;
                List<string> detectedTransitRefsList = projKeyValuePair.Value;

                string currentText = outputPlacePrefix + "Transit references warning: у проекта '" + projName + "' обнаружены следующие транзитивные референсы: ";

                foreach (var refName in detectedTransitRefsList)
                {
                    currentText += "'" + refName + "', ";
                }
                currentText = currentText.Remove(currentText.Length - 2);

                problemsStringList.Add(new ProblemString(currentText, ""));
            }

            return problemsStringList;
        }
    }
}