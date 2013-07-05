using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate.Utility;
using Net.Utility.Security;

namespace Utility.Test
{
    [TestClass]
    public class NHibernateUtilityTest
    {
        [TestMethod]
        public void ShowSqlsTest()
        {
            foreach (var item in SqlManager.GetAllText())
            {
                Console.WriteLine(string.Format("name:{0},\n sql:\n{1}",item.Key,item.Value));
            }
            //var a = SqlManager.GetSqlText("a");
            //Console.WriteLine(CiphertextService.MD5Encryption("test"));
        }
        [TestMethod]
        public void GetSqlTextTest()
        {
            Console.WriteLine(SqlManager.GetSqlText("test"));
        }
    }
}
