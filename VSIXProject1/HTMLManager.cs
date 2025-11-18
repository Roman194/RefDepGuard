using System;
using System.IO;
using HtmlAgilityPack;

namespace VSIXProject1
{
    public class HTMLManager
    {

        public static bool LoadReferencesDataToGraphicReport(string solutionName, string solutionAddress) //Сделать общий export manager? solutionName из solutionAddress?
        {
            bool isLoadSuccessful = true;
            string currentDateTime = DateTimeManager.GetCurrentDateTimeInRightFormat();

            try
            {
                string currentReportDirectory = solutionAddress + "\\reports\\graph_type\\" + currentDateTime;
                Directory.CreateDirectory(currentReportDirectory);

                string generatedHtml = GetCurrentHTMLCode();

                StreamWriter sw = new StreamWriter(currentReportDirectory + "\\" + solutionName + "_references_report.html");
                sw.Write(generatedHtml);

                sw.Flush();
                sw.Close();
            }
            catch (Exception ex)
            {
                isLoadSuccessful = false;
            }

            return isLoadSuccessful;
        }

        private static string GetCurrentHTMLCode()
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            HtmlNode divNode = htmlDoc.CreateElement("div");

            HtmlNode preNode = htmlDoc.CreateElement("pre");
            preNode.InnerHtml = GetCurrentMermaidCode();
            preNode.AddClass("mermaid");

            HtmlNode scriptNode = htmlDoc.CreateElement("script");
            scriptNode.InnerHtml = "import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs';";
            scriptNode.AddClass("module");

            divNode.AppendChild(preNode);
            divNode.AppendChild(scriptNode);

            return divNode.OuterHtml.Replace("class=\"module\"", "type=\"module\""); //на момент написания кода либа не даёт возможности задавать тип нода (только его читать), поэтому реализовано такое ухищрение
        }

        private static string GetCurrentMermaidCode()
        {
            return "classDiagram\r\n    class Animal {\r\n        +int Age\r\n        +Breathe()\r\n        +Eat(Food food) Energy\r\n    }\r\n    class Dog {\r\n        +Bark(int times, int volume) Sound\r\n    }\r\n    Animal <|-- Dog : A dog is an animal";
        }
    }
}
