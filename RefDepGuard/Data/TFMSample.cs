using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RefDepGuard.Models;
using VSLangProj80;

namespace RefDepGuard.Data
{
    public class TFMSample
    {

        public static List<string> PossibleTargetFrameworkMonikiers()
        {
            return new List<string> { "all", "net", "netstandard", "netcoreapp", "netcore", "netf", "netnano", "netmf", "sl", "wp", "uap" };
        }

        public static List<string> PossibleComparableTFMsWithNetStandard() //all не рассматривается!
        {
            return new List<string> { "net", "netf", "netcoreapp", "uap" };
        }

        public static Tuple<string, List<int>> GetNearestExistingNetstandartVersion(List<int> currentMaxFrVersion)
        { //Выдаёт ближайшую существующую netstandard версию с округлением вниз (кроме случаев когда есть ближайшая только сверху)
            var existingNetStdVersion = new List<List<int>> {
                new List<int> { 1, 0},
                new List<int> { 1, 1},
                new List<int> { 1, 2},
                new List<int> { 1, 3},
                new List<int> { 1, 4},
                new List<int> { 1, 5},
                new List<int> { 1, 6},
                new List<int> { 2, 0},
                new List<int> { 2, 1}
            };

            if (currentMaxFrVersion[0] > 2)
                return new Tuple<string, List<int>>("2.1", existingNetStdVersion.Last());

            if (currentMaxFrVersion[0] < 1)
                return new Tuple<string, List<int>>("1.0", existingNetStdVersion.First());

            var minDelta = existingNetStdVersion
                .Where(el => el[0] == currentMaxFrVersion[0])
                .Min(el => Math.Abs(el[1] - currentMaxFrVersion[1])
                );

            var nearestNetStdVersion = existingNetStdVersion.First(el => 
                el[0] == currentMaxFrVersion[0] &&
                minDelta == Math.Abs(el[1] - currentMaxFrVersion[1]));

            return new Tuple<string, List<int>>(nearestNetStdVersion[0] + "." + nearestNetStdVersion[1], nearestNetStdVersion);
        }

        public static Dictionary<string, NetstandardMinProjTypeVersions> MinProjTypeVersionsPerNetstandardVersion()
        {
            return new Dictionary<string, NetstandardMinProjTypeVersions>
            {
                ["1.0"] = new NetstandardMinProjTypeVersions("0", "4.5", "8.0"),
                ["1.1"] = new NetstandardMinProjTypeVersions("0", "4.5", "8.0"),
                ["1.2"] = new NetstandardMinProjTypeVersions("0", "4.5.1", "8.1"),
                ["1.3"] = new NetstandardMinProjTypeVersions("0", "4.6", "10.0"),
                ["1.4"] = new NetstandardMinProjTypeVersions("0", "4.6.1", "10.0"),
                ["1.5"] = new NetstandardMinProjTypeVersions("0", "4.6.1", "10.0.16299"),
                ["1.6"] = new NetstandardMinProjTypeVersions("0", "4.6.1", "10.0.16299"),
                ["2.0"] = new NetstandardMinProjTypeVersions("2.0", "4.6.1", "10.0.16299"),
                ["2.1"] = new NetstandardMinProjTypeVersions("3.0", "-", "-"), //Нет поддержки для uap и netf
            };
        }
    }
}
