using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using RefDepGuard.Managers.CheckRules;
using RefDepGuard.CheckRules;
using RefDepGuard.Applied.Models.ConfigFile;
using RefDepGuard.Applied.Models.Project;
using RefDepGuard.Applied.Models.RefDepGuard;

namespace RefDepGuard
{
    /// <summary>
    /// This class is responsible for managing the checking of rules based on the configuration files and the current state of the projects in the solution.
    /// </summary>
    public class CheckRulesExtentionManager
    {
        /// <summary>
        /// This is the main method of checking rules module.
        /// It is responsible for checking the rules based on the configuration files and the current state of the projects in the solution. 
        /// It performs various checks, such as not null checks, max framework version checks, references checks, and transit references detection. 
        /// It collects all the errors and warnings found during the process and exports them to ELP together with the required parameters for other parts 
        /// of the extension.
        /// </summary>
        /// <param name="configFilesData">ConfigFilesData current commited value</param>
        /// <param name="errorListProvider">ELP class value</param>
        /// <param name="currentCommitedProjState">Current commited projects state values</param>
        /// <param name="uIShell">IVsUIShell interface value</param>
        /// <returns>RefDepGuardExportParameters and ConfigFilesData (to provide "Single source of truth" and "One flow through modules" principes)</returns>
        /// <see cref="RefDepGuardExportParameters"/>
        public static Tuple<RefDepGuardExportParameters, ConfigFilesData> CheckRulesFromConfigFiles(
            ConfigFilesData configFilesData, ErrorListProvider errorListProvider, Dictionary<string, ProjectState> currentCommitedProjState, IVsUIShell uIShell
            )
        {
            var exportParametersNConfigFilesDataTuple = CheckRulesManager.CheckConfigFileRulesForExtention(configFilesData, currentCommitedProjState);

            //Export to ELP to show all finded problems in the error list of the IDE.
            ELPStoreManager.StoreErrorListProviderByValues(exportParametersNConfigFilesDataTuple.Item1.RefDepGuardFindedProblemsData, configFilesData, errorListProvider);

            return exportParametersNConfigFilesDataTuple;
        }
    }
}