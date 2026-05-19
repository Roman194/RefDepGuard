
/// <summary>
/// Shows the state of the config file search for both files (Global / current solution).
/// </summary>
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