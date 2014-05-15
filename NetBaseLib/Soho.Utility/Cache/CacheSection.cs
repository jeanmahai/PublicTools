using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;
using System.Xml;

namespace Soho.Utility
{
    public class CacheSection : IConfigurationSectionHandler
    {
        private static CacheSetting s_CacheSetting = null;
        private static bool s_HasInit = false;
        private static object s_SyncObj = new object();
        public static CacheSetting GetSetting()
        {
            if (s_HasInit == false)
            {
                lock (s_SyncObj)
                {
                    if (s_HasInit == false)
                    {
                        s_CacheSetting = ConfigurationManager.GetSection("cache") as CacheSetting;
                        s_HasInit = true;
                    }
                }
            }
            return s_CacheSetting;
        }

        public object Create(object parent, object configContext, XmlNode section)
        {
            CacheSetting setting = new CacheSetting();
            if (section != null)
            {
                string tmp = GetNodeAttribute(section, "default");
                setting.DefaultCacheName = (tmp != null && tmp.Length > 0) ? tmp : null;
                XmlNode[] nodeList = GetChildrenNodes(section, "item");
                foreach (XmlNode node in nodeList)
                {
                    string name = GetNodeAttribute(node, "name");
                    if (name == null || name.Length <= 0)
                    {
                        throw new ConfigurationErrorsException("The attribute 'name' of the xml node 'cache/item' cannot be empty in config file.");
                    }
                    if (setting.ContainsKey(name))
                    {
                        throw new ConfigurationErrorsException("Duplicated name '" + name + "' of the xml node 'cache/item' in config file.");
                    }
                    string type = GetNodeAttribute(node, "type");
                    if (type == null || type.Length <= 0)
                    {
                        throw new ConfigurationErrorsException("The attribute 'type' of the xml node 'cache/item' cannot be empty in config file.");
                    }
                    NameValueCollection parms = new NameValueCollection();
                    XmlNode[] paramList = GetChildrenNodes(node.SelectSingleNode("parameters"), "add");
                    foreach (XmlNode pa in paramList)
                    {
                        parms.Add(GetNodeAttribute(pa, "key"), GetNodeAttribute(pa, "value"));
                    }
                    setting.Add(name, new CacheItemConfig
                    {
                        Name = name,
                        Type = type,
                        Parameters = parms
                    });
                }
            }
            return setting;
        }

        private XmlNode[] GetChildrenNodes(XmlNode node, string nodeName)
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

        private string GetNodeAttribute(XmlNode node, string attributeName)
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
    }

    public class CacheSetting : Dictionary<string, CacheItemConfig>
    {
        public string DefaultCacheName
        {
            get;
            set;
        }
    }

    public class CacheItemConfig
    {
        public string Name
        {
            get;
            set;
        }

        public string Type
        {
            get;
            set;
        }

        public NameValueCollection Parameters
        {
            get;
            set;
        }
    }
}
