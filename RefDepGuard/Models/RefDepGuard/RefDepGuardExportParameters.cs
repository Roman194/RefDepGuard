
namespace RefDepGuard.Data
{
    /// <summary>
    /// Shows all export parameters of the extention
    /// </summary>
    public class RefDepGuardExportParameters
    {
        public RefDepGuardFindedProblems RefDepGuardFindedProblemsData;
        public RequiredParameters RequiredParametersData;

        /// <param name="refDepGuardFindedProblems">RefDepGuardFindedProblems value</param>
        /// <param name="requiredParameters">RequiredParameters value</param>
        public RefDepGuardExportParameters(RefDepGuardFindedProblems refDepGuardFindedProblems, RequiredParameters requiredParameters)
        {
            RefDepGuardFindedProblemsData = refDepGuardFindedProblems;
            RequiredParametersData = requiredParameters;
        }
    }
}