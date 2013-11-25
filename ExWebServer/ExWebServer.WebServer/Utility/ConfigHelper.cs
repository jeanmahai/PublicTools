using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using ExWebServer.WebServer.Module.ConfigEntity;

namespace ExWebServer.WebServer.Utility
{
    public class ConfigHelper
    {
        public static ServerEntity LoadServerConfig()
        {
            ServerEntity result = new ServerEntity();
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configurations/Server.xml");
            XDocument xml = XDocument.Load(path);
            XElement root = xml.Root;
            result.RunWays = root.Element("RunWays").Value.Trim();
            result.Port = int.Parse(root.Element("Port").Value.Trim());
            result.SendThreads = int.Parse(root.Element("SendThreads").Value.Trim());
            result.MaxPostLength = long.Parse(root.Element("MaxPostLength").Value.Trim());
            result.MaxHeaderLines = int.Parse(root.Element("MaxHeaderLines").Value.Trim());
            result.MaxQueuedRequests = int.Parse(root.Element("MaxQueuedRequests").Value.Trim());
            result.HostList = new List<string>();
            XElement hostList = root.Element("HostList");
            foreach (XElement item in hostList.Elements("Host"))
            {
                result.HostList.Add(string.Format("{0}:{1}", item.Value.Trim(), result.Port));
            }
            return result;
        }
    }
}
