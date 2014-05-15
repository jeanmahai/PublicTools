using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Soho.Utility
{
    /// <summary>
    /// 进制转换，进制加减
    /// </summary>
    public class RadixHelper
    {
        private static char[] rDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        /// <summary>
        /// 任意进制转换,将fromBase进制表示的value转换为toBase进制
        /// </summary>
        /// <param name="value">需要被转换的字符串</param>
        /// <param name="fromBase">源进制的位数</param>
        /// <param name="toBase">目标进制的位数</param>
        /// <returns></returns>
        public static string Source2TargetRadix(string value, int fromBase, int toBase)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (fromBase < 2 || fromBase > 36)
            {
                throw new ArgumentException(String.Format("源进制必须是2至36之间的数字！", fromBase));
            }

            if (toBase < 2 || toBase > 36)
            {
                throw new ArgumentException(String.Format("源进制和目标进制必须是2至36之间的数字！", toBase));
            }

            long m = x2h(value, fromBase);
            string r = h2x(m, toBase);
            return r;
        }

        /// <summary>
        /// 将指定基数的数字的 System.String 表示形式转换为等效的 64 位有符号整数。
        /// </summary>
        /// <param name="value">包含数字的 System.String。</param>
        /// <param name="fromBase">value 中数字的基数，它必须是[2,36]</param>
        /// <returns>等效于 value 中的数字的 64 位有符号整数。- 或 - 如果 value 为 null，则为零。</returns>
        private static long x2h(string value, int fromBase)
        {
            value = value.Trim();
            if (string.IsNullOrEmpty(value))
            {
                return 0L;
            }

            string sDigits = new string(rDigits, 0, fromBase);
            long result = 0;
            value = value.ToUpper();// 2
            for (int i = 0; i < value.Length; i++)
            {
                if (!sDigits.Contains(value[i].ToString()))
                {
                    throw new ArgumentException(string.Format("源数据不在定义的36进制基本符号中，源数据中每一位字符必须是数字或字母！", value[i], fromBase));
                }
                else
                {
                    try
                    {
                        result += (long)Math.Pow(fromBase, i) * getcharindex(rDigits, value[value.Length - i - 1]);//   2
                    }
                    catch
                    {
                        throw new OverflowException("运算溢出.");
                    }
                }
            }

            return result;
        }

        private static int getcharindex(char[] arr, char value)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == value)
                {
                    return i;
                }
            }
            return 0;
        }

        //long转化为toBase进制
        private static string h2x(long value, int toBase)
        {
            int digitIndex = 0;
            long longPositive = Math.Abs(value);
            int radix = toBase;
            char[] outDigits = new char[63];

            for (digitIndex = 0; digitIndex <= 64; digitIndex++)
            {
                if (longPositive == 0) { break; }

                outDigits[outDigits.Length - digitIndex - 1] =
                    rDigits[longPositive % radix];
                longPositive /= radix;
            }

            return new string(outDigits, outDigits.Length - digitIndex, digitIndex);
        }

        /// <summary>
        /// 相同进制radixLength的数字相加
        /// </summary>
        /// <param name="value">原始数</param>
        /// <param name="addedNumber">加数</param>
        /// <param name="radixLength">原始数和加数的进制位数</param>
        /// <returns>该进制的和</returns>
        public static string RadixAddSameBase(string value, string addedNumber, int radixLength)
        {
            long hvalue = x2h(value, radixLength);
            long haddedNumber = x2h(addedNumber, radixLength);
            hvalue = hvalue + haddedNumber;
            string targetValue = h2x(hvalue, radixLength);
            return targetValue;

        }

        /// <summary>
        /// 指定进制radixLength的数字加上一个10进制的整数，求和（按指定进制radixLength返回）
        /// </summary>
        /// <param name="value">原始数</param>
        /// <param name="addedNumber">10进制的加数</param>
        /// <param name="radixLength">原始数的进制位数</param>
        /// <returns>该进制的和</returns>
        public static string RadixAdd10Base(string value, int addedNumber, int radixLength)
        {
            long hvalue = x2h(value, radixLength);
            hvalue = hvalue + addedNumber;
            string targetValue = h2x(hvalue, radixLength);
            return targetValue;

        }
    }
}
