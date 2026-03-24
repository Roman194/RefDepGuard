using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefDepGuard.Console.Managers
{
    public class FileStreamManager
    {
        public static string ReadInfoFromFile(string currentFile)
        {
            string currentFileContent = "";

            using (FileStream fileStream = new FileStream(currentFile, FileMode.Open))
            {
                StreamReader sr = new StreamReader(fileStream);
                currentFileContent = sr.ReadToEnd();
            }

            return currentFileContent;
        }
    }
}
