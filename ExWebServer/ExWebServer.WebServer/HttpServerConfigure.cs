using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ExWebServer.WebServer.Comet;
using ExWebServer.SocketBase;
using Common.Utility.Json;

namespace ExWebServer.WebServer
{
    public class HttpServerConfigure : SocketServerConfigure
    {
        public List<string> HostList { get; set; }
        public List<Comet.CometCommand> ComandList { get; set; }

        private int ParseHostListString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            string[] hosts = value.Split(new char[] { ';', ',' });
            if (hosts == null || hosts.Length < 1)
                return 0;
            HostList = new List<string>(hosts.Length);
            foreach (string host in hosts)
            {
                if (!string.IsNullOrEmpty(host))
                    HostList.Add(host.Trim().ToLower());
            }
            return HostList.Count;
        }

        public override bool LoadConfig(string configFile)
        {
            bool result = base.LoadConfig(configFile);

            if (!result)
                return result;

            string value = "";

            //  读取Host
            //value = _ConfigFile.GetItem("Http", "Host");
            ParseHostListString(value);

            NameValueCollection cometCommands = null;//_ConfigFile.GetSection("CometCommand");
            string commandJson = "";
            if (cometCommands != null && cometCommands.Count > 0)
            {
                ComandList = new List<CometCommand>(cometCommands.Count);
                for (int i = 0; i < cometCommands.Count; i++)
                {
                    commandJson = cometCommands[i];
                    try
                    {
                        CometCommand command = JsonHelper.JsonToObj<CometCommand>(commandJson);
                        if (command != null)
                        {
                            ComandList.Add(command);
                        }
                    }
                    catch { }
                }
            }

            return result;
        }
    }
}
