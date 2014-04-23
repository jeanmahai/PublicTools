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

        /// <summary>
        /// 初始化服务器支持的协议
        /// </summary>
        /// <param name="commandList"></param>
        public void InitCommand(List<string> commandList)
        {
            string commandJson = string.Empty;
            ComandList = new List<CometCommand>(commandList.Count);
            for (int i = 0; i < commandList.Count; i++)
            {
                commandJson = commandList[i];
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
    }
}
