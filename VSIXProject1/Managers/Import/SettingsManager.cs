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
using VSIXProject1.Data;
using VSIXProject1.Managers.Applied;
using VSIXProject1.Models;

namespace VSIXProject1.Managers.Import
{
    internal class SettingsManager
    {
        private static UsingSolutionsDTO usingSolutions;

        //Посмотреть как оптимизировать с ConfigFileManager
        public static bool CheckIfSolutionIsFamiliarToExt(DTE dte, IVsUIShell uiShell)
        {
            SolutionNameManager.SetSolutionNameInfoInRightFormat(dte);

            string usingSolutionsExtendedName = SolutionNameManager.GetPackageName() + "\\.rdg_settings\\using_solutions.rdg";
            string solutionName = SolutionNameManager.GetSolutionName();

            if (File.Exists(usingSolutionsExtendedName))
            {
                try
                {
                    using (FileStream fileStream = new FileStream(usingSolutionsExtendedName, FileMode.Open))
                    {
                        StreamReader sr = new StreamReader(fileStream);

                        string currentFileContent = sr.ReadToEnd();
                        if (String.IsNullOrEmpty(currentFileContent))
                            throw new Exception();

                         usingSolutions = JsonConvert.DeserializeObject<UsingSolutionsDTO>(currentFileContent);
                    }

                    var findedSolution = usingSolutions.Solutions.ToList().Find(solution => solution == solutionName);

                    if (findedSolution != null)
                    {
                        return true; //Solution найден в настройках, для него используется расширение 
                    }
                    
                }
                catch (Exception)
                {
                    
                }
            }

            return ShowNotFamiliarMessageAndMakeFamiliarIfNeeded(uiShell, usingSolutionsExtendedName, solutionName); //В противном случае спрашиваем у пользователя нужно ли ему использовать в solution расширение
        }

        private static bool ShowNotFamiliarMessageAndMakeFamiliarIfNeeded(IVsUIShell uiShell, string usingSolutionsExtendedName, string solutionName)
        {
            bool userAction = MessageManager.ShowYesNoPrompt(
                uiShell,
                "Нужно ли использовать RefDepGuard в рамках решения '"+ solutionName +"'?.\r\nПри нажатии 'Да' расширение активируется и будет проверять соответствие проектов решения заявленным правилам, а также администрировать сборку решения",
                "RefDepGuard: Первая загрузка решения"
                );

            if (userAction) //Если да, то записываем расширение в файл
            {
                if (usingSolutions == null)
                {
                    usingSolutions = GenerateDefaultUsingSolutionFile();
                }

                usingSolutions.Solutions.Add(solutionName);

                CreateOrRewriteUsingSolutionsFile(usingSolutionsExtendedName);

                return true;
            }

            return false; //Если нет, то и ничего генерировать не будем
        }

        private static UsingSolutionsDTO GenerateDefaultUsingSolutionFile()
        {
            var usingSolutions = new UsingSolutionsDTO();
            usingSolutions.Name = "Using_solutions";
            usingSolutions.Solutions = new List<string>();
            return usingSolutions;
        }

        private static void CreateOrRewriteUsingSolutionsFile(string usingSolutionsExtendedName) //Переиспользовать с ConfigFile?
        {
            string usingSolutionsDirectory = usingSolutionsExtendedName.Substring(0, usingSolutionsExtendedName.LastIndexOf('\\'));

            var dirInfo = new DirectoryInfo(usingSolutionsDirectory);
            dirInfo.Create();
            dirInfo.Attributes |= FileAttributes.Hidden;

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
