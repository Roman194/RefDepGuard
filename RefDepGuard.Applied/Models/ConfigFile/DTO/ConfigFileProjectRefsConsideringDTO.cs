using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.Applied.Models.ConfigFile.DTO
{
    /// <summary>
    /// Shows a part of the solution config file DTO which is responsible for "consider_global_and_solution_references"
    /// </summary>
    public class ConfigFileProjectRefsConsideringDTO
    {
        public bool required;
        public bool unacceptable;
    }
}