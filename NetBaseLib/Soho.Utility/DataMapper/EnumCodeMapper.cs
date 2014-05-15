using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Soho.Utility
{
    public static class EnumCodeMapper
    {
        private static Dictionary<Type, Tuple<Dictionary<int, object>, Dictionary<object, int>>> s_Mapper = new Dictionary<Type, Tuple<Dictionary<int, object>, Dictionary<object, int>>>();
        private static object s_SyncObject = new object();

        private static Dictionary<Type, Dictionary<int, List<object>>> s_ExtraCodeMapper = new Dictionary<Type, Dictionary<int, List<object>>>(); 
        private static object s_ExtraSyncObject = new object();

        #region 因为DB中有些遗留数据，导致一个enum可能对应由多个DB的code值，所以这里增加了一个方法，可以设置额外的enum和code的mapping关系

        public static void AddExtraCodeMap<TEnum>(Dictionary<TEnum, object[]> maps) where TEnum : struct
        {
            foreach (var entry in maps)
            {
                AddExtraCodeMap(entry.Key, entry.Value);
            }
        }

        public static void AddExtraCodeMap<TEnum>(TEnum value, params object[] codes) where TEnum : struct
        {
            Type type = typeof(TEnum);
            if (!type.IsEnum)
            {
                throw new ArgumentException("The type of '" + type.FullName + "' is not enum.", "value");
            }
            if (codes == null || codes.Length <= 0)
            {
                return;
            }
            foreach (object code in codes)
            {
                Type t = code.GetType();
                if (t != typeof(string) && t != typeof(int))
                {
                    throw new ArgumentException("The type of object in array only can be string or int.", "codes");
                }
            }
            int v = (int)(object)value;
            Dictionary<int, List<object>> map;
            lock (s_ExtraSyncObject)
            {
                if (s_ExtraCodeMapper.TryGetValue(type, out map))
                {
                    if (map.ContainsKey(v))
                    {
                        throw new ArgumentException("Duplicated extra code mapping with value '" + value.ToString() + "' for enum '" + type.FullName + "'.", "value");
                    }
                    foreach (var entry in map)
                    {
                        foreach (object code in codes)
                        {
                            if (entry.Value.Contains(code))
                            {
                                throw new ArgumentException("Duplicated extra code mapping with code '" + code + "' for enum '" + type.FullName + "'.", "value");
                            }
                        }
                    }
                    map.Add(v, new List<object>(codes));
                }
                else
                {
                    map = new Dictionary<int, List<object>>();
                    map.Add(v, new List<object>(codes));
                    s_ExtraCodeMapper.Add(type, map);
                }
            }
        }

        #endregion

        public static void AddMap<TEnum>(Dictionary<TEnum, string> maps) where TEnum : struct
        {
            foreach (var entry in maps)
            {
                AddMap(entry.Key, entry.Value);
            }
        }

        public static void AddMap<TEnum>(Dictionary<TEnum, int> maps) where TEnum : struct
        {
            foreach (var entry in maps)
            {
                AddMap(entry.Key, entry.Value);
            }
        }

        public static void AddMap<TEnum>(TEnum value, string code) where TEnum : struct
        {
            InnerAddMap(value, code);
        }

        public static void AddMap<TEnum>(TEnum value, int code) where TEnum : struct
        {
            InnerAddMap(value, code);
        }

        public static void RemoveMap<TEnum>() where TEnum : struct
        {
            Type type = typeof(TEnum);
            if (!type.IsEnum)
            {
                throw new ArgumentException("The type of '" + type.FullName + "' is not enum.", "value");
            }
            if (s_Mapper.ContainsKey(type))
            {
                lock (s_SyncObject)
                {
                    if (s_Mapper.ContainsKey(type))
                    {
                        s_Mapper.Remove(type);
                    }
                }
            }
        }

        private static void InnerAddMap<TEnum>(TEnum value, object code) where TEnum : struct
        {
            Type type = typeof(TEnum);
            if (!type.IsEnum)
            {
                throw new ArgumentException("The type of '" + type.FullName + "' is not enum.", "value");
            }
            int v = (int)(object)value;
            Tuple<Dictionary<int, object>, Dictionary<object, int>> c;
            lock (s_SyncObject)
            {
                if (s_Mapper.TryGetValue(type, out c))
                {
                    if (c.Item1.ContainsKey(v))
                    {
                        throw new ArgumentException("Duplicated mapping with value '" + value.ToString() + "' for enum '" + type.FullName + "'.", "value");
                    }
                    if (c.Item2.ContainsKey(code))
                    {
                        throw new ArgumentException("Duplicated mapping with code '" + code + "' for enum '" + type.FullName + "'.", "value");
                    }
                    c.Item1.Add(v, code);
                    c.Item2.Add(code, v);
                }
                else
                {
                    c = new Tuple<Dictionary<int, object>, Dictionary<object, int>>(new Dictionary<int, object>(14) { { v, code } },
                        new Dictionary<object, int>(14) { { code, v } });
                    s_Mapper.Add(type, c);
                }
            }
        }

        public static bool TryGetCode(object enumValue, out object code)
        {
            Type type = enumValue.GetType();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                            && type.GetGenericArguments() != null
                            && type.GetGenericArguments().Length == 1)
            {
                type = type.GetGenericArguments()[0];
            }
            if (!type.IsEnum)
            {
                code = null;
                return false;
            }
            int v = (int)enumValue;
            Tuple<Dictionary<int, object>, Dictionary<object, int>> c;
            if (s_Mapper.TryGetValue(type, out c))
            {
                if (c.Item1.ContainsKey(v))
                {
                    code = c.Item1[v];
                    return true;
                }
            }
            code = null;
            return false;
        }

        public static bool TryGetEnum(object code, Type enumType, out object enumValue)
        {
            Type type = enumType;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                            && type.GetGenericArguments() != null
                            && type.GetGenericArguments().Length == 1)
            {
                type = type.GetGenericArguments()[0];
            }
            if (!type.IsEnum)
            {
                enumValue = null;
                return false;
            }
            Tuple<Dictionary<int, object>, Dictionary<object, int>> c;
            if (s_Mapper.TryGetValue(type, out c))
            {
                if (c.Item2.ContainsKey(code))
                {
                    enumValue = Enum.Parse(type, c.Item2[code].ToString());
                    return true;
                }
            }
            // --- begin： 因为DB中有些遗留数据，导致一个enum可能对应由多个DB的code值
            // --- 所以这里增加了一个处理，可以使用额外的enum和code的mapping关系
            Dictionary<int, List<object>> extraMapper;
            if (s_ExtraCodeMapper.TryGetValue(type, out extraMapper))
            {
                foreach (var entry in extraMapper)
                {
                    if (entry.Value.Contains(code))
                    {
                        enumValue = Enum.Parse(type, entry.Key.ToString());
                        return true;
                    }
                }
            }
            // --- end
            enumValue = null;
            return false;
        }

        public static bool IsExistMap(Type enumType)
        {
            if (s_Mapper == null || s_Mapper.Count == 0 ||enumType==null) return false;
            var result = s_Mapper.ContainsKey(enumType);
            return result;
        }
    }
}
