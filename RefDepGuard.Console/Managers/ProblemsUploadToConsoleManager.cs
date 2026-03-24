using RefDepGuard.CheckRules.Models;
using RefDepGuard.CheckRules.Models.ConfigFile;
using RefDepGuard.CheckRules.Models.ExportModels;
using RefDepGuard.CheckRules.Models.FrameworkVersion.Errors;
using RefDepGuard.CheckRules.Models.FrameworkVersion.Warnings;
using RefDepGuard.CheckRules.Models.FrameworkVersion.Warnings.Conflicts;
using RefDepGuard.CheckRules.Models.RefDepGuard;
using RefDepGuard.CheckRules.Models.Reference.Errors;
using RefDepGuard.CheckRules.Models.Reference.Warnings;

namespace RefDepGuard.Console.Managers
{
    public class ProblemsUploadToConsoleManager
    {
        public static void UploadCheckRuleProblems(RefDepGuardFindedProblems refDepGuardFindedProblems, ConfigFilesData configFilesData)
        {
            RefDepGuardErrors refDepGuardErrors = refDepGuardFindedProblems.RefDepGuardErrors;
            RefDepGuardWarnings refDepGuardWarnings = refDepGuardFindedProblems.RefDepGuardWarnings;


            System.Console.WriteLine("\r\n    -> ОШИБКИ:\r\n");

            foreach (ReferenceError error in refDepGuardErrors.RefsErrorList)
            {
                string referenceTypeText = error.IsReferenceRequired ? "Отсутсвует обязательный" : "Присутствует недопустимый";
                string actionForUser = error.IsReferenceRequired ? "Добавьте" : "Удалите";
                string referenceLevelText = "";
                string documentName = error.ErrorRelevantProjectName + ".csproj";

                switch (error.CurrentRuleLevel)
                {
                    case ProblemLevel.Solution: referenceLevelText = "уровня Solution"; break;
                    case ProblemLevel.Global: referenceLevelText = "глобального уровня"; break;
                }

                string errorText = "    - Reference error: " + referenceTypeText + " референс " + referenceLevelText + "'" + error.ReferenceName +
                    "' для проекта '" + error.ErrorRelevantProjectName + "'.\r\n" + actionForUser + " его через обозреватель решений";
                System.Console.WriteLine(errorText + " (Файл: " + documentName + ")\r\n");
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
                    case ProblemLevel.Solution: referenceLevelText = "уровня Solution"; break;
                    case ProblemLevel.Global: referenceLevelText = "глобального уровня"; break;
                }

                string errorText = "    - Match error: референс '" + referenceMatchError.ReferenceName + projectName + "' " + referenceLevelText +
                    matchErrorDescription + ".\r\nУстраните противоречие в правиле";

                System.Console.WriteLine(errorText + " (Файл: " + documentName + ")\r\n");
            }

            foreach (ConfigFilePropertyNullError configFilePropertyNullError in refDepGuardErrors.ConfigPropertyNullErrorList)
            {
                string relevantProjectName = (configFilePropertyNullError.ErrorRelevantProjectName != "") ?
                    " для проекта '" + configFilePropertyNullError.ErrorRelevantProjectName + "'" : "";
                string errorText = "    - Null property error: Config-файл не содержит свойство '" + configFilePropertyNullError.PropertyName + "'" +
                    relevantProjectName + ".\r\nПроверьте его на предмет отсутствия синтаксических ошибок и соответствия шаблону файла конфигурации";
                string documentName = configFilePropertyNullError.IsGlobal ? "global_config_guard.rdg" : configFilesData.SolutionName + "_config_guard.rdg";

                System.Console.WriteLine(errorText + " (Файл: " + documentName + ")\r\n");
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
                string errorText = "    - framework_max_version deviant value error: параметр 'framework_max_version' " + globalPrefix + "Config-файла " +
                    relevantProjectName + errorType + ".\r\nПроверьте его на предмет отсутствия синтаксических ошибок и соответствия шаблону файла конфигурации";

                System.Console.WriteLine(errorText + " (Файл: " + documentName + ")\r\n");
            }

            foreach (MaxFrameworkVersionIllegalTemplateUsageError maxFrameworkVersionIllegalTemplateUsageError in refDepGuardErrors.MaxFrameworkVersionIllegalTemplateUsageErrorList)
            {
                string errorDescr = maxFrameworkVersionIllegalTemplateUsageError.IsIllegalTFMUsageError ?
                    "а попытка задать ограничение для TFM, не обнаруженного в текущем проекте.\r\nПроверьте указанную строку max_framework_version на предмет соответствия релевантных проекту TFM" :
                    "о использование шаблона задания нескольких ограничений, что недопустимо для этого проекта (в проекте заявлен только один тип TFM).\r\nИсправьте значение параметра на допустимое";

                string errorText = "    - framework_max_version illegal template usage error: в параметре 'framework_max_version' проекта '" +
                    maxFrameworkVersionIllegalTemplateUsageError.ProjName +
                    "' обнаружен" + errorDescr;

                string documentName = configFilesData.SolutionName + "_config_guard.rdg";

                System.Console.WriteLine(errorText + " (Файл: " + documentName + ")\r\n");
            }

            foreach (FrameworkVersionComparabilityError frameworkVersionComparabilityError in refDepGuardErrors.FrameworkVersionComparabilityErrorList)
            {
                string ruleLevel = "ограничение глобального уровня";
                string documentName = (frameworkVersionComparabilityError.ErrorLevel == ProblemLevel.Global) ?
                    configFilesData.SolutionName + "_config_guard.rdg" : "global_config_guard.rdg";

                switch (frameworkVersionComparabilityError.ErrorLevel)
                {
                    case ProblemLevel.Solution: ruleLevel = "ограничение уровня решения"; break;
                    case ProblemLevel.Project: ruleLevel = "ограничение уровня проекта"; break;
                }
                string errorText = "    - Framework version comparability error: 'TargetFrameworkVersion' проекта '" + frameworkVersionComparabilityError.ErrorRelevantProjectName +
                    "' имеет версию '" + frameworkVersionComparabilityError.TargetFrameworkVersion + "', в то время как максимально допустимой для него версией является '" +
                    frameworkVersionComparabilityError.MaxFrameworkVersion + "' (" + ruleLevel + ").\r\nИзмените версию проекта или модифицируйте конфигурацию Config-файла";

                System.Console.WriteLine(errorText + " (Файл: " + documentName + ")\r\n");
            }


            System.Console.WriteLine("\r\n    -> ПРЕДУПРЕЖДЕНИЯ:\r\n");

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
                string warningAction = referenceMatchWarning.IsReferenceStraight ? ".\r\nУстраните дублирование правила" : ".\r\nУстраните противоречие в правиле";
                string documentName = (referenceMatchWarning.HighReferenceLevel == ProblemLevel.Global) ?
                    "global_config_guard.rdg" : configFilesData.SolutionName + "_config_guard.rdg";

                switch (referenceMatchWarning.HighReferenceLevel)
                {
                    case ProblemLevel.Solution: highReferenceLevelText = "уровня Solution"; break;
                    case ProblemLevel.Global: highReferenceLevelText = "глобального уровня"; break;
                }
                string warningText = "    - Match Warning: референс '" + referenceMatchWarning.ReferenceName + projectName + "' " + lowReferenceLevelText + referenceTypeText +
                    warningDescription + highReferenceLevelText + warningAction;

                System.Console.WriteLine(warningText + " (Файл: " + documentName + ")\r\n");
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
                string warningText = "    - Project not found warning: проект '" + currentProjectNotFoundWarning.ReferenceName + "', указанный в референс-правиле " +
                    ruleLevel + ", не обнаружен в solution" + ".\r\nПроверьте правило на корректность написания в config-файле";

                System.Console.WriteLine(warningText + " (Файл: " + documentName + ")\r\n");
            }

            foreach (var currentProjectMatchWarning in refDepGuardWarnings.ProjectMatchWarningList)
            {
                string placeWhereProjectNotFound = currentProjectMatchWarning.IsNoProjectInConfigFile ? "config-файле" : "solution";

                string warningText = "    - Project match warning: проект '" + currentProjectMatchWarning.ProjName + "' не обнаружен в " + placeWhereProjectNotFound +
                    ".\r\nПроверьте проект на корректность написания его имени в config-файле";

                System.Console.WriteLine(warningText + " (Файл: " + configFilesData.SolutionName + "_config_guard.rdg" + ")\r\n");
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
                warningText = "    - framework_max_version deviant value warning: параметр 'framework_max_version' " + relevantProjectName + " содержит значение '" +
                    maxFrameworkVersionDeviantValue.DeviantValue + "', а должен содержать значение с точкой (формата 'x.x').\r\nПриведите значение к корректному формату";

                System.Console.WriteLine(warningText + " (Файл: " + documentName + ")\r\n");
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

                string warningText = "    - framework_max_version conflict warning: значение '" + maxFrameworkVersionConflictValue.LowLevelMaxFrameVersion
                    + "' параметра 'framework_max_version' " + lowErrorLevelText + " превосходит значение '" + maxFrameworkVersionConflictValue.HighLevelMaxFrameVersion
                    + "' одноимённого параметра " + highErrorLevelText + ".\r\nУстраните противоречие";

                System.Console.WriteLine(warningText + " (Файл: " + documentName + ")\r\n");
            }

            foreach (MaxFrameworkVersionReferenceConflictWarning maxFrameworkVersionReferenceConflictWarning in refDepGuardWarnings.MaxFrameworkVersionReferenceConflictWarningsList)
            {
                string errorCause = (maxFrameworkVersionReferenceConflictWarning.IsOneProjectsTypeConflict) ?
                    "большее значение значение параметра 'framework_max_version' " :
                    "несовместимое значение параметра 'framework_max_version' для текущего значения проекта типа 'netstandard' ";
                string warningText = "    - framework_max_version reference conflict warning: значение '" + maxFrameworkVersionReferenceConflictWarning.ProjFrameworkVersion
                    + "' параметра 'framework_max_version' проекта " + maxFrameworkVersionReferenceConflictWarning.ProjName + " приводит к потенциальному конфликту версий TargetFramework" +
                    ", так как имеется референс на проект, имеющий " + errorCause + "(проект: " + maxFrameworkVersionReferenceConflictWarning.RefName
                    + ", Версия: " + maxFrameworkVersionReferenceConflictWarning.RefFrameworkVersion + ").\r\nУстраните противоречие";
                string documentName = configFilesData.SolutionName + "_config_guard.rdg";

                System.Console.WriteLine(warningText + " (Файл: " + documentName + ")\r\n");
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
                string warningText = "    - framework_max_version TFM not found warning: Не найден TargetFramework, имеющий значение '" +
                    maxFrameworkVersionTFMNotFoundWarning.TFMName + "' (" + warningLevel + ").\r\nПроверьте указанную строку max_framework_version на предмет соответствия существующим TFM";

                System.Console.WriteLine(warningText + " (Файл: " + documentName + ")\r\n");
            }

            foreach (var projName in refDepGuardWarnings.UntypedWarningsList)
            {
                string currentText = "    - Warning: Не получилось произвести проверку версии 'TargetFramework' для проекта '" + projName +
                    "', так как программе не удалось получить из .csproj файла корректное значение для этого свойства.\r\nПроверьте, что проект имеет корректную версию 'TargetFramework'";

                System.Console.WriteLine(currentText + " (Файл: " + configFilesData.SolutionName + ".csproj" + ")\r\n");
            }

            foreach (var projKeyValuePair in refDepGuardWarnings.DetectedTransitRefsDict)
            {
                string projName = projKeyValuePair.Key;
                List<string> detectedTransitRefsList = projKeyValuePair.Value;

                string currentText = "    - Transit references warning: у проекта '" + projName + "' обнаружены следующие транзитивные референсы: ";

                foreach (var refName in detectedTransitRefsList)
                {
                    currentText += "'" + refName + "', ";
                }

                currentText = currentText.Remove(currentText.Length - 2);

                System.Console.WriteLine(currentText + " (Файл: " + configFilesData.SolutionName + ".csproj" + ")\r\n");
            }
        }
        public static void UploadRefsNotFoundError()
        {
            var currentText = "    - Error: Не получилось проверить соответствие референсов правилам, так как они не были обнаружены на момент фиксации" +
                "состояния решения.\r\nПроверьте, что в solution действительно содержатся референсы между проектами и произведите проверку вручную или " +
                "автоматически вместе со сборкой";

            System.Console.WriteLine(currentText);
        }

        public static void UploadConfigFileSyntaxError(bool isGlobal)
        {
            var globalPrefix = isGlobal ? "глобального" : "";
            var solFilePrefix = isGlobal ? "" : " текущего решения";

            var currentText = "\r\n    - Error: Не получилось спарсить данные из " + globalPrefix + " файла конфигурации"+ solFilePrefix + 
                ".\r\nПроверьте файл на отсутствие синтаксических ошибок";

            System.Console.WriteLine(currentText);
        }

        public static void UploadConfigFileNotFoundError(bool isGlobal)
        {
            var globalPrefix = isGlobal ? "глобальный" : "";
            var solFilePrefix = isGlobal ? "" : " текущего решения";

            var currentText = "\r\n    - Error: Не получилось найти " + globalPrefix + " файл конфигурации" + solFilePrefix +
                " в корневой папке.\r\nПроверьте корневую папку на наличие этого файла и корректность его названия согласно USER_GUIDE";

            System.Console.WriteLine(currentText);
        }
    }
}