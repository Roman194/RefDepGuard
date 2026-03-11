
namespace RefDepGuard.Data
{
    /// <summary>
    /// Shows a possible file error messages for a current config file
    /// </summary>
    public class FileErrorMessage
    {
        public string BadDataErrorMessage;
        public string FileNotFoundErrorMessage;

        /// <param name="badDataErrorMessage">syntax error string message</param>
        /// <param name="fileNotFoundErrorMessage">file not found string message</param>
        public FileErrorMessage(string badDataErrorMessage, string fileNotFoundErrorMessage)
        {
            BadDataErrorMessage = badDataErrorMessage;
            FileNotFoundErrorMessage = fileNotFoundErrorMessage;
        }
    }
}