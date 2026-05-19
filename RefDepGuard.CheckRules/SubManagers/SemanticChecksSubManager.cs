using RefDepGuard.Applied.Models.Project;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RefDepGuard.CheckRules.SubManagers
{
    /// <summary>
    /// This sub-manager is responsible for semantic checks of project names, which are based on the idea that if there are 
    /// 2+ projects with the same "sema" in their names, then the other projects with similar "semas" may have typos in these "semas". 
    /// The "sema" is a part of the project name, which is separated by a dot and has 3+ characters. 
    /// </summary>
    public class SemanticChecksSubManager
    {
        private static List<ProjectNameSemanticWarning> projectNameSemanticWarningsList = new List<ProjectNameSemanticWarning>();

        /// <summary>
        /// Clears the list of project name semantic warnings. This method should be called before performing a new check.
        /// </summary>
        public static void ClearSemanticCheckLists()
        {
            if (projectNameSemanticWarningsList != null)
                projectNameSemanticWarningsList.Clear();
        }

        /// <summary>
        /// The main method of the SubManager. 
        /// Checks the project names on semantic similarity. If there are 2+ projects with the same "sema" in their names, then the other projects with similar 
        /// "semas" may have typos in these "semas".
        /// </summary>
        /// <param name="projectNames">List with all project names from current solution</param>
        public static void CheckProjectNamesSemantic(List<string> projectNames)
        {
            var findedSemasHashSet = new HashSet<string>();

            foreach(var projName in projectNames)
            {
                foreach (var otherProjName in projectNames)
                {
                    if (otherProjName != projName)
                    {
                        var projNameSemasArray = projName.Split('.');
                        var otherProjNameSemasArray = otherProjName.Split('.');

                        for(int i = 0; i < Math.Min(projNameSemasArray.Length, otherProjNameSemasArray.Length); i++)
                        {
                            var currentProjNameSema = projNameSemasArray[i];
                            var currentOtherProjNameSema = otherProjNameSemasArray[i];

                            if (currentProjNameSema == currentOtherProjNameSema)
                            {
                                //If there is some "sema" that matches for 2+ projects, then it is already a rule, so we add it to the hash set and continue
                                findedSemasHashSet.Add(currentProjNameSema);
                                continue;
                            }
                                    
                            if (currentProjNameSema.Length < 3 || currentOtherProjNameSema.Length < 3)
                                continue;

                            //If current configuration is a deviation
                            if (!findedSemasHashSet.Contains(currentProjNameSema) && findedSemasHashSet.Contains(currentOtherProjNameSema))
                            {
                                int differSymbolsCount = 0;

                                //Check on differing nonrepeating symbols
                                if (currentProjNameSema.Length >= currentOtherProjNameSema.Length)
                                    differSymbolsCount = currentProjNameSema.Except(currentOtherProjNameSema).ToList().Count;
                                else
                                    differSymbolsCount = currentOtherProjNameSema.Except(currentProjNameSema).ToList().Count;

                                //Form a dicts with the number of used symbols in the strings
                                var currentProjNameSemaDict = new Dictionary<char, int>();
                                var currentOtherProjNameSemaDict = new Dictionary<char, int>();

                                foreach (var item in currentProjNameSema)
                                {
                                    if (currentProjNameSemaDict.Keys.Contains(item))
                                        currentProjNameSemaDict[item] = (currentProjNameSemaDict[item] + 1);
                                    else
                                        currentProjNameSemaDict.Add(item, 1);
                                }

                                foreach (var item in currentOtherProjNameSema)
                                {
                                    if (currentOtherProjNameSemaDict.Keys.Contains(item))
                                        currentOtherProjNameSemaDict[item] = (currentOtherProjNameSemaDict[item] + 1);
                                    else
                                        currentOtherProjNameSemaDict.Add(item, 1);
                                }

                                //Check on differing symblos that are repeating in the strings
                                foreach(var currentChar in currentProjNameSemaDict.Keys)
                                {
                                    if (currentOtherProjNameSemaDict.ContainsKey(currentChar))
                                    {
                                        if (currentProjNameSemaDict[currentChar] != currentOtherProjNameSemaDict[currentChar])
                                            differSymbolsCount += Math.Abs(currentOtherProjNameSemaDict[currentChar] - currentProjNameSemaDict[currentChar]);
                                    }
                                }

                                // If there are more than 2 differing symbols, or there are 2 differing symbols but one of the "semas" has less than 5 characters,
                                // then it is not a typo, but a planned difference, and there is no point in checking deeper in the hierarchy
                                if (differSymbolsCount > 2 || (differSymbolsCount == 2 && (currentProjNameSema.Length < 5 || currentOtherProjNameSema.Length < 5)))
                                    break;

                                // If there are 1-2 differing symbols, and both "semas" have 5+ characters, then it is a typo, and we add a warning to the list.
                                // We also do not check deeper in the hierarchy to avoid creating multiple warnings for the same project.

                                //If there is already a warning for this project, then we do not add it to the list again
                                var projNameSemanticWarning = projectNameSemanticWarningsList.Find(ell => ell.ProjectName == projName);

                                if (projNameSemanticWarning == null)
                                    projectNameSemanticWarningsList.Add(new ProjectNameSemanticWarning(projName, currentOtherProjNameSema, currentProjNameSema));

                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the list of project name semantic warnings. 
        /// Each warning contains a project name and its "sema" that may have a typo, and a "sema" that is similar to it and is used in other projects.
        /// </summary>
        /// <returns></returns>
        public static List<ProjectNameSemanticWarning> GetProjectNamesSemanticWarningList()
        {
            return projectNameSemanticWarningsList;
        }
    }
}