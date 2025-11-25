using HtmlAgilityPack;
using System.Collections.Generic;
using System.IO;
using VSIXProject1.Data;
using VSIXProject1.Data.FrameworkVersion;
using VSIXProject1.Data.Reference;

namespace VSIXProject1
{
    public class HTMLSubManager
    {

        public static void LoadReferencesDataToGraphicReport(string solutionName, string solutionAddress, string currentReportDirectory, 
            Dictionary<string, ProjectState> commitedProjectsState, RefDepGuardErrors refDepGuardErrors, RefDepGuardWarnings refDepGuardWarning, 
            RequiredParameters requiredExportParameters) 
        {
            string generatedHtml = GetCurrentHTMLCode(commitedProjectsState, refDepGuardErrors, refDepGuardWarning, requiredExportParameters);

            StreamWriter sw = new StreamWriter(currentReportDirectory + "\\" + solutionName + "_references_report.html");
            sw.Write(generatedHtml);

            sw.Flush();
            sw.Close();
        }

        private static string GetCurrentHTMLCode(Dictionary<string, ProjectState> commitedProjectsState, RefDepGuardErrors refDepGuardErrors, RefDepGuardWarnings refDepGuardWarning, RequiredParameters requiredExportParameters)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            HtmlNode divNode = htmlDoc.CreateElement("div");

            HtmlNode preNode = htmlDoc.CreateElement("pre");
            preNode.InnerHtml = GetCurrentMermaidCode(commitedProjectsState, refDepGuardErrors, refDepGuardWarning, requiredExportParameters);
            preNode.AddClass("mermaid");

            HtmlNode scriptNode = htmlDoc.CreateElement("script");
            scriptNode.InnerHtml = "import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs';";
            scriptNode.AddClass("module");

            divNode.AppendChild(preNode);
            divNode.AppendChild(scriptNode);

            return divNode.OuterHtml.Replace("class=\"module\"", "type=\"module\""); //на момент написания кода либа не даёт возможности задавать тип нода (только его читать), поэтому реализовано такое ухищрение
        }

        private static string GetCurrentMermaidCode(Dictionary<string, ProjectState> commitedProjectsState, RefDepGuardErrors refDepGuardErrors, RefDepGuardWarnings refDepGuardWarnings, RequiredParameters requiredExportParameters)
        {
            string outputMermaidCode = "flowchart LR\r\n";
            Dictionary <string, string> projectNameToNodeIdCompare = new Dictionary<string, string>();

            List<RequiredReference> requiredReferences = requiredExportParameters.RequiredReferences;
            List<ReferenceError> refErrors = refDepGuardErrors.RefsErrorList;
            List<MaxFrameworkVersionDeviantValueError> maxFrVersionDeviantValuesList = refDepGuardErrors.MaxFrameworkVersionDeviantValueList;
            List<FrameworkVersionComparabilityError> projectComparabilityError = refDepGuardErrors.FrameworkVersionComparabilityErrorList;
            List<MaxFrameworkVersionReferenceConflictWarning> maxFrVersionRefConflictWarning = refDepGuardWarnings.MaxFrameworkVersionReferenceConflictWarningsList;

            int currentNodeNum = 0;
            foreach (var currentProject in commitedProjectsState) //Сначала задаём сами ноды (проекты)
            {
                var currentProjectName = currentProject.Key;
                var currentProjectMaxFrVersion = new RequiredMaxFrVersion("", ErrorLevel.Project);
                var currentProjectTargetFrVersion = currentProject.Value.CurrentFrameworkVersion;

                var currentProjectMaxFrVersionString = "";

                if (maxFrVersionDeviantValuesList.Find(value => value.ErrorRelevantProjectName == currentProjectName) != null ||
                    maxFrVersionDeviantValuesList.Find(value => value.ErrorRelevantProjectName == "") != null)
                {
                    currentProjectMaxFrVersionString = "?";
                }
                else
                {
                    if (requiredExportParameters.MaxRequiredFrameworkVersion.ContainsKey(currentProjectName))
                    {
                        currentProjectMaxFrVersion = requiredExportParameters.MaxRequiredFrameworkVersion[currentProjectName];
                        currentProjectMaxFrVersionString = "Max: " + currentProjectMaxFrVersion.VersionText;

                        switch (currentProjectMaxFrVersion.ErrorLevel)
                        {
                            case ErrorLevel.Global: currentProjectMaxFrVersionString += " G"; break;
                            case ErrorLevel.Solution: currentProjectMaxFrVersionString += " S"; break;
                        }
                    }
                }

                var nodeId = "node_" + currentNodeNum;
                outputMermaidCode += GetProjectNode(nodeId, currentProjectName, currentProjectTargetFrVersion, currentProjectMaxFrVersionString);
                projectNameToNodeIdCompare.Add(currentProjectName, nodeId);

                var projectError = projectComparabilityError.Find(value => value.ErrorRelevantProjectName == currentProjectName);
                if (projectError != null)
                {
                    outputMermaidCode += SetErrorProjectStyle(nodeId);
                }

                currentNodeNum++;
            }

            currentNodeNum = 0;
            int currentRefNum = 0;
            foreach (var currentProject in commitedProjectsState)
            {
                var currentProjectName = currentProject.Key;
                var currentProjectRefsList = currentProject.Value;
                string currentNodeId = "node_" + currentNodeNum;
                foreach(var currentProjectRef in currentProjectRefsList.CurrentReferences) //Затем для каждого из нодов задаём все связи
                {
                    if (projectNameToNodeIdCompare.ContainsKey(currentProjectRef))
                    {
                        string refNodeId = projectNameToNodeIdCompare[currentProjectRef];
                        string errorText = "";

                        var currentError = refErrors.Find(value => value.ReferenceName == currentProjectRef && value.ErrorRelevantProjectName == currentProjectName);
                        var currentRefError = maxFrVersionRefConflictWarning.Find(value => value.RefName == currentProjectRef && value.ProjName == currentProjectName);
                        if (currentError != null) //Поиск на соответствие среди ошибок
                        {
                            errorText = "Запрещённая связь!";
                        }
                        else
                        {
                            if (currentRefError != null)
                            {
                                errorText = "Потенциальный конфликт версий!";
                            }
                        }

                        outputMermaidCode += GetProjectLink(currentNodeId, refNodeId, errorText);

                        if (currentError != null)
                        {
                            outputMermaidCode += SetErrorLinkStyle(currentRefNum);
                        }
                        else
                        {
                            if (currentRefError != null)
                            {
                                outputMermaidCode += SetWarningLinkStyle(currentRefNum);
                            }
                            else
                            {
                                //Поиск на соответствие среди обязательных
                                var reqRef = requiredReferences.Find(value => value.ReferenceName == currentProjectRef && (value.RelevantProject == currentProjectName || value.RelevantProject == ""));
                                if (reqRef != null)
                                {
                                    outputMermaidCode += SetRequiredPrLinkStyle(currentRefNum);
                                }
                            }
                        }

                        currentRefNum++;
                    }
                }
                currentNodeNum++;
            }

            foreach (var refError in refErrors)
            {
                if (refError.IsReferenceRequired)
                {
                    if (projectNameToNodeIdCompare.ContainsKey(refError.ReferenceName))
                    {
                        string currentNodeId = projectNameToNodeIdCompare[refError.ErrorRelevantProjectName];
                        string refNodeId = projectNameToNodeIdCompare[refError.ReferenceName];

                        outputMermaidCode += GetProjectLink(currentNodeId, refNodeId, "Отсутсвует обязательная связь!");
                        outputMermaidCode += SetErrorLinkStyle(currentRefNum);

                        currentRefNum++;

                    }
                }
            }

            return outputMermaidCode;
        }

        private static string GetProjectNode(string nodeId, string projectName, string projectTargetFrVersion, string projectMaxFrVersion)
        {
            if(projectMaxFrVersion == "")
                return nodeId + "[**" + projectName + "**\r\n" + "   " + projectTargetFrVersion + "]\r\n";
            else
                return nodeId + "[**" + projectName + "**\r\n" + "   " + projectTargetFrVersion + "\r\n   " + projectMaxFrVersion + "]\r\n";
        }

        private static string GetProjectLink(string startNodeId, string endNodeId, string errorText)
        {
            if(errorText != "")
                return startNodeId + " --> |\"" + errorText + "\"| " + endNodeId + "\r\n";
            else
                return startNodeId + " --> " + endNodeId + "\r\n";
        }

        private static string SetRequiredPrLinkStyle(int i)
        {
            return "linkStyle " + i + " stroke-width:4px;\r\n";
        }

        private static string SetErrorLinkStyle(int i)
        {
            return "linkStyle " + i + " stroke:red,stroke-width:4px,color:red;\r\n";
        }

        private static string SetWarningLinkStyle(int i)
        {
            return "linkStyle " + i + " stroke:orange,stroke-width:4px,color:orange;\r\n";
        }

        private static string SetErrorProjectStyle(string nodeId)
        {
            return "style " + nodeId + " stroke:red,stroke-width:2px, color: red;\r\n";
        }
    }
}
