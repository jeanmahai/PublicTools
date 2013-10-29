using System;
using System.IO;
using System.Data;
using System.Text;
using System.Diagnostics;
using System.Security;
using System.Security.Cryptography;

namespace ExWebServer.WebServer.Utility.Encryption
{
    /// <summary>
    /// 此类提供MD5，SHA1，SHA256，SHA512等四种算法，加密字串的长度依次增大。
    /// </summary>
    public class HashEncrypt
    {
        
        /// <summary>
        /// 类初始化
        /// </summary>
        public HashEncrypt()
        {
            
        }

        public string MD5Encrypt(string strIN)
        {
            byte[] tmpByte;
            MD5 md5 = new MD5CryptoServiceProvider();

            tmpByte = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(strIN));
            string strResult = BitConverter.ToString(tmpByte);
            strResult = strResult.Replace("-", "");
            md5.Clear();
            md5 = null;

            return strResult;
        }

        public string SHA1Encrypt(string strIN)
        {
            byte[] tmpByte;
            SHA1 sha1 = new SHA1Managed();

            tmpByte = sha1.ComputeHash(System.Text.Encoding.ASCII.GetBytes(strIN));
            string strResult = BitConverter.ToString(tmpByte);
            strResult = strResult.Replace("-", "");
            sha1.Clear();
            sha1 = null;
            return strResult;
        }

        public string SHA256Encrypt(string strIN)
        {
            byte[] tmpByte;
            SHA256 sha256 = new SHA256Managed();

            tmpByte = sha256.ComputeHash(System.Text.Encoding.ASCII.GetBytes(strIN));
            string strResult = BitConverter.ToString(tmpByte);
            strResult = strResult.Replace("-", "");
            sha256.Clear();
            sha256 = null;
            return strResult;

        }

        public string SHA512Encrypt(string strIN)
        {
            byte[] tmpByte;
            SHA512 sha512 = new SHA512Managed();

            tmpByte = sha512.ComputeHash(System.Text.Encoding.ASCII.GetBytes(strIN));
            string strResult = BitConverter.ToString(tmpByte);
            strResult = strResult.Replace("-", "");
            sha512.Clear();
            sha512 = null;
            return strResult;

        }

        /// <summary>
        /// 使用DES加密（Added by niehl 2005-4-6）
        /// </summary>
        /// <param name="originalValue">待加密的字符串</param>
        /// <param name="key">密钥(最大长度8)</param>
        /// <param name="IV">初始化向量(最大长度8)</param>
        /// <returns>加密后的字符串</returns>
        public string DESEncrypt(string originalValue, string key, string IV)
        {
            //将key和IV处理成8个字符

            IV += "12345678";
            key = key.Substring(0, 8);
            IV = IV.Substring(0, 8);

            SymmetricAlgorithm sa;
            ICryptoTransform ct;
            MemoryStream ms;
            CryptoStream cs;
            byte[] byt;

            sa = new DESCryptoServiceProvider();
            sa.Key = System.Text.Encoding.UTF8.GetBytes(key);
            sa.IV = System.Text.Encoding.UTF8.GetBytes(IV);
            ct = sa.CreateEncryptor();

            byt = System.Text.Encoding.UTF8.GetBytes(originalValue);

            ms = new MemoryStream();
            cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
            cs.Write(byt, 0, byt.Length);
            cs.FlushFinalBlock();

            cs.Close();

            return Convert.ToBase64String(ms.ToArray());
        }

        public string DESEncrypt(string originalValue, string key)
        {
            return DESEncrypt(originalValue, key, key);
        }

        /// <summary>
        /// 使用DES解密（Added by niehl 2005-4-6）
        /// </summary>
        /// <param name="encryptedValue">待解密的字符串</param>
        /// <param name="key">密钥(最大长度8)</param>
        /// <param name="IV">m初始化向量(最大长度8)</param>
        /// <returns>解密后的字符串</returns>
        public string DESDecrypt(string encryptedValue, string key, string IV)
        {
            //将key和IV处理成8个字符
            key += "12345678";
            IV += "12345678";
            key = key.Substring(0, 8);
            IV = IV.Substring(0, 8);

            SymmetricAlgorithm sa;
            ICryptoTransform ct;
            MemoryStream ms;
            CryptoStream cs;
            byte[] byt;

            sa = new DESCryptoServiceProvider();
            sa.Key = System.Text.Encoding.UTF8.GetBytes(key);
            sa.IV = System.Text.Encoding.UTF8.GetBytes(IV);
            ct = sa.CreateDecryptor();

            byt = Convert.FromBase64String(encryptedValue);

            ms = new MemoryStream();
            cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
            cs.Write(byt, 0, byt.Length);
            cs.FlushFinalBlock();

            cs.Close();

            return System.Text.Encoding.UTF8.GetString(ms.ToArray());

        }

        public string DESDecrypt(string encryptedValue, string key)
        {
            return DESDecrypt(encryptedValue, key, key);

        }
    }
}
