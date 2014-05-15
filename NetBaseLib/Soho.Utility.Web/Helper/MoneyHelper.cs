using System;

namespace Soho.Utility.Web
{
    /// <summary>
    /// 货币辅助工具类
    /// </summary>
    public static class MoneyHelper
    {
        #region 格式化为不带人民币符号的货币
        /// <summary>
        /// 格式化为人民币，不带人民币符号，精度为小数点后两位。
        /// </summary>
        /// <param name="price">需要格式化的值</param>
        /// <returns>格式化过的人民币。</returns>
        public static string FormatRMB(decimal price)
        {
            return FormatRMB(price, 2);
        }

        /// <summary>
        /// 格式化为人民币，不带人民币符号，指定小数点后位数。
        /// </summary>
        /// <param name="price">需要格式化的值</param>
        /// <param name="bitCount">小数点后位数</param>
        /// <returns>格式化过的人民币。</returns>
        public static string FormatRMB(decimal price, int bitCount)
        {
            string priceFormat = bitCount >= 0 ?
                    string.Format("f{0}", bitCount) : "f0";

            if (price < decimal.Zero)
            {
                return string.Format(@"-{0}", Math.Abs(price).ToString(priceFormat));
            }
            return price.ToString(priceFormat);
        }
        #endregion

        #region 格式化为带人民币符号的货币
        /// <summary>
        /// 格式化为人民币，带人民币符号，精度为小数点后两位。
        /// </summary>
        /// <param name="price">需要格式化的值</param>
        /// <returns>格式化过的人民币。</returns>
        public static string FormatRMBWithSign(decimal price)
        {
            return FormatRMBWithSign(price, 2);
        }

        /// <summary>
        /// 格式化为人民币，带人民币符号，指定小数点后位数。
        /// </summary>
        /// <param name="price">需要格式化的值</param>
        /// <param name="bitCount">小数点后位数</param>
        /// <returns>格式化过的人民币。</returns>
        public static string FormatRMBWithSign(decimal price, int bitCount)
        {
            string priceFormat = bitCount >= 0 ?
                    string.Format("f{0}", bitCount) : "f0";

            if (price < decimal.Zero)
            {
                return string.Format(@"&yen;-{0}", Math.Abs(price).ToString(priceFormat));
            }
            return string.Format(@"&yen;{0}", price.ToString(priceFormat));
        }
        #endregion

        #region 格式化为带人民币符号的货币，并且自定义人民币符号和价格的样式
        /// <summary>
        /// 格式化为人民币，自定义人民币符号和价格样式，精度为小数点后两位。
        /// </summary>
        /// <param name="price">需要格式化的值</param>
        /// <param name="html">
        /// 人民币符号和价格样式
        /// 样式格式：<![CDATA[<s class="ico_y">&yen;</s><span class="digi">{0}</span>]]>
        /// 需要将价格处设置为：{0}
        /// </param>
        /// <returns>格式化过的人民币。</returns>
        public static string FormatRMBWithSign(decimal price, string html)
        {
            return FormatRMBWithSign(price, 2, html);
        }

        /// <summary>
        /// 格式化为人民币，自定义人民币符号和价格样式，指定小数点后位数
        /// </summary>
        /// <param name="price">需要格式化的值</param>
        /// <param name="bitCount">小数点后位数</param>
        /// <param name="html">
        /// 人民币符号和价格样式
        /// 样式格式：<![CDATA[<s class="ico_y">&yen;</s><span class="digi">{0}</span>]]>
        /// 需要将价格处设置为：{0}
        /// </param>
        /// <returns>格式化过的人民币。</returns>
        public static string FormatRMBWithSign(decimal price, int bitCount, string html)
        {
            string priceFormat = bitCount >= 0 ?
                    string.Format("f{0}", bitCount) : "f0";

            if (price < decimal.Zero)
            {
                string result = string.Format(@"-{0}", Math.Abs(price).ToString(priceFormat));
                return string.Format(html, result);
            }
            return string.Format(html, price.ToString(priceFormat));
        }
        #endregion
    }
}
