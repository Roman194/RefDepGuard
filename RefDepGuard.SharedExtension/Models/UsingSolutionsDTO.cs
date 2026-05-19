using System.Collections.Generic;

namespace RefDepGuard.Models
{
    /// <summary>
    /// It's a DTO model for a settings of the extention. Uses for Newtonsoft.Json
    /// </summary>
    public class UsingSolutionsDTO
    {
        public string name;
        public List<string> using_solutions;
        public List<string> ignoring_solutions;
    }
}