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

namespace Common.Utility.Web
{
    internal static class CookieConfig
    {
        private const string COOKIE_CONFIG_FILE_PATH = "Configuration/Cookie.config";

        private static string _ConfigFolder = string.Empty;
        private static string ConfigFolder
        {
            get
            {
                if (_ConfigFolder == null)
                {
                    _ConfigFolder = Path.GetDirectoryName(COOKIE_CONFIG_FILE_PATH);
                }
                return _ConfigFolder;
            }
        }

        private static string CookieConfigFilePath
        {
            get
            {
                string path = COOKIE_CONFIG_FILE_PATH;
                string p = Path.GetPathRoot(path);
                if (p == null || p.Trim().Length <= 0) // 说明是相对路径
                {
                    return Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
                }
                return path;
            }
        }

        /// <summary>
        /// Cookie基本配置
        /// </summary>
        private static Dictionary<string, Dictionary<string, string>> _CookieBaseConfigList = null;
        /// <summary>
        /// Cookie基本配置
        /// </summary>
        private static Dictionary<string, Dictionary<string, string>> CookieBaseConfigList
        {
            get
            {
                if (_CookieBaseConfigList == null)
                {
                    _CookieBaseConfigList = new Dictionary<string, Dictionary<string, string>>();
                    XmlDocument doc = new XmlDocument();
                    doc.Load(CookieConfigFilePath);
                    XmlNodeList nodeList = doc.GetElementsByTagName("cookies");
                    if (nodeList != null && nodeList.Count > 0)
                    {
                        foreach (XmlNode xmlNode in nodeList)
                        {
                            Dictionary<string, string> item = new Dictionary<string, string>();
                            item["nodeName"] = xmlNode.Attributes["nodeName"].Value;
                            item["persistType"] = xmlNode.Attributes["persistType"].Value;
                            item["securityLevel"] = xmlNode.Attributes["securityLevel"].Value;
                            _CookieBaseConfigList[item["nodeName"]] = item;
                        }
                    }
                }
                return _CookieBaseConfigList;
            }
        }

        /// <summary>
        /// Cookie配置
        /// </summary>
        private static Dictionary<string, Dictionary<string, string>> _CookieConfigList = null;
        /// <summary>
        /// Cookie配置
        /// </summary>
        private static Dictionary<string, Dictionary<string, string>> CookieConfigList
        {
            get
            {
                if (_CookieConfigList == null)
                {
                    _CookieConfigList = new Dictionary<string, Dictionary<string, string>>();
                    XmlDocument doc = new XmlDocument();
                    doc.Load(CookieConfigFilePath);
                    XmlNodeList nodeList = doc.GetElementsByTagName("cookies");
                    if (nodeList != null && nodeList.Count > 0)
                    {
                        foreach (XmlNode xmlNode in nodeList)
                        {
                            Dictionary<string, string> item = new Dictionary<string, string>();
                            item["nodeName"] = xmlNode.Attributes["nodeName"].Value;
                            foreach (XmlNode childNode in xmlNode.ChildNodes)
                            {
                                if (childNode.NodeType == XmlNodeType.Element)
                                    item[childNode.Name] = childNode.InnerText;
                            }
                            _CookieConfigList[item["nodeName"]] = item;
                        }
                    }
                }
                return _CookieConfigList;
            }
        }
        /// <summary>
        /// 默认配置
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetDefaultConfig(string nodeName)
        {
            Dictionary<string, string> defaultConfig = new Dictionary<string, string>();
            defaultConfig["nodeName"] = "defaultConfig";
            //节点名默认作为Cookie的存储名
            defaultConfig["cookieName"] = nodeName;
            defaultConfig["hashkey"] = "baeaaea5-3d57-4b98-abde-47ac0aa15d54";
            defaultConfig["rc4key"] = "5cb8b18c-7b5e-4f7b-a7c2-4603a250f39b";
            defaultConfig["domain"] = ConfigurationManager.AppSettings["CookieDomain"];
            defaultConfig["path"] = "/";
            defaultConfig["expires"] = "0";
            defaultConfig["securityExpires"] = "20";
            return defaultConfig;
        }
        /// <summary>
        /// 默认的基本配置
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetDefaultBaseConfig(string nodeName)
        {
            Dictionary<string, string> defaultConfig = new Dictionary<string, string>();
            defaultConfig["nodeName"] = "defaultConfig";
            defaultConfig["persistType"] = "Web";
            defaultConfig["securityLevel"] = "Middle";
            return defaultConfig;
        }
        public static Dictionary<string, string> GetCookieBaseConfig(string nodeName)
        {
            return CookieBaseConfigList != null && CookieBaseConfigList.ContainsKey(nodeName)
                && CookieBaseConfigList[nodeName] != null ? CookieBaseConfigList[nodeName] : GetDefaultBaseConfig(nodeName);
        }
        public static Dictionary<string, string> GetCookieConfig(string nodeName)
        {
            return CookieConfigList != null && CookieConfigList.ContainsKey(nodeName)
                && CookieConfigList[nodeName] != null ? CookieConfigList[nodeName] : GetDefaultConfig(nodeName);
        }
    }
}
