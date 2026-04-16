using System;
using System.Collections.Generic;
using System.Text;

namespace RefDepGuard.Applied.Models.Problem
{
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