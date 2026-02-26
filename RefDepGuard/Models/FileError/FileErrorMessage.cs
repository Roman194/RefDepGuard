
namespace RefDepGuard.Data
{
    public class FileErrorMessage
    {
        public string BadDataErrorMessage;
        public string FileNotFoundErrorMessage;

        public FileErrorMessage(string badDataErrorMessage, string fileNotFoundErrorMessage)
        {
            BadDataErrorMessage = badDataErrorMessage;
            FileNotFoundErrorMessage = fileNotFoundErrorMessage;
        }
    }
}
