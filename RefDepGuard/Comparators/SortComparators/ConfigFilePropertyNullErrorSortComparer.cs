using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RefDepGuard.Data.Reference;

namespace RefDepGuard.Comparators
{
    public class ConfigFilePropertyNullErrorSortComparer : IComparer<ConfigFilePropertyNullError>
    {
        public int Compare(ConfigFilePropertyNullError x, ConfigFilePropertyNullError y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int isGlobalCompare = y.IsGlobal.CompareTo(x.IsGlobal);
            if (isGlobalCompare != 0) //Если у обоих ISGlobal = true
                return isGlobalCompare;

            
            bool xIsEmpty = string.IsNullOrEmpty(x.ErrorRelevantProjectName);
            bool yIsEmpty = string.IsNullOrEmpty(y.ErrorRelevantProjectName);

            if (xIsEmpty && !yIsEmpty) return -1; 
            if (!xIsEmpty && yIsEmpty) return 1;

            return 0; // считаем равными если обе строки пустые или обе не пустые
        }
    }
}
