using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace Common.Utility.WCF
{
    public static class ServiceContextHelper
    {
        private const string PRIVATE_KEY = "sdfd^1&Nsdfo(835dz.{sz%dfg;*d2sfq75x";
        private const string PREFIX = "nesoft_wcf_service_";
        public static string GetValue(string key)
        {
            try
            {
                if (OperationContext.Current == null)
                {
                    return null;
                }
                string realKey = PREFIX + key;
                if (OperationContext.Current.IncomingMessageHeaders != null)
                {
                    int i = OperationContext.Current.IncomingMessageHeaders.FindHeader(PREFIX + key, string.Empty);
                    if (i > -1)
                    {
                        string str = OperationContext.Current.IncomingMessageHeaders.GetHeader<string>(realKey, string.Empty);
                        return DecryptAndCheckSign(str);
                    }
                }
                if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest != null && WebOperationContext.Current.IncomingRequest.Headers != null)
                {
                    var header = WebOperationContext.Current.IncomingRequest.Headers;
                    if (header.AllKeys.Contains(realKey))
                    {
                        string str = header[realKey];
                        return DecryptAndCheckSign(str);
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private static string DecryptAndCheckSign(string txt)
        {
            int i = txt.IndexOf(';');
            string num = txt.Substring(0, i);
            int len;
            if (!int.TryParse(num, out len) || len <= 0)
            {
                return null;
            }
            string orgin = DESCrypto.Decrypt(txt.Substring(i + 1, len));
            string sign = txt.Substring(i + 1 + len, txt.Length - i - 1 - len);
            if (sign == Md5Encrypt(orgin + PRIVATE_KEY))
            {
                return orgin;
            }
            return null;
        }

        private static string Md5Encrypt(string plainString)
        {
            byte[] hashValue = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(plainString));
            return Convert.ToBase64String(hashValue);
        }

        private static class DESCrypto
        {
            private static byte[] s_DesIV = new byte[] { 0x1d, 0x87, 0x34, 9, 0x41, 3, 0x61, 0x62 };
            private static byte[] s_DesKey = new byte[] { 1, 0x4d, 0x54, 0x22, 0x45, 90, 0x17, 0x2c };

            public static string Decrypt(string encryptedBase64ConnectString)
            {
                MemoryStream stream = new MemoryStream(200);
                stream.SetLength(0L);
                byte[] buffer = Convert.FromBase64String(encryptedBase64ConnectString);
                DES des = new DESCryptoServiceProvider();
                des.KeySize = 0x40;
                CryptoStream stream2 = new CryptoStream(stream, des.CreateDecryptor(s_DesKey, s_DesIV), CryptoStreamMode.Write);
                stream2.Write(buffer, 0, buffer.Length);
                stream2.FlushFinalBlock();
                stream.Flush();
                stream.Seek(0L, SeekOrigin.Begin);
                byte[] buffer2 = new byte[stream.Length];
                stream.Read(buffer2, 0, buffer2.Length);
                stream2.Close();
                stream.Close();
                return Encoding.Unicode.GetString(buffer2);
            }

            public static string Encrypt(string plainConnectString)
            {
                MemoryStream stream = new MemoryStream(200);
                stream.SetLength(0L);
                byte[] bytes = Encoding.Unicode.GetBytes(plainConnectString);
                DES des = new DESCryptoServiceProvider();
                CryptoStream stream2 = new CryptoStream(stream, des.CreateEncryptor(s_DesKey, s_DesIV), CryptoStreamMode.Write);
                stream2.Write(bytes, 0, bytes.Length);
                stream2.FlushFinalBlock();
                stream.Flush();
                stream.Seek(0L, SeekOrigin.Begin);
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                stream2.Close();
                stream.Close();
                return Convert.ToBase64String(buffer, 0, buffer.Length);
            }
        }
    }
}
