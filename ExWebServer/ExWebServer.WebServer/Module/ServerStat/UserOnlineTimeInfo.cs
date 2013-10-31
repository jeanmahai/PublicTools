using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExWebServer.WebServer.Module.ServerStat
{
    public class UserOnlineTimeInfo
    {
        public int UserID { get; set; }
        public int OnlineTimePlus { get; set; }

        public UserOnlineTimeInfo() { }

        public UserOnlineTimeInfo(int uid, int plusTime)
        {
            UserID = uid;
            OnlineTimePlus = plusTime;
        }
    }
}
