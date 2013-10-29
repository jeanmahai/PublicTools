using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExWebServer.WebServer.Module.ServerStat
{
    public class SiteOnlineStat
    {
        private int _CCU = 0;
        private int _MaxCCU = 0;

        public int SiteID { get; set; }
        public int PosID { get; set; }
        public int CCU 
        {
            get { return _CCU; }
            set
            {
                _CCU = value >= 0 ? value : 0;
                if (_CCU > _MaxCCU)
                    _MaxCCU = _CCU;
                LastUpdateTime = DateTime.Now; 
            }
        }
        public int MaxCCU { get { return _MaxCCU; } }

        public DateTime CreateTime { get; set; }
        public DateTime LastUpdateTime { get; set; }

        public SiteOnlineStat() 
        {
            SiteID = PosID = CCU = 0;
            CreateTime = DateTime.Now;
            LastUpdateTime = DateTime.MinValue;
        }

        public SiteOnlineStat(int siteID, int posID, int ccu)
        {
            SiteID = siteID;
            PosID = posID;
            CCU = ccu >= 0 ? ccu : 0;
            CreateTime = DateTime.Now;
            LastUpdateTime = DateTime.Now;
        }

        public void OnlineStatSaved()
        {
            _MaxCCU = _CCU;
        }
    }
}
