using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Xml.Linq;

namespace Common.Utility
{
    public static class CodeNamePairManager
    {
        private static Dictionary<string, Dictionary<string, CodeNamePairList>> s_Cache = new Dictionary<string, Dictionary<string, CodeNamePairList>>();
        private static object s_SyncObj = new object();

        private class CodeNamePairList : List<KeyValuePair<string, string>>
        {
            public string SelectAppendItem
            {
                get;
                set;
            }

            public string AllAppendItem
            {
                get;
                set;
            }
        }

        private static Dictionary<string, CodeNamePairList> LoadConfigFile(string domainName, string languageCode)
        {
            string getFullConfigPath = Path.Combine(CodeNamePairSection.GetSetting().BaseAbsoluteFolder, domainName + "." + languageCode + ".config");
            if (!File.Exists(getFullConfigPath))
            {
                throw new FileNotFoundException("Can't find the CodeNamePair config file: " + getFullConfigPath + "!");
            }

            Tuple<string, string> defaultAppendItem;
            CodeNamePairSection.GetSetting().AppendItems.TryGetValue(languageCode.ToUpper(), out defaultAppendItem);

            Dictionary<string, CodeNamePairList> rst = new Dictionary<string, CodeNamePairList>();
            XElement doc = XElement.Load(getFullConfigPath);
            foreach (var x in doc.Descendants("codeNamePair"))
            {
                if (x.Attribute("key") == null || x.Attribute("key").Value == null || x.Attribute("key").Value.Trim().Length <= 0)
                {
                    throw new ApplicationException("There are some 'codeNamePair' node without attribute 'key' in the CodeNamePair config file: " + getFullConfigPath + ", please check it!");
                }
                string key = x.Attribute("key").Value.Trim().ToUpper();
                if (rst.ContainsKey(key))
                {
                    throw new ApplicationException("Duplicated value '" + x.Attribute("key").Value.Trim() + "' of attribute 'key' in 'codeNamePair' node in the CodeNamePair config file: " + getFullConfigPath + ", please check it (ex: ignore case)!");
                }
                CodeNamePairList v = new CodeNamePairList();
                foreach (var listItem in x.Descendants("item"))
                {
                    if (listItem.Attribute("code") != null && listItem.Attribute("code").Value != null && listItem.Attribute("code").Value.Trim().Length > 0)
                    {
                        string itemKey = listItem.Attribute("code").Value.Trim();
                        string itemText = listItem.Attribute("name") == null ? null : (listItem.Attribute("name").Value == null ? null : listItem.Attribute("name").Value.Trim());
                        v.Add(new KeyValuePair<string, string>(itemKey, EnvironmentVariableManager.ReplaceVariable(itemText)));
                    }
                }

                var getCustomeAppendItem_SELECT = x.Attribute("selectAppendItem");
                var getCustomeAppendItem_ALL = x.Attribute("allAppendItem");
                if (null != getCustomeAppendItem_SELECT && getCustomeAppendItem_SELECT.Value != null && getCustomeAppendItem_SELECT.Value.Trim().Length > 0)
                {
                    v.SelectAppendItem = getCustomeAppendItem_SELECT.Value.Trim();
                }
                else if (defaultAppendItem != null && defaultAppendItem.Item1 != null && defaultAppendItem.Item1.Trim().Length > 0)
                {
                    v.SelectAppendItem = defaultAppendItem.Item1.Trim();
                }

                if (null != getCustomeAppendItem_ALL && getCustomeAppendItem_ALL.Value != null && getCustomeAppendItem_ALL.Value.Trim().Length > 0)
                {
                    v.AllAppendItem = getCustomeAppendItem_ALL.Value;
                }
                else if (defaultAppendItem != null && defaultAppendItem.Item2 != null && defaultAppendItem.Item2.Trim().Length > 0)
                {
                    v.AllAppendItem = defaultAppendItem.Item2.Trim();
                }
                rst.Add(key, v);
            }

            return rst;
        }

        /// <summary>
        /// 获取默认的附加项（“请选择”和“所有”）的配置数据，对应web.config中codeNamePair节点下的appendItems的对应语言的配置（根据当前线程获取LanguageCode）
        /// </summary>
        /// <param name="appendSelectItem">“请选择”附加项的配置数据</param>
        /// <param name="appendAllItem">“所有”附加项的配置数据</param>
        public static void GetDefaultAppendItem(out CodeNamePair appendSelectItem, out CodeNamePair appendAllItem)
        {
            Tuple<string, string> defaultAppendItem;
            CodeNamePairSection.GetSetting().AppendItems.TryGetValue(Thread.CurrentThread.CurrentCulture.Name.ToUpper(), out defaultAppendItem);

            if (defaultAppendItem != null && defaultAppendItem.Item1 != null && defaultAppendItem.Item1.Trim().Length > 0)
            {
                appendSelectItem = new CodeNamePair { Code = null, Name = defaultAppendItem.Item1.Trim() };
            }
            else
            {
                appendSelectItem = null;
            }

            if (defaultAppendItem != null && defaultAppendItem.Item2 != null && defaultAppendItem.Item2.Trim().Length > 0)
            {
                appendAllItem = new CodeNamePair { Code = null, Name = defaultAppendItem.Item2.Trim() };
            }
            else
            {
                appendAllItem = null;
            }
        }

        /// <summary>
        ///  获取配置的CodeNamePair列表（根据当前线程获取LanguageCode）
        /// </summary>
        /// <param name="domainName">Domain名称: [Customer,IM,Inventory,Invoice,MKT,PO,RMA,SO,Common]</param>
        /// <param name="key">对应到配置文件中xml节点codeNamePair的属性key的值</param>
        /// <returns>返回对应key的codeNamePair节点下的所有item的列表，每个item节点对应一个KeyValuePair对象，item节点的code属性的值赋值到KeyValuePair对象的属性Key上，item节点的name属性的值赋值到KeyValuePair对象的属性Value上</returns>
        public static List<CodeNamePair> GetList(string domainName, string key)
        {
            return GetList(domainName, key, Thread.CurrentThread.CurrentCulture.Name);
        }


        public static string GetName(string domainName, string key, string code)
        {
            List<CodeNamePair> list = GetList(domainName, key, Thread.CurrentThread.CurrentCulture.Name);
            CodeNamePair cnp = list.Find(f => f.Code.Trim().ToLower() == code.Trim().ToLower());
            if (cnp != null)
            {
                return cnp.Name;
            }
            return null;
        }


        /// <summary>
        ///  获取配置的CodeNamePair列表（根据当前线程获取LanguageCode）
        /// </summary>
        /// <param name="domainName">Domain名称: [Customer,IM,Inventory,Invoice,MKT,PO,RMA,SO,Common]</param>
        /// <param name="key">对应到配置文件中xml节点codeNamePair的属性key的值</param>
        /// <param name="appendSelectItem">返回配置文件中对应key的xml节点codeNamePair的属性customSelectAppendItem的值，作为“选择”的附加项（如果不存在此配置，则读取系统默认附加项）</param>
        /// <param name="appendAllItem">返回配置文件中对应key的xml节点codeNamePair的属性customAllAppendItem的值，作为“所有”的附加项（如果不存在此配置，则读取系统默认附加项）</param>
        /// <returns>返回对应key的codeNamePair节点下的所有item的列表，每个item节点对应一个KeyValuePair对象，item节点的code属性的值赋值到KeyValuePair对象的属性Key上，item节点的name属性的值赋值到KeyValuePair对象的属性Value上</returns>
        public static List<CodeNamePair> GetList(string domainName, string key,
            out CodeNamePair appendSelectItem, out CodeNamePair appendAllItem)
        {
            return GetList(domainName, key, Thread.CurrentThread.CurrentCulture.Name, out appendSelectItem, out appendAllItem);
        }

        /// <summary>
        ///  获取配置的对应语言编码的CodeNamePair列表
        /// </summary>
        /// <param name="domainName">Domain名称: [Customer,IM,Inventory,Invoice,MKT,PO,RMA,SO,Common]</param>
        /// <param name="key">对应到配置文件中xml节点codeNamePair的属性key的值</param>
        /// <param name="languageCode">语言编码</param>
        /// <returns>返回对应key的codeNamePair节点下的所有item的列表，每个item节点对应一个KeyValuePair对象，item节点的code属性的值赋值到KeyValuePair对象的属性Key上，item节点的name属性的值赋值到KeyValuePair对象的属性Value上</returns>
        private static List<CodeNamePair> GetList(string domainName, string key, string languageCode)
        {
            CodeNamePair appendSelectItem;
            CodeNamePair appendAllItem;
            return GetList(domainName, key, languageCode, out appendSelectItem, out appendAllItem);
        }

        /// <summary>
        ///  获取配置的对应语言编码的CodeNamePair列表
        /// </summary>
        /// <param name="domainName">Domain名称: [Customer,IM,Inventory,Invoice,MKT,PO,RMA,SO,Common]</param>
        /// <param name="key">对应到配置文件中xml节点codeNamePair的属性key的值</param>
        /// <param name="languageCode">语言编码</param>
        /// <param name="appendSelectItem">返回配置文件中对应key的xml节点codeNamePair的属性customSelectAppendItem的值，作为“选择”的附加项（如果不存在此配置，则读取系统默认附加项）</param>
        /// <param name="appendAllItem">返回配置文件中对应key的xml节点codeNamePair的属性customAllAppendItem的值，作为“所有”的附加项（如果不存在此配置，则读取系统默认附加项）</param>
        /// <returns>返回对应key的codeNamePair节点下的所有item的列表，每个item节点对应一个KeyValuePair对象，item节点的code属性的值赋值到KeyValuePair对象的属性Key上，item节点的name属性的值赋值到KeyValuePair对象的属性Value上</returns>
        private static List<CodeNamePair> GetList(string domainName, string key, string languageCode,
            out CodeNamePair appendSelectItem, out CodeNamePair appendAllItem)
        {
            string cacheKey = domainName.ToUpper().Trim() + "." + languageCode.ToUpper().Trim();
            Dictionary<string, CodeNamePairList> data;
            if (!s_Cache.TryGetValue(cacheKey, out data))
            {
                lock (s_SyncObj)
                {
                    if (!s_Cache.TryGetValue(cacheKey, out data))
                    {
                        data = LoadConfigFile(domainName, languageCode);
                        s_Cache.Add(cacheKey, data);
                    }
                }
            }
            CodeNamePairList list;
            if (!data.TryGetValue(key.ToUpper(), out list) || list == null)
            {
                appendSelectItem = null;
                appendAllItem = null;
                return new List<CodeNamePair>(0);
            }
            appendSelectItem = new CodeNamePair { Code = null, Name = list.SelectAppendItem };
            appendAllItem = new CodeNamePair { Code = null, Name = list.AllAppendItem };
            if (list == null || list.Count <= 0)
            {
                return new List<CodeNamePair>(0);
            }
            List<CodeNamePair> rstList = new List<CodeNamePair>(list.Count);
            foreach (var entry in list)
            {
                rstList.Add(new CodeNamePair { Code = entry.Key, Name = entry.Value });
            }
            return rstList;
        }

        internal static List<CodeNamePair> GetCodeNamePairList(string domainName, string key, CodeNamePairAppendItemType appendItemType)
        {
            CodeNamePair getCustomAppendSelectedItem;
            CodeNamePair getCustomAppendAllItem;

            List<CodeNamePair> returnList = CodeNamePairManager.GetList(domainName, key, out  getCustomAppendSelectedItem, out getCustomAppendAllItem);

            CodeNamePair selectedItem;
            CodeNamePair allItem;
            CodeNamePairManager.GetDefaultAppendItem(out selectedItem, out allItem);
            switch (appendItemType)
            {
                case CodeNamePairAppendItemType.All:
                    returnList.Insert(0, allItem);
                    break;
                case CodeNamePairAppendItemType.Select:
                    returnList.Insert(0, selectedItem);
                    break;
                case CodeNamePairAppendItemType.Custom_Select:
                    returnList.Insert(0, getCustomAppendSelectedItem);
                    break;
                case CodeNamePairAppendItemType.Custom_All:
                    returnList.Insert(0, getCustomAppendAllItem);
                    break;
                default:
                    break;
            }
            return returnList;
        }

        internal static void ClearCache(string cacheKey)
        {
            cacheKey = cacheKey.ToUpper();
            if (s_Cache.ContainsKey(cacheKey))
            {
                lock (s_SyncObj)
                {
                    if (s_Cache.ContainsKey(cacheKey))
                    {
                        s_Cache.Remove(cacheKey);
                    }
                }
            }
        }
    }
}
