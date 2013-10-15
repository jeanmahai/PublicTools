using System;
using System.Security.Cryptography;
using System.Text;

namespace Common.Utility
{
    public class Hash_MD5 : ICrypto
    {
        private readonly MD5 md5;

        public Hash_MD5()
        {
            md5 = MD5.Create();
        }

        public string Decrypt(string encryptedBase64String)
        {
            throw new ApplicationException("MD5不可逆!");
        }

        public string Encrypt(string plainString)
        {
            byte[] hashValue = md5.ComputeHash(Encoding.UTF8.GetBytes(plainString));
            return Convert.ToBase64String(hashValue);
        }

        public string Encrypt(string plainString, byte[] saltValue)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainString);

            byte[] SaltedPlainBytes = new byte[plainBytes.Length + saltValue.Length];

            plainBytes.CopyTo(SaltedPlainBytes, 0);
            saltValue.CopyTo(SaltedPlainBytes, plainBytes.Length);

            byte[] saltedencryptedBytes = md5.ComputeHash(SaltedPlainBytes);

            byte[] encryptedBytes = new byte[saltedencryptedBytes.Length + saltValue.Length];
            saltedencryptedBytes.CopyTo(encryptedBytes, 0);
            saltValue.CopyTo(encryptedBytes, saltedencryptedBytes.Length);

            return Convert.ToBase64String(encryptedBytes);
        }

        public string Encrypt(string plainString, int saltLength)
        {
            byte[] saltValue = new byte[saltLength];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            //用加密型强随机字节填充的数组
            rng.GetBytes(saltValue);
            return Encrypt(plainString, saltValue);
        }
    }
}
