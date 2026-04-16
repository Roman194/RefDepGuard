using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.Applied.Models.RefDepGuard
{
    /// <summary>
    /// Shows all export parameters of the extention
    /// </summary>
    public class RefDepGuardExportParameters
    {
        public RefDepGuardFindedProblems RefDepGuardFindedProblemsData;
        public RequiredExportParameters RequiredParametersData;

        /// <param name="refDepGuardFindedProblems">RefDepGuardFindedProblems value</param>
        /// <param name="requiredParameters">RequiredParameters value</param>
        public RefDepGuardExportParameters(RefDepGuardFindedProblems refDepGuardFindedProblems, RequiredExportParameters requiredParameters)
        {
            RefDepGuardFindedProblemsData = refDepGuardFindedProblems;
            RequiredParametersData = requiredParameters;
        }
    }
}