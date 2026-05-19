
namespace RefDepGuard.Applied.Models.Problem
{
    /// <summary>
    /// It's a class that shows the problem as a string with the name of the document where the problem is located. It can be used for both warnings and errors.
    /// </summary>
    public class ProblemString
    {
        public string ProblemText;
        public string DocumentName;

        public ProblemString(string problemText,  string documentName)
        {
            ProblemText = problemText;
            DocumentName = documentName;
        }
    }
}