using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.Applied.Models.FrameworkVersion
{
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