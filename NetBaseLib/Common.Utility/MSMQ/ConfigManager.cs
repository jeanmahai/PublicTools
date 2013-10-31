using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Common.Utility.MSMQ
{
    internal class ConfigManager
    {
        private static Dictionary<string, string> _Config = null;
        public static Dictionary<string, string> Config
        {
            get
            {
                if (_Config == null)
                {
                    _Config = LoadConfig();
                }
                return _Config;
            }
        }
        /// <summary>
        /// 获取MSMQ配置文件根目录
        /// </summary>
        /// <returns></returns>
        private static string GetBaseFolderPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, MSMQDefine.DEFAULT_FOLDER);
        }
        /// <summary>
        /// 加载配置
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, string> LoadConfig()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string configPath = Path.Combine(GetBaseFolderPath(), MSMQDefine.CONFIG_FILE);
            if (!File.Exists(configPath))
            {
                throw new ApplicationException("没有找到MSMQ配置文件");
            }
            else
            {
                using (FileStream fs = new FileStream(configPath, FileMode.Open, FileAccess.Read))
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(configPath);
                    XmlNodeList nodeList = xmldoc.SelectNodes("root/item");
                    foreach (XmlNode item in nodeList)
                    {
                        foreach (XmlNode _Item in item.ChildNodes)
                        {
                            result[string.Format("{0}-{1}", item.Attributes["name"].Value.Trim(), _Item.LocalName.Trim())] = _Item.InnerText.Trim();
                        }
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 读取配置条目
        /// </summary>
        /// <param name="agentName"></param>
        /// <param name="busName"></param>
        /// <returns></returns>
        public static string GetItem(string agentName, string busName)
        {
            return Config[string.Format("{0}-{1}", agentName, busName)];
        }
    }
}
