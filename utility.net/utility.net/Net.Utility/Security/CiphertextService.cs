using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Net.Utility.Security
{
    public class CiphertextService
    {
        public static string MD5Encryption(string input)
        {
            var md5 = MD5.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashValue= md5.ComputeHash(bytes);
            var result = new StringBuilder();
            foreach (var item in hashValue)
            {
                result.Append(item.ToString("x2"));
            }
            return result.ToString();
        }
        public static bool ValidateMD5(string input,string hashValue)
        {
            return MD5Encryption(input)==hashValue;
        }
    }
}
