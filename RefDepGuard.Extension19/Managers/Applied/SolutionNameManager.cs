using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace RefDepGuard.Managers.Applied
{

    /// <summary>
    /// This class manages the solution name information for the extension. 
    /// It extracts and stores the package extended name, solution name, and solution extended name from the DTE object in string formats.
    /// </summary>
    public class SolutionNameManager
    {
        private static string packageExtendedName;
        private static string solutionName;
        private static string solutionExtendedName;

        /// <summary>
        /// Extracts and stores the package extended name, solution name, and solution extended name from the DTE object in string formats.
        /// </summary>
        /// <param name="dte">DTE interface value</param>
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

        /// <summary>
        /// Gets the package extended name, which is the part of the solution's full name up to the last slash, representing the directory path of the solution.
        /// </summary>
        /// <returns>String value of package extended name</returns>
        public static string GetPackageName()
        {
            return packageExtendedName;
        }

        /// <summary>
        /// Gets the solution name, which is the part of the solution's full name, representing the name of the solution without its file extension 
        /// and a path to a file.
        /// </summary>
        /// <returns>String value of solution name</returns>
        public static string GetSolutionName()
        {
            return solutionName;
        }

        /// <summary>
        /// Gets the solution extended name, representing the full path of the solution without its file extension but with its absolute path.
        /// </summary>
        /// <returns></returns>
        public static string GetSolutionExtendedName()
        {
            return solutionExtendedName;
        }
    }
}
