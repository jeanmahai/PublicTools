using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Common.Utility.Web
{
    /// <summary>
    /// 经加密算法加密的COOKIE存取
    /// </summary>
    internal class SecurityCookie : ICookieEncryption
    {
        #region ICookieEncryption Members

        public string EncryptCookie<T>(T obj, Dictionary<string, string> parameters)
        {
            string strCookieValue = string.Empty;
            string strEncCookieValue = string.Empty;
            string strSHA1Sign = string.Empty;

            strCookieValue = SerializationUtility.JsonSerialize(obj);

            strEncCookieValue = RC4Encrypt.Encrypt(strCookieValue, parameters["rc4key"], RC4Encrypt.EncoderMode.HexEncoder).Trim();
            strSHA1Sign = HashEncrypt.SHA1Encrypt(strEncCookieValue + parameters["hashkey"]);
            strEncCookieValue = HttpUtility.UrlEncode(strEncCookieValue);
            strEncCookieValue = strSHA1Sign + strEncCookieValue;

            return strEncCookieValue;
        }

        public T DecryptCookie<T>(string cookieValue, Dictionary<string, string> parameters)
        {
            T result = default(T);
            string strEncCookieValue = string.Empty;
            string strContent = string.Empty;
            string strSHA1Sign = string.Empty;
            string strShA1Temp = string.Empty;

            try
            {
                if (cookieValue.Length < 40)
                    return result;
                //  取出签名和密文
                strSHA1Sign = cookieValue.Substring(0, 40);
                strEncCookieValue = cookieValue.Substring(40);
                //  签名校验
                strShA1Temp = HashEncrypt.SHA1Encrypt(HttpUtility.UrlDecode(strEncCookieValue).Trim() + parameters["hashkey"]);
                if (strSHA1Sign != strShA1Temp)
                    return result;
                strEncCookieValue = HttpUtility.UrlDecode(strEncCookieValue);
                //  还原成明文
                strContent = RC4Encrypt.Decrypt(strEncCookieValue, parameters["rc4key"], RC4Encrypt.EncoderMode.HexEncoder);
                if (strContent.Length == 0)
                    return result;

                result = SerializationUtility.JsonDeserialize<T>(strContent);

                return result;
            }
            catch
            {
                return result;
            }
        }

        #endregion
    }
}
