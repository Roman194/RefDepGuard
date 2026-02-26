using System.Collections.Generic;

namespace RefDepGuard.Models
{
    public class UsingSolutionsDTO
    {
        public string name;
        public List<string> using_solutions;
        public List<string> ignoring_solutions;
    }
}
