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
    public class SettingsManager
    {
        private static UsingSolutionsDTO usingSolutions;
        private static string UsingSolutionsExtendedName;
        private static string SolutionName;

        private static DirectoryInfo stuffDirInfo;
        private static DirectoryInfo settingsDirInfo;

        static SettingsManager()
        {
            //Здесь идёт дублирование с CacheManager по созданию rdgStuffDirectory (мб оставить это создание только в одном менеджере?)
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
        }

        public static bool CheckIfSolutionIsFamiliarToExt(IVsUIShell uiShell)
        {
            
            if (File.Exists(UsingSolutionsExtendedName))
            {
                try
                {
                    string currentFileContent = FileStreamManager.ReadInfoFromFile(UsingSolutionsExtendedName);

                    if (String.IsNullOrEmpty(currentFileContent))
                        throw new Exception();

                    usingSolutions = JsonConvert.DeserializeObject<UsingSolutionsDTO>(currentFileContent);

                    var findedSolution = usingSolutions.using_solutions.Find(solution => solution == SolutionName);

                    if (findedSolution != null)
                    {
                        return true; //Solution найден в настройках, для него используется расширение 
                    }

                    findedSolution = usingSolutions.ignoring_solutions.Find(solution => solution == SolutionName);
                    if (findedSolution != null)
                        return false;
                    
                }
                catch (Exception) //????
                {
                    
                }
            }

            return ShowUnfamiliarMessageAndMakeFamiliar(uiShell, SolutionName); //В противном случае спрашиваем у пользователя нужно ли ему использовать в solution расширение
        }

        public static bool UpdateSettingsByMakingSolutionFamiliar()
        {
            usingSolutions.ignoring_solutions.Remove(SolutionName);
            usingSolutions.using_solutions.Add(SolutionName);

            CreateOrRewriteUsingSolutionsFile();

            return true;
        }

        private static bool ShowUnfamiliarMessageAndMakeFamiliar(IVsUIShell uiShell, string solutionName)
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

            CreateOrRewriteUsingSolutionsFile();

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

        private static void CreateOrRewriteUsingSolutionsFile()
        {
            if (!Directory.Exists(UsingSolutionsExtendedName))
            {
                stuffDirInfo.Create();
                stuffDirInfo.Attributes |= FileAttributes.Hidden;

                settingsDirInfo.Create();
            }

            string json = JsonConvert.SerializeObject(usingSolutions, Formatting.Indented);
            FileStreamManager.WriteInfoToFile(UsingSolutionsExtendedName, json);
        }
    }
}
