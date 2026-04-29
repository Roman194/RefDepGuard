using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using RefDepGuard.Managers.Applied;
using RefDepGuard.Models;
using RefDepGuard.Applied;
using RefDepGuard.UI.Resources.StringResources;

namespace RefDepGuard.Managers.Import
{
    /// <summary>
    /// This class is responsible for managing the settings of the extension.
    /// </summary>
    public class SettingsManager
    {
        private static UsingSolutionsDTO usingSolutions;
        private static string UsingSolutionsExtendedName;
        private static string SolutionName;

        private static DirectoryInfo stuffDirInfo;
        private static DirectoryInfo settingsDirInfo;

        /// <summary>
        /// Checks if the solution is familiar to the extension. 
        /// It means that the solution name is in the list of using solutions in the settings file. 
        /// If the solution is familiar, it returns true, otherwise it returns false. 
        /// If the settings file doesn't exist or is empty, it shows a message to the user with a question about making the solution familiar to the extension and 
        /// updates the settings file based on the user's answer.
        /// </summary>
        /// <param name="uiShell">IVsUIShell interface value</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static bool CheckIfSolutionIsFamiliarToExt(IVsUIShell uiShell)
        {
            //Create directories and file name in the right format based on the solution name and package name
            UsingSolutionsExtendedName = SolutionNameManager.GetPackageName() + "\\.rdg\\rdg_settings\\using_solutions.rdg";
            SolutionName = SolutionNameManager.GetSolutionName();

            string rdgStuffDirectory = UsingSolutionsExtendedName.Substring(0, UsingSolutionsExtendedName.LastIndexOf('\\',
                UsingSolutionsExtendedName.LastIndexOf('\\') - 1));

            stuffDirInfo = new DirectoryInfo(rdgStuffDirectory);
            stuffDirInfo.Create();
            stuffDirInfo.Attributes |= FileAttributes.Hidden;

            string rdgSettingsDirectory = UsingSolutionsExtendedName.Substring(0, UsingSolutionsExtendedName.LastIndexOf('\\'));
            settingsDirInfo = new DirectoryInfo(rdgSettingsDirectory);
            settingsDirInfo.Create();


            if (File.Exists(UsingSolutionsExtendedName))//If file exists,
            {
                //try to find the solution name in the lists of using and ignoring solutions in the settings file
                string currentFileContent = FileStreamManager.ReadInfoFromFile(UsingSolutionsExtendedName);

                if (String.IsNullOrEmpty(currentFileContent))
                    ShowUnfamiliarMessageAndMakeFamiliar(uiShell, SolutionName);

                usingSolutions = JsonConvert.DeserializeObject<UsingSolutionsDTO>(currentFileContent);

                var findedSolution = usingSolutions.using_solutions.Find(solution => solution == SolutionName);

                if (findedSolution != null)
                {
                    return true; //Solution is familiar and can be used in the extension
                }

                findedSolution = usingSolutions.ignoring_solutions.Find(solution => solution == SolutionName);
                if (findedSolution != null)
                    return false; //User disables the usage of the extention for this solution
            }

            //If we still here then the solution is unfamiliar and we starts the function to make it familiar
            return ShowUnfamiliarMessageAndMakeFamiliar(uiShell, SolutionName);
        }

        /// <summary>
        /// Updates the settings file by moving the solution name from the list of ignoring solutions to the list of using solutions.
        /// </summary>
        /// <returns>true if the function ends correctly</returns>
        public static bool UpdateSettingsByMakingSolutionFamiliar()
        {
            usingSolutions.ignoring_solutions.Remove(SolutionName);
            usingSolutions.using_solutions.Add(SolutionName);

            CreateOrRewriteUsingSolutionsFile();

            return true;
        }

        /// <summary>
        /// Shows the message to the user with a question about making the solution familiar to the extension and updates the settings file based on the user's answer.
        /// </summary>
        /// <param name="uiShell">IVsUIShell interface value</param>
        /// <param name="solutionName">solution name string</param>
        /// <returns>user action bool result</returns>
        private static bool ShowUnfamiliarMessageAndMakeFamiliar(IVsUIShell uiShell, string solutionName)
        {
            bool userAction = MessageManager.ShowYesNoPrompt(
                uiShell,
                Resource.First_Solution_Load_Message_1 + " '"+ solutionName + "'" + Resource.First_Solution_Load_Message_2,
                Resource.Extension_Name + ": " + Resource.First_Solution_Load_Title
                );

            if (usingSolutions == null)//If the settings file doesn't exist or is empty, we need to create the default structure of the settings file 
                usingSolutions = GenerateDefaultUsingSolutionFile();

            if (userAction)//If user choose to use the extension for this solution, then we adds the solution name to the list of using solutions,
                usingSolutions.using_solutions.Add(solutionName);
            else//otherwise - to the list of ignoring solutions
                usingSolutions.ignoring_solutions.Add(solutionName);

            CreateOrRewriteUsingSolutionsFile();

            return userAction;
        }

        /// <summary>
        /// Generates the default structure of the settings file with empty lists of using and ignoring solutions.
        /// </summary>
        /// <returns>default UsingSolutionsDTO value</returns>
        private static UsingSolutionsDTO GenerateDefaultUsingSolutionFile()
        {
            var usingSolutions = new UsingSolutionsDTO();
            usingSolutions.name = "Using_solutions";
            usingSolutions.using_solutions = new List<string>();
            usingSolutions.ignoring_solutions = new List<string>();
            return usingSolutions;
        }

        /// <summary>
        /// Creates or rewrites the settings file with the current values of using and ignoring solutions.
        /// </summary>
        private static void CreateOrRewriteUsingSolutionsFile()
        {
            if (!Directory.Exists(UsingSolutionsExtendedName))
            {//If the settings file doesn't exist, we creates it and the missing directories for it
                stuffDirInfo.Create();
                stuffDirInfo.Attributes |= FileAttributes.Hidden;

                settingsDirInfo.Create();
            }

            string json = JsonConvert.SerializeObject(usingSolutions, Formatting.Indented);
            FileStreamManager.WriteInfoToFile(UsingSolutionsExtendedName, json);
        }
    }
}