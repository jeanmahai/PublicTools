using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;

namespace Common.Utility
{
    internal class LogSection : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            LogSetting setting = new LogSetting();
            setting.Emitters = new List<LogEmitterConfig>();
            setting.Source = GetNodeAttribute(section, "source");
            if (section != null)
            {
                XmlNode[] nodeList = GetChildrenNodes(section, "emitter");
                foreach (XmlNode node in nodeList)
                {
                    LogEmitterConfig emitter = new LogEmitterConfig();
                    emitter.Type = GetNodeAttribute(node, "type");
                    emitter.Parameters = GetParams(node);
                    setting.Emitters.Add(emitter);
                }
            }
            return setting;
        }

        private static Dictionary<string, string> GetParams(XmlNode node)
        {
            if (node.Attributes == null || node.Attributes.Count <= 1)
            {
                return new Dictionary<string, string>(0);
            }
            Dictionary<string, string> rst = new Dictionary<string, string>((node.Attributes.Count - 1) * 2);
            foreach (XmlAttribute attr in node.Attributes)
            {
                if (attr.Name == "type")
                {
                    continue;
                }
                string v = attr.Value == null ? string.Empty : attr.Value.Trim();
                rst.Add(attr.Name, v);
            }
            return rst;
        }       

        private static XmlNode[] GetChildrenNodes(XmlNode node, string nodeName)
        {
            if (node == null || node.ChildNodes == null || node.ChildNodes.Count <= 0)
            {
                return new XmlNode[0];
            }
            List<XmlNode> nodeList = new List<XmlNode>(node.ChildNodes.Count);
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == nodeName)
                {
                    nodeList.Add(child);
                }
            }
            return nodeList.ToArray();
        }

        private static string GetNodeAttribute(XmlNode node, string attributeName)
        {
            if (node.Attributes == null
                        || node.Attributes[attributeName] == null
                        || node.Attributes[attributeName].Value == null
                        || node.Attributes[attributeName].Value.Trim() == string.Empty)
            {
                return string.Empty;
            }
            return node.Attributes[attributeName].Value.Trim();
        }        

        private static LogSetting s_LogSetting = null;
        private static bool s_HasInit = false;
        private static object s_SyncObj = new object();
        public static LogSetting GetSetting()
        {
            if (s_HasInit == false)
            {
                lock (s_SyncObj)
                {
                    if (s_HasInit == false)
                    {
                        s_LogSetting = ConfigurationManager.GetSection("log") as LogSetting;
                        s_HasInit = true;
                    }
                }
            }
            return s_LogSetting;
        }
    }

    public class LogSetting
    {
        public List<LogEmitterConfig> Emitters { get; set; }

        public string Source { get; set; }
    }

    public class LogEmitterConfig
    {
        public string Type { get; set; }
        public Dictionary<string, string> Parameters
        {
            get;
            set;
        }
    }   
}
