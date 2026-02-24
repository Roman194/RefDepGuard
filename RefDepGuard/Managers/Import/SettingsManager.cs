using EnvDTE;
using Microsoft.Office.Interop.Excel;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RefDepGuard.Data;
using RefDepGuard.Managers.Applied;
using RefDepGuard.Models;

namespace RefDepGuard.Managers.Import
{
    internal class SettingsManager
    {
        private static UsingSolutionsDTO usingSolutions;
        private static string UsingSolutionsExtendedName;
        private static string SolutionName;

        //Посмотреть как оптимизировать с ConfigFileManager
        public static bool CheckIfSolutionIsFamiliarToExt(DTE dte, IVsUIShell uiShell)
        {
            SolutionNameManager.SetSolutionNameInfoInRightFormat(dte);

            UsingSolutionsExtendedName = SolutionNameManager.GetPackageName() + "\\.rdg\\rdg_settings\\using_solutions.rdg";
            SolutionName = SolutionNameManager.GetSolutionName();

            if (File.Exists(UsingSolutionsExtendedName))
            {
                try
                {
                    using (FileStream fileStream = new FileStream(UsingSolutionsExtendedName, FileMode.Open))
                    {
                        StreamReader sr = new StreamReader(fileStream);

                        string currentFileContent = sr.ReadToEnd();
                        if (String.IsNullOrEmpty(currentFileContent))
                            throw new Exception();

                         usingSolutions = JsonConvert.DeserializeObject<UsingSolutionsDTO>(currentFileContent);
                    }

                    var findedSolution = usingSolutions.using_solutions.Find(solution => solution == SolutionName);

                    if (findedSolution != null)
                    {
                        return true; //Solution найден в настройках, для него используется расширение 
                    }

                    findedSolution = usingSolutions.ignoring_solutions.Find(solution => solution == SolutionName);
                    if (findedSolution != null)
                        return false;
                    
                }
                catch (Exception)
                {
                    
                }
            }

            return ShowUnfamiliarMessageAndMakeFamiliar(uiShell, UsingSolutionsExtendedName, SolutionName); //В противном случае спрашиваем у пользователя нужно ли ему использовать в solution расширение
        }

        public static bool UpdateSettingsByMakingSolutionFamiliar()
        {
            usingSolutions.ignoring_solutions.Remove(SolutionName);
            usingSolutions.using_solutions.Add(SolutionName);

            CreateOrRewriteUsingSolutionsFile(UsingSolutionsExtendedName);

            return true;
        }

        private static bool ShowUnfamiliarMessageAndMakeFamiliar(IVsUIShell uiShell, string usingSolutionsExtendedName, string solutionName)
        {
            bool userAction = MessageManager.ShowYesNoPrompt(
                uiShell,
                "Нужно ли использовать RefDepGuard в рамках решения '"+ solutionName +"'?.\r\nПри нажатии 'Да' расширение активируется и будет проверять соответствие проектов решения заявленным правилам, а также администрировать сборку решения",
                "RefDepGuard: Первая загрузка решения"
                );

            if (usingSolutions == null)
                usingSolutions = GenerateDefaultUsingSolutionFile();

            if (userAction)
                usingSolutions.using_solutions.Add(solutionName);
            else
                usingSolutions.ignoring_solutions.Add(solutionName);

            CreateOrRewriteUsingSolutionsFile(usingSolutionsExtendedName);

            return userAction;
        }

        private static UsingSolutionsDTO GenerateDefaultUsingSolutionFile()
        {
            var usingSolutions = new UsingSolutionsDTO();
            usingSolutions.name = "Using_solutions";
            usingSolutions.using_solutions = new List<string>();
            usingSolutions.ignoring_solutions = new List<string>();
            return usingSolutions;
        }

        private static void CreateOrRewriteUsingSolutionsFile(string usingSolutionsExtendedName) //Переиспользовать с ConfigFile?
        {
            string rdgStuffDirectory = usingSolutionsExtendedName.Substring(0, usingSolutionsExtendedName.LastIndexOf('\\', 
                usingSolutionsExtendedName.LastIndexOf('\\') - 1));

            var dirInfo = new DirectoryInfo(rdgStuffDirectory);
            dirInfo.Create();
            dirInfo.Attributes |= FileAttributes.Hidden;

            string rdgSettingsDirectory = usingSolutionsExtendedName.Substring(0, usingSolutionsExtendedName.LastIndexOf('\\'));
            var dirSettingsInfo = new DirectoryInfo(rdgSettingsDirectory);
            dirSettingsInfo.Create();

            using (FileStream fileStream = File.Create(usingSolutionsExtendedName))
            {
                StreamWriter streamWriter = new StreamWriter(fileStream);
                string json = JsonConvert.SerializeObject(usingSolutions, Formatting.Indented);

                streamWriter.Write(json);

                streamWriter.Flush();
                fileStream.Flush();

                streamWriter.Close();
            }
        }
    }
}
