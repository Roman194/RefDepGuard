using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.CheckRules.Models
{
    /// <summary>
    /// It's a model that shows minimal values for different compatative with netstandard refs proj types
    /// </summary>
    public class NetstandardMinProjTypeVersions
    {
        public string MinNetcoreappVer;
        public string MinNetfVer;
        public string MinUapVer;

        /// <param name="minNetcoreappVer">minimal "netcoreapp" version</param>
        /// <param name="minNetfVer">minimal "netf" version</param>
        /// <param name="minUapVer">minipal "uap" version</param>
        public NetstandardMinProjTypeVersions(string minNetcoreappVer, string minNetfVer, string minUapVer)
        {
            MinNetcoreappVer = minNetcoreappVer;
            MinNetfVer = minNetfVer;
            MinUapVer = minUapVer;
        }
    }
}