using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1.Data
{
    public class TFMSample
    {

        public static List<string> PossibleTargetFrameworkMonikiers()
        {
            return new List<string> { "all", "net", "netstandard", "netcoreapp", "netcore", "netf", "netnano", "netmf", "sl", "wp", "uap" };
        }
    }
}
