using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefDepGuard.Data
{
    public class RefDepGuardExportParameters
    {
        public RefDepGuardFindedProblems RefDepGuardFindedProblemsData;
        public RequiredParameters RequiredParametersData;

        public RefDepGuardExportParameters(RefDepGuardFindedProblems refDepGuardFindedProblems, RequiredParameters requiredParameters)
        {
            RefDepGuardFindedProblemsData = refDepGuardFindedProblems;
            RequiredParametersData = requiredParameters;
        }
    }
}
