using System.Collections.Generic;

namespace RefDepGuard
{
    /// <summary>
    /// It's a DTO model for the solution config file instances 
    /// </summary>
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
