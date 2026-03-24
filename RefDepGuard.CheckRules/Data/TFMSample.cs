using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RefDepGuard.CheckRules.Models;

namespace RefDepGuard.CheckRules.Data
{
    /// <summary>
    /// Class that contains the sample data related to Target Framework Monikers (TFMs) and their compatibility with netstandard versions.
    /// </summary>
    public class TFMSample
    {

        /// <summary>
        /// Returns a list of possible Target Framework Monikers (TFMs) that can be used in .NET projects.
        /// Also inludes "all" as a special value that can be used to indicate compatibility with all TFMs and "netf" which was added to differ .net framework and .net projects.
        /// </summary>
        /// <returns>A List of string values of possible TFMs</returns>

        public static List<string> PossibleTargetFrameworkMonikiers()
        {
            return new List<string> { "all", "net", "netstandard", "netcoreapp", "netcore", "netf", "netnano", "netmf", "sl", "wp", "uap" };
        }

        /// <summary>
        /// Returns a list of Target Framework Monikers (TFMs) that can be considered comparable to netstandard versions when determining compatibility.
        /// Importatnly, "all" is not included in this list as it is a special value that indicates compatibility with all TFMs and should be handled separately in compatibility checks.
        /// <see cref="CheckProjectReferencesOnPotentialReferencesFrameworkVersionConflict"/>
        /// <see cref="MaxFrameworkRuleChecksSubManager"/>
        /// </summary>
        /// <returns>A list of comparable with netstandard TFM-s</returns>

        public static List<string> PossibleComparableTFMsWithNetStandard()
        {
            return new List<string> { "net", "netf", "netcoreapp", "uap" };
        }

        /// <summary>
        /// Returns the nearest existing netstandard version to the given maximum framework version of the project, with rounding down 
        /// (except for cases where there is only a higher nearest version above).
        /// </summary>
        /// <param name="currentMaxFrVersion">A list of numbers of current max_framework_version</param>
        /// <returns>The nearest existing netstandard version in string and list of nums formats</returns>

        public static Tuple<string, List<int>> GetNearestExistingNetstandartVersion(List<int> currentMaxFrVersion)
        {
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

            if (currentMaxFrVersion[0] > 2) //Cases when the max_framework_version is higher than the highest existing netstandard version (2.1) - we return 2.1 as the nearest one
                return new Tuple<string, List<int>>("2.1", existingNetStdVersion.Last());

            if (currentMaxFrVersion[0] < 1) //Cases when the max_framework_version is lower than the lowest existing netstandard version (1.0) - we return 1.0 as the nearest one
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

        /// <summary>
        /// Returns a dictionary where the keys are netstandard versions and the values are the minimum required versions of comparable TFMs (net, uap, netf) 
        /// for compatibility with each netstandard version.
        /// Uses for compatibility checks in <see cref="CheckProjectReferencesOnPotentialReferencesFrameworkVersionConflict"/> in <see cref="MaxFrameworkRuleChecksSubManager"/>
        /// </summary>
        /// <returns>A Dictionary with key as a netstandard version and value as a class value of comparable TFM-s 
        /// <see cref="NetstandardMinProjTypeVersions"/></returns>

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
                ["2.1"] = new NetstandardMinProjTypeVersions("3.0", "-", "-"), //No support for uap and netf TFM-s on this netstandard version
            };
        }
    }
}