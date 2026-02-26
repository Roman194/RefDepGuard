
namespace RefDepGuard.Models
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
