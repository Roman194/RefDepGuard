using RefDepGuard.Applied.Models.ConfigFile;
using RefDepGuard.CheckRules.SubManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefDepGuard.Tests.CheckRules.SubManagers
{

    [TestClass]
    public class CheckProjectsMatchUnitTests
    {

        [TestMethod]
        public void GetProjectsMatchAfterChecksWarning_Unit()
        {
            var result = CheckProjectsMatchSubManager.GetProjectsMatchAfterChecksWarning(UnitTestsSample.GetMAUIConfigFilesData(), UnitTestsSample.GetMAUIProjectState());

            Assert.AreEqual(result.Count, 0);

        }
    }
}
