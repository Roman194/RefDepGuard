using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1.Models
{
    public class NetstandardMinProjTypeVersions
    {
        public string MinNetcoreappVer;
        public string MinNetfVer;
        public string MinUapVer;

        public NetstandardMinProjTypeVersions(string minNetcoreappVer, string minNetfVer, string minUapVer)
        {
            MinNetcoreappVer = minNetcoreappVer;
            MinNetfVer = minNetfVer;
            MinUapVer = minUapVer;
        }
    }
}
