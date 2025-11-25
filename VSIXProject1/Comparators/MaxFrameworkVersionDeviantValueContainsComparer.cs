using System.Collections.Generic;
using VSIXProject1.Data.FrameworkVersion;

namespace VSIXProject1
{
    public class MaxFrameworkVersionDeviantValueContainsComparer : IEqualityComparer<MaxFrameworkVersionDeviantValueError>
    {
        public bool Equals(MaxFrameworkVersionDeviantValueError x, MaxFrameworkVersionDeviantValueError y)
        {
            return x.ErrorLevel == y.ErrorLevel && x.ErrorRelevantProjectName == y.ErrorRelevantProjectName;
        }

        public int GetHashCode(MaxFrameworkVersionDeviantValueError obj)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + obj.ErrorLevel.GetHashCode();
                hash = hash * 23 + (obj.ErrorRelevantProjectName?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}