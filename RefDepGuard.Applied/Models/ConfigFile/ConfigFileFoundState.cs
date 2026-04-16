using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.Applied.Models.ConfigFile
{
    public class ConfigFileFoundState
    {
        public bool Solution;
        public bool Global;

        public ConfigFileFoundState(bool solution, bool global)
        {
            Solution = solution;
            Global = global;
        }
    }
}