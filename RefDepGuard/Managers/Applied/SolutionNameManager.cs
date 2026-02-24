using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace RefDepGuard.Managers.Applied
{
    public class SolutionNameManager
    {
        private static string packageExtendedName;
        private static string solutionName;
        private static string solutionExtendedName;

        public static void SetSolutionNameInfoInRightFormat(DTE dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string dteSolutionFullName = dte.Solution.FullName;
            int lastDotIndex = dteSolutionFullName.LastIndexOf('.');
            int lastSlashIndex = dteSolutionFullName.LastIndexOf('\\');

            solutionExtendedName = dteSolutionFullName.Substring(0, lastDotIndex);
            packageExtendedName = dteSolutionFullName.Substring(0, lastSlashIndex);
            solutionName = dteSolutionFullName.Substring(lastSlashIndex + 1, lastDotIndex - lastSlashIndex - 1);
        }
        public static string GetPackageName()
        {
            return packageExtendedName;
        }
        public static string GetSolutionName()
        {
            return solutionName;
        }
        public static string GetSolutionExtendedName()
        {
            return solutionExtendedName;
        }
    }
}
