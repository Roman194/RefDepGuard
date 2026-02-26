using System.Collections.Generic;
using System.Linq;
using RefDepGuard.Data;

namespace RefDepGuard.Managers.CheckRules.SubManagers
{
    public class TransitRefsDetectSubManager
    {
        private static Dictionary<string, List<string>> detectedTransitRefsDict = new Dictionary<string, List<string>>();

        public static void CheckCurrentProjectOnTransitReferences(string projName, Dictionary<string, ProjectState> currentCommitedProjState)
        {
            List<string> checkingProjReferencesList = currentCommitedProjState[projName].CurrentReferences;
            List<string> findedTransitReferencesList = new List<string>();

            foreach (string currentStraightReference in checkingProjReferencesList)
            {
                //Прямые рефы проекта не должны попасть в этот список, поэтому над ними AddRange не делается
                findedTransitReferencesList.AddRange(
                    CheckCurrentTransitProjectOnReferences(currentStraightReference, currentCommitedProjState).Where(
                        value => !findedTransitReferencesList.Contains(value)
                        )
                    );
            }

            if (findedTransitReferencesList.Count > 0)
            {
                if (detectedTransitRefsDict.ContainsKey(projName))
                {
                    detectedTransitRefsDict[projName] = findedTransitReferencesList;
                    //Могу забить на то, что там хранилось, так как на момент проверки правил транзиты для каждого проекта проверяются и добавляются только один раз
                }
                else
                {
                    detectedTransitRefsDict.Add(projName, findedTransitReferencesList);
                }
            }
        }

        public static Dictionary<string, List<string>> GetDetectedTransitRefsDict()
        {
            return detectedTransitRefsDict;
        }

        public static void ClearDetectedTransitRefsDict()
        {
            detectedTransitRefsDict.Clear();
        }

        private static List<string> CheckCurrentTransitProjectOnReferences(string transitProjName, Dictionary<string, ProjectState> currentCommitedProjState)
        {
            List<string> checkingProjTransitReferencesList = currentCommitedProjState[transitProjName].CurrentReferences;
            List<string> findedReferencesList = new List<string>();

            if (checkingProjTransitReferencesList.Count != 0)
            {
                //Если есть рефы, то сначала пройтись по ним и добавить транзитивные связи из них

                foreach (string transitReference in checkingProjTransitReferencesList) {
                    findedReferencesList.AddRange(
                        CheckCurrentTransitProjectOnReferences(transitReference, currentCommitedProjState).Where(
                            value => !findedReferencesList.Contains(value)
                            )
                        );
                }

                //Затем добавить прямые связи этого "транзитивного" проекта
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
