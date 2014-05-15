using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Configuration;
using System.ServiceModel.Web;
using System.Web;

namespace Soho.Utility.Web
{
    internal static class CookieConfig
    {
        private const string COOKIE_CONFIG_FILE_PATH = "Configuration/Cookie.config";
        private const string COOKIE_CONFIG_FILE_PATH_NODE_NAME = "CookieConfigPath";

        private static string CookieConfigFilePath
        {
            get
            {
                string path = ConfigurationManager.AppSettings[COOKIE_CONFIG_FILE_PATH_NODE_NAME];
                if (string.IsNullOrWhiteSpace(path))
                {
                    path = COOKIE_CONFIG_FILE_PATH;
                }
                string p = Path.GetPathRoot(path);
                if (p == null || p.Trim().Length <= 0) // 说明是相对路径
                {
                    return Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
                }
                return path;
            }
        }

        private static Dictionary<string, CookieConfigEntity> GetAllCookieConfig()
        {
            string path = CookieConfigFilePath;
            if (string.IsNullOrWhiteSpace(path) || File.Exists(path) == false)
            {
                return new Dictionary<string, CookieConfigEntity>(0);
            }
            return new Dictionary<string, CookieConfigEntity>(0);
            //return CacheManager.GetWithLocalCache<Dictionary<string, CookieConfigEntity>>("WEB_CookieConfig_GetCookieConfig", () =>
            //{
            //    Dictionary<string, CookieConfigEntity> dic = new Dictionary<string, CookieConfigEntity>();
            //    XmlDocument doc = new XmlDocument();
            //    doc.Load(CookieConfigFilePath);
            //    XmlNodeList nodeList = doc.GetElementsByTagName("cookies");
            //    if (nodeList != null && nodeList.Count > 0)
            //    {
            //        foreach (XmlNode xmlNode in nodeList)
            //        {
            //            if (xmlNode == null)
            //            {
            //                continue;
            //            }
            //            CookieConfigEntity entity = new CookieConfigEntity();
            //            entity.NodeName = xmlNode.Attributes["nodeName"] != null ? xmlNode.Attributes["nodeName"].Value : null;
            //            entity.PersistType = xmlNode.Attributes["persistType"] != null ? xmlNode.Attributes["persistType"].Value : null;
            //            entity.SecurityLevel = xmlNode.Attributes["securityLevel"] != null ? xmlNode.Attributes["securityLevel"].Value : null;
            //            if (string.IsNullOrWhiteSpace(entity.NodeName))
            //            {
            //                throw new ApplicationException("Not set node name for cookie config in file '" + path + "'");
            //            }
            //            if (dic.ContainsKey(entity.NodeName))
            //            {
            //                throw new ApplicationException("Duplicated cookie config of node '" + entity.NodeName + "' in file '" + path + "'");
            //            }
            //            if (string.IsNullOrWhiteSpace(entity.PersistType))
            //            {
            //                entity.PersistType = "Auto"; // 如果没有配置persistType，则默认为web
            //            }
            //            if (string.IsNullOrWhiteSpace(entity.SecurityLevel))
            //            {
            //                entity.SecurityLevel = "Middle"; // 如果没有配置securityLevel，则默认为Middle
            //            }
            //            foreach (XmlNode childNode in xmlNode.ChildNodes)
            //            {
            //                if (childNode.NodeType == XmlNodeType.Element)
            //                {
            //                    if (entity.Properties.ContainsKey(childNode.Name))
            //                    {
            //                        entity.Properties[childNode.Name] = childNode.InnerText;
            //                    }
            //                    else
            //                    {
            //                        entity.Properties.Add(childNode.Name, childNode.InnerText);
            //                    }
            //                }
            //            }
            //            dic.Add(entity.NodeName, entity);
            //        }
            //    }
            //    return dic;
            //}, path);
        }

        private static CookieConfigEntity GetDefaultCookieConfig(string nodeName)
        {
            CookieConfigEntity defaultConfig = new CookieConfigEntity();
            defaultConfig.NodeName = "defaultConfig";
            defaultConfig.PersistType = "Auto";
            defaultConfig.SecurityLevel = "Middle";
            //节点名默认作为Cookie的存储名
            defaultConfig.Properties["cookieName"] = nodeName;
            defaultConfig.Properties["hashkey"] = "baeaaea5-3d57-4b98-abde-47ac0aa15d54";
            defaultConfig.Properties["rc4key"] = "5cb8b18c-7b5e-4f7b-a7c2-4603a250f39b";
            defaultConfig.Properties["domain"] = ((HttpContext.Current == null || HttpContext.Current.Request == null) ? "localhost" : HttpContext.Current.Request.Url.Host);
            defaultConfig.Properties["path"] = "/";
            defaultConfig.Properties["expires"] = "0";
            defaultConfig.Properties["securityExpires"] = "20";
            return defaultConfig;
        }

        public static CookieConfigEntity GetCookieConfig(string nodeName)
        {
            Dictionary<string, CookieConfigEntity> all = GetAllCookieConfig();
            CookieConfigEntity entity;
            if (all.TryGetValue(nodeName, out entity) && entity != null)
            {
                return entity;
            }
            return GetDefaultCookieConfig(nodeName);
        }
    }

    internal class CookieConfigEntity
    {
        public string NodeName { get; set; }
        public string PersistType { get; set; }
        public string SecurityLevel { get; set; }

        private Dictionary<string, string> m_Properties = new Dictionary<string, string>();
        public Dictionary<string, string> Properties { get { return m_Properties; } }
    }
}
