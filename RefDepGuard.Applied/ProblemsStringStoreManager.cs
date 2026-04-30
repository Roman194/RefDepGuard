using RefDepGuard.Applied.Models.ConfigFile;
using RefDepGuard.Applied.Models.FrameworkVersion.Errors;
using RefDepGuard.Applied.Models.FrameworkVersion.Warnings;
using RefDepGuard.Applied.Models.FrameworkVersion.Warnings.Conflicts;
using RefDepGuard.Applied.Models.Problem;
using RefDepGuard.Applied.Models.Project;
using RefDepGuard.Applied.Models.RefDepGuard;
using RefDepGuard.Applied.Models.Reference.Errors;
using RefDepGuard.Applied.Models.Reference.Warnings;
using RefDepGuard.Applied.Resources;
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
                string referenceTypeText = error.IsReferenceRequired ? Resource.No_Required_String : Resource.Unacceptable_String;
                string actionForUser = error.IsReferenceRequired ? Resource.Action_Add_String : Resource.Action_Remove_String;
                string referenceLevelText = "";
                string documentName = error.ErrorRelevantProjectName + ".csproj";

                switch (error.CurrentRuleLevel)
                {
                    case ProblemLevel.Solution: referenceLevelText = Resource.Solution_Level; break;
                    case ProblemLevel.Global: referenceLevelText = Resource.Global_Level; break;
                }

                string errorText = outputPlacePrefix + "Reference error: " + referenceTypeText + Resource.Reference_String + referenceLevelText + "'" + error.ReferenceName +
                    Resource.For_Project_String + error.ErrorRelevantProjectName + "'." + outputPlaceTransfer + actionForUser; 
                
                problemsStringList.Add(new ProblemString(errorText, documentName));
            }

            foreach (ReferenceMatchError referenceMatchError in refDepGuardErrors.RefsMatchErrorList)
            {
                string projectName = (referenceMatchError.ProjectName != "") ? ("'" + Resource.Of_A_Project_String + referenceMatchError.ProjectName) : "";
                string referenceLevelText = " ";
                string matchErrorDescription = referenceMatchError.IsProjNameMatchError ?
                    Resource.One_Proj_Match_Error_Description : Resource.Claim_Match_Error_Description;
                string documentName = (referenceMatchError.RuleLevel == ProblemLevel.Global) ?
                    "global_config_guard.rdg" : configFilesData.SolutionName + "_config_guard.rdg";

                switch (referenceMatchError.RuleLevel)
                {
                    case ProblemLevel.Solution: referenceLevelText += Resource.Solution_Level; break;
                    case ProblemLevel.Global: referenceLevelText += Resource.Global_Level; break;
                }

                string errorText = outputPlacePrefix + "Match error:" + Resource.Reference_String + "'" + referenceMatchError.ReferenceName + projectName + "'" + referenceLevelText
                   + matchErrorDescription + "." + outputPlaceTransfer + Resource.Reference_Match_Error_Action;

                problemsStringList.Add(new ProblemString(errorText, documentName));
            }

            foreach (ConfigFilePropertyNullError configFilePropertyNullError in refDepGuardErrors.ConfigPropertyNullErrorList)
            {
                string relevantProjectName = (configFilePropertyNullError.ErrorRelevantProjectName != "") ?
                    Resource.For_Project_String + configFilePropertyNullError.ErrorRelevantProjectName + "'" : "";
                string documentName = configFilePropertyNullError.IsGlobal ? 
                    "global_config_guard.rdg" : configFilesData.SolutionName + "_config_guard.rdg";

                string errorText = outputPlacePrefix + "Null property error: " + Resource.Config_File_Property_Null_Error_Message + configFilePropertyNullError.PropertyName +
                    relevantProjectName + "." + outputPlaceTransfer + Resource.Config_File_Property_Null_Error_Action;

                problemsStringList.Add(new ProblemString(errorText, documentName));
            }

            foreach (MaxFrameworkVersionDeviantValueError maxFrameworkVersionDeviantValue in refDepGuardErrors.MaxFrameworkVersionDeviantValueList)
            {
                string relevantProjectName = "";
                string globalPrefix = "";
                string errorType = maxFrameworkVersionDeviantValue.IsProjectTypeCopyError ?
                    Resource.Project_Type_Copy_Error : Resource.Invalid_Project_Type_Error;
                string documentName = (maxFrameworkVersionDeviantValue.ErrorLevel == ProblemLevel.Global) ?
                    "global_config_guard.rdg" : configFilesData.SolutionName + "_config_guard.rdg";

                switch (maxFrameworkVersionDeviantValue.ErrorLevel)
                {
                    case ProblemLevel.Global: globalPrefix = Resource.Global_String; break;
                    case ProblemLevel.Solution: globalPrefix = Resource.Solution_String; break;
                    case ProblemLevel.Project: relevantProjectName = Resource.Of_A_Project_String + maxFrameworkVersionDeviantValue.ErrorRelevantProjectName + "'"; break;
                }
                string errorText = outputPlacePrefix + "framework_max_version deviant value error: " + Resource.Max_Framework_Version_Deviant_Value_Error_Message + globalPrefix 
                    + Resource.Config_File_String + relevantProjectName + errorType + "." + outputPlaceTransfer + Resource.Config_File_Property_Null_Error_Action;

                problemsStringList.Add(new ProblemString(errorText, documentName));
            }

            foreach (MaxFrameworkVersionIllegalTemplateUsageError maxFrameworkVersionIllegalTemplateUsageError in refDepGuardErrors.MaxFrameworkVersionIllegalTemplateUsageErrorList)
            {
                string errorDescr = maxFrameworkVersionIllegalTemplateUsageError.IsIllegalTFMUsageError ?
                    (Resource.Illegal_TFM_Usage_Error_Message + outputPlaceTransfer + Resource.Illegal_TFM_Usage_Error_Action) :
                    (Resource.Incorrect_TFM_Usage_Error_Message + outputPlaceTransfer + Resource.Incorrect_TFM_Usage_Error_Action);
                string documentName = configFilesData.SolutionName + "_config_guard.rdg";

                string errorText = outputPlacePrefix + "framework_max_version illegal template usage error: " + Resource.Max_Framework_Version_Deviant_Value_Error_Message + 
                    Resource.Of_A_Project_String + maxFrameworkVersionIllegalTemplateUsageError.ProjName + "' " + errorDescr;

                problemsStringList.Add(new ProblemString(errorText, documentName));
            }

            foreach (FrameworkVersionComparabilityError frameworkVersionComparabilityError in refDepGuardErrors.FrameworkVersionComparabilityErrorList)
            {
                string ruleLevel = Resource.Global_Rule_Level;
                string currentTFMText = (frameworkVersionComparabilityError.ErrorRelevantTFM != "") ?
                    (Resource.For_string + "TFM '" + frameworkVersionComparabilityError.ErrorRelevantTFM + "'") : "";
                string documentName = (frameworkVersionComparabilityError.ErrorLevel == ProblemLevel.Global) ?
                    configFilesData.SolutionName + "_config_guard.rdg" : "global_config_guard.rdg";

                switch (frameworkVersionComparabilityError.ErrorLevel)
                {
                    case ProblemLevel.Solution: ruleLevel = Resource.Solution_Rule_Level; break;
                    case ProblemLevel.Project: ruleLevel = Resource.Project_Rule_Level; break;
                }
                string errorText = outputPlacePrefix + "Framework version comparability error: 'TargetFrameworkVersion' " + Resource.Of_A_Project_String + 
                    frameworkVersionComparabilityError.ErrorRelevantProjectName +
                    "'" + currentTFMText + Resource.Have_Version_String + frameworkVersionComparabilityError.TargetFrameworkVersion + Resource.Framework_Version_Comparability_Error_Message +
                    frameworkVersionComparabilityError.MaxFrameworkVersion + "' (" + ruleLevel + ")." + outputPlaceTransfer + 
                    Resource.Framework_Version_Comparability_Error_Action;

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
                string projectName = (referenceMatchWarning.ProjectName != "") ? ("'" + Resource.Of_A_Project_String + referenceMatchWarning.ProjectName) : "";
                string highReferenceLevelText = "";
                string lowReferenceLevelText = (referenceMatchWarning.LowReferenceLevel == ProblemLevel.Solution) ? Resource.Solution_Level : " ";
                string referenceTypeText = referenceMatchWarning.IsReferenceStraight ?
                    (referenceMatchWarning.IsHighLevelReq ? Resource.Req_Reference_Type : Resource.Unaccept_Reference_Type) :
                    (referenceMatchWarning.IsHighLevelReq ? Resource.Unaccept_Reference_Type : Resource.Req_Reference_Type); //В противном случае рассматривается cross match errors, а значит они имеют тип рефа, противиположный более "верхнему" правилу
                string warningDescription = referenceMatchWarning.IsReferenceStraight ?
                    Resource.Reference_Match_Warn_Duplicate_Message : Resource.Reference_Match_Warn_Contrad_Message;
                string warningAction = referenceMatchWarning.IsReferenceStraight ? ("." + outputPlaceTransfer + Resource.Refernce_Match_Duplicate_Warn_Action) : 
                    ("." + outputPlaceTransfer + Resource.Reference_Match_Contrad_Warn_Action);
                string documentName = (referenceMatchWarning.HighReferenceLevel == ProblemLevel.Global) ?
                    "global_config_guard.rdg" : configFilesData.SolutionName + "_config_guard.rdg";

                switch (referenceMatchWarning.HighReferenceLevel)
                {
                    case ProblemLevel.Solution: highReferenceLevelText = Resource.Solution_Level; break;
                    case ProblemLevel.Global: highReferenceLevelText = Resource.Global_Level; break;
                }
                string warningText = outputPlacePrefix + "Match Warning:" + Resource.Reference_String + "'" + referenceMatchWarning.ReferenceName + projectName + "' " + 
                    lowReferenceLevelText + referenceTypeText + warningDescription + highReferenceLevelText + warningAction;

                problemsStringList.Add(new ProblemString(warningText, documentName));
            }

            foreach (var currentProjectNotFoundWarning in refDepGuardWarnings.ProjectNotFoundWarningList)
            {
                string ruleLevel = Resource.Solution_Level;
                string documentName = (currentProjectNotFoundWarning.WarningLevel == ProblemLevel.Global) ?
                    "global_config_guard.rdg" : configFilesData.SolutionName + "_config_guard.rdg";

                switch (currentProjectNotFoundWarning.WarningLevel)
                {
                    case ProblemLevel.Global: ruleLevel = Resource.Global_Level; break;
                    case ProblemLevel.Project: ruleLevel = Resource.In_A_Project_String + currentProjectNotFoundWarning.ProjName + "' "; break;
                }
                string warningText = outputPlacePrefix + "Project not found warning:" + Resource.Project_String + currentProjectNotFoundWarning.ReferenceName + 
                    Resource.Project_Not_Found_Warning_Message + ruleLevel + Resource.Project_Not_Found_Warning_Message_2 + "." + outputPlaceTransfer + 
                    Resource.Project_Not_Found_Warning_Action;

                problemsStringList.Add(new ProblemString(warningText, documentName));
            }

            foreach (var currentProjectMatchWarning in refDepGuardWarnings.ProjectMatchWarningList)
            {
                string placeWhereProjectNotFound = currentProjectMatchWarning.IsNoProjectInConfigFile ? Resource.Config_File_String_2 : Resource.Solution_String;

                string warningText = outputPlacePrefix + "Project match warning:" + Resource.Project_String + currentProjectMatchWarning.ProjName + 
                    Resource.Project_Match_Warning_Message + placeWhereProjectNotFound + "." + outputPlaceTransfer + 
                    Resource.Project_Match_Warning_Action;

                problemsStringList.Add(new ProblemString(warningText, configFilesData.SolutionName + "_config_guard.rdg"));
            }

            foreach (MaxFrameworkVersionDeviantValueWarning maxFrameworkVersionDeviantValue in refDepGuardWarnings.MaxFrameworkVersionDeviantValueWarningList)
            {
                string relevantProjectName = Resource.Global_Config_File_String;
                string warningText = "";
                string documentName = (maxFrameworkVersionDeviantValue.WarningLevel == ProblemLevel.Global) ?
                    "global_config_guard.rdg" : configFilesData.SolutionName + "_config_guard.rdg";

                switch (maxFrameworkVersionDeviantValue.WarningLevel)
                {
                    case ProblemLevel.Solution: relevantProjectName = Resource.Solution_Config_File_String; break;
                    case ProblemLevel.Project: relevantProjectName = Resource.Of_A_Project_String + maxFrameworkVersionDeviantValue.WarningRelevantProjectName + "'"; break;
                }
                warningText = outputPlacePrefix + "framework_max_version deviant value warning: " + Resource.Max_Framework_Version_Deviant_Value_Error_Message + " " + 
                    relevantProjectName + Resource.Contains_Value_String + maxFrameworkVersionDeviantValue.DeviantValue +
                    Resource.Max_Fr_Version_Deviant_Value_Warn_Message + outputPlaceTransfer + Resource.Max_Fr_Version_Deviant_Value_Warn_Action;

                problemsStringList.Add(new ProblemString(warningText, documentName));
            }

            foreach (MaxFrameworkVersionConflictWarning maxFrameworkVersionConflictValue in refDepGuardWarnings.MaxFrameworkVersionConflictWarningsList)
            {
                string documentName = configFilesData.SolutionName + "_config_guard.rdg";
                string highErrorLevelText = "";
                string lowErrorLevelText = "";

                if (maxFrameworkVersionConflictValue.HighWarnLevel == maxFrameworkVersionConflictValue.LowWarnLevel)
                    highErrorLevelText = Resource.Supertype_All_Level;
                else
                {
                    switch (maxFrameworkVersionConflictValue.HighWarnLevel)
                    {
                        case ProblemLevel.Global: highErrorLevelText = Resource.Global_Level; break;
                        case ProblemLevel.Solution: highErrorLevelText = Resource.Solution_Level; break;
                    }
                }

                switch (maxFrameworkVersionConflictValue.LowWarnLevel)
                {
                    case ProblemLevel.Solution: lowErrorLevelText = Resource.Solution_Level; break;
                    case ProblemLevel.Project: lowErrorLevelText = Resource.In_A_Project_String + maxFrameworkVersionConflictValue.WarningRelevantProjectName + "'"; break;
                }

                string warningText = outputPlacePrefix + "framework_max_version conflict warning: " + Resource.Value_String + maxFrameworkVersionConflictValue.LowLevelMaxFrameVersion
                    + "' " + Resource.Of_The_Fr_Max_Version_String + lowErrorLevelText + Resource.Exceed_String + Resource.Value_String + 
                    maxFrameworkVersionConflictValue.HighLevelMaxFrameVersion
                    + Resource.Eponymous_Parameter_String + highErrorLevelText + "." + outputPlaceTransfer + Resource.Reference_Match_Error_Action;

                problemsStringList.Add(new ProblemString(warningText, documentName));
            }

            foreach (MaxFrameworkVersionReferenceConflictWarning maxFrameworkVersionReferenceConflictWarning in refDepGuardWarnings.MaxFrameworkVersionReferenceConflictWarningsList)
            {
                string warningCause = (maxFrameworkVersionReferenceConflictWarning.IsOneProjectsTypeConflict) ?
                    Resource.Max_Fr_Version_Conflict_Bigger_Value_Error_Cause :
                    Resource.Max_Fr_Version_Reference_Conflict_Incomparable_Error_Cause;
                
                string warningText = outputPlacePrefix + "framework_max_version reference conflict warning: " + Resource.Value_String + 
                    maxFrameworkVersionReferenceConflictWarning.ProjFrameworkVersion + "' " + Resource.Of_The_Fr_Max_Version_String + Resource.Of_A_Project_String + 
                    maxFrameworkVersionReferenceConflictWarning.ProjName + Resource.Max_Fr_Version_Reference_Conflict_Warn_Message + warningCause + "(" + Resource.Project_String 
                    + maxFrameworkVersionReferenceConflictWarning.RefName + "'," + Resource.Version_String + maxFrameworkVersionReferenceConflictWarning.RefFrameworkVersion 
                    + "')." + outputPlaceTransfer + Resource.Reference_Match_Error_Action;
                string documentName = configFilesData.SolutionName + "_config_guard.rdg";

                problemsStringList.Add(new ProblemString(warningText, documentName));
            }

            foreach (MaxFrameworkVersionTFMNotFoundWarning maxFrameworkVersionTFMNotFoundWarning in refDepGuardWarnings.MaxFrameworkVersionTFMNotFoundWarningList)
            {
                string warningLevel = Resource.Global_Rule_Level;
                string documentName = (maxFrameworkVersionTFMNotFoundWarning.WarningLevel == ProblemLevel.Global) ?
                    "global_config_guard.rdg" : configFilesData.SolutionName + "_config_guard.rdg";

                switch (maxFrameworkVersionTFMNotFoundWarning.WarningLevel)
                {
                    case ProblemLevel.Solution: warningLevel = Resource.Solution_Rule_Level; break;
                    case ProblemLevel.Project: warningLevel = Resource.Project_String + maxFrameworkVersionTFMNotFoundWarning.ProjName + "'"; break;
                }
                string warningText = outputPlacePrefix + "framework_max_version TFM not found warning: "  + Resource.TFM_Not_Found_Warn_Message + 
                    maxFrameworkVersionTFMNotFoundWarning.TFMName + 
                    "'" + Resource.Is_Not_Found_String +"(" + warningLevel + ")." + outputPlaceTransfer + Resource.TFM_Not_Found_Warn_Action;

                problemsStringList.Add(new ProblemString(warningText, documentName));
            }

            foreach(MaxFrameworkIllegalTemplateUsageWarning maxFrameworkIllegalTemplateUsageWarning in refDepGuardWarnings.MaxFrameworkIllegalTemplateUsageWarningList)
            {
                string warningLevel = maxFrameworkIllegalTemplateUsageWarning.ProblemLevelInfo == ProblemLevel.Global ? Resource.Global_Level : Resource.Solution_Level;
                string warningText = outputPlacePrefix + "framework_max_version illegal template usage warning: " + Resource.In_The_Fr_Max_Version_String + warningLevel +
                    Resource.Illegal_Template_Usage_Warn_Message + outputPlaceTransfer + Resource.Illegal_Template_Usage_Warn_Action;

                string documentName = (maxFrameworkIllegalTemplateUsageWarning.ProblemLevelInfo == ProblemLevel.Global) ?
                    "global_config_guard.rdg" : configFilesData.SolutionName + "_config_guard.rdg";

                problemsStringList.Add(new ProblemString(warningText, documentName));
            }

            foreach(ProjectNameSemanticWarning projNameSemaWarning in refDepGuardWarnings.ProjectNameSemanticWarningList)
            {
                string warningText = outputPlacePrefix + "Project name semantic warning: " + Resource.In_The_Proj_Name_String + projNameSemaWarning.ProjectName +
                    Resource.Proj_Name_Semantic_Warn_Message + projNameSemaWarning.ExpectedSema + Resource.Proj_Name_Semantic_Warn_Message_1 + projNameSemaWarning.FindedSema 
                    + "')." + outputPlaceTransfer + Resource.Proj_Name_Semantic_Warn_Action;
                string documentName = "global_config_guard.rdg";

                problemsStringList.Add(new ProblemString(warningText, documentName));
            }

            foreach (var projName in refDepGuardWarnings.UntypedWarningsList)
            {
                string currentText = outputPlacePrefix + "Warning: " + Resource.Untyped_Warn_Message + Resource.For_Project_String + projName +
                    Resource.Untyped_Warn_Message_2 + outputPlaceTransfer + Resource.Untyped_Warn_Action;

                problemsStringList.Add(new ProblemString(currentText, ""));
            }

            //Насколько Tuple всё же норм решение? М.б создать кастмные типы данных?
            foreach (var projKeyValuePair in refDepGuardWarnings.DetectedNDuplicatedTransitRefsDict.Item1) //Detecyed transit refs
            {
                string projName = projKeyValuePair.Key;
                List<string> detectedTransitRefsList = projKeyValuePair.Value;

                string currentText = outputPlacePrefix + "Transit references warning: " + Resource.Project_String + projName + Resource.Transit_Refs_Message;

                foreach (var refName in detectedTransitRefsList)
                {
                    currentText += "'" + refName + "', ";
                }
                currentText = currentText.Remove(currentText.Length - 2);

                problemsStringList.Add(new ProblemString(currentText, ""));
            }

            foreach (var projKeyValuePair in refDepGuardWarnings.DetectedNDuplicatedTransitRefsDict.Item2) //Duplicated transit refs
            {
                string projName = projKeyValuePair.Key;
                List<string> detectedTransitRefsList = projKeyValuePair.Value;

                string currentText = outputPlacePrefix + "Transit references duplicate warning: " + Resource.Project_String + projName + Resource.Transit_Refs_Duplicate_Warn_Message;

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