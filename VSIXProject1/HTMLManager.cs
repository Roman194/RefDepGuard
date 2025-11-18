using EnvDTE;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using VSIXProject1.Data;
using VSIXProject1.Data.FrameworkVersion;
using VSIXProject1.Data.Reference;

namespace VSIXProject1
{
    public class HTMLManager
    {

        public static bool LoadReferencesDataToGraphicReport(string solutionName, string solutionAddress, Dictionary<string, ProjectState> commitedProjectsState, RefDepGuardErrors refDepGuardErrors, RequiredExportParameters requiredExportParameters) 
            //Сделать общий export manager? solutionName из solutionAddress?
        {
            bool isLoadSuccessful = true;
            string currentDateTime = DateTimeManager.GetCurrentDateTimeInRightFormat();

            try
            {
                string currentReportDirectory = solutionAddress + "\\reports\\graph_type\\" + currentDateTime;
                Directory.CreateDirectory(currentReportDirectory);

                string generatedHtml = GetCurrentHTMLCode(commitedProjectsState, refDepGuardErrors, requiredExportParameters);

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

        private static string GetCurrentHTMLCode(Dictionary<string, ProjectState> commitedProjectsState, RefDepGuardErrors refDepGuardErrors, RequiredExportParameters requiredExportParameters)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            HtmlNode divNode = htmlDoc.CreateElement("div");

            HtmlNode preNode = htmlDoc.CreateElement("pre");
            preNode.InnerHtml = GetCurrentMermaidCode(commitedProjectsState, refDepGuardErrors, requiredExportParameters);
            preNode.AddClass("mermaid");

            HtmlNode scriptNode = htmlDoc.CreateElement("script");
            scriptNode.InnerHtml = "import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs';";
            scriptNode.AddClass("module");

            divNode.AppendChild(preNode);
            divNode.AppendChild(scriptNode);

            return divNode.OuterHtml.Replace("class=\"module\"", "type=\"module\""); //на момент написания кода либа не даёт возможности задавать тип нода (только его читать), поэтому реализовано такое ухищрение
        }

        private static string GetCurrentMermaidCode(Dictionary<string, ProjectState> commitedProjectsState, RefDepGuardErrors refDepGuardErrors, RequiredExportParameters requiredExportParameters)
        {
            string outputMermaidCode = "flowchart LR\r\n";
            Dictionary <string, string> projectNameToNodeIdCompare = new Dictionary<string, string>();

            List<RequiredReference> requiredReferences = requiredExportParameters.RequiredReferences;
            List<ReferenceError> refErrors = refDepGuardErrors.RefsErrorList;
            List<FrameworkVersionComparabilityError> projectComparabilityError = refDepGuardErrors.FrameworkVersionComparabilityErrorList;

            int currentNodeNum = 0;
            foreach (var currentProject in commitedProjectsState)
            {
                var currentProjectName = currentProject.Key;
                var currentProjectMaxFrVersion = new Data.FrameworkVersion.RequiredMaxFrVersion("", ErrorLevel.Project);
                var currentProjectTargetFrVersion = currentProject.Value.CurrentFrameworkVersion;

                var currentProjectMaxFrVersionString = "-";

                if (requiredExportParameters.MaxRequiredFrameworkVersion.ContainsKey(currentProjectName))
                {
                    currentProjectMaxFrVersion = requiredExportParameters.MaxRequiredFrameworkVersion[currentProjectName];
                    currentProjectMaxFrVersionString = currentProjectMaxFrVersion.VersionText;
                }
                
                switch (currentProjectMaxFrVersion.ErrorLevel)
                {
                    case ErrorLevel.Global: currentProjectMaxFrVersionString += " G"; break;
                    case ErrorLevel.Solution: currentProjectMaxFrVersionString += " S"; break;
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

            //нужно ли сортировать по количеству рефов?

            currentNodeNum = 0;
            int currentRefNum = 0;
            foreach (var currentProject in commitedProjectsState)
            {
                var currentProjectName = currentProject.Key;
                var currentProjectRefsList = currentProject.Value;
                string currentNodeId = "node_" + currentNodeNum;
                foreach(var currentProjectRef in currentProjectRefsList.CurrentReferences)
                {
                    if (projectNameToNodeIdCompare.ContainsKey(currentProjectRef))
                    {
                        string refNodeId = projectNameToNodeIdCompare[currentProjectRef];
                        string errorText = "";

                        var currentError = refErrors.Find(value => value.ReferenceName == currentProjectRef && value.ErrorRelevantProjectName == currentProjectName);
                        if (currentError != null)
                        {
                            errorText = "Запрещённая связь!";
                        }

                        outputMermaidCode += GetProjectLink(currentNodeId, refNodeId, errorText);

                        if (currentError != null)
                        {
                            outputMermaidCode += SetErrorLinkStyle(currentRefNum);
                        }

                        var reqRef = requiredReferences.Find(value => value.ReferenceName == currentProjectRef && value.RelevantProject ==  currentProjectName);
                        if (reqRef != null) //Протетсить!
                        {
                            outputMermaidCode += SetRequiredPrLinkStyle(currentRefNum);
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

        private static string SetErrorProjectStyle(string nodeId)
        {
            return "style " + nodeId + " stroke:red,stroke-width:2px, color: red;\r\n";
        }
    }
}
