using Newtonsoft.Json;
using RefDepGuard.Applied.Models.ConfigFile;
using RefDepGuard.Applied.Models.ConfigFile.DTO;
using RefDepGuard.Applied.Models.Project;
using RefDepGuard.TargetFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefDepGuard.Tests
{
    public class UnitTestsSample
    {
        public static string configFileSolutionMAUI = "{\r\n  \"name\": \"MauiApp1\",\r\n  \"framework_max_version\": \"-\",\r\n  \"solution_required_references\": [],\r\n  \"solution_unacceptable_references\": [],\r\n  \"projects\": {\r\n    \"MauiApp1\": {\r\n      \"framework_max_version\": \"9.0\",\r\n      \"consider_global_and_solution_references\": {\r\n        \"required\": true,\r\n        \"unacceptable\": true\r\n      },\r\n      \"required_references\": [],\r\n      \"unacceptable_references\": []\r\n    },\r\n    \"BlazorApp1\": {\r\n      \"framework_max_version\": \"-\",\r\n      \"consider_global_and_solution_references\": {\r\n        \"required\": true,\r\n        \"unacceptable\": true\r\n      },\r\n      \"required_references\": [],\r\n      \"unacceptable_references\": []\r\n    }\r\n  }\r\n}";

        public static string configFileGlobalMAUI = "{ \"name\":\"Global\",\"framework_max_version\":\"-\",\"global_required_references\":[],\"global_unacceptable_references\":[]}";

        public static string targetFrameworkMAUI = "net9.0-android;net9.0-ios;net9.0-maccatalyst";
        public static List<string> mAUIReferences = new List<string> { "BlazorApp1" };

        public static string targetFrameworkBlazor = "net8.0";

        public static ConfigFilesData GetMAUIConfigFilesData()
        {
            ConfigFileGlobalDTO configFileGlobal = JsonConvert.DeserializeObject<ConfigFileGlobalDTO>(configFileGlobalMAUI);

            ConfigFileSolutionDTO configFileSolution = JsonConvert.DeserializeObject<ConfigFileSolutionDTO>(configFileSolutionMAUI);

            return new ConfigFilesData(configFileSolution, configFileGlobal, FileParseError.None, "MauiApp1", @"C:\Users\zuzinra\source\repos\MauiApp1");
        }

        public static Dictionary<string, ProjectState> GetMAUIProjectState()
        {
            var currentProjState = new Dictionary<string, ProjectState>();
            currentProjState.Add("MauiApp1", new ProjectState(TFManager.ConvertTargetFrameworkToTransferFormat(targetFrameworkMAUI), targetFrameworkMAUI, mAUIReferences));
            currentProjState.Add("BlazorApp1", new ProjectState(TFManager.ConvertTargetFrameworkToTransferFormat(targetFrameworkBlazor), targetFrameworkBlazor, new List<string>()));

            return currentProjState;
        }

    }
}
