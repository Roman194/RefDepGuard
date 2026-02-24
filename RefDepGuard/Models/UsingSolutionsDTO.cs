using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefDepGuard.Models
{
    public class UsingSolutionsDTO
    {
        public string name;
        public List<string> using_solutions;
        public List<string> ignoring_solutions;
    }
}
