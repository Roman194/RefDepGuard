using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject1.Data.FrameworkVersion;
using VSIXProject1.Data.Reference;

namespace VSIXProject1.Data
{
    public class RequiredExportParameters
    {
        public List<RequiredReference> RequiredReferences;
        public Dictionary<string, RequiredMaxFrVersion> MaxRequiredFrameworkVersion;

        public RequiredExportParameters(List<RequiredReference> requiredReferences, Dictionary<string, RequiredMaxFrVersion> maxRequiredFrameworkVersion)
        {
            RequiredReferences = requiredReferences;
            MaxRequiredFrameworkVersion = maxRequiredFrameworkVersion;
        }
    }
}
