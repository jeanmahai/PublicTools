using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Xml.Linq;

namespace Soho.Utility.DataAccess.SearchEngine
{
    /// <summary>
    /// 搜索引擎配置管理类
    /// 用于加载搜索引擎配置文件，并维护返回数据类型和使用的提供程序间的对应关系
    /// </summary>
    internal class SearchEngineConfigManager
    {
        private static Dictionary<string, ISearchProvider> s_ProviderDic = new Dictionary<string, ISearchProvider>();
        private static Dictionary<Type, string> s_ItemDic = new Dictionary<Type, string>();

        /// <summary>
        /// Web.config中<appSettings>下自定义配置节点名称
        /// </summary>
        private const string NODE_NAME = "SolrSearchConfigFolder";

        /// <summary>
        /// 默认的Solr Searcher 配置文件相对于应用程序启动目录的配置目录路径
        /// </summary>
        private const string DEFAULT_FOLDER = "Configuration";

        /// <summary>
        /// Solr Searcher 配置文件名称
        /// </summary>
        private const string CONFIG_FILE = "SearchEngineConfig.xml";

        /// <summary>
        /// 配置文件所处文件夹路径
        /// </summary>
        private static string m_SettingFolderPath = GetBaseFolderPath();

        /// <summary>
        /// 取得配置文件所处文件夹路径
        /// </summary>
        /// <returns></returns>
        private static string GetBaseFolderPath()
        {
            string path = ConfigurationManager.AppSettings[NODE_NAME];
            if (path == null || path.Trim().Length <= 0)
            {
                return Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, DEFAULT_FOLDER);
            }
            string p = Path.GetPathRoot(path);
            if (p == null || p.Trim().Length <= 0) // 说明是相对路径
            {
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
            }
            return path;
        }

        /// <summary>
        /// 尝试根据返回数据类型取得数据检索提供程序，如果类型没有注册对应的提供程序，则返回Null
        /// </summary>
        /// <param name="dataType">返回数据类型</param>
        /// <param name="provider">数据类型注册的数据检索提供程序</param>
        /// <returns>是否正确取得数据检索提供程序，true：是；false：否</returns>
        public static bool TryGetProvider(Type dataType, out ISearchProvider provider)
        {
            provider = null;

            string providerName;
            if (s_ItemDic.TryGetValue(dataType, out providerName) && !string.IsNullOrWhiteSpace(providerName))
            {
                return s_ProviderDic.TryGetValue(providerName, out provider);
            }
            return false;
        }

        /// <summary>
        /// 静态构造，加载配置文件并初始化检索提供程序
        /// </summary>
        static SearchEngineConfigManager()
        {
            LoadConfig();
        }

        /// <summary>
        /// 加载搜索引擎配置文件，并进行初始化工作
        /// </summary>
        private static void LoadConfig()
        {
            string configPath = Path.Combine(m_SettingFolderPath, CONFIG_FILE);
            if (!File.Exists(configPath))
            {
                throw new ApplicationException("没有找到SearchEngine配置文件");
            }
            else
            {
                using (FileStream fs = new FileStream(configPath, FileMode.Open, FileAccess.Read))
                {
                    XDocument xDoc = XDocument.Load(fs);

                    #region 加载所有的Provider

                    var providerContainer = xDoc.Root.Element("providers");
                    if (providerContainer != null)
                    {
                        var providers = providerContainer.Descendants("provider");
                        if (providers != null && providers.Count() > 0)
                        {
                            foreach (var providerCfg in providers)
                            {
                                LoadSearchProvider(providerCfg);
                            }
                        }
                    }
                    #endregion

                    #region 加载所有的返回数据类型

                    var itemContainer = xDoc.Root.Element("items");
                    if (itemContainer != null)
                    {
                        var items = itemContainer.Descendants("item");
                        if (items != null && items.Count() > 0)
                        {
                            foreach (var itemCfg in items)
                            {
                                LoadItem(itemCfg);
                            }
                        }
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        /// 加载检索提供程序
        /// </summary>
        /// <param name="providerCfg">检索提供程序配置</param>
        private static void LoadSearchProvider(XElement providerCfg)
        {
            ISearchProvider provider = null;
            string providerName = providerCfg.Attribute("name").Value.Trim();
            string providerAssembly = providerCfg.Attribute("type").Value.Trim();

            Type tp = Type.GetType(providerAssembly, true);
            if (typeof(ISearchProvider).IsAssignableFrom(tp))
            {
                provider = (ISearchProvider)Activator.CreateInstance(tp);
                provider.ParseConfig(providerCfg);
            }

            if (provider != null && !s_ProviderDic.ContainsKey(providerName))
            {
                s_ProviderDic.Add(providerName.ToLower(), provider);
            }
        }

        /// <summary>
        /// 加载返回数据类型
        /// </summary>
        /// <param name="itemCfg">返回数据类型配置</param>
        private static void LoadItem(XElement itemCfg)
        {
            string itemAssembly = itemCfg.Attribute("type").Value.Trim();
            string providerName = itemCfg.Attribute("provider").Value.Trim();

            Type tp = Type.GetType(itemAssembly, true);
            if (s_ItemDic != null && !s_ItemDic.ContainsKey(tp))
            {
                s_ItemDic.Add(tp, providerName.ToLower());
            }
        }
    }
}