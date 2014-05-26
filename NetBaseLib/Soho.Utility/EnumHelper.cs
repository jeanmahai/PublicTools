using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace Soho.Utility
{
    public static class EnumHelper
    {
        #region Private Member

        private static Dictionary<Type, Dictionary<object, string>> s_Cache = new Dictionary<Type, Dictionary<object, string>>();
        private static object s_SyncObj = new object();

        private static Dictionary<object, string> GetDescriptions(Type enumType)
        {
            if (!enumType.IsEnum && !IsGenericEnum(enumType)) // enumType既不是enum也不是enum?
            {
                throw new ApplicationException("The generic type 'TEnum' must be enum or Nullable<enum>.");
            }
            enumType = GetRealEnum(enumType);
            Dictionary<object, string> rst;
            if (s_Cache.TryGetValue(enumType, out rst))
            {
                return rst;
            }
            lock (s_SyncObj)
            {
                if (s_Cache.TryGetValue(enumType, out rst))
                {
                    return rst;
                }
                FieldInfo[] fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
                Dictionary<object, string> map = new Dictionary<object, string>(fields.Length * 2);
                foreach (FieldInfo field in fields)
                {
                    object[] displayAttr = field.GetCustomAttributes(typeof(DisplayAttribute), false);
                    if (displayAttr != null && displayAttr.Length > 0 && !(displayAttr[0] as DisplayAttribute).Display)
                    {
                        continue;
                    }
                    object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    string name = string.Empty;
                    if (objs != null && objs.Length > 0)
                    {
                        DescriptionAttribute a = objs[0] as DescriptionAttribute;
                        if (a != null && a.Description != null)
                        {
                            name = a.Description;
                        }
                    }
                    object key = field.GetValue(null);
                    map.Add(key, name);
                }
                s_Cache.Add(enumType, map);
                return map;
            }
        }

        private static string GetDescription(object enumValue, Type enumType)
        {
            var dic = GetDescriptions(enumType);
            string tmp;
            if (dic.TryGetValue(enumValue, out tmp) && tmp != null)
            {
                return tmp;
            }
            return string.Empty;
        }

        private static bool IsGenericEnum(Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                    && type.GetGenericArguments() != null
                    && type.GetGenericArguments().Length == 1 && type.GetGenericArguments()[0].IsEnum);
        }

        private static Type GetRealEnum(Type type)
        {
            Type t = type;
            while (IsGenericEnum(t))
            {
                t = type.GetGenericArguments()[0];
            }
            return t;
        }

        #endregion

        /// <summary>
        /// 获取枚举类型下所有枚举值的描述集合
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <returns>枚举值与其描述的对应关系的集合，Key为枚举值，Value为其对应的描述</returns>
        public static Dictionary<TEnum, string> GetDescriptions<TEnum>() where TEnum : struct
        {
            Dictionary<object, string> dic = GetDescriptions(typeof(TEnum));
            Dictionary<TEnum, string> newDic = new Dictionary<TEnum, string>(dic.Count * 2);
            foreach (var entry in dic)
            {
                newDic.Add((TEnum)entry.Key, entry.Value);
            }
            return newDic;
        }

        /// <summary>
        /// 获取枚举值的描述
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            return value == null ? string.Empty : GetDescription(value, value.GetType());
        }
    }

    public class DisplayAttribute : Attribute
    {
        private bool display;

        public bool Display
        {
            get { return display; }
            set { display = value; }
        }
        public DisplayAttribute(bool display)
        {
            this.display = display;
        }
    }
}
