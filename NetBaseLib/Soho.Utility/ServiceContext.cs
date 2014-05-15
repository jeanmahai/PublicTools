using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Web;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Web;
using System.Security.Cryptography;
using System.IO;

namespace Soho.Utility
{
    public static class ServiceContext
    {
        public static IContext Current
        {
            get
            {
                // 可以配置自定义的上下文对象容器，便于以后切换服务端具体的技术框架容器，以及方便做UnitTest（可以定义mock的上下文对象）
                string contextTypeName = ConfigurationManager.AppSettings["ServiceContextType"];
                if (contextTypeName != null && contextTypeName.Trim().Length > 0)
                {
                    return (IContext)Activator.CreateInstance(Type.GetType(contextTypeName, true));
                }
                // 默认不配置则使用WebService的方式的上下文
                return new WebServiceContext();
            }
        }
    }

    public interface IContext
    {
        /// <summary>
        /// 当前操作用户的系统唯一编号
        /// </summary>
        int UserSysNo { get; }

        /// <summary>
        /// 当前用户的ID
        /// </summary>
        string UserID { get; }

        /// <summary>
        /// 当前操作用户的显示名
        /// </summary>
        string UserDisplayName { get; }        

        /// <summary>
        /// 当前操作客户端的IP地址
        /// </summary>
        string ClientIP { get; }

        /// <summary>
        /// 将当前的 IContext 实例附加到指定的 IContext 实例
        /// </summary>
        /// <param name="owner">要附加的IContext实例</param>
        void Attach(IContext owner);
    }

    internal class WebServiceContext : IContext
    {
        private const string publicKey = "RGVz25uIIYZDdxYCEexY1WQx";

        /// <summary>
        /// 解析混淆的random number
        /// </summary>
        /// <param name="randomCustomerSysNo"></param>
        /// <returns></returns>
        private static string ExtractRandomSys(string randomCustomerSysNo)
        {
            if (string.IsNullOrEmpty(randomCustomerSysNo) == true)
            {
                return string.Empty;
            }

            char[] arrayNo = randomCustomerSysNo.ToCharArray();
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < arrayNo.Length; i++)
            {
                if (i % 2 == 0)
                {
                    sb.Append(arrayNo[i]);
                }
            }

            return sb.ToString();
        }

        private string ExtractRandomCustSys(string randomCustomerSysNo)
        {
            if (string.IsNullOrEmpty(randomCustomerSysNo) == true)
            {
                return string.Empty;
            }

            char[] arrayNo = randomCustomerSysNo.ToCharArray();
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < arrayNo.Length; i++)
            {
                if (i % 2 == 1)
                {
                    sb.Append(arrayNo[i]);
                }
            }

            return sb.ToString();
        }

        private Dictionary<string, string> GetCustomerLoginCookie()
        {
            Dictionary<string, string> kv = new Dictionary<string, string>();
            var cookie = HttpContext.Current.Request.Cookies["CustomerLogin"];
            if (cookie != null)
            {
                if (cookie.Values != null && cookie.Values.Count > 0)
                {
                    foreach (string key in cookie.Values)
                    {
                        kv.Add(key, cookie.Values[key]);
                    }
                }
            }
            return kv;
        }

        private byte[] StringToByte(string stringToConvert, int length)
        {
            char[] charArray = stringToConvert.ToCharArray();
            byte[] byteArray = new byte[length];
            int byteArrayLength = charArray.Length >= length ? length : charArray.Length;
            for (int i = 0; i < byteArrayLength; i++)
            {
                byteArray[i] = Convert.ToByte(charArray[i]);
            }

            return byteArray;
        }

        /// <summary>
        /// 把字节数组转换成字符串。
        /// </summary>
        /// <param name="buff">要转换的字节数组。</param>
        /// <param name="encoding">编码类型。</param>        
        private string BytesToString(byte[] buff, Encoding encoding)
        {
            return Convert.ToBase64String(buff);
        }

        private string Encrypt(string plainText, string key, Encoding encoding)
        {
            TripleDES tripleDes = new TripleDESCryptoServiceProvider();
            tripleDes.Key = StringToByte(publicKey, 24);
            tripleDes.IV = StringToByte(key, 8);
            byte[] desKey = tripleDes.Key;
            byte[] desIV = tripleDes.IV;

            ICryptoTransform encryptor = tripleDes.CreateEncryptor(desKey, desIV);

            string encryptedString = string.Empty;
            using (MemoryStream mStream = new MemoryStream())
            {
                CryptoStream cStream = new CryptoStream(mStream, encryptor, CryptoStreamMode.Write);

                // Write all data to the crypto stream and flush it.
                byte[] toEncryptedBytes = encoding.GetBytes(plainText);
                cStream.Write(toEncryptedBytes, 0, toEncryptedBytes.Length);
                cStream.FlushFinalBlock();

                // Get the encrypted array of bytes.
                byte[] encryptedBytes = mStream.ToArray();

                encryptedString = BytesToString(encryptedBytes, encoding);
            }

            return encryptedString;
        }

        private string GenerateCustomerKey(string randomCustomerSysNo)
        {
            if (string.IsNullOrEmpty(randomCustomerSysNo))
            {
                return string.Empty;
            }

            string customerSysNo = ExtractRandomCustSys(randomCustomerSysNo);
            string randomSysNo = ExtractRandomSys(randomCustomerSysNo);
            string key = customerSysNo + randomSysNo;
            string encreptValue = Encrypt(customerSysNo, key, Encoding.UTF8);
            return encreptValue;
        }

        public int UserSysNo
        {
            get
            {
                var kv = GetCustomerLoginCookie();
                if (kv.Keys.Contains("SysNo"))
                {
                    string randomCustomerSysNo = kv["SysNo"];

                    string newKey = GenerateCustomerKey(randomCustomerSysNo);
                    if (newKey != kv["Key"])
                    {
                        return -1;
                    }

                    return int.Parse(ExtractRandomCustSys(randomCustomerSysNo));
                }
                return -1;
            }
        }

        public string UserID
        {
            get
            {
                var kv = GetCustomerLoginCookie();
                if (kv.Keys.Contains("ID"))
                    return kv["ID"];
                return string.Empty;
            }
        }

        public string UserDisplayName
        {
            get
            {
                var kv = GetCustomerLoginCookie();
                if (kv.Keys.Contains("Name"))
                    return kv["Name"];
                return string.Empty;
            }
        }

        public string ClientIP
        {
            get
            {                
                if (HttpContext.Current.Request.ServerVariables["HTTP_VIA"] != null)
                {
                    return HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
                }
                else
                {
                    return HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();
                }
            }
        }

        public void Attach(IContext owner)
        {
            
        }       
    }

    internal class ConsoleServiceContext : IContext
    {
        public int UserSysNo
        {
            get { return 1; }
        }

        public string UserID
        {
            get
            {
                return "admin";
            }
        }

        public string UserDisplayName
        {
            get { return "Mock"; }
        }

        public string ClientIP
        {
            get { return "127.0.0.1"; }
        }

        public void Attach(IContext owner)
        {

        }
    }

    internal class WCFRestServiceContext : IContext
    {
        private const string X_User_SysNo = "X-User-SysNo";
        private const string X_User_Display_Name = "X-User-Display-Name";
        //private const string X_Portal_TimeZone = "X-Portal-TimeZone";

        private OperationContext m_RealContext;
        private System.Globalization.CultureInfo m_Culture; 

        public WCFRestServiceContext()
        {
            m_RealContext = OperationContext.Current;
            m_Culture = System.Threading.Thread.CurrentThread.CurrentCulture;
        }

        public OperationContext RealContext
        {
            get 
            {
                return m_RealContext;
            }
        }

        public System.Globalization.CultureInfo Culture
        {
            get
            {
                return m_Culture;
            }
        }

        public int UserSysNo
        {
            get
            {
                if (WebOperationContext.Current != null
                    && WebOperationContext.Current.IncomingRequest != null
                    && WebOperationContext.Current.IncomingRequest.Headers != null)
                {
                    int tmp;
                    string userSysNo = WebOperationContext.Current.IncomingRequest.Headers[X_User_SysNo];
                    if (userSysNo != null && userSysNo.Trim().Length > 0 && int.TryParse(HttpUtility.UrlDecode(userSysNo), out tmp))
                    {
                        return tmp;
                    }
                }
                return -1;
            }
        }

        public string UserID
        {
            get
            {                
                return string.Empty;
            }
        }

        public string UserDisplayName 
        {
            get
            {
                if (WebOperationContext.Current != null
                    && WebOperationContext.Current.IncomingRequest != null
                    && WebOperationContext.Current.IncomingRequest.Headers != null)
                {
                    string name = WebOperationContext.Current.IncomingRequest.Headers[X_User_Display_Name];
                    if (name != null)
                    {
                        return HttpUtility.UrlDecode(name.Trim());
                    }
                }
                return null;
            }
        }

        public string ClientIP
        {
            get
            {
                if (OperationContext.Current != null)
                {
                    RemoteEndpointMessageProperty endpointProperty = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                    if (endpointProperty != null)
                    {
                        return endpointProperty.Address;
                    }
                }
                return string.Empty;
            }
        }

        #region IContext Members


        public void Attach(IContext owner)
        {
            WCFRestServiceContext context = owner as WCFRestServiceContext;
            if (context != null)
            {
                var realContext = context.RealContext;
                OperationContext.Current = realContext;

                System.Threading.Thread.CurrentThread.CurrentCulture = context.Culture;
            }
        }

        #endregion
    }
}
