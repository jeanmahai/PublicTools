using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.ViewModel
{
    public class LotteryHitsVM
    {
        public string Name { get; set; }
        public int Hits { get; set; }
    }
    public class LotteryShortVM
    {
        public DateTime LotteryTime { get; set; }
        public int No { get; set; }
        public string BigSmall { get; set; }
        public string SideCenter { get; set; }
        public string OddEven { get; set; }
    }
    public class TrendVM
    {
        public TrendVM()
        {
            Lotteries=new List<LotteryShortVM>();
            LotteryHits=new List<LotteryHitsVM>();
        }
        public List<LotteryHitsVM> LotteryHits { get; set; }
        public List<LotteryShortVM> Lotteries { get; set; }
    }
}
