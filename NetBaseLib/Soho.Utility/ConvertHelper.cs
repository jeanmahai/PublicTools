using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Soho.Utility
{
    public class ConvertHelper
    {
        /// <summary>
        /// 将整型数组转换为字节数组
        /// </summary>
        /// <param name="intArray">整型数组</param>
        /// <returns></returns>
        public static byte[] GetBytes(int[] intArray)
        {
            byte[] result = null;
            if (intArray == null || intArray.Length < 1)
                return result;

            int intSize = 4, pos = 0;
            result = new byte[intArray.Length * intSize];
            byte[] intBuf = new byte[intSize];

            for (int i = 0; i < intArray.Length; i++)
            {
                intBuf = BitConverter.GetBytes(intArray[i]);
                Array.Reverse(intBuf);
                Array.Copy(intBuf, 0, result, pos, intSize);
                pos += intSize;
            }

            return result;
        }
    }
}
