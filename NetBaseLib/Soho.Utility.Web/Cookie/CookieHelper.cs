using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Soho.Utility.Web
{
    /// <summary>
    /// Cookie工具
    /// </summary>
    public static class CookieHelper
    {
        #region Private Member

        private static void LoadConfig(string nodeName, out string persistType, out string securityLevel,
            out Dictionary<string, string> parameters)
        {
            var entity = CookieConfig.GetCookieConfig(nodeName);
            parameters = entity.Properties;
            persistType = entity.PersistType;
            securityLevel = entity.SecurityLevel;
        }

        private static ICookiePersist s_Mobile = new MobileCookiePersister();
        private static ICookiePersist s_Web = new WebCookiePersister();
        private static ICookiePersist CreatePersister(string persistType)
        {
            switch (persistType.ToUpper())
            {
                case "MOBILE":
                    return s_Mobile;
                case "WEB":
                    return s_Web;
                case "AUTO":
                    return DiscernPersistType();
                default:
                    return (ICookiePersist)Activator.CreateInstance(Type.GetType(persistType, true));
            }
        }

        private static ICookiePersist DiscernPersistType()
        {
            if (HttpContext.Current.Request.Headers.AllKeys.Contains(MobileCookie.COOKIE_NAME)
                || HttpContext.Current.Request.Browser.IsMobileDevice
                || HttpContext.Current.Request.Headers.AllKeys.Contains("x-soho-app-id")
                //mobile 所有的请求必须经过core里的方法进行调用,否则头信息不能回写
                //|| (HttpContext.Current.Request.QueryString.AllKeys.Count()>0&&HttpContext.Current.Request.QueryString["type"].ToUpper()=="MOBILE")
                )
                return s_Mobile;
            return s_Web;
        }

        private static ICookieEncryption s_Normal = new NormalCookie();
        private static ICookieEncryption s_Security = new SecurityCookie();
        private static ICookieEncryption s_HighSecurity = new HighSecurityCookie();
        private static ICookieEncryption CreateCookieHelper(string securityLevel)
        {
            switch (securityLevel.ToUpper())
            {
                case "LOW":
                    return s_Normal;
                case "MIDDLE":
                    return s_Security;
                case "HIGH":
                    return s_HighSecurity;
                default:
                    return (ICookieEncryption)Activator.CreateInstance(Type.GetType(securityLevel, true));
            }
        }

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

        #endregion

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

            ICookieEncryption safer = CreateCookieHelper(securityLevel);
            string cookieValue = safer.EncryptCookie<T>(obj, parameters);
            if (persistType.ToUpper().Equals("AUTO"))
            {
                ICookiePersist persister = s_Web;
                persister.Save(GetCookieName(nodeName, parameters), cookieValue, parameters);
                persister = s_Mobile;
                persister.Save(GetCookieName(nodeName, parameters), cookieValue, parameters);
            }
            else
            {
                ICookiePersist persister = CreatePersister(persistType);
                persister.Save(GetCookieName(nodeName, parameters), cookieValue, parameters);
            }
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
            var result = safer.DecryptCookie<T>(cookieValue, parameters);
            if (result != null)
            {
                return result;
            }
            else
            {
                persister = s_Web;
                cookieValue = persister.Get(GetCookieName(nodeName, parameters), parameters);
                return safer.DecryptCookie<T>(cookieValue, parameters);
            }
        }

        /// <summary>
        /// 清除指定的Cookie
        /// </summary>
        /// <param name="nodeName">配置的Cookie节点名，若未配置，则使用此名作为cookie存储名且使用默认配置</param>
        public static void RemoveCookie(string nodeName)
        {
            string persistType;
            string securityLevel;
            Dictionary<string, string> parameters;
            LoadConfig(nodeName, out persistType, out securityLevel, out parameters);

            ICookiePersist persister = CreatePersister(persistType);
            persister.Remove(GetCookieName(nodeName, parameters), parameters);
        }
    }
}
