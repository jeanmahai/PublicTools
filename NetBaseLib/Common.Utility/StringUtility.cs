using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Common.Utility
{
    public static class StringUtility
    {
        // Methods
        public static string[] BubbleSort(string[] r)
        {
            for (int i = 0; i < r.Length; i++)
            {
                bool flag = false;
                for (int j = r.Length - 2; j >= i; j--)
                {
                    if (string.CompareOrdinal(r[j + 1], r[j]) < 0)
                    {
                        string str = r[j + 1];
                        r[j + 1] = r[j];
                        r[j] = str;
                        flag = true;
                    }
                }
                if (!flag)
                {
                    return r;
                }
            }
            return r;
        }

        public static string[] ChangeIntArrayToStringArray(int[] beChangeArray)
        {
            string[] strArray = new string[beChangeArray.Length];
            int index = 0;
            foreach (int num2 in beChangeArray)
            {
                strArray[index] = num2.ToString();
                index++;
            }
            return strArray;
        }

        public static decimal[] ChangeStringArrayToDecimalArray(string[] beChangeArray)
        {
            decimal[] numArray = new decimal[beChangeArray.Length];
            int index = 0;
            decimal result = 0M;
            foreach (string str in beChangeArray)
            {
                numArray[index] = 0M;
                result = 0M;
                if (decimal.TryParse(str, out result))
                {
                    numArray[index] = result;
                }
                index++;
            }
            return numArray;
        }

        public static int[] ChangeStringArrayToIntArray(string[] beChangeArray)
        {
            int[] numArray = new int[beChangeArray.Length];
            int index = 0;
            int result = 0;
            foreach (string str in beChangeArray)
            {
                numArray[index] = 0;
                result = 0;
                if (int.TryParse(str, out result))
                {
                    numArray[index] = result;
                }
                index++;
            }
            return numArray;
        }

        public static string GetLeftString(string description, int leftLength)
        {
            if (!IsEmpty(description) && (description.Length > leftLength))
            {
                return description.Substring(0, leftLength);
            }
            return description;
        }

        public static string GetRandomStr(int strLength)
        {
            return GetRandomStr(strLength, 0);
        }

        public static string GetRandomStr(int strLength, int randomSeed)
        {
            string[] strArray = "1,2,3,4,5,6,7,8,9,0,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z,A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z".Split(new char[] { ',' });
            Random random = null;
            if (randomSeed > 0)
            {
                random = new Random(randomSeed);
            }
            else
            {
                random = new Random();
            }
            string str2 = "";
            while (str2.Length < strLength)
            {
                str2 = str2 + strArray[random.Next(strArray.Length)];
            }
            return str2;
        }

        public static string GetRightString(string description, int rightLength)
        {
            if (!IsEmpty(description) && (description.Length > rightLength))
            {
                return description.Substring(description.Length - rightLength);
            }
            return description;
        }

        public static bool IsChineseMobileNumber(string input)
        {
            return Regex.IsMatch(input, "^(13[0-9]|15[0|3|6|8|9])[0-9]{8}$");
        }

        public static bool IsChinesePhoneNumber(string input)
        {
            return Regex.IsMatch(input, @"^[0-9\-]{0,20}$");
        }

        public static bool IsDateTime(string input)
        {
            DateTime time;
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            return DateTime.TryParse(input, out time);
        }

        public static bool IsDecimal(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            return Regex.IsMatch(input, @"^\d+[.]?\d*$");
        }

        public static bool IsEmailAddress(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            return Regex.IsMatch(input, @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
        }

        public static bool IsEmpty(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                return (input.Trim() == string.Empty);
            }
            return true;
        }

        public static bool IsInteger(string input)
        {
            int num;
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            return int.TryParse(input, out num);
        }

        public static bool IsIPV4Address(string input)
        {
            return Regex.IsMatch(input, @"^((?:2[0-5]{2}|1\d{2}|[1-9]\d|[1-9])\.(?:(?:2[0-5]{2}|1\d{2}|[1-9]\d|\d)\.){2}(?:2[0-5]{2}|1\d{2}|[1-9]\d|\d)):(\d|[1-9]\d|[1-9]\d{2,3}|[1-5]\d{4}|6[0-4]\d{3}|654\d{2}|655[0-2]\d|6553[0-5])$");
        }

        public static bool IsIPV6Address(string input)
        {
            return Regex.IsMatch(input, "^([0-9A-Fa-f]{1,4}:){7}[0-9A-Fa-f]{1,4}$");
        }

        public static bool IsMoney(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            return Regex.IsMatch(input, "^([0-9]+|[0-9]{1,3}(,[0-9]{3})*)(.[0-9]{1,2})?$");
        }

        public static bool IsNotEmpty(string input)
        {
            if (input == "")
            {
                return false;
            }
            return true;
        }

        public static bool IsNullOrEmpty(string value)
        {
            if (value == null)
            {
                return true;
            }

            return value.Trim().Length == 0;
        }

        public static bool IsURL(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            return Regex.IsMatch(input, @"(mailto\:|(news|(ht|f)tp(s?))\://)(([^[:space:]]+)|([^[:space:]]+)( #([^#]+)#)?) ");
        }

        public static string RemoveHtmlTag(string str)
        {
            Regex regex = new Regex(@"<\/*[^<>]*>");
            return regex.Replace(str, string.Empty);
        }

        /// <summary>
        /// 判断是否包含"<'或者">"字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool CheckHtml(string str)
        {
            bool rtn = str.IndexOf('<') > 0 || str.IndexOf('>') > 0;

            return rtn;
        }

        public static bool CheckInputType(string str)
        {
            return str.Any(t => (t > 65248) || t == 12288);
        }

        public static string TrimNull(object input)
        {
            return (input != null ? input.ToString().Trim() : string.Empty);
        }

        /// <summary>
        /// 将传入的List<T>对象转换为使用逗号隔开的字符串。
        /// </summary>
        /// <typeparam name="T">List元素类型</typeparam>
        /// <param name="list">元素列表</param>
        /// <returns>返回的字符串</returns>
        public static string ToListString<T>(this List<T> list)
        {
            string result = string.Empty;
            foreach (var item in list)
            {
                if (item.GetType() == typeof(string))
                {
                    result += "'" + item + "',";
                }
                else
                {
                    result += item + ",";
                }
            }
            return result.TrimEnd(',');
        }

        /// <summary>
        /// 将传入列表中的每个元素的指定属性值转换为使用逗号隔开的字符串。
        /// </summary>
        /// <typeparam name="T">List元素类型</typeparam>
        /// <param name="list">元素列表</param>
        /// <param name="PropName">需要关注的列表中的元素属性</param>
        /// <returns>返回的字符串</returns>
        public static string ToListString<T>(this List<T> list, string PropName)
        {
            string result = string.Empty;
            Type ty = typeof(T);
            PropertyInfo pty = ty.GetProperty(PropName);
            foreach (var item in list)
            {
                if (pty != null)
                {
                    object obj = pty.GetValue(item, null);
                    result += string.Format(obj.GetType() == typeof(int) ? "{0}," : "'{0}',", obj);
                }
            }
            return result.TrimEnd(',');
        }

        public static void ForEach<T>(this IEnumerable<T> value, Action<T> action)
        {
            foreach (var v in value)
            {
                action(v);
            }
        }

        public static string Join<T>(this IEnumerable<T> collection, string separator)
        {
            if (!collection.Any())
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            collection.ForEach(e => sb.Append(e.ToString() + separator));

            return sb.ToString().Substring(0, sb.Length - separator.Length);
        }

        //gqc 好像不用\n分隔, 所以用此方法分隔
        public static string[] SplitByLine(this string value)
        {
            return Regex.Split(value, "[\r\n]+");
        }

        /// <summary>
        /// 判断字符串中是否有包含列表的字符串
        /// </summary>
        /// <param name="str">待验证字符串</param>
        /// <param name="taget">字符串列表</param>
        /// <returns>如果有包含返回真，否则返回假</returns>
        public static bool IsContains(this string str, List<string> taget)
        {
            bool result = false;
            if (string.IsNullOrEmpty(str))
            {
                return result;
            }

            foreach (string s in taget)
            {
                if (str.Contains(s))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public static bool IsEquals(this String str, List<string> taget)
        {
            bool result = false;
            if (string.IsNullOrEmpty(str))
            {
                return result;
            }

            //精确匹配
            foreach (string s in taget)
            {
                if (str.Trim().Equals(s))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> value)
        {
            return new HashSet<T>(value);
        }

        public static void AddRange<T>(this HashSet<T> value, IEnumerable<T> collection)
        {
            foreach (var li in collection)
            {
                value.Add(li);
            }
        }
    }
}
