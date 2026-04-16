using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace RefDepGuard
{
    /// <summary>
    /// Helper class to manage message boxes and yes no prompts in the extension.
    /// </summary>

    public class MessageManager
    {
        /// <summary>
        /// Helper method to show a message box with an OK button and an information icon.
        /// </summary>
        /// <param name="serviceProvider">IServiceProvider interface value</param>
        /// <param name="message">The message for MessageBox</param>
        /// <param name="title">The title for MessageBox</param>
        public static void ShowMessageBox(IServiceProvider serviceProvider, string message, string title)
        {
            VsShellUtilities.ShowMessageBox(
                    serviceProvider,
                    message,
                    title,
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        /// <summary>
        /// Helper method to show a yes/no prompt with a question icon and return true if the user clicks yes.
        /// </summary>
        /// <param name="uiShell">IVsUIShell interface value</param>
        /// <param name="message">The message for prompt</param>
        /// <param name="title">The title for prompt</param>
        /// <returns>User agreement/disagreement on offered action</returns>

        public static bool ShowYesNoPrompt(IVsUIShell uiShell, string message, string title)
        {
            if (VsShellUtilities.PromptYesNo(message, title, OLEMSGICON.OLEMSGICON_INFO, uiShell))
            {
                return true;
            }

            return false;
        }
    }
}
