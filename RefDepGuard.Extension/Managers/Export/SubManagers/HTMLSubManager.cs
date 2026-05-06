using HtmlAgilityPack;
using RefDepGuard.Applied;
using RefDepGuard.Applied.Models.ConfigFile;
using RefDepGuard.Applied.Models.FrameworkVersion;
using RefDepGuard.Applied.Models.FrameworkVersion.Errors;
using RefDepGuard.Applied.Models.FrameworkVersion.Warnings.Conflicts;
using RefDepGuard.Applied.Models.Problem;
using RefDepGuard.Applied.Models.Project;
using RefDepGuard.Applied.Models.RefDepGuard;
using RefDepGuard.Applied.Models.Reference;
using RefDepGuard.Applied.Models.Reference.Errors;
using RefDepGuard.UI.Resources.StringResources;
using System.Collections.Generic;
using System.Linq;

namespace RefDepGuard
{
    /// <summary>
    /// This class is responsible for managing the generation of HTML report with graphic representation of the references between projects of the solution 
    /// (dependency graph).
    /// </summary>
    public class HTMLSubManager
    {
        /// <summary>
        /// Main method of the SubManager. Gets the current HTML code in string format and redirect it to write it to a report file.
        /// </summary>
        /// <param name="configFilesData">ConfigFilesData value</param>
        /// <param name="currentReportDirectory">current report directory string</param>
        /// <param name="commitedProjectsState">commited projects state dictionary</param>
        /// <param name="refDepGuardExportParameters">RefDepGuardExportParameters value</param>
        public static void LoadReferencesDataToGraphicReport(ConfigFilesData configFilesData, string currentReportDirectory, Dictionary<string, ProjectState> commitedProjectsState, 
            RefDepGuardExportParameters refDepGuardExportParameters) 
        {
            string generatedHtml = GetCurrentHTMLCode(commitedProjectsState, refDepGuardExportParameters);
            string currentReportFile = currentReportDirectory + "\\" + configFilesData.SolutionName + "_references_report.html";

            FileStreamManager.WriteInfoToFile(currentReportFile, generatedHtml);
        }

        /// <summary>
        /// Generates the current HTML code in string format with the usage of standart template and current Mermaid code.
        /// </summary>
        /// <param name="commitedProjectsState">commited projects state dictionary</param>
        /// <param name="refDepGuardExportParameters">RefDepGuardExportParameters value</param>
        /// <returns>the HTML code string</returns>
        private static string GetCurrentHTMLCode(Dictionary<string, ProjectState> commitedProjectsState, RefDepGuardExportParameters refDepGuardExportParameters)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            HtmlNode divNode = htmlDoc.CreateElement("div");

            HtmlNode preNode = htmlDoc.CreateElement("pre");
            preNode.InnerHtml = GetCurrentMermaidCode(commitedProjectsState, refDepGuardExportParameters);
            preNode.AddClass("mermaid");

            HtmlNode scriptNode = htmlDoc.CreateElement("script");
            scriptNode.InnerHtml = "import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs';";
            scriptNode.AddClass("module");

            divNode.AppendChild(preNode);
            divNode.AppendChild(scriptNode);

            //on the moment of writing the code, the library used for HTML generation doesn't allow to set the type of the node (only to read it),
            //so such a trick is implemented
            return divNode.OuterHtml.Replace("class=\"module\"", "type=\"module\"");
        }

        /// <summary>
        /// Generates the current Mermaid code in string format based on the commited projects state and RefDepGuardExportParameters values.
        /// </summary>
        /// <param name="commitedProjectsState">commited projects state dictionary</param>
        /// <param name="refDepGuardExportParameters">RefDepGuardExportParameters value</param>
        /// <returns>The Mermaid code string</returns>
        private static string GetCurrentMermaidCode(Dictionary<string, ProjectState> commitedProjectsState, RefDepGuardExportParameters refDepGuardExportParameters)
        {
            Dictionary <string, string> projectNameToNodeIdCompare = new Dictionary<string, string>();
            RefDepGuardErrors refDepGuardErrors = refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardErrors;
            RefDepGuardWarnings refDepGuardWarnings = refDepGuardExportParameters.RefDepGuardFindedProblemsData.RefDepGuardWarnings;

            List<RequiredReference> requiredReferences = refDepGuardExportParameters.RequiredParametersData.RequiredReferences;
            Dictionary<string, List<RequiredMaxFrVersion>> requiredExportParameters = refDepGuardExportParameters.RequiredParametersData.MaxRequiredFrameworkVersion;
            List<ReferenceError> refErrors = refDepGuardErrors.RefsErrorList;
            List<ReferenceMatchError> refMatchErrors = refDepGuardErrors.RefsMatchErrorList;
            List<MaxFrameworkVersionDeviantValueError> maxFrVersionDeviantValuesList = refDepGuardErrors.MaxFrameworkVersionDeviantValueList;
            List<FrameworkVersionComparabilityError> projectComparabilityError = refDepGuardErrors.FrameworkVersionComparabilityErrorList;
            List<MaxFrameworkVersionReferenceConflictWarning> maxFrVersionRefConflictWarning = refDepGuardWarnings.MaxFrameworkVersionReferenceConflictWarningsList;

            string outputMermaidCode = "flowchart LR\r\n";

            int currentNodeNum = 0;
            foreach (var currentProject in commitedProjectsState) //For each node (project)
            {
                //generate the project visualization
                var currentProjectName = currentProject.Key;
                var currentProjectMaxFrVersion = new RequiredMaxFrVersion("", new List<int>(), ProblemLevel.Project, "", false);
                var currentProjectTargetFrVersion = currentProject.Value.CurrentFrameworkVersionsString;

                var currentProjectMaxFrVersionString = "";

                var nodeId = "node_" + currentNodeNum;
                var warningProjectStylesCode = "";
                
                if (requiredExportParameters.ContainsKey(currentProjectName))
                {//If there is a requirement for max framework version for the current project,
                 //we add it to the node description and check if there are any warnings related to set its style.

                    currentProjectMaxFrVersionString = "Max: ";

                    foreach (var currentProjectTFM in requiredExportParameters[currentProjectName])//For each project TFM
                    {
                        currentProjectMaxFrVersionString += currentProjectTFM.VersionText;

                        switch (currentProjectTFM.ReqLevel)
                        {
                            case ProblemLevel.Global: currentProjectMaxFrVersionString += " G"; break;
                            case ProblemLevel.Solution: currentProjectMaxFrVersionString += " S"; break;
                        }

                        if (requiredExportParameters[currentProjectName].Last() != currentProjectTFM)
                            currentProjectMaxFrVersionString += "; ";
                    }

                    if(currentProjectMaxFrVersion.IsConflictWarningRelevantForThisProject)
                        warningProjectStylesCode += SetWarningProjectStyle(nodeId);

                }
                else
                {
                    if (maxFrVersionDeviantValuesList.Find(value => value.ErrorRelevantProjectName == currentProjectName) != null ||
                        maxFrVersionDeviantValuesList.Find(value => value.ErrorRelevantProjectName == "") != null)
                        currentProjectMaxFrVersionString = "?";
                        
                }

                outputMermaidCode += GetProjectNode(nodeId, currentProjectName, currentProjectTargetFrVersion, currentProjectMaxFrVersionString);
                projectNameToNodeIdCompare.Add(currentProjectName, nodeId);
                outputMermaidCode += warningProjectStylesCode;

                //Check if there are any errors related to the project and set the style if there are such ones
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
                foreach(var currentProjectRef in currentProjectRefsList.CurrentReferences)//for each reference
                {
                    if (projectNameToNodeIdCompare.ContainsKey(currentProjectRef))//If the reference is a reference to another project of the solution,
                        //we generate the link between them and check if there are any errors or warnings related to this link to set its style
                    {
                        string refNodeId = projectNameToNodeIdCompare[currentProjectRef];
                        string errorText = "";

                        var currentError = refErrors.Find(value => value.ReferenceName == currentProjectRef && value.ErrorRelevantProjectName == currentProjectName);
                        var currentRefError = maxFrVersionRefConflictWarning.Find(value => value.RefName == currentProjectRef && value.ProjName == currentProjectName);
                        if (currentError != null) //Поиск на соответствие среди ошибок
                        {
                            errorText = Resource.Forbidden_Reference_Title;
                        }
                        else
                        {
                            if (currentRefError != null)
                            {
                                errorText = Resource.Potential_Version_Conflict_Title;
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
                                var reqRef = requiredReferences.Find(value => value.ReferenceName == currentProjectRef && (value.RelevantProject == currentProjectName || value.RelevantProject == ""));
                                var refMatchError = refMatchErrors.Find(value => value.ReferenceName == currentProjectRef && (value.ProjectName == currentProjectName || value.ProjectName == ""));
                                if (reqRef != null && refMatchError == null)
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

            //adds missing references
            foreach (var refError in refErrors)//for each reference error
            {
                if (refError.IsReferenceRequired)//if it is a reference required error,
                {
                    var refMatchError = refMatchErrors.Find(value => 
                        value.ReferenceName == refError.ReferenceName && 
                        (value.ProjectName == refError.ErrorRelevantProjectName || value.ProjectName == "")
                     );

                    //we check if the project and reference of this error are in the solution and if they are,
                    //we generate the link between them with the relevant error text and style
                    if (refMatchError == null && projectNameToNodeIdCompare.ContainsKey(refError.ReferenceName) )
                    {
                        string currentNodeId = projectNameToNodeIdCompare[refError.ErrorRelevantProjectName];
                        string refNodeId = projectNameToNodeIdCompare[refError.ReferenceName];

                        outputMermaidCode += GetProjectLink(currentNodeId, refNodeId, Resource.Required_Reference_Not_Found_Title);
                        outputMermaidCode += SetErrorLinkStyle(currentRefNum);

                        currentRefNum++;

                    }
                }
            }

            return outputMermaidCode;
        }

        /// <summary>
        /// Generates the Mermaid code for the project node based on the project name, its target framework version and max required framework version 
        /// (if there is such requirement for this project).
        /// </summary>
        /// <param name="nodeId">current node string id</param>
        /// <param name="projectName">node project name string</param>
        /// <param name="projectTargetFrVersion">node target framework version string</param>
        /// <param name="projectMaxFrVersion">node max_fr_ver string</param>
        /// <returns>the project node Mermaid code</returns>
        private static string GetProjectNode(string nodeId, string projectName, string projectTargetFrVersion, string projectMaxFrVersion)
        {
            if(projectMaxFrVersion == "")
                return nodeId + "[\"`**" + projectName + "**\r\n" + "   " + projectTargetFrVersion.Replace(";", "\r\n") + "`\"]\r\n";
            else
                return nodeId + "[\"`**" + projectName + "**\r\n" + "   " + projectTargetFrVersion.Replace(";", "\r\n") + "\r\n   " + projectMaxFrVersion + "`\"]\r\n";
        }

        /// <summary>
        /// Generates the Mermaid code for the link between two project nodes based on the project names and error text (if there is an error related to this link).
        /// </summary>
        /// <param name="startNodeId">current start node id string</param>
        /// <param name="endNodeId">current end node id string</param>
        /// <param name="errorText">error text string</param>
        /// <returns>the project Mermaid link code</returns>
        private static string GetProjectLink(string startNodeId, string endNodeId, string errorText)
        {
            if(errorText != "")
                return startNodeId + " --> |\"" + errorText + "\"| " + endNodeId + "\r\n";
            else
                return startNodeId + " --> " + endNodeId + "\r\n";
        }

        /// <summary>
        /// Generates the Mermaid code for the link style of the required reference link based on the link number.
        /// </summary>
        /// <param name="i">link id string</param>
        /// <returns>project link style string</returns>
        private static string SetRequiredPrLinkStyle(int i)
        {
            return "linkStyle " + i + " stroke-width:4px;\r\n";
        }

        /// <summary>
        /// Generates the Mermaid code for the link style of the erroneous link based on the link number.
        /// </summary>
        /// <param name="i">link id string</param>
        /// <returns>project link style string</returns>
        private static string SetErrorLinkStyle(int i)
        {
            return "linkStyle " + i + " stroke:red,stroke-width:4px,color:red;\r\n";
        }

        /// <summary>
        /// Generates the Mermaid code for the link style of the link with potential conflict of framework versions based on the link number.
        /// </summary>
        /// <param name="i"link id string></param>
        /// <returns>project link style string</returns>
        private static string SetWarningLinkStyle(int i)
        {
            return "linkStyle " + i + " stroke:orange,stroke-width:4px,color:orange;\r\n";
        }

        /// <summary>
        /// Generates the Mermaid code for the project node style of the project with error based on the node number.
        /// </summary>
        /// <param name="nodeId">node id string</param>
        /// <returns>error project style string</returns>
        private static string SetErrorProjectStyle(string nodeId)
        {
            return "style " + nodeId + " stroke:red,stroke-width:2px, color: red;\r\n";
        }

        /// <summary>
        /// Generates the Mermaid code for the project node style of the project with potential conflict of framework versions based on the node number.
        /// </summary>
        /// <param name="nodeId">node id string</param>
        /// <returns>warning project style string</returns>
        private static string SetWarningProjectStyle(string nodeId)
        {
            return "style " + nodeId + " stroke:orange,stroke-width:2px, color: orange;\r\n";
        }
    }
}