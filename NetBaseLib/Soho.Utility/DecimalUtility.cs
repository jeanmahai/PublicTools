using System;

namespace Soho.Utility
{
    public static class DecimalUtility
    {
        public static decimal? Round(this decimal? value, int decimals)
        {
            if (value.HasValue)
            {
                return Decimal.Round(value.Value, 2);
            }
            return null;
        }

        /// <summary>
        /// 截取小数点后位数
        /// </summary>
        /// <param name="source">源数字</param>
        /// <param name="len">小数点位数</param>
        /// <returns></returns>
        public static string TruncateDecimal(this decimal source, int len)
        {

            string destination = string.Empty;
            int i = source.ToString().IndexOf(".");
            if (i < 0)
            {
                destination = source.ToString();
            }
            else
            {
                if (source.ToString().Length >= (i + len + 1))
                    destination = source.ToString().Substring(0, i + len + 1);
                else
                    destination = source.ToString();
            }
            return destination;

        }

        /// <summary>
        /// 截取小数点后位数
        /// </summary>
        /// <param name="source">源数字</param>
        /// <param name="len">小数点位数</param>
        /// <returns></returns>
        public static string TruncateDecimal(this decimal? source, int len)
        {
            if (!source.HasValue)
            {
                return string.Empty;
            }
            return TruncateDecimal(source.Value, len);

        }
    }
}
