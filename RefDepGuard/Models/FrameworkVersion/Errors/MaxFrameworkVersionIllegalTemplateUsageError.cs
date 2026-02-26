
namespace RefDepGuard.Models.FrameworkVersion
{
    public class MaxFrameworkVersionIllegalTemplateUsageError
    {
        public string ProjName;

        public MaxFrameworkVersionIllegalTemplateUsageError(string projName)
        {
            ProjName = projName;
        }
    }
}
