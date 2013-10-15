using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Configuration;
using System.IO;

namespace Common.Utility
{
    public static class MoneyUtility
    {
        private static readonly string[] ChineseNum = { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };

        public static string GetChineseMoney(decimal moneyValue)
        {
            var result = "";
            if (moneyValue == 0)
                return "零";

            if (moneyValue < 0)
            {
                moneyValue *= -1;
                result = "负";
            }
            var intMoney = Convert.ToInt32(moneyValue * 100);
            var strMoney = intMoney.ToString();
            var moneyLength = strMoney.Length;
            var strBuf = new StringBuilder(100);
            if (moneyLength > 14)
            {
                throw new Exception("Money Value Is Too Large");
            }

            //处理亿部分
            if (moneyLength > 10 && moneyLength <= 14)
            {
                strBuf.Append(GetSmallMoney(strMoney.Substring(0, strMoney.Length - 10)));
                strMoney = strMoney.Substring(strMoney.Length - 10, 10);
                strBuf.Append("亿");
            }

            //处理万部分
            if (moneyLength > 6)
            {
                strBuf.Append(GetSmallMoney(strMoney.Substring(0, strMoney.Length - 6)));
                strMoney = strMoney.Substring(strMoney.Length - 6, 6);
                strBuf.Append("万");
            }

            //处理元部分
            if (moneyLength > 2)
            {
                strBuf.Append(GetSmallMoney(strMoney.Substring(0, strMoney.Length - 2)));
                strMoney = strMoney.Substring(strMoney.Length - 2, 2);
                strBuf.Append("元");
            }

            //处理角、分处理分
            if (Convert.ToInt32(strMoney) == 0)
            {
                strBuf.Append("整");
            }
            else
            {
                if (moneyLength > 1)
                {
                    var intJiao = Convert.ToInt32(strMoney.Substring(0, 1));
                    strBuf.Append(ChineseNum[intJiao]);
                    if (intJiao != 0)
                    {
                        strBuf.Append("角");
                    }
                    strMoney = strMoney.Substring(1, 1);
                }

                var intFen = Convert.ToInt32(strMoney.Substring(0, 1));
                if (intFen != 0)
                {
                    strBuf.Append(ChineseNum[intFen]);
                    strBuf.Append("分");
                }
            }
            var temp = strBuf.ToString();
            while (temp.IndexOf("零零") != -1)
            {
                strBuf.Replace("零零", "零");
                temp = strBuf.ToString();
            }

            strBuf.Replace("零亿", "亿");
            strBuf.Replace("零万", "万");
            strBuf.Replace("亿万", "亿");

            return result + strBuf;
        }

        public static string GetFinanceMoney(decimal? moneyValue)
        {
            decimal money = 0.00M;

            if (moneyValue != null)
            {
                if (decimal.TryParse(moneyValue.ToString(), out money))
                {
                    return decimal.Round(money, 2).ToString("0.00");
                }
                return moneyValue.ToString();
            }
            return money.ToString();
        }

        private static string GetSmallMoney(string moneyValue)
        {
            var intMoney = Convert.ToInt32(moneyValue);
            if (intMoney == 0)
            {
                return "";
            }
            var strMoney = intMoney.ToString();
            int temp;
            var strBuf = new StringBuilder(10);
            if (strMoney.Length == 4)
            {
                temp = Convert.ToInt32(strMoney.Substring(0, 1));
                strMoney = strMoney.Substring(1, strMoney.Length - 1);
                strBuf.Append(ChineseNum[temp]);
                if (temp != 0)
                    strBuf.Append("仟");
            }
            if (strMoney.Length == 3)
            {
                temp = Convert.ToInt32(strMoney.Substring(0, 1));
                strMoney = strMoney.Substring(1, strMoney.Length - 1);
                strBuf.Append(ChineseNum[temp]);
                if (temp != 0)
                    strBuf.Append("佰");
            }
            if (strMoney.Length == 2)
            {
                temp = Convert.ToInt32(strMoney.Substring(0, 1));
                strMoney = strMoney.Substring(1, strMoney.Length - 1);
                strBuf.Append(ChineseNum[temp]);
                if (temp != 0)
                    strBuf.Append("拾");
            }
            if (strMoney.Length == 1)
            {
                temp = Convert.ToInt32(strMoney);
                strBuf.Append(ChineseNum[temp]);
            }
            return strBuf.ToString();
        }
    }
}