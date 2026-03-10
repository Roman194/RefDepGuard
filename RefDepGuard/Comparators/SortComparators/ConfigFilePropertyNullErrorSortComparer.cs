using System.Collections.Generic;
using RefDepGuard.Data.Reference;

namespace RefDepGuard.Comparators
{

    /// <summary>
    /// This class implements the IComparer interface to provide a custom sorting logic for ConfigFilePropertyNullError objects.
    /// <see cref="ConfigFilePropertyNullError"/> for details on the properties of the objects being compared."/>
    /// </summary>
    public class ConfigFilePropertyNullErrorSortComparer : IComparer<ConfigFilePropertyNullError>
    {
        public int Compare(ConfigFilePropertyNullError x, ConfigFilePropertyNullError y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int isGlobalCompare = y.IsGlobal.CompareTo(x.IsGlobal);
            if (isGlobalCompare != 0) //If x and y ISGlobal-s = true
                return isGlobalCompare;

            
            bool xIsEmpty = string.IsNullOrEmpty(x.ErrorRelevantProjectName);
            bool yIsEmpty = string.IsNullOrEmpty(y.ErrorRelevantProjectName);

            if (xIsEmpty && !yIsEmpty) return -1; 
            if (!xIsEmpty && yIsEmpty) return 1;

            return 0; // both srings are empty or both are not empty - we consider them equal in terms of sorting
        }
    }
}
