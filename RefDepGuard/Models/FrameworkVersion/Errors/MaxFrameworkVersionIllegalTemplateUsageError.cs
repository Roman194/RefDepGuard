
namespace RefDepGuard.Models.FrameworkVersion
{
    public class MaxFrameworkVersionIllegalTemplateUsageError
    {
        public string ProjName;
        public bool IsIllegalTFMUsageError;

        public MaxFrameworkVersionIllegalTemplateUsageError(string projName, bool isIllegalTFMUsageError)
        {
            ProjName = projName;
            IsIllegalTFMUsageError = isIllegalTFMUsageError;
        }
    }
}
