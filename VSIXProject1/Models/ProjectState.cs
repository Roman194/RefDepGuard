using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1.Data
{
    public class ProjectState
    {
        public string CurrentFrameworkVersions;
        public List<string> CurrentReferences;
        
        public ProjectState(string currentFrameworkVersions, List<string> currentReferences) {
            CurrentFrameworkVersions = currentFrameworkVersions;
            CurrentReferences = currentReferences;
        }
    }
}
