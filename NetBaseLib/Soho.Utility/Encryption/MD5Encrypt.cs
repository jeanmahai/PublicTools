using System;
using System.IO;
using System.Data;
using System.Text;
using System.Diagnostics;
using System.Security;
using System.Security.Cryptography;

namespace Soho.Utility.Encryption
{
    /// <summary>
    /// 提供加密MD5算法
    /// </summary>
    public class MD5Encrypt
    {
        /// <summary>
        /// MD5加密，返回32位小写
        /// </summary>
        /// <param name="data">被加密数据</param>
        /// <returns></returns>
        public static string MD5Encrypt32(string data)
        {
            byte[] tmpByte;
            MD5 md5 = new MD5CryptoServiceProvider();

            tmpByte = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(data));
            string strResult = BitConverter.ToString(tmpByte);
            strResult = strResult.Replace("-", "");
            md5.Clear();
            md5 = null;

            return strResult.ToLower();
        }

        /// <summary>
        /// MD5加密，返回16位小写
        /// </summary>
        /// <param name="data">被加密数据</param>
        /// <returns></returns>
        public static string MD5Encrypt16(string data)
        {
            return MD5Encrypt32(data).Substring(0, 16);
        }

        /// <summary>
        /// MD5加密，返回8位小写
        /// </summary>
        /// <param name="data">被加密数据</param>
        /// <returns></returns>
        public static string MD5Encrypt8(string data)
        {
            return MD5Encrypt32(data).Substring(0, 8);
        }
    }
}
