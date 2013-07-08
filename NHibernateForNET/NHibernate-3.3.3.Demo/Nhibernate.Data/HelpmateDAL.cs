using System;
using System.Collections.Generic;
using System.Globalization;
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

            var query = (from a in Session.Query<SourceData_28_Beijing>()
                         join b in Session.Query<LotteryType>() on a.RetNum equals b.RetNum
                         group a by a.RetNum
                             into g
                             select new
                                        {
                                            no = g.Key.ToString(CultureInfo.InvariantCulture),
                                            total = g.Count()
                                        });
            var q2 = (from a in Session.Query<SourceData_28_Beijing>()
                      join b in Session.Query<LotteryType>() on a.RetNum equals b.RetNum
                      group b by b.BigOrSmall
                          into g
                          select new { no = g.Key, total = g.Count() }
                     );
            var q3 = (from a in Session.Query<SourceData_28_Beijing>()
                      join b in Session.Query<LotteryType>() on a.RetNum equals b.RetNum
                      group b by b.MiddleOrSide
                          into g
                          select new { no = g.Key, total = g.Count() }
                     );
            var q4 = (from a in Session.Query<SourceData_28_Beijing>()
                      join b in Session.Query<LotteryType>() on a.RetNum equals b.RetNum
                      group b by b.OddOrDual
                          into g
                          select new { no = g.Key, total = g.Count() }
            );
           
            var result = query.ToList();
            result.AddRange(q2);
            //var total = query.SingleOrDefault();
            //return Session.Query<SourceData_28_Beijing>().ToList();
            return result;
        }
    }
}
