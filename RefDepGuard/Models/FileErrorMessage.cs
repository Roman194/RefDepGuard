using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
