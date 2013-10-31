using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using ExWebServer.WebServer.Utility;
using ExWebServer.WebServer.Utility.Encryption;

namespace ExWebServer.WebServer.Handler.SSO
{
    public class SSOCore
    {
        const string X_SSO_KEYNAME_S1 = "sk1";
        const string X_SSO_KEYNAME_S2 = "sk2";

        const string X_SSO_KEY_S1 = "gHacMdbjVhY3fNdh13kMndh6#@3klpOkc7HcmdjhIgd4ndye";
        const string X_SSO_KEY_S2 = "h1jsdG2mNv8aBdhke6G7J3Jsmdjh&klpvd7NdpRgivcndOye";

        private Utility.Encryption.RC4Encrypt _objEnc = new RC4Encrypt();
        private Utility.Encryption.HashEncrypt _objHash = new HashEncrypt();

        public string _strError = "";

        public CookieUserInfo UserInfoFromCookie = null;

        public string Domain { get; set; }

        public SSOCore()
        {
            this.UserInfoFromCookie = new CookieUserInfo();
            this.Domain = "";
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="strInput"></param>
        /// <returns></returns>
        private string Encrypt(string strInput)
        {
            return _objEnc.Encrypt(strInput, X_SSO_KEY_S1, Utility.Encryption.RC4Encrypt.EncoderMode.Base64Encoder);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="strInput"></param>
        /// <returns></returns>
        private string Decrypt(string strInput)
        {
            return _objEnc.Decrypt(strInput, X_SSO_KEY_S1, Utility.Encryption.RC4Encrypt.EncoderMode.Base64Encoder);
        }

        private string SHA1Sign(string strInput)
        {
            return _objHash.SHA1Encrypt(strInput + X_SSO_KEY_S2);
        }

        /// <summary>
        /// 检验SSO Cookie是否有效
        /// </summary>
        /// <returns>1=SSOCookie有效</returns>
        public int CheckSSOCookie(string ssoCookie)
        {
            string strEncBuffer = "";
            string strContent = "";
            string strSHA1Sign = "", strShA1Temp = "";

            try
            {

                if (ssoCookie.Length < 40)
                    return 0;
                //  取出签名和密文
                strSHA1Sign = ssoCookie.Substring(0, 40);
                strEncBuffer = ssoCookie.Substring(40);
                //  签名校验
                strShA1Temp = SHA1Sign(HttpUtility.UrlDecode(strEncBuffer).Trim());
                if (strSHA1Sign != strShA1Temp)
                    return 0;
                strEncBuffer = HttpUtility.UrlDecode(strEncBuffer);
                //  还原成明文
                strContent = Decrypt(strEncBuffer);
                if (strContent.Length == 0)
                    return 0;
                //  从明文中取出各个字段值
                char[] delimiterChars = { ';' };
                string[] arTemp = strContent.Split(delimiterChars);
                if (arTemp.Length >= 4)
                {
                    this.UserInfoFromCookie.nUID = Convert.ToInt32(arTemp[0]);
                    this.UserInfoFromCookie.strRealName = HttpUtility.UrlDecode(arTemp[1]);
                    this.UserInfoFromCookie.nServerID = Convert.ToInt32(arTemp[2]);
                    this.UserInfoFromCookie.nOSID = Convert.ToInt32(arTemp[3]);
                    //this._objUserInfo.nClassID = Convert.ToInt32(arTemp[5]);
                    //RenewOnlineUserItem(this._objUserInfo.nUID);
                    return 1;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}
