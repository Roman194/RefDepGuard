using RefDepGuard.CheckRules.Data;
using RefDepGuard.Applied.Models.FrameworkVersion.Errors;
using RefDepGuard.Applied.Models.FrameworkVersion.Warnings;
using RefDepGuard.Applied.Models.RefDepGuard;
using RefDepGuard.CheckRules.SubManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using RefDepGuard.Applied.Models.ConfigFile;
using RefDepGuard.Applied.Models.Project;
using RefDepGuard.Applied.Models.Problem;
using RefDepGuard.Applied.Models.ConfigFile.DTO;
using RefDepGuard.Applied.Models.Reference;
using RefDepGuard.Applied.Models.FrameworkVersion;
using RefDepGuard.Applied.Models;
using RefDepGuard.CheckRules.Models;


namespace RefDepGuard.CheckRules
{
    /// <summary>
    /// This class is responsible for managing the checking of rules based on the configuration files and the current state of the projects in the solution.
    /// </summary>
    public static class CheckRulesManager
    {
        //Some of the "problems" list are stored here because they are generated during the process of parsing the config files, and some of them are generated
        //during the process of generating max_fr_ver dictionary.
        //Other are stored in corresponding submanagers, but they are all collected and sorted in the end of the main method of this manager and then exported to ELP together.
        private static List<MaxFrameworkVersionDeviantValueError> maxFrameworkVersionDeviantValueErrorList = new List<MaxFrameworkVersionDeviantValueError>();
        private static List<MaxFrameworkVersionIllegalTemplateUsageError> maxFrameworkVersionIllegalTemplateUsageErrorsList = new List<MaxFrameworkVersionIllegalTemplateUsageError>();
        private static List<MaxFrameworkVersionDeviantValueWarning> maxFrameworkVersionDeviantValueWarningList = new List<MaxFrameworkVersionDeviantValueWarning>();
        private static List<MaxFrameworkVersionTFMNotFoundWarning> maxFrameworkVersionTFMNotFoundWarningList = new List<MaxFrameworkVersionTFMNotFoundWarning>();

        private static List<RequiredReference> requiredReferencesList = new List<RequiredReference>();
        private static List<RequiredMaxFrVersion> requiredMaxFrVersionList = new List<RequiredMaxFrVersion>();

        //Lists with all errors and warnings, that will be exported to ELP in the end of the main method of this manager.
        private static RefDepGuardErrors refDepGuardErrors;
        private static RefDepGuardWarnings refDepGuardWarnings;

        //This field is used to store the parameters that will be exported to ELP and used in other parts of the extension, such as the exports.
        private static RequiredExportParameters requiredExportParameters;
        private static RefDepGuardFindedProblems refDepGuardFindedProblems;


        /// <summary>
        /// This is the main method of checking rules module.
        /// It is responsible for checking the rules based on the configuration files and the current state of the projects in the solution. 
        /// It performs various checks, such as not null checks, max framework version checks, references checks, and transit references detection. 
        /// It collects all the errors and warnings found during the process and exports them to ELP together with the required parameters for other parts 
        /// of the extension.
        /// </summary>
        /// <param name="configFilesData">ConfigFilesData current commited value</param>
        /// <param name="currentCommitedSolState">Current commited projects state values</param>
        /// <returns>RefDepGuardExportParameters and ConfigFilesData (to provide "Single source of truth" and "One flow through modules" principes)</returns>
        /// <see cref="RefDepGuardExportParameters"/>
        public static Tuple<RefDepGuardExportParameters, ConfigFilesData> CheckConfigFileRulesForExtension(
            ConfigFilesData configFilesData, Dictionary<string, ProjectState> currentCommitedSolState)
        {

            refDepGuardFindedProblems = CheckConfigFileRulesForConsole(configFilesData, currentCommitedSolState);

            var requiredMaxFrVersionsDict = MaxFrameworkRuleChecksSubManager.GetRequiredMaxFrVersionsDict();
            requiredExportParameters = new RequiredExportParameters(requiredReferencesList, requiredMaxFrVersionsDict);

            return new Tuple<RefDepGuardExportParameters, ConfigFilesData>(
                new RefDepGuardExportParameters(refDepGuardFindedProblems, requiredExportParameters),
                configFilesData
            );
        }

        public static RefDepGuardFindedProblems CheckConfigFileRulesForConsole(ConfigFilesData configFilesData, Dictionary<string, ProjectState> currentCommitedSolState)
        {
            ConfigFileGlobalDTO configFileGlobal = configFilesData.ConfigFileGlobal;
            ConfigFileSolutionDTO configFileSolution = configFilesData.ConfigFileSolution;
            string solutionName = configFilesData.SolutionName;

            //before starting the checks, we need to clear the lists with errors and warnings, that are stored in this manager and corresponding submanagers, to avoid duplicates
            ClearErrorAndWarningLists();

            var configPropertyNullErrorList = NotNullChecksSubManager.CheckConfigPropertiesOnNotNull(configFilesData);

            var maxGlobalFrameworkVersionByTypes = GetMaxFrameworkVersionDictionaryByTypes(configFileGlobal?.framework_max_version ?? "-", ProblemLevel.Global);
            var maxSolutionFrameworkVersionByTypes = GetMaxFrameworkVersionDictionaryByTypes(configFileSolution?.framework_max_version ?? "-", ProblemLevel.Solution);

            //A checks for conflicts between max framework versions of the same level (if there are several) on global and solution levels.
            //
            //On project level such conflicts are not possible, because if there are several max framework version restrictions, they must be specified by user in
            //the form of a template with types,and if there is a template with types, then it is not possible to specify several restrictions for the same type on the same level.
            MaxFrameworkRuleChecksSubManager.CheckMaxFrameworkVersionOneLevelConflict(maxGlobalFrameworkVersionByTypes, ProblemLevel.Global);
            MaxFrameworkRuleChecksSubManager.CheckMaxFrameworkVersionOneLevelConflict(maxSolutionFrameworkVersionByTypes, ProblemLevel.Solution);

            List<string> solutionRequiredReferences = configFileSolution?.solution_required_references ?? new List<string>();
            List<string> solutionUnacceptableReferences = configFileSolution?.solution_unacceptable_references ?? new List<string>();

            List<string> globalRequiredReferences = configFileGlobal?.global_required_references ?? new List<string>();
            List<string> globalUnacceptableReferences = configFileGlobal?.global_unacceptable_references ?? new List<string>();

            List<ReferenceAffiliation> unionSolutionAndGlobalReferencesByType = new List<ReferenceAffiliation>
            {
                new ReferenceAffiliation(ProblemLevel.Solution, solutionRequiredReferences, solutionUnacceptableReferences),
                new ReferenceAffiliation(ProblemLevel.Global, globalRequiredReferences, globalUnacceptableReferences)
            };

            requiredReferencesList.AddRange(globalRequiredReferences.ConvertAll(value => new RequiredReference(value, "")));
            requiredReferencesList.AddRange(solutionRequiredReferences.ConvertAll(value => new RequiredReference(value, "")));

            //A check for conflict between max framework versions on global and solution levels
            if (maxGlobalFrameworkVersionByTypes.Count > 0 && maxSolutionFrameworkVersionByTypes.Count > 0)
                MaxFrameworkRuleChecksSubManager.CheckProjectMaxFrameworkVersionDifferentLevelsConflicts(
                    maxSolutionFrameworkVersionByTypes, maxGlobalFrameworkVersionByTypes, "-", ProblemLevel.Solution, ProblemLevel.Global
                    );

            //A check on difference between projects in the solution and projects specified in the config file and updating the config data if needed
            //(if the user allowed automatic update of projects list in config file)
            var projectMatchWarningList = CheckProjectsMatchSubManager.GetProjectsMatchAfterChecksWarning(configFilesData, currentCommitedSolState);

            //A check on exsisting of the projects that are specified as the references in the config file on the global/solution level
            (globalRequiredReferences, globalUnacceptableReferences) =
                RefsRuleChecksSubManager.CheckReferencesOnProjectExisting(globalRequiredReferences, globalUnacceptableReferences, currentCommitedSolState, ProblemLevel.Global);

            (solutionRequiredReferences, solutionUnacceptableReferences) =
                RefsRuleChecksSubManager.CheckReferencesOnProjectExisting(solutionRequiredReferences, solutionUnacceptableReferences, currentCommitedSolState, ProblemLevel.Solution);

            //A check for conflicts between referneces on the solution and global levels
            RefsRuleChecksSubManager.CheckRulesOnMatchConflicts(
                solutionRequiredReferences, solutionUnacceptableReferences, globalRequiredReferences, globalUnacceptableReferences
                );

            bool isTransitReferencesDetectionNeeded = (configFileGlobal?.report_on_transit_references ?? false) && (configFileSolution?.report_on_transit_references ?? false);

            foreach (KeyValuePair<string, ProjectState> currentProjState in currentCommitedSolState)//foreach project
            {
                var projName = currentProjState.Key;
                var projReferences = currentProjState.Value.CurrentReferences;
                var projFrameworkVersions = currentProjState.Value.CurrentFrameworkVersions;

                if (configFilesData.ConfigFileSolution?.projects?.ContainsKey(projName) ?? false)//If the project exsists (still needs this check as the user can disable auto adding of missing in config file projects)
                {
                    ConfigFileProjectDTO currentProjectConfigFileSettings = configFileSolution.projects[projName];

                    bool isConsiderRequiredReferences = currentProjectConfigFileSettings.consider_global_and_solution_references?.required ?? true;
                    bool isConsiderUnacceptableReferences = currentProjectConfigFileSettings.consider_global_and_solution_references?.unacceptable ?? true;

                    //Transit references detection is needed on project level if it is allowed on global and solution levels and if the user didn't disable it for the project specifically
                    bool isTransitReferencesDetectionNeededOnThisProj = (currentProjectConfigFileSettings?.report_on_transit_references ?? false) && isTransitReferencesDetectionNeeded;

                    Dictionary<string, List<int>> projTypes = currentCommitedSolState[projName].CurrentFrameworkVersions;
                    var maxFrameworkVersionByTypes =
                        GetMaxFrameworkVersionDictionaryByTypes(currentProjectConfigFileSettings?.framework_max_version ?? "-", ProblemLevel.Project, projName, projTypes.Keys.ToList());

                    List<string> requiredReferences = currentProjectConfigFileSettings?.required_references ?? new List<string>();
                    List<string> unacceptableReferences = currentProjectConfigFileSettings?.unacceptable_references ?? new List<string>();

                    List<List<string>> configFileProjectAndSolutionReferences = new List<List<string>>
                    {
                        requiredReferences, unacceptableReferences, solutionRequiredReferences, solutionUnacceptableReferences
                    };

                    requiredReferencesList.AddRange(requiredReferences.ConvertAll(value => new RequiredReference(value, projName)));

                    //A check on exsisting of the projects that are specified as the references in the config file on the project level
                    (requiredReferences, unacceptableReferences) =
                        RefsRuleChecksSubManager.CheckReferencesOnProjectExisting(requiredReferences, unacceptableReferences, currentCommitedSolState, ProblemLevel.Project, projName);

                    //A check for conflicts between referneces on the project level and solution/global levels
                    RefsRuleChecksSubManager.CheckProjectRulesOnMatchConflicts(
                        solutionRequiredReferences, solutionUnacceptableReferences, globalRequiredReferences, globalUnacceptableReferences, requiredReferences,
                        unacceptableReferences, projName, isConsiderRequiredReferences, isConsiderUnacceptableReferences);

                    //A check an accordance of the current projects state references to the project level rules from the config file
                    RefsRuleChecksSubManager.CheckRulesForProjectReferences(projName, projReferences, requiredReferences, true);
                    RefsRuleChecksSubManager.CheckRulesForProjectReferences(projName, projReferences, unacceptableReferences, false);

                    foreach (ReferenceAffiliation referenceAffiliation in unionSolutionAndGlobalReferencesByType)
                    {
                        if (isConsiderRequiredReferences)//if it is allowed to use global/solution required references
                            //then check an accordance of the current projects state references to the global and solution level rules from the config file
                            RefsRuleChecksSubManager.CheckRulesForSolutionOrGlobalReferences(
                                projName, projReferences, referenceAffiliation.RequiredReferences, referenceAffiliation.RulesLevel,
                                true, configFileProjectAndSolutionReferences);

                        if (isConsiderUnacceptableReferences)//if it is allowed to use global/solution unacceptable references
                            //then check an accordance of the current projects state references to the global and solution level rules from the config file
                            RefsRuleChecksSubManager.CheckRulesForSolutionOrGlobalReferences(
                                projName, projReferences, referenceAffiliation.UnacceptableReferences, referenceAffiliation.RulesLevel,
                                false, configFileProjectAndSolutionReferences);
                    }

                    //A check for transit references if it is needed for the current project
                    if (isTransitReferencesDetectionNeededOnThisProj)
                        TransitRefsDetectSubManager.CheckCurrentProjectOnTransitReferences(projName, currentCommitedSolState);

                    //A checks on max_framework_version rules
                    if (maxFrameworkVersionByTypes.Count == 0)//If no rules on project level
                    {
                        if (maxSolutionFrameworkVersionByTypes.Count > 0)//If there are rules on solution level
                        { //then check project TFM versions on accordance with solution level rules
                            MaxFrameworkRuleChecksSubManager.CheckProjectTargetFrameworkVersion(
                                projFrameworkVersions, maxSolutionFrameworkVersionByTypes, projName, ProblemLevel.Solution, maxGlobalFrameworkVersionByTypes
                                );
                        }
                        else
                        {
                            if (maxGlobalFrameworkVersionByTypes.Count > 0) //If there are rules on global level
                                //then check project TFM versions on accordance with global level rules
                                MaxFrameworkRuleChecksSubManager.CheckProjectTargetFrameworkVersion(
                                    projFrameworkVersions, maxGlobalFrameworkVersionByTypes, projName, ProblemLevel.Global
                                    );
                            else
                                MaxFrameworkRuleChecksSubManager.CheckProjectTargetFrameworkVersion(
                                    projFrameworkVersions, maxFrameworkVersionByTypes, projName, ProblemLevel.Undefined
                                    );
                        }
                    }
                    else
                    {
                        //Firstly check if there are conflicts between max framework version rules on project level and solution/global levels
                        if (maxSolutionFrameworkVersionByTypes.Count > 0)
                            MaxFrameworkRuleChecksSubManager.CheckProjectMaxFrameworkVersionDifferentLevelsConflicts(
                                maxFrameworkVersionByTypes, maxSolutionFrameworkVersionByTypes, projName, ProblemLevel.Project, ProblemLevel.Solution
                                );

                        if (maxGlobalFrameworkVersionByTypes.Count > 0)
                            MaxFrameworkRuleChecksSubManager.CheckProjectMaxFrameworkVersionDifferentLevelsConflicts(
                                maxFrameworkVersionByTypes, maxGlobalFrameworkVersionByTypes, projName, ProblemLevel.Project, ProblemLevel.Global
                                );

                        //Then check project TFM versions on accordance with project level rules
                        MaxFrameworkRuleChecksSubManager.CheckProjectTargetFrameworkVersion(
                            projFrameworkVersions, maxFrameworkVersionByTypes, projName, ProblemLevel.Project
                            );
                    }
                }
            }

            //For a correct check of potential conflicts between max framework versions on the exsisting references it is needed to collect info about projects, their max versions and conflicts between them.
            //That's why this check is performed after the main loop through projects.
            foreach (KeyValuePair<string, ProjectState> currentProjState in currentCommitedSolState)
            {
                var projName = currentProjState.Key;
                var projReferences = currentProjState.Value.CurrentReferences;

                if (configFilesData.ConfigFileSolution?.projects?.ContainsKey(projName) ?? false)
                {
                    MaxFrameworkRuleChecksSubManager.CheckProjectReferencesOnPotentialReferencesFrameworkVersionConflict(projName, projReferences);
                }
            }

            //After all checks are performed, we collect all the errors and warnings from different submanagers and sort them before exporting to ELP.
            var refsRuleChecksWarnings = RefsRuleChecksSubManager.GetReferenceWarnings();
            var refsRuleCheckErrors = RefsRuleChecksSubManager.GetReferenceErrors();
            var detectedTransitRefs = TransitRefsDetectSubManager.GetDetectedTransitRefsDict();

            var maxFrameworkVersionWarnings = MaxFrameworkRuleChecksSubManager.GetMaxFrameworkVersionWarnings();
            var maxFrameworkRuleProblems = MaxFrameworkRuleChecksSubManager.GetMaxFrameworkRuleProblems();
            //var requiredMaxFrVersionsDict = MaxFrameworkRuleChecksSubManager.GetRequiredMaxFrVersionsDict();

            refsRuleCheckErrors.RefsErrorList.Sort((x, y) => //Sorts only errors!
                x.CurrentRuleLevel.CompareTo(y.CurrentRuleLevel));
            refsRuleCheckErrors.RefsMatchErrorList.Sort((x, y) =>
                x.RuleLevel.CompareTo(y.RuleLevel));
            configPropertyNullErrorList.Sort((x, y) =>
                x.IsGlobal.CompareTo(y.IsGlobal)); //Проверить насколько хорошо сортирует! А насколько вообще нужно их сортировать?
            maxFrameworkVersionDeviantValueErrorList.Sort((x, y) =>
                x.ErrorLevel.CompareTo(y.ErrorLevel));
            maxFrameworkRuleProblems.FrameworkVersionComparabilityErrorList.Sort((x, y) => 
                x.ErrorLevel.CompareTo(y.ErrorLevel));

            refDepGuardErrors = new RefDepGuardErrors(
                refsRuleCheckErrors.RefsErrorList, refsRuleCheckErrors.RefsMatchErrorList, configPropertyNullErrorList, maxFrameworkVersionDeviantValueErrorList,
                maxFrameworkVersionIllegalTemplateUsageErrorsList, maxFrameworkRuleProblems.FrameworkVersionComparabilityErrorList);

            refDepGuardWarnings = new RefDepGuardWarnings(
                refsRuleChecksWarnings.ReferenceMatchWarningsList, refsRuleChecksWarnings.ProjectNotFoundWarningsList, projectMatchWarningList,
                maxFrameworkVersionDeviantValueWarningList, maxFrameworkVersionWarnings.MaxFrameworkVersionConflictWarningsList,
                maxFrameworkVersionWarnings.MaxFrameworkVersionReferenceConflictWarningsList, maxFrameworkVersionTFMNotFoundWarningList,
                maxFrameworkRuleProblems.UntypedWarningsList, detectedTransitRefs);

            return new RefDepGuardFindedProblems(refDepGuardWarnings, refDepGuardErrors);
        }

        /// <summary>
        /// This method is responsible for clearing the lists with errors and warnings that are stored in this manager and corresponding submanagers, 
        /// to avoid duplicates when the main method is called several times during the lifecycle of the extension.
        /// </summary>
        private static void ClearErrorAndWarningLists()
        {
            if (maxFrameworkVersionDeviantValueErrorList != null)
                maxFrameworkVersionDeviantValueErrorList.Clear();

            if (maxFrameworkVersionDeviantValueWarningList != null)
                maxFrameworkVersionDeviantValueWarningList.Clear();

            if (maxFrameworkVersionIllegalTemplateUsageErrorsList != null)
                maxFrameworkVersionIllegalTemplateUsageErrorsList.Clear();

            if (maxFrameworkVersionTFMNotFoundWarningList != null)
                maxFrameworkVersionTFMNotFoundWarningList.Clear();

            if (requiredReferencesList != null)
                requiredReferencesList.Clear();

            NotNullChecksSubManager.ClearConfigPropertyNullErrorList();
            RefsRuleChecksSubManager.ClearRefsErrorsAndWarnings();
            MaxFrameworkRuleChecksSubManager.ClearErrorAndWarningLists();
            CheckProjectsMatchSubManager.ClearErrorLists();
            TransitRefsDetectSubManager.ClearDetectedTransitRefsDict();
        }

        /// <summary>
        /// This method is responsible for transforming the max framework version string from a config file to a dictionary format for easier access and checks.
        /// </summary>
        /// <param name="currentMaxFrameworkVersion">A string max_framework_version value</param>
        /// <param name="errorLevel">A current level of the max_fr_ver rules</param>
        /// <param name="projName">A name of the current project</param>
        /// <param name="projTypes">TFM-s of this project</param>
        /// <returns>Max framework version rules in dictionary format</returns>
        private static Dictionary<string, List<int>> GetMaxFrameworkVersionDictionaryByTypes(string currentMaxFrameworkVersion, ProblemLevel errorLevel, string projName = "", List<string> projTypes = null)
        {
            projTypes = projTypes ?? new List<string>();

            if (currentMaxFrameworkVersion == "-")
                return new Dictionary<string, List<int>>();

            if ((currentMaxFrameworkVersion.Contains(';') || currentMaxFrameworkVersion.Contains(':')) && errorLevel == ProblemLevel.Project && projTypes.Count == 1)
            {//If it is project level and there is a template with types, but only one type is specified in the project TFM-s
                //then add error about illegal usage of template and return an empty dictionary.
                if (maxFrameworkVersionIllegalTemplateUsageErrorsList.Find(error => error.ProjName == projName) == null)
                    maxFrameworkVersionIllegalTemplateUsageErrorsList.Add(new MaxFrameworkVersionIllegalTemplateUsageError(projName, false));

                return new Dictionary<string, List<int>>();
            }

            if (!currentMaxFrameworkVersion.Contains(':')) //If it isn't template usage then we need to convert it to template format
            {
                if (errorLevel == ProblemLevel.Project)
                {
                    if (projTypes.Count == 1) //If there is only one TFM specified for the project, then we can just add it to the beginning of the string.
                        currentMaxFrameworkVersion = projTypes.FirstOrDefault() + ":" + currentMaxFrameworkVersion;
                    else
                    {//If there are several TFMs, then we need to create a template with types for each of them.
                        string currentMaxFrameworkVersionProjTypeString = "";
                        foreach (var projType in projTypes)
                        {
                            currentMaxFrameworkVersionProjTypeString += projType + ":" + currentMaxFrameworkVersion;

                            if (projTypes.IndexOf(projType) != projTypes.Count - 1)
                                currentMaxFrameworkVersionProjTypeString += "; ";
                        }

                        currentMaxFrameworkVersion = currentMaxFrameworkVersionProjTypeString;
                    }
                }
                else //If it is not project level, then we just add "all" type to the beginning of the string.
                    currentMaxFrameworkVersion = "all:" + currentMaxFrameworkVersion;
            }

            var currentMaxFrameworkVersionArray = currentMaxFrameworkVersion.Split(';');//Split by template elements if there are several of them
            var maxFrameworkDictionary = new Dictionary<string, List<int>>();

            foreach (string maxFrameworkVersion in currentMaxFrameworkVersionArray) //Foreach template element
            {
                var maxFrameworkVersionElementSplited = maxFrameworkVersion.Replace(" ", "").Split(':');

                //If there is a template element with incorrect format (have empty project type or its version)
                if (String.IsNullOrEmpty(maxFrameworkVersionElementSplited[0]) || String.IsNullOrEmpty(maxFrameworkVersionElementSplited[1]))
                {
                    //then add error about incorrect format of max_framework_version element and return empty dictionary, because we can't be sure about the correctness of the data to perform checks with it.
                    if (maxFrameworkVersionDeviantValueErrorList.Find(error => error.ErrorLevel == errorLevel) == null)
                        maxFrameworkVersionDeviantValueErrorList.Add(new MaxFrameworkVersionDeviantValueError(errorLevel, "", false));

                    return new Dictionary<string, List<int>>();
                }

                //If user used template with types on project level, but specified in it a type that doesn't exist in the project TFMs and this type isn't "all",
                if (errorLevel == ProblemLevel.Project && !projTypes.Contains(maxFrameworkVersionElementSplited[0]) && maxFrameworkVersionElementSplited[0] != "all")
                {
                    //then add error about illegal usage of template and return an empty dictionary
                    if (maxFrameworkVersionIllegalTemplateUsageErrorsList.Find(error => error.ProjName == projName) == null)
                        maxFrameworkVersionIllegalTemplateUsageErrorsList.Add(new MaxFrameworkVersionIllegalTemplateUsageError(projName, true));

                    return new Dictionary<string, List<int>>();
                }

                //If user used template with type that doesn't exist then add warning about that
                if (!TFMSample.PossibleTargetFrameworkMonikiers().Contains(maxFrameworkVersionElementSplited[0]))
                {
                    if (maxFrameworkVersionTFMNotFoundWarningList.Find(warning =>
                            warning.TFMName == maxFrameworkVersionElementSplited[0] && warning.WarningLevel == errorLevel && warning.ProjName == projName) == null)
                        maxFrameworkVersionTFMNotFoundWarningList.Add(new MaxFrameworkVersionTFMNotFoundWarning(maxFrameworkVersionElementSplited[0], errorLevel, projName));

                    continue; //but we continue processing this element, because maybe other types in the template are correct and we can use them for checks. Just skip incorrect type with warning.
                }

                var maxFrameworkVersionNumbers = maxFrameworkVersionElementSplited[1].Split('.');//Split template element version by dot to get version numbers
                var maxFrameworkVersionNumsList = new List<int>();

                foreach (var maxFrameworkVersionNumber in maxFrameworkVersionNumbers)//Foreach number of version in template element
                {
                    int maxVersionCurrentNum;
                    if (!Int32.TryParse(maxFrameworkVersionNumber, out maxVersionCurrentNum))//Try to parse it to int
                    {
                        //if it is not possible, then adds deviant value error and return an empty dictionary
                        MaxFrameworkVersionDeviantValueError potentialMaxFrameworkVersionDeviantValueError = new MaxFrameworkVersionDeviantValueError(errorLevel, projName, false);
                        if (errorLevel == ProblemLevel.Project)
                        {
                            maxFrameworkVersionDeviantValueErrorList.Add(potentialMaxFrameworkVersionDeviantValueError);
                        }
                        else
                        {
                            if (maxFrameworkVersionDeviantValueErrorList.Find(error => error.ErrorLevel == errorLevel && error.ErrorRelevantProjectName == projName) == null)
                                maxFrameworkVersionDeviantValueErrorList.Add(potentialMaxFrameworkVersionDeviantValueError);
                        }

                        return new Dictionary<string, List<int>>();
                    }
                    maxFrameworkVersionNumsList.Add(maxVersionCurrentNum);//If it is possible, then add this number to the list of version numbers for this template element
                }

                if (maxFrameworkVersionNumsList.Count == 1)//If there is only one num in version
                {
                    //then add warning about that and add 0 as a second number to make it in a correct format for checks.
                    maxFrameworkVersionNumsList.Add(0);
                    maxFrameworkVersionDeviantValueWarningList.Add(new MaxFrameworkVersionDeviantValueWarning(errorLevel, projName, maxFrameworkVersionElementSplited[1]));
                }

                if (maxFrameworkDictionary.ContainsKey(maxFrameworkVersionElementSplited[0]))//If there are several template elements with the same type
                { //then add error about that and return an empty dictionary
                    if (maxFrameworkVersionDeviantValueErrorList.Find(error => error.ErrorLevel == errorLevel && error.ErrorRelevantProjectName == projName) == null)
                        maxFrameworkVersionDeviantValueErrorList.Add(new MaxFrameworkVersionDeviantValueError(errorLevel, projName, true));

                    break;
                }

                //If everything is correct, then add this template element with version numbers to the dictionary
                maxFrameworkDictionary.Add(maxFrameworkVersionElementSplited[0], maxFrameworkVersionNumsList);
            }

            return maxFrameworkDictionary;
        }
    }
}