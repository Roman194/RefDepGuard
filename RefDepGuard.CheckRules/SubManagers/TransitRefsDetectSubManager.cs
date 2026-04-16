using System.Collections.Generic;
using System.Linq;
using RefDepGuard.Applied.Models.Project;

namespace RefDepGuard.CheckRules.SubManagers
{
    /// <summary>
    /// This class is responsible for detecting transit references for project of the solution (which are enabled for that).
    /// </summary>
    public class TransitRefsDetectSubManager
    {
        private static Dictionary<string, List<string>> detectedTransitRefsDict = new Dictionary<string, List<string>>();

        /// <summary>
        /// Clears the dictionary of detected transit refs for current project. This method can be called before performing a new check.
        /// </summary>
        public static void ClearDetectedTransitRefsDict()
        {
            detectedTransitRefsDict.Clear();
        }

        /// <summary>
        /// The main method of the SubManager. Detects transit references for current project of the solution.
        /// </summary>
        /// <param name="projName">current project name string</param>
        /// <param name="currentCommitedProjState">commited project state dictionary</param>
        public static void CheckCurrentProjectOnTransitReferences(string projName, Dictionary<string, ProjectState> currentCommitedProjState)
        {
            List<string> checkingProjReferencesList = currentCommitedProjState[projName].CurrentReferences;
            List<string> findedTransitReferencesList = new List<string>();

            //Straight references of the project are not supposed to be in this list, so AddRange is not done on them

            foreach (string currentStraightReference in checkingProjReferencesList)//for each direct ref of the project
            {
                findedTransitReferencesList.AddRange(
                    CheckCurrentTransitProjectOnReferences(currentStraightReference, currentCommitedProjState).Where(
                        value => !findedTransitReferencesList.Contains(value)
                        )
                    );
            }

            if (findedTransitReferencesList.Count > 0)//If there are some transit refs, add them to the dictionary of detected transit refs for current project
            {
                if (detectedTransitRefsDict.ContainsKey(projName))
                {
                    detectedTransitRefsDict[projName] = findedTransitReferencesList;
                }
                else
                {
                    detectedTransitRefsDict.Add(projName, findedTransitReferencesList);
                }
            }
        }

        /// <summary>
        /// Gets the dictionary of detected transit refs for current project. 
        /// The key is the project name, the value is the list of transit references for this project.
        /// </summary>
        /// <returns>Dictionary of transit refs of the solution</returns>
        public static Dictionary<string, List<string>> GetDetectedTransitRefsDict()
        {
            return detectedTransitRefsDict;
        }

        /// <summary>
        /// Recursively checks the transit references for the current transit project. 
        /// It first checks the direct references of the transit project, then for each of them it checks their direct references and so on, 
        /// until there are no more references to check.
        /// </summary>
        /// <param name="transitProjName">current transit projecr name string</param>
        /// <param name="currentCommitedProjState">dictionary of the commited projects state</param>
        /// <returns>list of strings of finded transit/direct refs for the current transitProjName</returns>
        private static List<string> CheckCurrentTransitProjectOnReferences(string transitProjName, Dictionary<string, ProjectState> currentCommitedProjState)
        {
            List<string> checkingProjTransitReferencesList = currentCommitedProjState[transitProjName].CurrentReferences;
            List<string> findedReferencesList = new List<string>();

            if (checkingProjTransitReferencesList.Count != 0)
            {//If there are some refs in transitProjName, then we need to check them on transit refs
                //Если есть рефы, то сначала пройтись по ним и добавить транзитивные связи из них

                foreach (string transitReference in checkingProjTransitReferencesList)
                {
                    findedReferencesList.AddRange(
                        CheckCurrentTransitProjectOnReferences(transitReference, currentCommitedProjState).Where(
                            value => !findedReferencesList.Contains(value)
                            )
                        );
                }

                //And then add direct refs of transitProjName
                findedReferencesList.AddRange(
                    checkingProjTransitReferencesList.Where(
                        value => !findedReferencesList.Contains(value)
                        )
                    );
            }

            return findedReferencesList;
        }
    }
}