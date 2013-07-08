using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;
using Nhibernate.DataModel.Model;

namespace Nhibernate.Data
{
    public class HelpmateDAL : DALBase
    {
        public List<LotteryType> LotteryTypes()
        {
            return Session.Query<LotteryType>().ToList();
        }

        public object QueryTrend()
        {
            //var l1 = new List<SourceData_28_Beijing>();
            //var l2 = new List<LotteryType>();
            //var query = from a in l1
            //            join b in l2 on a.RetNum equals b.RetNum
            //            select a;

            var query = from a in Session.Query<SourceData_28_Beijing>()
                        join b in Session.Query<LotteryType>() on a.RetNum equals b.RetNum
                        select new
                                   {
                                       no=a.RetNum
                                   };
            //var total = query.SingleOrDefault();
            //return Session.Query<SourceData_28_Beijing>().ToList();
            return query.ToList();
        }
    }
}
