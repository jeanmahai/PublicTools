using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Common.Utility.Web
{
    /// <summary>
    /// 明文存储的COOKIE存取
    /// </summary>
    internal class NormalCookie : ICookieEncryption
    {
        #region ICookieEncryption Members

        public string EncryptCookie<T>(T obj, Dictionary<string, string> parameters)
        {
            return SerializationUtility.JsonSerialize(obj);
        }

        public T DecryptCookie<T>(string cookieValue, Dictionary<string, string> parameters)
        {
            return SerializationUtility.JsonDeserialize<T>(cookieValue);
        }

        #endregion
    }
}
