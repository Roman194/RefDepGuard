using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace RefDepGuard
{
    public class MessageManager
    {
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
