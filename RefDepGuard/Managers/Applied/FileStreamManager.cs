using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefDepGuard.Managers.Applied
{
    public class FileStreamManager
    {
        public static void WriteInfoToFile(string currentFile, string infoToWrite)
        {
            using (FileStream fileStream = File.Create(currentFile))
            {
                StreamWriter streamWriter = new StreamWriter(fileStream);

                streamWriter.Write(infoToWrite);

                streamWriter.Flush();
                fileStream.Flush();

                streamWriter.Close();
            }
        }

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
