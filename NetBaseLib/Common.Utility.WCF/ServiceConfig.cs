using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Xml;

namespace Common.Utility.WCF
{
    internal static class ServiceConfig
    {
        private static IAuthorize s_Authorizer = null;
        private static bool s_HasSetAuthorizerType = false;
        private static object s_SyncObj = new object();
        public static IAuthorize GetAuthorizer()
        {
            if (!s_HasSetAuthorizerType)
            {
                lock (s_SyncObj)
                {
                    if(!s_HasSetAuthorizerType)
                    {
                        string path = GetConfigPath();
                        if (File.Exists(path))
                        {
                            XmlDocument x = new XmlDocument();
                            x.Load(path);
                            XmlNode node = x.SelectSingleNode(@"//serviceList");
                            if (node != null)
                            {
                                string name = GetNodeAttribute(node, "authorizerType");
                                if (name != null && name.Trim().Length > 0)
                                {
                                    Type type = Type.GetType(name, false);
                                    s_Authorizer = (IAuthorize)Activator.CreateInstance(type);                 
                                }
                            }
                        }
                        s_HasSetAuthorizerType = true;
                    }
                }
            }
            return s_Authorizer;
        }

        private static string GetConfigPath()
        {
            string path = ConfigurationManager.AppSettings["RestServiceConfigPath"];
            if (path == null || path.Trim().Length <= 0)
            {
                return Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Configuration/RestService.config");
            }
            string p = Path.GetPathRoot(path);
            if (p == null || p.Trim().Length <= 0) // 说明是相对路径
            {
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
            }
            return path;
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

        public static List<ServiceData> GetAllService()
        {
            string path = GetConfigPath();
            if (!File.Exists(path))
            {
                return new List<ServiceData>(0);
            }
            XmlDocument x = new XmlDocument();
            x.Load(path);
            XmlNode node = x.SelectSingleNode(@"//serviceList");
            if (node == null || node.ChildNodes == null || node.ChildNodes.Count <= 0)
            {
                return new List<ServiceData>(0);
            }
            XmlNode[] mList = GetChildrenNodes(node, "service");
            if (mList == null || mList.Length <= 0)
            {
                return new List<ServiceData>(0);
            }
            List<ServiceData> rst = new List<ServiceData>(mList.Length);
            foreach (var n in mList)
            {
                string pathStr = GetNodeAttribute(n, "path");
                string typeName = GetNodeAttribute(n, "type");
                string binding = GetNodeAttribute(n, "binding");
                if (typeName != null && typeName.Trim().Length > 0)
                {
                    rst.Add(new ServiceData { Path = pathStr, Type = typeName, Binding = binding });
                }
            }
            return rst;
        }
    }

    internal class ServiceData
    {
        public string Path
        {
            get;
            set;
        }

        public string Type
        {
            get;
            set;
        }

        public string Binding
        {
            get;
            set;
        }
    }
}
