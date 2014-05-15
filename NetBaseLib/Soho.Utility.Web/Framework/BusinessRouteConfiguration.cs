using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace Soho.Utility.Web.Framework
{
    /// <summary>
    /// 业务路由配置
    /// </summary>
    internal class BusinessRouteConfiguration
    {
        private const string BUSINESSROUTE_CONFIG_FILE_PATH = "Configuration/BusinessRoute.config";

        private static string _ConfigFolder = string.Empty;
        private static string ConfigFolder
        {
            get
            {
                if (_ConfigFolder == null)
                {
                    _ConfigFolder = Path.GetDirectoryName(BUSINESSROUTE_CONFIG_FILE_PATH);
                }
                return _ConfigFolder;
            }
        }

        private static string BusinessRouteConfigFilePath
        {
            get
            {
                string path = BUSINESSROUTE_CONFIG_FILE_PATH;
                string p = Path.GetPathRoot(path);
                if (p == null || p.Trim().Length <= 0) // 说明是相对路径
                {
                    return Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
                }
                return path;
            }
        }

        /// <summary>
        /// Cookie业务路由配置列表
        /// </summary>
        private static List<BusinessRouteItem> _BusinessRouteList = null;
        /// <summary>
        /// 业务路由配置列表
        /// </summary>
        public static List<BusinessRouteItem> BusinessRouteList
        {
            get
            {
                if (_BusinessRouteList == null)
                {
                    _BusinessRouteList = new List<BusinessRouteItem>();
                    XmlDocument doc = new XmlDocument();
                    doc.Load(BusinessRouteConfigFilePath);
                    XmlNodeList nodeList = doc.GetElementsByTagName("BusinessRoute");
                    if (nodeList != null && nodeList.Count > 0)
                    {
                        foreach (XmlNode xmlNode in nodeList)
                        {
                            BusinessRouteItem item = new BusinessRouteItem();
                            item.Route = xmlNode.Attributes["Route"].Value;
                            item.IsMust = string.IsNullOrWhiteSpace(xmlNode.Attributes["IsMust"].Value) ? false : (xmlNode.Attributes["IsMust"].Value.Equals("1") ? true : false);
                            _BusinessRouteList.Add(item);
                        }
                    }
                }
                return _BusinessRouteList;
            }
        }
    }

    /// <summary>
    /// 业务路由配置条目
    /// </summary>
    internal class BusinessRouteItem
    {
        /// <summary>
        /// 路由
        /// </summary>
        public string Route { get; set; }
        /// <summary>
        /// 是否只能是MVC
        /// </summary>
        public bool IsMust { get; set; }
    }
}
