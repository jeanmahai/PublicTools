using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace Soho.Utility
{
    public static class DataConvertor
    {
        private static bool IsGenericEnum(Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                    && type.GetGenericArguments() != null
                    && type.GetGenericArguments().Length == 1 && type.GetGenericArguments()[0].IsEnum);
        }

        public static object GetDefaultValue<T>()
        {
            return default(T);
        }

        public static T GetValue<T>(object data, string columnName, string propertyName)
        {
            if (data == null || data == DBNull.Value)
            {
                return default(T);
            }
            try
            {
                if (typeof(T) == data.GetType() || typeof(T).IsAssignableFrom(data.GetType()))
                {
                    return (T)((object)data);
                }
                else if (typeof(T) == typeof(string))
                {
                    string tmp = data.ToString();
                    if(tmp == null)
                    {
                        return default(T);
                    }
                    return (T)(object)(tmp.Trim());
                }
                else if (data is string && data.ToString().Trim().Length <= 0)
                {
                    return default(T);
                }
                else if (typeof(T) == typeof(char) || typeof(T) == typeof(char?))
                {
                    return (T)((object)Convert.ToChar(data));
                }
                else if (typeof(T) == typeof(sbyte) || typeof(T) == typeof(sbyte?))
                {
                    return (T)((object)Convert.ToSByte(data));
                }
                else if (typeof(T) == typeof(byte) || typeof(T) == typeof(byte?))
                {
                    return (T)((object)Convert.ToByte(data));
                }
                else if (typeof(T) == typeof(short) || typeof(T) == typeof(short?))
                {
                    return (T)((object)Convert.ToInt16(data));
                }
                else if (typeof(T) == typeof(ushort) || typeof(T) == typeof(ushort?))
                {
                    return (T)((object)Convert.ToUInt16(data));
                }
                else if (typeof(T) == typeof(int) || typeof(T) == typeof(int?))
                {
                    return (T)((object)Convert.ToInt32(data));
                }
                else if (typeof(T) == typeof(uint) || typeof(T) == typeof(uint?))
                {
                    return (T)((object)Convert.ToUInt32(data));
                }
                else if (typeof(T) == typeof(long) || typeof(T) == typeof(long?))
                {
                    return (T)((object)Convert.ToInt64(data));
                }
                else if (typeof(T) == typeof(ulong) || typeof(T) == typeof(ulong?))
                {
                    return (T)((object)Convert.ToUInt64(data));
                }
                else if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?))
                {
                    return (T)((object)Convert.ToDateTime(data));
                }
                else if (typeof(T) == typeof(decimal) || typeof(T) == typeof(decimal?))
                {
                    return (T)((object)Convert.ToDecimal(data));
                }
                else if (typeof(T) == typeof(float) || typeof(T) == typeof(float?))
                {
                    return (T)((object)Convert.ToSingle(data));
                }
                else if (typeof(T) == typeof(double) || typeof(T) == typeof(double?))
                {
                    return (T)((object)Convert.ToDouble(data));
                }
                else if (typeof(T) == typeof(bool) || typeof(T) == typeof(bool?))
                {
                    return (T)((object)Convert.ToBoolean(data));
                }
                else if (typeof(T).IsEnum || IsGenericEnum(typeof(T)))
                {
                    Type destinationType;
                    if (IsGenericEnum(typeof(T)))
                    {
                        destinationType = typeof(T).GetGenericArguments()[0];
                    }
                    else
                    {
                        destinationType = typeof(T);
                    }
                    return (T)Enum.Parse(destinationType, data.ToString(), false);
                }
                return (T)data;
            }
            catch
            {
                string msg = "Can't cast '" + data.GetType().FullName + "' to '" + typeof(T).FullName + "'. Values is '" + data.ToString() + "'";
                if (propertyName != null)
                {
                    msg += " Propert Name : " + propertyName + ";";
                }
                if (columnName != null)
                {
                    msg += " Column Name : " + columnName;
                }
                throw new InvalidCastException(msg);
            }
        }

        public static object GetValueByType(Type destinationType, object data, string columnName, string propertyName)
        {
            if (data == null || data == DBNull.Value)
            {
                return typeof(DataConvertor).GetMethod("GetDefaultValue", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(destinationType).Invoke(null, null);
            }
            try
            {
                if (destinationType == data.GetType() || destinationType.IsAssignableFrom(data.GetType()))
                {
                    return ((object)data);
                }
                else if (destinationType == typeof(string))
                {
                    string tmp = data.ToString();
                    if(tmp == null)
                    {
                        return null;
                    }
                    return tmp.Trim();
                }
                else if (data is string && data.ToString().Trim().Length <= 0)
                {
                    return typeof(DataConvertor).GetMethod("GetDefaultValue", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(destinationType).Invoke(null, null);
                }
                else if (destinationType == typeof(char) || destinationType == typeof(char?))
                {
                    return ((object)Convert.ToChar(data));
                }
                else if (destinationType == typeof(sbyte) || destinationType == typeof(sbyte?))
                {
                    return ((object)Convert.ToSByte(data));
                }
                else if (destinationType == typeof(byte) || destinationType == typeof(byte?))
                {
                    return ((object)Convert.ToByte(data));
                }
                else if (destinationType == typeof(short) || destinationType == typeof(short?))
                {
                    return ((object)Convert.ToInt16(data));
                }
                else if (destinationType == typeof(ushort) || destinationType == typeof(ushort?))
                {
                    return ((object)Convert.ToUInt16(data));
                }
                else if (destinationType == typeof(int) || destinationType == typeof(int?))
                {
                    return ((object)Convert.ToInt32(data));
                }
                else if (destinationType == typeof(uint) || destinationType == typeof(uint?))
                {
                    return ((object)Convert.ToUInt32(data));
                }
                else if (destinationType == typeof(long) || destinationType == typeof(long?))
                {
                    return ((object)Convert.ToInt64(data));
                }
                else if (destinationType == typeof(ulong) || destinationType == typeof(ulong?))
                {
                    return ((object)Convert.ToUInt64(data));
                }
                else if (destinationType == typeof(DateTime) || destinationType == typeof(DateTime?))
                {
                    return ((object)Convert.ToDateTime(data));
                }
                else if (destinationType == typeof(decimal) || destinationType == typeof(decimal?))
                {
                    return ((object)Convert.ToDecimal(data));
                }
                else if (destinationType == typeof(float) || destinationType == typeof(float?))
                {
                    return ((object)Convert.ToSingle(data));
                }
                else if (destinationType == typeof(double) || destinationType == typeof(double?))
                {
                    return ((object)Convert.ToDouble(data));
                }
                else if (destinationType == typeof(bool) || destinationType == typeof(bool?))
                {
                    return ((object)Convert.ToBoolean(data));
                }
                else if (destinationType.IsEnum || IsGenericEnum(destinationType))
                {
                    Type realType;
                    if (IsGenericEnum(destinationType))
                    {
                        realType = destinationType.GetGenericArguments()[0];
                    }
                    else
                    {
                        realType = destinationType;
                    }
                    return Enum.Parse(realType, data.ToString(), true);
                }
                return data;
            }
            catch
            {
                string msg = "Can't cast '" + data.GetType().FullName + "' to '" + destinationType.FullName + "'.";
                if (propertyName != null)
                {
                    msg += " Propert Name : " + propertyName + ";";
                }
                if (columnName != null)
                {
                    msg += " Column Name : " + columnName;
                }
                throw new InvalidCastException(msg);
            }
        }

        public static bool CompareType(Type p1, Type p2)
        {
            if (p1 == p2)
            {
                return true;
            }
            if (p2.IsAssignableFrom(p1))
            {
                return true;
            }
            if (p2.IsGenericType && p2.GetGenericTypeDefinition() == typeof(Nullable<>)
                    && p2.GetGenericArguments() != null
                    && p2.GetGenericArguments().Length == 1 && p2.GetGenericArguments()[0].IsAssignableFrom(p1))
            {
                return true;
            }
            return false;
        }

        public static string GetEnumText(FieldInfo[] fields, string val)
        {
            string result = "";
            Type type = typeof(DescriptionAttribute);
            if (fields != null && fields.Length > 0)
            {
                foreach (FieldInfo fi in fields)
                {
                    if (fi.Name == val)
                    {
                        object[] arr = fi.GetCustomAttributes(type, true);
                        if (arr.Length > 0)
                            result = ((DescriptionAttribute)arr[0]).Description;
                        break;
                    }
                }
            }
            return result;
        }
    }
}
