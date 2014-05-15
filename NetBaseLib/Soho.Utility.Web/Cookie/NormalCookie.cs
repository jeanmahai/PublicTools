using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Soho.Utility.Web
{
    /// <summary>
    /// 明文存储的COOKIE存取
    /// </summary>
    internal class NormalCookie : ICookieEncryption
    {
        #region ICookieEncryption Members

        public string EncryptCookie<T>(T obj, Dictionary<string, string> parameters)
        {
            return SerializationUtility.JsonSerialize2(obj);
        }

        public T DecryptCookie<T>(string cookieValue, Dictionary<string, string> parameters)
        {
            return SerializationUtility.JsonDeserialize2<T>(cookieValue);
        }

        #endregion
    }
}
