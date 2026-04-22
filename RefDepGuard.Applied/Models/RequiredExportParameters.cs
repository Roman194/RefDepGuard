using RefDepGuard.Applied.Models.FrameworkVersion;
using RefDepGuard.Applied.Models.Reference;
using System.Collections.Generic;

namespace RefDepGuard.Applied.Models
{
    /// <summary>
    /// This model shows required parameters: references and max_req_fr_versions
    /// </summary>
    public class RequiredExportParameters
    {
        public List<RequiredReference> RequiredReferences;
        public Dictionary<string, List<RequiredMaxFrVersion>> MaxRequiredFrameworkVersion;

        /// <param name="requiredReferences">list of RequiredReference objects</param>
        /// <param name="maxRequiredFrameworkVersion">dictionary of RequiredMaxFrVersion objects</param>
        public RequiredExportParameters(List<RequiredReference> requiredReferences, Dictionary<string, List<RequiredMaxFrVersion>> maxRequiredFrameworkVersion)
        {
            RequiredReferences = requiredReferences;
            MaxRequiredFrameworkVersion = maxRequiredFrameworkVersion;
        }
    }
}