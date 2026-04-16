using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.Applied.Models.ConfigFile.DTO
{
    /// <summary>
    /// It's a DTO that shows a config file projects instances
    /// </summary>
    public class ConfigFileProjectDTO
    {
        public string framework_max_version;
        public bool report_on_transit_references;
        public ConfigFileProjectRefsConsideringDTO consider_global_and_solution_references;
        public List<string> required_references;
        public List<string> unacceptable_references;
    }
}