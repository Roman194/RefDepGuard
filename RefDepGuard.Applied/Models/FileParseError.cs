using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.Applied.Models
{
    /// <summary>
    /// It's enum that shows a one from the config file error configurations
    /// </summary>
    public enum FileParseError
    {
        None,
        Global,
        Solution,
        All
    }
}