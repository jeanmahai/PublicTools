using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using ExWebServer.WebServer.Utility;
using ExWebServer.WebServer.Utility.Encryption;

namespace ExWebServer.WebServer.Handler.SSO
{
    public class JJSSOCore
    {
        //const string X_SSO_KEYNAME_USERID = "User_id";
        //const string X_SSO_KEYNAME_NICKNAME = "User_Nick";
        //const string X_SSO_KEYNAME_USERCOOKIEKEY = "UserCookieKey";

        const string TK_WEB_COOKIE_KEY = "jmowfbSONvkW7coYu15t";

        private Utility.Encryption.HashEncrypt _objHash = new HashEncrypt();

        public string _strError = "";

        public CookieUserInfo UserInfoFromCookie = null;

        public string Domain { get; set; }

        public JJSSOCore()
        {
            this.UserInfoFromCookie = new CookieUserInfo();
            this.Domain = "";
        }

        private string MD5Sign(string strInput)
        {
            return _objHash.MD5Encrypt(string.Format("{0}{1}", strInput, TK_WEB_COOKIE_KEY));
        }

        /// <summary>
        /// 检验SSO Cookie是否有效
        /// </summary>
        /// <returns>1=SSOCookie有效</returns>
        public int CheckSSOCookie(string userID, string nickName, string cookieKey)
        {
            try
            {

                if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(nickName) || string.IsNullOrEmpty(cookieKey))
                    return 0;

                string md5String = MD5Sign(userID.ToString());
                if (string.Compare(cookieKey, md5String, true) != 0)
                    return 0;
                //  签名验证通过
                this.UserInfoFromCookie.nUID = Int32.Parse(userID);
                //this.UserInfoFromCookie.strRealName = HttpUtility.UrlDecode(userNick, Encoding.GetEncoding("GB2312"));
                this.UserInfoFromCookie.strRealName = HttpUtility.UrlDecode(nickName, Encoding.GetEncoding(936));

                return 1;
            }
            catch
            {
                return 0;
            }
        }
    }
}
