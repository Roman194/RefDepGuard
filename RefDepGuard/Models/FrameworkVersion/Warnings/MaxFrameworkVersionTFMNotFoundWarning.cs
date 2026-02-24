using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefDepGuard.Models.FrameworkVersion
{
    public class MaxFrameworkVersionTFMNotFoundWarning
    {
        public string TFMName;
        public ProblemLevel WarningLevel;
        public string ProjName;

        public MaxFrameworkVersionTFMNotFoundWarning(string tFMName,  ProblemLevel warningLevel, string projName)
        {
            TFMName = tFMName;
            WarningLevel = warningLevel;
            ProjName = projName;
        }
    }
}
