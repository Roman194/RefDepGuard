using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefDepGuard.Data
{
    public class ProjectState
    {
        public Dictionary<string, List<int>> CurrentFrameworkVersions;
        public string CurrentFrameworkVersionsString;
        public List<string> CurrentReferences;
        
        public ProjectState(Dictionary<string, List<int>> currentFrameworkVersions, string currentFrameworkVersionsString, List<string> currentReferences) {
            CurrentFrameworkVersions = currentFrameworkVersions;
            CurrentFrameworkVersionsString = currentFrameworkVersionsString;
            CurrentReferences = currentReferences;
        }
    }
}
