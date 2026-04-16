using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.Applied.Models.ConfigFile.DTO
{
    /// <summary>
    /// It's a DTO for the global config file instances
    /// </summary>
    public class ConfigFileGlobalDTO
    {
        public string name;
        public string framework_max_version;
        public bool report_on_transit_references;
        public List<string> global_required_references;
        public List<string> global_unacceptable_references;
    }
}