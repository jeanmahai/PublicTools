using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Threading;

namespace Soho.Utility
{
    public static class ResouceManager
    {
        //private static List<string> s_LanguageCodeList = new List<string>() { "EN-US", "ZH-CN", "ZH-TW"};
        //private const string s_DefaultLanguageCode = "ZH-CN";
        private const string s_FileSuffixName = "XML";
        private static object s_SyncObj = new object();
        private static Dictionary<string, MessageResource> s_MessageResDictionary = new Dictionary<string, MessageResource>();

        internal static void ClearResouceCache()
        {
            if (s_MessageResDictionary != null && s_MessageResDictionary.Count > 0)
            {
                lock (s_SyncObj)
                {
                    if (s_MessageResDictionary != null && s_MessageResDictionary.Count > 0)
                    {
                        s_MessageResDictionary.Clear();
                    }
                }
            }
        }

        internal static void LoadResouce(string cacheKey, string filePath)
        {
            cacheKey = cacheKey.ToUpper();
            MessageResource res = SerializationUtility.LoadFromXml<MessageResource>(filePath);
            lock (s_SyncObj)
            {
                if (s_MessageResDictionary.ContainsKey(cacheKey))
                {
                    s_MessageResDictionary[cacheKey] = res;
                }
                else
                {
                    s_MessageResDictionary.Add(cacheKey, res);
                }
            }
        }

        internal static void RemoveResouce(string cacheKey)
        {
            cacheKey = cacheKey.ToUpper();
            if (s_MessageResDictionary.ContainsKey(cacheKey))
            {
                lock (s_SyncObj)
                {
                    if (s_MessageResDictionary.ContainsKey(cacheKey))
                    {
                        s_MessageResDictionary.Remove(cacheKey);
                    }
                }
            }
        }

        /// <summary>
        /// 获取Message的描述信息
        /// </summary>
        /// <param name="resouceFileTitle">资源文件的名称（不包含语言和后缀名部分，但需要包含相对于Resources根目录的路径，使用‘.’分隔目录，不区分大小写）</param>
        /// <param name="keyName">Message的键值</param>
        /// <param name="inputLanguageCode">需要调用的Message语言版本</param>
        /// <returns>Message 描述</returns>
        public static string GetMessageString(string resouceFileTitle, string keyName, string inputLanguageCode)
        {
            string fileLanguageExtension = string.Empty;
            string fileName = string.Format("{0}.{1}.{2}", resouceFileTitle.Trim(), inputLanguageCode.Trim(), s_FileSuffixName); 
            string upperFileName = fileName.ToUpper();
            if (!s_MessageResDictionary.ContainsKey(upperFileName))
            {
                throw new InvalidOperationException(string.Format("The resource content of file [{0}] can't be found in memory cache.", fileName));
            }
            MessageResource res = s_MessageResDictionary[upperFileName];
            if (res != null && res.MessageList != null && res.MessageList.Count > 0)
            {
                MessageEntity mess = res.MessageList.Find(c => String.Compare(c.KeyName, keyName, true) == 0);
                if (mess != null)
                {
                    return (mess.Text == null ? string.Empty : mess.Text.Trim());
                }
            }
            throw new InvalidOperationException(string.Format("Can't found key:[{0}] resouce string in resouce file [{1}]", keyName, fileName));
        }

        /// <summary>
        /// 获取Message的描述信息，本方法将自动获取Client端传入的UI Language Code
        /// </summary>
        /// <param name="resouceFileTitle">资源文件的名称（不包含语言和后缀名部分，但需要包含相对于Resources根目录的路径，使用‘.’分隔目录，不区分大小写）</param>
        /// <param name="keyName">Message的键值</param>
        /// <returns>Message 描述</returns>
        public static string GetMessageString(string resouceFileTitle, string keyName)
        {
            string inputLanguageCode = Thread.CurrentThread.CurrentCulture.Name;
            return GetMessageString(resouceFileTitle, keyName,inputLanguageCode);
        }
    }
}
