using System.IO;

namespace RefDepGuard.Managers.Applied
{
    /// <summary>
    /// This class provides methods to read from and write to files using FileStream. 
    /// It includes a method to write a string to a specified file and another method to read the entire content of a specified file and return it as a string.
    /// </summary>
    public class FileStreamManager
    {
        /// <summary>
        /// Writes the provided string information to a specified file. If the file already exists, it will be overwritten.
        /// </summary>
        /// <param name="currentFile">The absolute path to a current file that will be created or ovwerwritten</param>
        /// <param name="infoToWrite">Info that will be written to a file in string format</param>
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

        /// <summary>
        /// Reads the entire content of a specified file and returns it as a string. If the file does not exist, an exception will be thrown.
        /// Warning: This metod should be called only in contecsts where the exception is handled
        /// </summary>
        /// <param name="currentFile">The absolute path to a current file that will be readed</param>
        /// <returns>A readed info in a string format</returns>
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
