using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1
{
    public class ConfigFileSolutionDTO
    {
        public string name;
        public string framework_max_version;
        public bool report_on_transit_references;
        public List<string> solution_required_references;
        public List<string> solution_unacceptable_references;

        public Dictionary<string, ConfigFileProjectDTO> projects;
    }
}
