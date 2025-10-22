using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1.Data.Reference
{
    public class RefDepGuardErrors
    {
        public List<ConfigFilePropertyNullError> ConfigPropertyNullErrorList;
        public List<ReferenceError> RefsErrorList;
        public List<ReferenceMatchError> RefsMatchErrorList;

        public RefDepGuardErrors(List<ConfigFilePropertyNullError> configPropertyNullErrorList, List<ReferenceError> refsErrorList, List<ReferenceMatchError> refsMatchErrorList)
        {
            ConfigPropertyNullErrorList = configPropertyNullErrorList;
            RefsErrorList = refsErrorList;
            RefsMatchErrorList = refsMatchErrorList;
        }
    }
}
