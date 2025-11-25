using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject1.Data.FrameworkVersion;
using VSIXProject1.Data.Reference;

namespace VSIXProject1.Data
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
