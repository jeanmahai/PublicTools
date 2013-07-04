using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Net.Utility.Security;

namespace Utility.Test
{
    [TestClass]
    public class UtilityTest
    {
        [TestMethod]
        public void MD5Encryption()
        {
            Console.WriteLine(CiphertextService.MD5Encryption("test"));
        }
        [TestMethod]
        public void ValidateMD5()
        {
            Console.WriteLine(CiphertextService.ValidateMD5("test","098f6bcd4621d373cade4e832627b4f6"));
        }
    }
}
