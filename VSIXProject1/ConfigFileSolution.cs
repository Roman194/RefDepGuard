using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1
{
    public class ConfigFileSolution
    {
        public string name;
        public string framework_max_version;
        public List<ConfigFileReference> global_required_references;
        public List<ConfigFileReference> global_unnacceptable_references;

        public List<ConfigFileProject> projects;
    }
}
