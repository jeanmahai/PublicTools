using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;
using System.IO;

namespace Common.Utility
{
    public class CodeNamePairSection : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            CodeNamePairSetting setting = new CodeNamePairSetting();
            if (section != null)
            {
                setting.BaseAbsoluteFolder = GetNodeAttribute(section, "baseFolder");
                setting.AppendItems = GetAppendItems(section, "appendItems", "appendItem");
            }
            if (setting.BaseAbsoluteFolder == null || setting.BaseAbsoluteFolder.Trim().Length <= 0)
            {
                setting.BaseAbsoluteFolder = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Configuration\\CodeNamePairs");
            }
            else
            {
                string p = Path.GetPathRoot(setting.BaseAbsoluteFolder);
                if (p == null || p.Trim().Length <= 0) // 说明是相对路径
                {
                    setting.BaseAbsoluteFolder = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, setting.BaseAbsoluteFolder);
                }
            }
            if (setting.AppendItems == null)
            {
                setting.AppendItems = new Dictionary<string, Tuple<string, string>>(0);
            }
            return setting;
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

        private Dictionary<string, Tuple<string, string>> GetAppendItems(XmlNode node, string pNodeName, string nodeName)
        {
            node = node.SelectSingleNode(pNodeName);
            if (node == null || node.ChildNodes == null || node.ChildNodes.Count <= 0)
            {
                return new Dictionary<string, Tuple<string, string>>(0);
            }
            Dictionary<string, Tuple<string, string>> rst = new Dictionary<string, Tuple<string, string>>(node.ChildNodes.Count * 2);
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == nodeName)
                {
                    string lang = GetNodeAttribute(child, "languageCode").ToUpper();
                    if (rst.ContainsKey(lang))
                    {
                        throw new ApplicationException("Duplicated language code '" + lang + "' in codeNamePair's appendItem setting in config file.");
                    }
                    string select = GetNodeAttribute(child, "selectAppendItem");
                    string all = GetNodeAttribute(child, "allAppendItem");
                    rst.Add(lang, new Tuple<string, string>(select, all));
                }
            }
            return rst;
        }

        private static CodeNamePairSetting s_Setting;
        private static object s_SyncObj = new object();

        internal static CodeNamePairSetting GetSetting()
        {
            if (s_Setting == null)
            {
                lock (s_SyncObj)
                {
                    if (s_Setting == null)
                    {
                        s_Setting = ConfigurationManager.GetSection("codeNamePair") as CodeNamePairSetting;
                        if (s_Setting == null)
                        {
                            s_Setting = new CodeNamePairSetting()
                            {
                                AppendItems = new Dictionary<string, Tuple<string, string>>(0),
                                BaseAbsoluteFolder = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Configuration\\CodeNamePairs")
                            };
                        }
                    }
                }
            }
            return s_Setting;
        }
    }

    internal class CodeNamePairSetting
    {
        public string BaseAbsoluteFolder
        {
            get;
            set;
        }

        public Dictionary<string, Tuple<string, string>> AppendItems
        {
            get;
            set;
        }
    }
}
