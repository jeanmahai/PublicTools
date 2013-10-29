using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExWebServer.WebServer.Handler.SSO
{
    public class CookieUserInfo
    {
        private int _nUID;
        private string _strRealName;

        public int nUID { get { return _nUID; } set { _nUID = value > 0 ? value : 0; } }
        public string strRealName { get { return _strRealName; } set { _strRealName = string.IsNullOrEmpty(value) ? "" : value.Trim(); } }
        public int nServerID { get; set; }
        public int nOSID { get; set; }

        private void InitConfig()
        {
            this.nUID = 0;
            this.nServerID = 0;
            this.strRealName = "";
        }

        public CookieUserInfo()
        {
            InitConfig();
        }
    }
}
