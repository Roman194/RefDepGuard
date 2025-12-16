using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject1.Data;
using VSIXProject1.Data.ConfigFile;
using VSIXProject1.Models;

namespace VSIXProject1.Managers.CheckRules.SubManagers
{
    public class CheckProjectsMatchSubManager
    {
        private static List<ProjectMatchWarning> projectMatchWarningList = new List<ProjectMatchWarning>();

        public static void ClearErrorLists()
        {
            if(projectMatchWarningList != null)
                projectMatchWarningList.Clear();
        }

        public static Tuple<ConfigFilesData, List<ProjectMatchWarning>> CheckAndUpdateProjectsOnMatch(ConfigFilesData configFilesData, Dictionary<string, ProjectState> currentCommitedProjState, IVsUIShell uIShell)
        { //Подумать и объединить это с проверкой рефов?
            var addedProjectsList = new List<string>();
            var removedProjectsList = new List<string>();
            //Как минимум подумать над обобщением added и removed разделов

            foreach (KeyValuePair<string, ProjectState> currentProjState in currentCommitedProjState)//проверка на наличие добавленных проектов
            {
                var projName = currentProjState.Key;

                if (!configFilesData.configFileSolution?.projects?.ContainsKey(projName) ?? false)
                {
                    addedProjectsList.Add(projName);
                }
            }

            foreach (var dictValue in configFilesData.configFileSolution?.projects) //проверка на наличие удалённых проектов
            {
                var projName = dictValue.Key;
                if (!currentCommitedProjState.ContainsKey(projName))
                {
                    removedProjectsList.Add(projName);
                }
            }

            if (addedProjectsList.Count > 0)
            {
                var message = "В решении обнаружен проект '" + addedProjectsList.First() + "', отсутствующий в config-файле ('" + configFilesData.solutionName + "_config_guard.rdg'). Добавить его в файл конфигураций?";

                if(addedProjectsList.Count > 1)
                {
                    message = "В решении обнаружены проекты '";

                    for(int i = 0; i < addedProjectsList.Count; i++)
                    {
                        if (i > 5)
                            break;

                        message += "'" + addedProjectsList[i] + "', ";
                    }

                    message += "отсутствующие в config-файле ('" + configFilesData.solutionName + "_config_guard.rdg'). Добавить их в файл конфигураций?";
                } 

                if (MessageManager.ShowYesNoPrompt(uIShell, message, "RefDepGuard"))
                {
                    //Добавить в config-файл

                    configFilesData = ConfigFileManager.UpdateSolutionConfigFile(configFilesData, addedProjectsList, true);

                }
                else
                {
                    //вывести warning типа "RefDepGuard projects match warning"
                    //Conatins не требуется?
                    foreach (var currentAddedProject in addedProjectsList)
                    {
                        projectMatchWarningList.Add(
                            new ProjectMatchWarning(currentAddedProject, true)
                            );
                    }
                }
            }

            if (removedProjectsList.Count > 0)
            {
                //Проект есть в config, но его нет в Solution - значит он был удалён

                var message = "В config-файле обнаружен проект '" + removedProjectsList.First() + "', отсутствующий в solution. Удалить его из файла конфигураций?";

                if (removedProjectsList.Count > 1)
                {
                    message = "В config-файле обнаружены проекты '";

                    for (int i = 0; i < removedProjectsList.Count; i++)
                    {
                        if (i > 5)
                            break;

                        message += "'" + removedProjectsList[i] + "', ";
                    }

                    message += "отсутствующие в solution. Удалить их из файла конфигураций?";
                }

                if (MessageManager.ShowYesNoPrompt(uIShell, message, "RefDepGuard"))
                {
                    //Удалить проект(ы) из config-файла 
                    configFilesData = ConfigFileManager.UpdateSolutionConfigFile(configFilesData, removedProjectsList, false);
                    
                }
                else
                {
                    //вывести warning типа "RefDepGuard projects match warning"
                    //Conatins не требуется?
                    foreach (var currentRemovedProject in removedProjectsList)
                    {
                        projectMatchWarningList.Add(
                            new ProjectMatchWarning(currentRemovedProject, false)
                            );
                    }
                }
            }

            return new Tuple<ConfigFilesData, List<ProjectMatchWarning>>(configFilesData, projectMatchWarningList);
        }

    }
}
