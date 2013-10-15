using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Common.Utility
{
    public static class EntityConverter<S, T>
    {
        public static Action<S, T> UnifiedManualMap
        {
            get;
            set;
        }

        private static object Convert(Type sourceType, Type targetType, object source, bool propertyNameIgnoreCase, string columnName, string propertyName, List<string> ignoreTargetProperties)
        {
            if (source != null && sourceType != source.GetType())
            {
                sourceType = source.GetType();
            }
            while (source != null && sourceType.IsGenericType && sourceType.GetGenericTypeDefinition() == typeof(Nullable<>)
                   && sourceType.GetGenericArguments() != null
                   && sourceType.GetGenericArguments().Length == 1)
            {
                if ((bool)Invoker.PropertyGet(sourceType, source, "HasValue", false, true))
                {
                    source = Invoker.PropertyGet(sourceType, source, "Value", false, true);
                }
                else
                {
                    source = null;
                }
                sourceType = sourceType.GetGenericArguments()[0];
            } // 通过循环找出源数据的真实类型（Nullable<>里的泛型参数类型）

            // 1. 为空
            if (source == null || source == DBNull.Value) // 对象实例为空
            {
                return typeof(DataConvertor).GetMethod("GetDefaultValue", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(targetType).Invoke(null, null);
            }

            // 2. 为简单类型或object 
            TypeCode s_Code = Type.GetTypeCode(sourceType);
            TypeCode t_Code = Type.GetTypeCode(targetType);
            // enum的TypeCode为TypeCode.Int32
            if (s_Code != TypeCode.Object || t_Code != TypeCode.Object
                || sourceType == typeof(object) || targetType == typeof(object))
            {
                return DataConvertor.GetValueByType(targetType, source, columnName, propertyName);
            }

            // 3. 为实现了IDictionary<TKey, TValue>接口的集合
            if (targetType.IsGenericType && targetType.GetGenericArguments().Length == 2
                    && typeof(IDictionary<int, int>).IsAssignableFrom(targetType.GetGenericTypeDefinition().MakeGenericType(typeof(int), typeof(int)))
                    && sourceType.IsGenericType && sourceType.GetGenericArguments().Length == 2
                    && typeof(IDictionary<int, int>).IsAssignableFrom(sourceType.GetGenericTypeDefinition().MakeGenericType(typeof(int), typeof(int)))
                )
            {
                object dictionary = Invoker.CreateInstance(targetType);
                Type target_key_type = targetType.GetGenericArguments()[0];
                Type target_value_type = targetType.GetGenericArguments()[1];
                Type source_key_type = sourceType.GetGenericArguments()[0];
                Type source_value_type = sourceType.GetGenericArguments()[1];
                IEnumerable iterator = (IEnumerable)source;
                foreach (object x in iterator)
                {
                    Type xType = x.GetType();
                    object source_key = xType.GetProperty("Key").GetGetMethod().Invoke(x, new object[0]);
                    object source_value = xType.GetProperty("Value").GetGetMethod().Invoke(x, new object[0]);

                    object target_key = Convert(source_key_type, target_key_type, source_key, propertyNameIgnoreCase, columnName, propertyName, new List<string>(0));
                    object target_value = Convert(source_value_type, target_value_type, source_value, propertyNameIgnoreCase, columnName, propertyName, new List<string>(0));

                    Invoker.MethodInvoke(dictionary, "Add", target_key, target_value);
                }
                return dictionary;
            }

            // 4. 为List<T>集合
            if (targetType.IsGenericType && targetType.GetGenericArguments().Length == 1
                    && typeof(ICollection<int>).IsAssignableFrom(targetType.GetGenericTypeDefinition().MakeGenericType(typeof(int)))
                    && sourceType.IsGenericType && sourceType.GetGenericArguments().Length == 1
                    && typeof(IEnumerable<int>).IsAssignableFrom(sourceType.GetGenericTypeDefinition().MakeGenericType(typeof(int)))
                )
            {
                object list = Invoker.CreateInstance(targetType);
                Type listType = targetType.GetGenericArguments()[0];
                Type s_eType = sourceType.GetGenericArguments()[0];
                IEnumerable iterator = (IEnumerable)source;
                foreach (object x in iterator)
                {
                    Invoker.MethodInvoke(list, "Add", Convert(s_eType, listType, x, propertyNameIgnoreCase, columnName, propertyName, new List<string>(0)));
                }
                return list;
            }

            // 5. 为非集合的复杂类型
            PropertyInfo[] sourceProArray = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] targetProArray = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            object target = Invoker.CreateInstance(targetType);
            foreach (var s_pro in sourceProArray)
            {
                if (!s_pro.CanRead)
                {
                    continue;
                }
                foreach (var t_pro in targetProArray)
                {
                    if (ignoreTargetProperties.Contains(t_pro.Name))
                    {
                        continue;
                    }
                    StringComparison com = propertyNameIgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                    if (t_pro.CanWrite && string.Compare(t_pro.Name, s_pro.Name, com) == 0)
                    {
                        Type s_p_type = Invoker.GetPropertyType(sourceType, s_pro.Name);
                        Type t_p_type = Invoker.GetPropertyType(targetType, t_pro.Name);
                        object value = Invoker.PropertyGet(sourceType, source, s_pro.Name, false, true);
                        Invoker.PropertySet(target, t_pro.Name, Convert(s_p_type, t_p_type, value, propertyNameIgnoreCase,
                                columnName + (columnName.Length > 0 ? "." : "") + s_pro.Name, propertyName + (propertyName.Length > 0 ? "." : "") + t_pro.Name,
                                new List<string>()));
                        break;
                    }
                }
            }
            return target;
        }

        public static T Convert(S source, params string[] ignoreTargetProperties)
        {
            return Convert(source, null, ignoreTargetProperties);
        }

        public static T Convert(S source, Action<S, T> manualMap, params string[] ignoreTargetProperties)
        {
            return Convert(source, manualMap, true, ignoreTargetProperties);
        }

        public static T Convert(S source, Action<S, T> manualMap, bool propertyNameIgnoreCase, params string[] ignoreTargetProperties)
        {
            List<string> list = ignoreTargetProperties == null ? new List<string>(0) : new List<string>(ignoreTargetProperties);
            T target = (T)Convert(typeof(S), typeof(T), source, propertyNameIgnoreCase, "", "", list);  //EntityConvertorCreater<S, T>.Instance.ConvertEntity(source, null, null);
            if (manualMap != null)
            {
                manualMap(source, target);
            }
            if (UnifiedManualMap != null)
            {
                UnifiedManualMap(source, target);
            }
            return target;
        }

        public static C Convert<C>(IEnumerable<S> sourceList, params string[] ignoreTargetProperties) where C : class, ICollection<T>, new()
        {
            return Convert<C>(sourceList, null, ignoreTargetProperties);
        }

        public static C Convert<C>(IEnumerable<S> sourceList, Action<S, T> manualMap, params string[] ignoreTargetProperties) where C : class, ICollection<T>, new()
        {
            return Convert<C>(sourceList, manualMap, true, ignoreTargetProperties);
        }

        public static C Convert<C>(IEnumerable<S> sourceList, Action<S, T> manualMap, bool propertyNameIgnoreCase, params string[] ignoreTargetProperties) where C : class, ICollection<T>, new()
        {
            if (sourceList == null)
            {
                return null;
            }
            C rstList = new C();
            IEnumerator<S> enumerator = sourceList.GetEnumerator();
            while (enumerator.MoveNext())
            {
                rstList.Add(Convert(enumerator.Current, manualMap, propertyNameIgnoreCase, ignoreTargetProperties));
            }
            return rstList;
        }

        public static List<T> Convert(IEnumerable<S> sourceList, params string[] ignoreTargetProperties)
        {
            return Convert<List<T>>(sourceList, ignoreTargetProperties);
        }

        public static List<T> Convert(IEnumerable<S> sourceList, Action<S, T> manualMap, params string[] ignoreTargetProperties)
        {
            return Convert<List<T>>(sourceList, manualMap, ignoreTargetProperties);
        }

        public static List<T> Convert(IEnumerable<S> sourceList, Action<S, T> manualMap, bool propertyNameIgnoreCase, params string[] ignoreTargetProperties)
        {
            return Convert<List<T>>(sourceList, manualMap, propertyNameIgnoreCase, ignoreTargetProperties);
        }

        /*
        public static void AutoFill(S source, T target)
        {
            AutoFill(source, target, null);
        }

        public static void AutoFill(S source, T target, Action<S, T> manualMap)
        {
            AutoFill(source, target, false, true, manualMap);
        }

        public static void AutoFill(S source, T target, bool onlyNullBeFilled, bool canFilledByNull, Action<S, T> manualMap)
        {
            EntityConvertorCreater<S, T>.Instance.AutoFill(source, target, onlyNullBeFilled, canFilledByNull);
            if (manualMap != null)
            {
                manualMap(source, target);
            }
            if (UnifiedManualMap != null)
            {
                UnifiedManualMap(source, target);
            }
        }

        public static void AutoFill(IEnumerable<S> sourceList, ICollection<T> targetList, Action<S, T> manualMap)
        {
            IEnumerator<S> enumerator = sourceList.GetEnumerator();
            while (enumerator.MoveNext())
            {
                targetList.Add(Convert(enumerator.Current, manualMap));
            }
        }
         */
    }
}
