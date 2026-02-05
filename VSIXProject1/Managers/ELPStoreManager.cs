using Microsoft.VisualStudio.Shell;
using VSIXProject1.Data;
using VSIXProject1.Data.ConfigFile;
using VSIXProject1.Data.FrameworkVersion;
using VSIXProject1.Data.Reference;
using VSIXProject1.Models.FrameworkVersion;

namespace VSIXProject1.Managers.CheckRules
{
    public class ELPStoreManager
    {
        public static void StoreErrorListProviderByValues(RefDepGuardFindedProblems refDepGuardFindedProblems, ConfigFilesData configFilesData, ErrorListProvider errorListProvider)
        {
            RefDepGuardErrors refDepGuardErrors = refDepGuardFindedProblems.RefDepGuardErrors;
            RefDepGuardWarnings refDepGuardWarnings = refDepGuardFindedProblems.RefDepGuardWarnings;

            ClearErrorListProvider(errorListProvider);

            foreach (var projName in refDepGuardWarnings.UntypedWarningsList)
            {
                string currentText = "RefDepGuard warning: Не получилось произвести проверку версии 'TargetFramework' для проекта '" + projName + "', так как программе не удалось получить из .csproj файла корректное значение для этого свойства. Проверьте, что проект имеет корректную версию 'TargetFramework'";

                StoreErrorTask(errorListProvider, currentText, configFilesData.solutionName + ".csproj", TaskErrorCategory.Warning);
            }

            foreach(var currentProjectMatchWarning in refDepGuardWarnings.ProjectMatchWarningList)
            {
                string placeWhereProjectNotFound = "solution";

                if (currentProjectMatchWarning.IsNoProjectInConfigFile)
                    placeWhereProjectNotFound = "config-файле";

                string currentText = "RefDepGuard Project match warning: проект '" + currentProjectMatchWarning.ProjName + "' не обнаружен в " + placeWhereProjectNotFound + ". Проверьте проект на корректность написания его имени в config-файле";

                StoreErrorTask(errorListProvider, currentText, configFilesData.solutionName + "_config_guard.rdg", TaskErrorCategory.Warning);
            }

            foreach(var currentProjectNotFoundWarning in refDepGuardWarnings.ProjectNotFoundWarningList)
            {
                string documentName = configFilesData.solutionName + "_config_guard.rdg";
                string ruleLevel = "уровня Solution";

                switch (currentProjectNotFoundWarning.WarningLevel)
                {
                    case ErrorLevel.Global: ruleLevel = "глобального уровня"; break;
                    case ErrorLevel.Solution: ruleLevel = "уровня Solution"; break;
                    case ErrorLevel.Project: ruleLevel = "в проекте '"+ currentProjectNotFoundWarning.ProjName + "' "; break;
                }

                if(currentProjectNotFoundWarning.WarningLevel == ErrorLevel.Global)
                {
                    documentName = "global_config_guard.rdg";
                    ruleLevel = "глобального уровня";
                }
                
                string currentText = "RefDepGuard Project not found warning: проект '" + currentProjectNotFoundWarning.ReferenceName + "', указанный в референс-правиле " + ruleLevel + ", не обнаружен в solution" + ". Проверьте правило на корректность написания в config-файле";

                StoreErrorTask(errorListProvider, currentText, documentName, TaskErrorCategory.Warning);
            }

            foreach (MaxFrameworkVersionDeviantValueError maxFrameworkVersionDeviantValue in refDepGuardErrors.MaxFrameworkVersionDeviantValueList)
            {
                string documentName = configFilesData.solutionName + "_config_guard.rdg";
                string relevantProjectName = "";
                string globalPrefix = "";
                string errorType = maxFrameworkVersionDeviantValue.IsProjectTypeCopyError ? " содержит один и тот же тип проекта в шаблоне более одного раза" : " содержит некорректную запись своего значения";

                switch (maxFrameworkVersionDeviantValue.ErrorLevel)
                {
                    case ErrorLevel.Global: documentName = "global_config_guard.rdg"; globalPrefix = "глобального "; break;
                    case ErrorLevel.Solution: relevantProjectName = "уровня Solution"; break;
                    case ErrorLevel.Project: relevantProjectName = "проекта '" + maxFrameworkVersionDeviantValue.ErrorRelevantProjectName + "'"; break;
                }

                string errorText = "RefDepGuard framework_max_version deviant value error: параметр 'framework_max_version' " + globalPrefix + "Config-файла " + relevantProjectName + errorType + ". Проверьте его на предмет отсутствия синтаксических ошибок и соответствия шаблону файла конфигурации";

                StoreErrorTask(errorListProvider, errorText, documentName, TaskErrorCategory.Error);
            }

            foreach (FrameworkVersionComparabilityError frameworkVersionComparabilityError in refDepGuardErrors.FrameworkVersionComparabilityErrorList)
            {
                string documentName = configFilesData.solutionName + "_config_guard.rdg";
                string ruleLevel = "";


                switch (frameworkVersionComparabilityError.ErrorLevel)
                {
                    case ErrorLevel.Global: documentName = "global_config_guard.rdg"; ruleLevel = "ограничение глобального уровня"; break;
                    case ErrorLevel.Solution: ruleLevel = "ограничение уровня решения"; break;
                    case ErrorLevel.Project: ruleLevel = "ограничение уровня проекта"; break;
                }

                string errorText = "RefDepGuard Framework version comparability error: 'TargetFrameworkVersion' проекта '" + frameworkVersionComparabilityError.ErrorRelevantProjectName + "' имеет версию '" + frameworkVersionComparabilityError.TargetFrameworkVersion
                    + "', в то время как максимально допустимой для него версией является '" + frameworkVersionComparabilityError.MaxFrameworkVersion + "' (" + ruleLevel + "). Измените версию проекта или модифицируйте конфигурацию Config-файла";

                StoreErrorTask(errorListProvider, errorText, documentName, TaskErrorCategory.Error);
            }

            foreach (ConfigFilePropertyNullError configFilePropertyNullError in refDepGuardErrors.ConfigPropertyNullErrorList)
            {
                string documentName = configFilesData.solutionName + "_config_guard.rdg";
                string relevantProjectName = "";

                if (configFilePropertyNullError.IsGlobal)
                    documentName = "global_config_guard.rdg";

                if (configFilePropertyNullError.ErrorRelevantProjectName != "")
                    relevantProjectName = " для проекта '" + configFilePropertyNullError.ErrorRelevantProjectName + "'";

                string errorText = "RefDepGuard Null property error: Config-файл не содержит свойство '" + configFilePropertyNullError.PropertyName + "'" + relevantProjectName + ". Проверьте его на предмет отсутствия синтаксических ошибок и соответствия шаблону файла конфигурации";

                StoreErrorTask(errorListProvider, errorText, documentName, TaskErrorCategory.Error);
            }

            foreach (ReferenceMatchError referenceMatchError in refDepGuardErrors.RefsMatchErrorList)
            {
                string projectName = "";
                string referenceLevelText = "";
                string documentName = configFilesData.solutionName + "_config_guard.rdg";
                string matchErrorDescription = "";

                if (referenceMatchError.IsProjNameMatchError)
                    matchErrorDescription = " совпадает с именем проекта";
                else
                    matchErrorDescription = " одновременно заявлен как обязательный и недопустимый";

                if (referenceMatchError.ProjectName != "")
                    projectName = "' проекта '" + referenceMatchError.ProjectName;


                switch (referenceMatchError.ReferenceLevelValue)
                {
                    case ErrorLevel.Solution: referenceLevelText = "уровня Solution"; break;
                    case ErrorLevel.Global: referenceLevelText = "глобального уровня"; documentName = "global_config_guard.rdg"; break;
                    case ErrorLevel.Project: break;
                }

                string errorText = "RefDepGuard Match error: референс '" + referenceMatchError.ReferenceName + projectName + "' " + referenceLevelText + matchErrorDescription + ". Устраните противоречие в правиле";

                StoreErrorTask(errorListProvider, errorText, documentName, TaskErrorCategory.Error);
            }


            foreach (ReferenceError error in refDepGuardErrors.RefsErrorList)
            {
                string referenceTypeText = "";
                string referenceLevelText = "";
                string documentName = error.ErrorRelevantProjectName + ".csproj";
                string actionForUser = "";

                if (error.IsReferenceRequired)
                {
                    referenceTypeText = "Отсутсвует обязательный";
                    actionForUser = "Добавьте";
                }
                else
                {
                    referenceTypeText = "Присутствует недопустимый";
                    actionForUser = "Удалите";
                }

                switch (error.CurrentReferenceLevel)
                {
                    case ErrorLevel.Solution: referenceLevelText = "уровня Solution"; break;
                    case ErrorLevel.Global: referenceLevelText = "глобального уровня"; break;
                    case ErrorLevel.Project: break;
                }

                string errorText = "RefDepGuard Reference error: " + referenceTypeText + " референс " + referenceLevelText + " '" + error.ReferenceName + "' для проекта '" + error.ErrorRelevantProjectName + "'. " + actionForUser + " его через обозреватель решений";

                StoreErrorTask(errorListProvider, errorText, documentName, TaskErrorCategory.Error);
            }

            foreach (MaxFrameworkVersionDeviantValueWarning maxFrameworkVersionDeviantValue in refDepGuardWarnings.MaxFrameworkVersionDeviantValueWarningList)
            {
                string documentName = configFilesData.solutionName + "_config_guard.rdg";
                string relevantProjectName = "";

                switch (maxFrameworkVersionDeviantValue.WarningLevel)
                {
                    case ErrorLevel.Global: documentName = "global_config_guard.rdg"; relevantProjectName = "глобального Config-файла"; break;
                    case ErrorLevel.Solution: relevantProjectName = "Config-файла уровня Solution"; break;
                    case ErrorLevel.Project: relevantProjectName = "проекта '" + maxFrameworkVersionDeviantValue.WarningRelevantProjectName + "'"; break;
                }

                string errorText = "RefDepGuard framework_max_version deviant value warning: параметр 'framework_max_version' " + relevantProjectName + " содержит значение '"+ maxFrameworkVersionDeviantValue.DeviantValue +"', а должен содержать значение с точкой (формата 'x.x'). Приведите значение к корректному формату";

                StoreErrorTask(errorListProvider, errorText, documentName, TaskErrorCategory.Warning);
            }

            foreach (MaxFrameworkVersionConflictWarning maxFrameworkVersionConflictValue in refDepGuardWarnings.MaxFrameworkVersionConflictWarningsList)
            { //Выделить в error случаи, когда рассматриваюся версии одного уровня? (случаи с all)
                string documentName = configFilesData.solutionName + "_config_guard.rdg";
                string highErrorLevelText = "";
                string lowErrorLevelText = "";

                if (maxFrameworkVersionConflictValue.HighErrorLevel == maxFrameworkVersionConflictValue.LowErrorLevel)
                    highErrorLevelText = ", указанное в супертипе 'all' на том же уровне";

                else
                {
                    switch (maxFrameworkVersionConflictValue.HighErrorLevel)
                    {
                        case ErrorLevel.Global: highErrorLevelText = "глобального уровня"; break;
                        case ErrorLevel.Solution: highErrorLevelText = "уровня Solution"; break;
                    }
                }

                switch (maxFrameworkVersionConflictValue.LowErrorLevel)
                {
                    case ErrorLevel.Solution: lowErrorLevelText = "уровня Solution"; break;
                    case ErrorLevel.Project: lowErrorLevelText = "в проекте '" + maxFrameworkVersionConflictValue.WarningRelevantProjectName + "'"; break;
                }

                string errorText = "RefDepGuard framework_max_version conflict warning: значение '" + maxFrameworkVersionConflictValue.LowLevelMaxFrameVersion
                    + "' параметра 'framework_max_version' " + lowErrorLevelText + " превосходит значение '" + maxFrameworkVersionConflictValue.HighLevelMaxFrameVersion
                    + "' одноимённого параметра " + highErrorLevelText + ". Устраните противоречие";

                StoreErrorTask(errorListProvider, errorText, documentName, TaskErrorCategory.Warning);
            }

            foreach (MaxFrameworkVersionReferenceConflictWarning maxFrameworkVersionReferenceConflictWarning in refDepGuardWarnings.MaxFrameworkVersionReferenceConflictWarningsList)
            {
                string documentName = configFilesData.solutionName + "_config_guard.rdg";

                string errorText = "RefDepGuard framework_max_version reference conflict warning: значение '" + maxFrameworkVersionReferenceConflictWarning.ProjFrameworkVersion
                    + "' параметра 'framework_max_version' проекта " + maxFrameworkVersionReferenceConflictWarning.ProjName + " приводит к потенциальному конфликту версий TargetFramework" +
                    ", так как имеется референс на проект, имеющий большее значение значение параметра 'framework_max_version' (проект: " + maxFrameworkVersionReferenceConflictWarning.RefName
                    + ", Версия: " + maxFrameworkVersionReferenceConflictWarning.RefFrameworkVersion + "). Устраните противоречие";

                StoreErrorTask(errorListProvider, errorText, documentName, TaskErrorCategory.Warning);
            }

            foreach (ReferenceMatchWarning referenceMatchWarning in refDepGuardWarnings.RefsMatchWarningList)
            {
                string documentName = configFilesData.solutionName + "_config_guard.rdg";
                string projectName = "";
                string highReferenceLevelText = "";
                string lowReferenceLevelText = "";
                string referenceTypeText = "";
                string warningDescription = "";
                string warningAction = "";


                if (referenceMatchWarning.ProjectName != "")
                {
                    projectName = "' проекта '" + referenceMatchWarning.ProjectName;
                }

                if (referenceMatchWarning.LowReferenceLevel == ErrorLevel.Solution)
                {
                    lowReferenceLevelText = "уровня Solution";
                }

                switch (referenceMatchWarning.HighReferenceLevel)
                {
                    case ErrorLevel.Solution: highReferenceLevelText = "уровня Solution"; break;
                    case ErrorLevel.Global: highReferenceLevelText = "глобального уровня"; documentName = "global_config_guard.rdg"; break;
                    case ErrorLevel.Project: break;
                }

                if (referenceMatchWarning.IsReferenceStraight)
                {
                    warningDescription = " дубирует правило с одноимённым референсом ";
                    warningAction = ". Устраните дублирование правила";

                    if (referenceMatchWarning.IsHighLevelReq)
                        referenceTypeText = " является обязательным и";
                    else
                        referenceTypeText = " является недопустимым и";
                }
                else //В противном случае рассматривается cross match errors, а значит они имеют тип рефа, противиположный более "верхнему" правилу
                {
                    warningDescription = " противоречит правилу с одноимённым референсом ";
                    warningAction = ". Устраните противоречие в правиле";

                    if (referenceMatchWarning.IsHighLevelReq)
                        referenceTypeText = " является недопустимым и";
                    else
                        referenceTypeText = " является обязательным и";
                }

                string errorText = "RefDepGuard Match Warning: референс '" + referenceMatchWarning.ReferenceName + projectName + "' " + lowReferenceLevelText + referenceTypeText + warningDescription + highReferenceLevelText + warningAction;

                StoreErrorTask(errorListProvider, errorText, documentName, TaskErrorCategory.Warning);
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
