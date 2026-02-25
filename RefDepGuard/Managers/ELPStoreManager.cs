using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using RefDepGuard.Data;
using RefDepGuard.Data.ConfigFile;
using RefDepGuard.Data.FrameworkVersion;
using RefDepGuard.Data.Reference;
using RefDepGuard.Models.FrameworkVersion;

namespace RefDepGuard.Managers.CheckRules
{
    public class ELPStoreManager
    {
        public static void StoreErrorListProviderByValues(RefDepGuardFindedProblems refDepGuardFindedProblems, ConfigFilesData configFilesData, ErrorListProvider errorListProvider)
        {
            RefDepGuardErrors refDepGuardErrors = refDepGuardFindedProblems.RefDepGuardErrors;
            RefDepGuardWarnings refDepGuardWarnings = refDepGuardFindedProblems.RefDepGuardWarnings;

            ClearErrorListProvider(errorListProvider);

            //Errors
            foreach (ReferenceError error in refDepGuardErrors.RefsErrorList)
            {
                string referenceTypeText = error.IsReferenceRequired ? "Отсутсвует обязательный" : "Присутствует недопустимый";
                string actionForUser = error.IsReferenceRequired ? "Добавьте" : "Удалите";
                string referenceLevelText = "";
                string documentName = error.ErrorRelevantProjectName + ".csproj";

                switch (error.CurrentReferenceLevel)
                {
                    case ProblemLevel.Solution: referenceLevelText = "уровня Solution"; break;
                    case ProblemLevel.Global: referenceLevelText = "глобального уровня"; break;
                }

                string errorText = "RefDepGuard Reference error: " + referenceTypeText + " референс " + referenceLevelText + " '" + error.ReferenceName + "' для проекта '" + error.ErrorRelevantProjectName + "'. " + actionForUser + " его через обозреватель решений";

                StoreErrorTask(errorListProvider, errorText, documentName, TaskErrorCategory.Error);
            }

            foreach (ReferenceMatchError referenceMatchError in refDepGuardErrors.RefsMatchErrorList)
            {
                string projectName = (referenceMatchError.ProjectName != "") ? "' проекта '" + referenceMatchError.ProjectName : "";
                string referenceLevelText = "";
                string matchErrorDescription = referenceMatchError.IsProjNameMatchError ? 
                    " совпадает с именем проекта" : " одновременно заявлен как обязательный и недопустимый";
                string errorText = "";
                string documentName = (referenceMatchError.ReferenceLevelValue == ProblemLevel.Global) ? 
                    "global_config_guard.rdg" : configFilesData.solutionName + "_config_guard.rdg";

                switch (referenceMatchError.ReferenceLevelValue)
                {
                    case ProblemLevel.Solution: referenceLevelText = "уровня Solution"; break;
                    case ProblemLevel.Global: referenceLevelText = "глобального уровня"; break;
                }

                errorText = "RefDepGuard Match error: референс '" + referenceMatchError.ReferenceName + projectName + "' " + referenceLevelText + 
                    matchErrorDescription + ". Устраните противоречие в правиле";

                StoreErrorTask(errorListProvider, errorText, documentName, TaskErrorCategory.Error);
            }

            foreach (ConfigFilePropertyNullError configFilePropertyNullError in refDepGuardErrors.ConfigPropertyNullErrorList)
            {
                string relevantProjectName = (configFilePropertyNullError.ErrorRelevantProjectName != "") ?
                    " для проекта '" + configFilePropertyNullError.ErrorRelevantProjectName + "'" : "";
                string errorText = "RefDepGuard Null property error: Config-файл не содержит свойство '" + configFilePropertyNullError.PropertyName + "'" + 
                    relevantProjectName + ". Проверьте его на предмет отсутствия синтаксических ошибок и соответствия шаблону файла конфигурации";
                string documentName = configFilePropertyNullError.IsGlobal ? "global_config_guard.rdg" : configFilesData.solutionName + "_config_guard.rdg";
                
                StoreErrorTask(errorListProvider, errorText, documentName, TaskErrorCategory.Error);
            }

            foreach (MaxFrameworkVersionDeviantValueError maxFrameworkVersionDeviantValue in refDepGuardErrors.MaxFrameworkVersionDeviantValueList)
            {
                string relevantProjectName = "";
                string globalPrefix = "";
                string errorType = maxFrameworkVersionDeviantValue.IsProjectTypeCopyError ? 
                    " содержит один и тот же тип проекта в шаблоне более одного раза" : " содержит некорректную запись своего значения";
                string errorText = "";
                string documentName = (maxFrameworkVersionDeviantValue.ErrorLevel == ProblemLevel.Global) ? 
                    "global_config_guard.rdg" : configFilesData.solutionName + "_config_guard.rdg";

                switch (maxFrameworkVersionDeviantValue.ErrorLevel)
                {
                    case ProblemLevel.Global: globalPrefix = "глобального "; break;
                    case ProblemLevel.Solution: relevantProjectName = "уровня Solution"; break;
                    case ProblemLevel.Project: relevantProjectName = "проекта '" + maxFrameworkVersionDeviantValue.ErrorRelevantProjectName + "'"; break;
                }
                errorText = "RefDepGuard framework_max_version deviant value error: параметр 'framework_max_version' " + globalPrefix + "Config-файла " + 
                    relevantProjectName + errorType + ". Проверьте его на предмет отсутствия синтаксических ошибок и соответствия шаблону файла конфигурации";

                StoreErrorTask(errorListProvider, errorText, documentName, TaskErrorCategory.Error);
            }

            foreach (MaxFrameworkVersionIllegalTemplateUsageError maxFrameworkVersionIllegalTemplateUsageError in refDepGuardErrors.MaxFrameworkVersionIllegalTemplateUsageErrorList)
            {
                string errorText = "RefDepGuard framework_max_version illegal template usage error: в параметре 'framework_max_version' проекта '" +
                    maxFrameworkVersionIllegalTemplateUsageError.ProjName + 
                    "' обнаружено использование шаблона задания нескольких ограничений, что недопустимо для этого проекта (в проекте заявлен только один тип TFM). " +
                    "Исправьте значение параметра на допустимое";
                string documentName = configFilesData.solutionName + "_config_guard.rdg";

                StoreErrorTask(errorListProvider, errorText, documentName, TaskErrorCategory.Error);
            }

            foreach (FrameworkVersionComparabilityError frameworkVersionComparabilityError in refDepGuardErrors.FrameworkVersionComparabilityErrorList)
            {
                string ruleLevel = "ограничение глобального уровня";
                string errorText = "";
                string documentName = (frameworkVersionComparabilityError.ErrorLevel == ProblemLevel.Global) ? 
                    configFilesData.solutionName + "_config_guard.rdg" : "global_config_guard.rdg";

                switch (frameworkVersionComparabilityError.ErrorLevel)
                {
                    case ProblemLevel.Solution: ruleLevel = "ограничение уровня решения"; break;
                    case ProblemLevel.Project: ruleLevel = "ограничение уровня проекта"; break;
                }
                errorText = "RefDepGuard Framework version comparability error: 'TargetFrameworkVersion' проекта '" + frameworkVersionComparabilityError.ErrorRelevantProjectName + 
                    "' имеет версию '" + frameworkVersionComparabilityError.TargetFrameworkVersion + "', в то время как максимально допустимой для него версией является '" +
                    frameworkVersionComparabilityError.MaxFrameworkVersion + "' (" + ruleLevel + "). Измените версию проекта или модифицируйте конфигурацию Config-файла";

                StoreErrorTask(errorListProvider, errorText, documentName, TaskErrorCategory.Error);
            }

            //Warnings
            foreach (ReferenceMatchWarning referenceMatchWarning in refDepGuardWarnings.RefsMatchWarningList)
            {
                string projectName = (referenceMatchWarning.ProjectName != "") ? "' проекта '" + referenceMatchWarning.ProjectName : "";
                string highReferenceLevelText = "";
                string lowReferenceLevelText = (referenceMatchWarning.LowReferenceLevel == ProblemLevel.Solution) ? "уровня Solution" : "";
                string referenceTypeText = referenceMatchWarning.IsReferenceStraight ? 
                    (referenceMatchWarning.IsHighLevelReq ? " является обязательным и" : " является недопустимым и") : 
                    (referenceMatchWarning.IsHighLevelReq ? " является недопустимым и" : " является обязательным и"); //В противном случае рассматривается cross match errors, а значит они имеют тип рефа, противиположный более "верхнему" правилу
                string warningText = "";
                string warningDescription = referenceMatchWarning.IsReferenceStraight ? 
                    " дубирует правило с одноимённым референсом " : " противоречит правилу с одноимённым референсом ";
                string warningAction = referenceMatchWarning.IsReferenceStraight ? ". Устраните дублирование правила" : ". Устраните противоречие в правиле";
                string documentName = (referenceMatchWarning.HighReferenceLevel == ProblemLevel.Global) ?
                    "global_config_guard.rdg" : configFilesData.solutionName + "_config_guard.rdg";

                switch (referenceMatchWarning.HighReferenceLevel)
                {
                    case ProblemLevel.Solution: highReferenceLevelText = "уровня Solution"; break;
                    case ProblemLevel.Global: highReferenceLevelText = "глобального уровня"; break;
                }
                warningText = "RefDepGuard Match Warning: референс '" + referenceMatchWarning.ReferenceName + projectName + "' " + lowReferenceLevelText + referenceTypeText +
                    warningDescription + highReferenceLevelText + warningAction;

                StoreErrorTask(errorListProvider, warningText, documentName, TaskErrorCategory.Warning);
            }

            foreach (var currentProjectNotFoundWarning in refDepGuardWarnings.ProjectNotFoundWarningList)
            {
                string ruleLevel = "уровня Solution";
                string warningText = "";
                string documentName = (currentProjectNotFoundWarning.WarningLevel == ProblemLevel.Global) ? 
                    "global_config_guard.rdg" : configFilesData.solutionName + "_config_guard.rdg";

                switch (currentProjectNotFoundWarning.WarningLevel)
                {
                    case ProblemLevel.Global: ruleLevel = "глобального уровня"; break;
                    case ProblemLevel.Project: ruleLevel = "в проекте '" + currentProjectNotFoundWarning.ProjName + "' "; break;
                }
                warningText = "RefDepGuard Project not found warning: проект '" + currentProjectNotFoundWarning.ReferenceName + "', указанный в референс-правиле " + ruleLevel + ", не обнаружен в solution" + ". Проверьте правило на корректность написания в config-файле";

                StoreErrorTask(errorListProvider, warningText, documentName, TaskErrorCategory.Warning);
            }

            foreach (var currentProjectMatchWarning in refDepGuardWarnings.ProjectMatchWarningList)
            {
                string placeWhereProjectNotFound = currentProjectMatchWarning.IsNoProjectInConfigFile ? "config-файле" : "solution";

                string warningText = "RefDepGuard Project match warning: проект '" + currentProjectMatchWarning.ProjName + "' не обнаружен в " + placeWhereProjectNotFound +
                    ". Проверьте проект на корректность написания его имени в config-файле";

                StoreErrorTask(errorListProvider, warningText, configFilesData.solutionName + "_config_guard.rdg", TaskErrorCategory.Warning);
            }

            foreach (MaxFrameworkVersionDeviantValueWarning maxFrameworkVersionDeviantValue in refDepGuardWarnings.MaxFrameworkVersionDeviantValueWarningList)
            {
                string relevantProjectName = "глобального Config-файла";
                string warningText = "";
                string documentName = (maxFrameworkVersionDeviantValue.WarningLevel == ProblemLevel.Global) ? 
                    "global_config_guard.rdg" : configFilesData.solutionName + "_config_guard.rdg";

                switch (maxFrameworkVersionDeviantValue.WarningLevel)
                {
                    case ProblemLevel.Solution: relevantProjectName = "Config-файла уровня Solution"; break;
                    case ProblemLevel.Project: relevantProjectName = "проекта '" + maxFrameworkVersionDeviantValue.WarningRelevantProjectName + "'"; break;
                }
                warningText = "RefDepGuard framework_max_version deviant value warning: параметр 'framework_max_version' " + relevantProjectName + " содержит значение '"+ 
                    maxFrameworkVersionDeviantValue.DeviantValue +"', а должен содержать значение с точкой (формата 'x.x'). Приведите значение к корректному формату";

                StoreErrorTask(errorListProvider, warningText, documentName, TaskErrorCategory.Warning);
            }

            foreach (MaxFrameworkVersionConflictWarning maxFrameworkVersionConflictValue in refDepGuardWarnings.MaxFrameworkVersionConflictWarningsList)
            { //Выделить в error случаи, когда рассматриваюся версии одного уровня? (случаи с all)
                string documentName = configFilesData.solutionName + "_config_guard.rdg";
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

                string errorText = "RefDepGuard framework_max_version conflict warning: значение '" + maxFrameworkVersionConflictValue.LowLevelMaxFrameVersion
                    + "' параметра 'framework_max_version' " + lowErrorLevelText + " превосходит значение '" + maxFrameworkVersionConflictValue.HighLevelMaxFrameVersion
                    + "' одноимённого параметра " + highErrorLevelText + ". Устраните противоречие";

                StoreErrorTask(errorListProvider, errorText, documentName, TaskErrorCategory.Warning);
            }

            foreach (MaxFrameworkVersionReferenceConflictWarning maxFrameworkVersionReferenceConflictWarning in refDepGuardWarnings.MaxFrameworkVersionReferenceConflictWarningsList)
            {
                string errorCause = (maxFrameworkVersionReferenceConflictWarning.IsOneProjectsTypeConflict) ?
                    "большее значение значение параметра 'framework_max_version' " :
                    "несовместимое значение параметра 'framework_max_version' для проекта типа 'netstandard' ";
                string errorText = "RefDepGuard framework_max_version reference conflict warning: значение '" + maxFrameworkVersionReferenceConflictWarning.ProjFrameworkVersion
                    + "' параметра 'framework_max_version' проекта " + maxFrameworkVersionReferenceConflictWarning.ProjName + " приводит к потенциальному конфликту версий TargetFramework" +
                    ", так как имеется референс на проект, имеющий " + errorCause + "(проект: " + maxFrameworkVersionReferenceConflictWarning.RefName
                    + ", Версия: " + maxFrameworkVersionReferenceConflictWarning.RefFrameworkVersion + "). Устраните противоречие";
                string documentName = configFilesData.solutionName + "_config_guard.rdg";

                StoreErrorTask(errorListProvider, errorText, documentName, TaskErrorCategory.Warning);
            }

            foreach (MaxFrameworkVersionTFMNotFoundWarning maxFrameworkVersionTFMNotFoundWarning in refDepGuardWarnings.MaxFrameworkVersionTFMNotFoundWarningList)
            {
                string warningLevel = "глобальный уровень";
                string warningText = "";
                string documentName = (maxFrameworkVersionTFMNotFoundWarning.WarningLevel == ProblemLevel.Global) ? 
                    "global_config_guard.rdg" : configFilesData.solutionName + "_config_guard.rdg";

                switch (maxFrameworkVersionTFMNotFoundWarning.WarningLevel)
                {
                    case ProblemLevel.Solution: warningLevel = "уровень решения"; break;
                    case ProblemLevel.Project: warningLevel = "уровень проекта '" + maxFrameworkVersionTFMNotFoundWarning.ProjName + "'"; break;
                }
                warningText = "RefDepGuard framework_max_version TFM not found warning: Не найден TargetFramework, имеющий значение '" + 
                    maxFrameworkVersionTFMNotFoundWarning.TFMName +"' ("+ warningLevel +"). Проверьте указанную строку max_framework_version на предмет соответствия существующим TFM";

                StoreErrorTask(errorListProvider, warningText, documentName, TaskErrorCategory.Warning);
            }

            foreach (var projName in refDepGuardWarnings.UntypedWarningsList)
            {
                string currentText = "RefDepGuard warning: Не получилось произвести проверку версии 'TargetFramework' для проекта '" + projName + 
                    "', так как программе не удалось получить из .csproj файла корректное значение для этого свойства. Проверьте, что проект имеет корректную версию 'TargetFramework'";

                StoreErrorTask(errorListProvider, currentText, configFilesData.solutionName + ".csproj", TaskErrorCategory.Warning);
            }

            foreach (var projKeyValuePair in refDepGuardWarnings.DetectedTransitRefsDict)
            {
                string projName = projKeyValuePair.Key;
                List<string> detectedTransitRefsList = projKeyValuePair.Value;

                string currentText = "RefDepGuard Transit references warning: у проекта '" + projName + "' обнаружены следующие транзитивные референсы: ";

                foreach (var refName in detectedTransitRefsList)
                {
                    currentText += "'" + refName + "', ";
                }

                currentText = currentText.Remove(currentText.Length - 2);

                StoreErrorTask(errorListProvider, currentText, configFilesData.solutionName + ".csproj", TaskErrorCategory.Warning);

            }

            if (errorListProvider != null)
                errorListProvider.Show();
        }

        public static void ClearErrorListProvider(ErrorListProvider errorListProvider)
        {
            if (errorListProvider != null)
                errorListProvider.Tasks.Clear();
        }

        public static void ShowNoProblemsFindedMessage(ErrorListProvider errorListProvider)
        {
            var currentText = "RefDepGuard: проблемы не обнаружены";
            StoreErrorTask(errorListProvider, currentText, "", TaskErrorCategory.Message);
            errorListProvider.Show();
        }

        public static void ShowUnsuccessfulCheckingRulesWarning(ErrorListProvider errorListProvider)
        {
            ClearErrorListProvider(errorListProvider);

            var currentText = "RefDepGuard warning: Не получилось проверить соответствие референсов правилам, так как они не были обнаружены на момент фиксации состояния. Проверьте, что в solution действительно содержатся референсы между проектами и произведите проверку вручную или автоматически вместе со сборкой";
            StoreErrorTask(errorListProvider, currentText, "", TaskErrorCategory.Warning);
            errorListProvider.Show();
        }

        public static void ShowUnsuccessfulConfigFileParseWarning(ErrorListProvider errorListProvider, string fileName)
        {
            var currentText = "RefDepGuard warning: Не получилось спарсить данные из " + fileName + ". Правила из этого файла не учтены в проверке";
            StoreErrorTask(errorListProvider, currentText, "", TaskErrorCategory.Warning);
        }

        private static void StoreErrorTask(ErrorListProvider errorListProvider, string currentText, string currentDocument, TaskErrorCategory currentTask)
        {
            ErrorTask errorTask = new ErrorTask
            {
                Category = TaskCategory.User,
                ErrorCategory = currentTask,
                Document = currentDocument,
                Text = currentText
            };

            errorListProvider.Tasks.Add(errorTask);
        }
    }
}
