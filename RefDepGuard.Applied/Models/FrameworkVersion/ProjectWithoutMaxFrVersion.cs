using System.Collections.Generic;

namespace RefDepGuard.Applied.Models.FrameworkVersion
{
    /// <summary>
    /// It's a model that shows a project without max framework version parameter in config file, 
    /// but still it should have relevant TargetFramework versions
    /// </summary>
    public class ProjectWithoutMaxFrVersion
    {
        public string TFM;
        public List<int> TargetFrameworkNums;

        public ProjectWithoutMaxFrVersion(string tFM, List<int> targetFrameworkNums)
        {
            TFM = tFM;
            TargetFrameworkNums = targetFrameworkNums;
        }
    }
}