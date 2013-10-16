using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Utility.Web
{
    /// <summary>
    /// Cookie工具
    /// </summary>
    public static class CookieHelper
    {
        /// <summary>
        /// 读取配置
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="persistType"></param>
        /// <param name="securityLevel"></param>
        /// <param name="parameters"></param>
        private static void LoadConfig(string nodeName, out string persistType, out string securityLevel,
            out Dictionary<string, string> parameters)
        {
            parameters = CookieConfig.GetCookieConfig(nodeName);
            Dictionary<string, string> baseConfig = CookieConfig.GetCookieBaseConfig(nodeName);
            persistType = baseConfig["persistType"];
            securityLevel = baseConfig["securityLevel"];
        }

        private static ICookiePersist _Mobile = new MobileCookiePersister();
        private static ICookiePersist _Web = new WebCookiePersister();
        private static ICookiePersist CreatePersister(string persistType)
        {
            switch (persistType.ToUpper())
            {
                case "MOBILE":
                    return _Mobile;
                case "WEB":
                    return _Web;
                default:
                    return (ICookiePersist)Activator.CreateInstance(Type.GetType(persistType, true));
            }
        }

        private static ICookieEncryption _Normal = new NormalCookie();
        private static ICookieEncryption _Security = new SecurityCookie();
        private static ICookieEncryption _HighSecurity = new HighSecurityCookie();
        private static ICookieEncryption CreateCookieHelper(string securityLevel)
        {
            switch (securityLevel.ToUpper())
            {
                case "LOW":
                    return _Normal;
                case "MIDDLE":
                    return _Security;
                case "HIGH":
                    return _HighSecurity;
                default:
                    return (ICookieEncryption)Activator.CreateInstance(Type.GetType(securityLevel, true));
            }
        }

        /// <summary>
        /// 从配置中读取Cookie名称
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static string GetCookieName(string nodeName, Dictionary<string, string> parameters)
        {
            string name;
            if (parameters != null && parameters.TryGetValue("cookieName", out name)
                && !string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
            return nodeName;
        }

        /// <summary>
        /// 保存Cookie
        /// </summary>
        /// <typeparam name="T">需要存放的Cookie值的类型</typeparam>
        /// <param name="nodeName">配置的Cookie节点名，若未配置，则使用此名作为cookie存储名且使用默认配置</param>
        /// <param name="obj">需要存放的Cookie值</param>
        public static void SaveCookie<T>(string nodeName, T obj)
        {
            string persistType;
            string securityLevel;
            Dictionary<string, string> parameters;
            LoadConfig(nodeName, out persistType, out securityLevel, out parameters);

            ICookiePersist persister = CreatePersister(persistType);
            ICookieEncryption safer = CreateCookieHelper(securityLevel);
            string cookieValue = safer.EncryptCookie<T>(obj, parameters);
            persister.Save(GetCookieName(nodeName, parameters), cookieValue, parameters);
        }
        /// <summary>
        /// 读取Cookie
        /// </summary>
        /// <typeparam name="T">返回的类型</typeparam>
        /// <param name="nodeName">配置的Cookie节点名，若未配置，则使用此名作为cookie存储名且使用默认配置</param>
        /// <returns></returns>
        public static T GetCookie<T>(string nodeName)
        {
            string persistType;
            string securityLevel;
            Dictionary<string, string> parameters;
            LoadConfig(nodeName, out persistType, out securityLevel, out parameters);

            ICookiePersist persister = CreatePersister(persistType);
            string cookieValue = persister.Get(GetCookieName(nodeName, parameters), parameters);
            ICookieEncryption safer = CreateCookieHelper(securityLevel);
            return safer.DecryptCookie<T>(cookieValue, parameters);
        }
    }

}
