using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1.Models.FrameworkVersion
{
    public class MaxFrameworkVersionIllegalTemplateUsageError
    {
        public string ProjName;

        public MaxFrameworkVersionIllegalTemplateUsageError(string projName)
        {
            ProjName = projName;
        }
    }
}
