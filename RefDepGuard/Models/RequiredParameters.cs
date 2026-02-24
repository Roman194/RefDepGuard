using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RefDepGuard.Data.FrameworkVersion;
using RefDepGuard.Data.Reference;

namespace RefDepGuard.Data
{
    public class RequiredParameters
    {
        public List<RequiredReference> RequiredReferences;
        public Dictionary<string, RequiredMaxFrVersion> MaxRequiredFrameworkVersion;

        public RequiredParameters(List<RequiredReference> requiredReferences, Dictionary<string, RequiredMaxFrVersion> maxRequiredFrameworkVersion)
        {
            RequiredReferences = requiredReferences;
            MaxRequiredFrameworkVersion = maxRequiredFrameworkVersion;
        }
    }
}
