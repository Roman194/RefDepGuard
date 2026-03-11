using System.Collections.Generic;
using RefDepGuard.Data.FrameworkVersion;
using RefDepGuard.Data.Reference;

namespace RefDepGuard.Data
{
    /// <summary>
    /// This model shows required parameters: references and max_req_fr_versions
    /// </summary>
    public class RequiredParameters
    {
        public List<RequiredReference> RequiredReferences;
        public Dictionary<string, RequiredMaxFrVersion> MaxRequiredFrameworkVersion;

        /// <param name="requiredReferences">list of RequiredReference objects</param>
        /// <param name="maxRequiredFrameworkVersion">dictionary of RequiredMaxFrVersion objects</param>
        public RequiredParameters(List<RequiredReference> requiredReferences, Dictionary<string, RequiredMaxFrVersion> maxRequiredFrameworkVersion)
        {
            RequiredReferences = requiredReferences;
            MaxRequiredFrameworkVersion = maxRequiredFrameworkVersion;
        }
    }
}
