using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Soho.Utility
{
    public static class SoapEntityMapping
    {
        private static Dictionary<string, Tuple<Type, string, PropertyMapperList>> s_Cache1 = new Dictionary<string, Tuple<Type, string, PropertyMapperList>>();
        private static Dictionary<Type, Tuple<Type, string, PropertyMapperList>> s_Cache2 = new Dictionary<Type, Tuple<Type, string, PropertyMapperList>>();

        public static void Add<T>(PropertyMapperList mapper = null, bool throwExceptionWhenAddDuplicatedMapping = false)
        {
            Add(typeof(T), typeof(T).Name, mapper, throwExceptionWhenAddDuplicatedMapping);
        }

        public static void Add<T>(string soapClassName, PropertyMapperList mapper = null, bool throwExceptionWhenAddDuplicatedMapping = false)
        {
            Add(typeof(T), soapClassName, mapper, throwExceptionWhenAddDuplicatedMapping);
        }

        public static void Add(Type entityClassType, string soapClassName, PropertyMapperList mapper = null, bool throwExceptionWhenAddDuplicatedMapping = false)
        {
            if (s_Cache1.ContainsKey(SoapClient.Namespace + "." + soapClassName))
            {
                if (throwExceptionWhenAddDuplicatedMapping)
                {
                    throw new ApplicationException("Duplicated mapping for soap class name '" + soapClassName + "'.");
                }
                return;
            }
            if (s_Cache2.ContainsKey(entityClassType))
            {
                if (throwExceptionWhenAddDuplicatedMapping)
                {
                    throw new ApplicationException("Duplicated mapping for entity class '" + entityClassType.FullName + "'.");
                }
                return;
            }
            if (mapper == null)
            {
                mapper = new PropertyMapperList();
                var infoList = entityClassType.GetProperties();
                foreach (var info in infoList)
                {
                    if (info.CanRead)
                    {
                        mapper.Add(info.Name);
                    }
                }
            }
            s_Cache1.Add(SoapClient.Namespace + "." + soapClassName, new Tuple<Type, string, PropertyMapperList>(entityClassType, soapClassName, mapper));
            s_Cache2.Add(entityClassType, new Tuple<Type, string, PropertyMapperList>(entityClassType, soapClassName, mapper));
        }

        internal static object ConvertFromSoapObject(object data, Type entityType)
        {
            if (data == null || data == DBNull.Value)
            {
                return null;
            }
            Type type = data.GetType();
            if (entityType.IsAssignableFrom(type))
            {
                return data;
            }
            // 1. 返回的Soap对象类型为数组
            if (type.IsArray)
            {
                Array ary = (Array)data;
                // 先获得数组元素的类型
                Type x = type.GetElementType();
                Tuple<Type, string, PropertyMapperList> aryItem;
                // 根据Soap数组元素类型来找是否有映射到了某个Entity类型上，如果没有为该元素类型
                // 映射Entity类型，则不做任何类型数据转换，直接返回Soap数据
                if (s_Cache1.TryGetValue(x.FullName, out aryItem) == false)
                {
                    if (entityType.IsArray) // 数组
                    {
                        Array rst = Array.CreateInstance(entityType.GetElementType(), ary.Length);
                        for (int i = 0; i < ary.Length; i++)
                        {
                            rst.SetValue(ary.GetValue(i), i);
                        }
                        return rst;
                    }
                    else // 集合
                    {
                        object tmpx = Invoker.CreateInstance(entityType);
                        for (int i = 0; i < ary.Length; i++)
                        {
                            Invoker.MethodInvoke(tmpx, "Add", ary.GetValue(i));
                        }
                        return tmpx;
                    }
                }
                object entity;
                if (entityType.IsArray) // 数组
                {
                    entity = Array.CreateInstance(aryItem.Item1, ary.Length);
                    Array tmp = (Array)entity;
                    for (int i = 0; i < ary.Length; i++)
                    {
                        tmp.SetValue(ConvertFromSoapObject(ary.GetValue(i), aryItem.Item1), i);
                    }
                }
                else // 集合
                {
                    entity = Invoker.CreateInstance(entityType);
                    for (int i = 0; i < ary.Length; i++)
                    {
                        Invoker.MethodInvoke(entity, "Add", ConvertFromSoapObject(ary.GetValue(i), aryItem.Item1));
                    }
                }
                return entity;
            }
            // 2. 返回的为非数组的类型
            Tuple<Type, string, PropertyMapperList> item;
            if (s_Cache1.TryGetValue(type.FullName, out item) == false)
            {
                return data;
            }
            object returnObj = Invoker.CreateInstance(item.Item1);
            foreach (var mp in item.Item3)
            {
                Type tmp = Invoker.GetPropertyType(item.Item1, mp.PropertyName, false, true);
                object v = (object)Invoker.PropertyGet(data, mp.SoapClassPropertyName);
                v = mp.PropertyConvertFrom == null ? v : mp.PropertyConvertFrom(v);
                if (tmp.IsNullableType() && Invoker.ExistPropertyGet(type, mp.SoapClassPropertyName + "Specified"))
                {
                    bool tx = (bool)Invoker.PropertyGet(data, mp.SoapClassPropertyName + "Specified");
                    if (tx)
                    {
                        object v1 = ConvertFromSoapObject(v, tmp);
                        Invoker.PropertySet(returnObj, mp.PropertyName, v1);
                    }
                    else
                    {
                        Invoker.PropertySet(returnObj, mp.PropertyName, null);
                    }
                }
                else
                {
                    object v1 = ConvertFromSoapObject(v, tmp);
                    Invoker.PropertySet(returnObj, mp.PropertyName, v1);
                }
            }
            return returnObj;
        }

        internal static object ConvertToSoapObject(object data, Assembly asy)
        {
            if (data == null || data == DBNull.Value) // 对象实例为空
            {
                return null;
            }
            Type sourceType = data.GetType();

            // 1. 源数据为数组的时候
            if (sourceType.IsArray)
            {
                // 先获得数组元素的类型
                Type x = sourceType.GetElementType();
                Tuple<Type, string, PropertyMapperList> aryItem;
                // 根据数组元素类型来找是否有映射到了某个Soap类型上，如果没有为该元素类型
                // 映射Soap类型，则不做任何类型数据转换，直接返回源数据
                if (s_Cache2.TryGetValue(x, out aryItem) == false)
                {
                    return data;
                }
                // 将源数据转换为数组对象
                Array ary = (Array)data;
                int len = ary.Length;
                // 根据所找到的元素类型映射到的Soap类型，创建该Soap的数组
                Array destinationObject = Array.CreateInstance(asy.GetType(SoapClient.Namespace + "." + aryItem.Item2, true), len);
                // 为数组每个元素进行数据类型转换，再赋值
                for (int i = 0; i < len; i++)
                {
                    destinationObject.SetValue(ConvertToSoapObject(ary.GetValue(i), asy), i);
                }
                return destinationObject;
            }
            // 2. 如果是实现了IEnumerable<T>的类型，比如List<T>，需要变为对应的数组T[]
            if (sourceType.IsGenericType && sourceType.GetGenericArguments().Length == 1
                    && typeof(IEnumerable<int>).IsAssignableFrom(sourceType.GetGenericTypeDefinition().MakeGenericType(typeof(int)))
                )
            {
                // 先获得集合元素的类型
                Type x = sourceType.GetGenericArguments()[0];
                Tuple<Type, string, PropertyMapperList> aryItem;
                // 根据集合元素类型来找是否有映射到了某个Soap类型上，如果没有为该元素类型
                // 映射Soap类型，则不做任何类型数据转换，直接将源数据变为数组返回
                if (s_Cache2.TryGetValue(x, out aryItem) == false)
                {
                    // 生成List<x>对象
                    IList array = (IList)Invoker.CreateInstance(typeof(List<>).MakeGenericType(x));
                    var iterator = (IEnumerable)data;
                    foreach (object en in iterator)
                    {
                        array.Add(en);
                    }
                    return Invoker.MethodInvoke(array, "ToArray");
                }
                IList dest = (IList)Invoker.CreateInstance(typeof(List<>).MakeGenericType(asy.GetType(SoapClient.Namespace + "." + aryItem.Item2, true)));
                var iterator1 = (IEnumerable)data;
                foreach (object en in iterator1)
                {
                    dest.Add(ConvertToSoapObject(en, asy));
                }
                return Invoker.MethodInvoke(dest, "ToArray");
            }
            // 3. 为非数组且非IEnumerable<T>的类型
            Tuple<Type, string, PropertyMapperList> item;
            // 如果为true，说明没有为该sourceType映射soap类型，则不做任何类型数据转换
            if (s_Cache2.TryGetValue(sourceType, out item) == false)
            {
                return data;
            }
            string entityTypeName = SoapClient.Namespace + "." + item.Item2;
            Type type = asy.GetType(entityTypeName, true);
            object entity = Invoker.CreateInstance(type);
            foreach (var mp in item.Item3)
            {
                Type tmp = Invoker.GetPropertyType(type, mp.SoapClassPropertyName, false, true);
                object v = (object)Invoker.PropertyGet(data, mp.PropertyName);
                v = mp.PropertyConvertTo == null ? v : mp.PropertyConvertTo(v);
                if (tmp.IsValueType && Invoker.ExistPropertySet(type, mp.SoapClassPropertyName + "Specified"))
                {
                    if (tmp.IsNullableType())
                    {
                        Invoker.PropertySet(entity, mp.SoapClassPropertyName + "Specified", v != null);
                    }
                    else
                    {
                        Invoker.PropertySet(entity, mp.SoapClassPropertyName + "Specified", true);
                    }
                }
                object v1 = ConvertToSoapObject(v, asy);
                Invoker.PropertySet(entity, mp.SoapClassPropertyName, v1);
            }
            return entity;
        }

        private static bool IsNullableType(this Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                    && type.GetGenericArguments() != null
                    && type.GetGenericArguments().Length == 1);
        }
    }

    public class PropertyMapperList : List<PropertyMapper>
    {
        //private static List<PropertyMapper<T>> m_PropertiesMapping = new List<PropertyMapper<T>>();

        public void Add(string propertyName, Func<object, object> propertyConvertTo = null, Func<object, object> propertyConvertFrom = null)
        {
            Add(propertyName, propertyName, propertyConvertTo, propertyConvertFrom);
        }

        public void Add(string propertyName, string soapClassPropertyName, Func<object, object> propertyConvertTo = null, Func<object, object> propertyConvertFrom = null)
        {
            this.Add(new PropertyMapper(propertyName, soapClassPropertyName, propertyConvertTo, propertyConvertFrom));
        }
    }

    public class PropertyMapper
    {
        public string PropertyName { get; private set; }
        public string SoapClassPropertyName { get; private set; }
        public Func<object, object> PropertyConvertTo { get; private set; }
        public Func<object, object> PropertyConvertFrom { get; private set; }

        public PropertyMapper(string propertyName, string soapClassPropertyame, Func<object, object> to, Func<object, object> from)
        {
            PropertyName = propertyName;
            SoapClassPropertyName = soapClassPropertyame;
            PropertyConvertTo = to;
            PropertyConvertFrom = from;
        }
    }
}
