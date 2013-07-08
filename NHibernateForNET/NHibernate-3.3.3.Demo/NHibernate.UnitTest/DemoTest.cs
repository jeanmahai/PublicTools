using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nhibernate.Data;

namespace NHibernate.UnitTest
{
    [TestClass]
    public class DemoTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var dal = new HelpmateDAL();
            dal.QueryTrend();
            //var a = dal.LotteryTypes();
        }
    }
}
