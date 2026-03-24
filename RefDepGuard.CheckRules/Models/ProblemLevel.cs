using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.CheckRules.Models
{
    /// <summary>
    /// It's enum that shows which level is relevant for a current problem/rule. It can be undefined for max_fr_ver ref conflicts
    /// </summary>
    public enum ProblemLevel
    {
        Global,
        Solution,
        Project,
        Undefined
    }
}
