using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExWebServer.WebServer.Comet
{
    public enum CometCommandID
    {
        NA = 0,
        Default = 100,
        QueryNotification = 101,
        QueryOnlineUser = 102,
        QueryOnlineUserList = 103
    }
}
