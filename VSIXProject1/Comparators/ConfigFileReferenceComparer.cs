using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1
{
    public class ConfigFileReferenceComparer : IEqualityComparer<ConfigFileReference>
    {
        public bool Equals(ConfigFileReference x, ConfigFileReference y)
        {
            return x.reference == y.reference;
        }

        public int GetHashCode(ConfigFileReference obj)
        {
            return obj.reference.GetHashCode();
        }
    }
}
