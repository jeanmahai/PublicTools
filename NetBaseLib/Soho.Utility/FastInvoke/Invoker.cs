using System;

namespace Soho.Utility
{
    public static class Invoker
    {
        public static bool SaveDll
        {
            get
            {
                return InvokerFactory.SaveDll;
            }
            set
            {
                InvokerFactory.SaveDll = value;
            }
        }

        #region Method

        public static object CreateInstance<T>(params object[] parameters)
        {
            return CreateInstance(typeof(T), parameters);
        }

        public static object CreateInstance(Type type, params object[] parameters)
        {
            IInvoke invoker = InvokerFactory.GetInvoker(type);
            return invoker.CreateInstance(parameters);
        }

        public static object MethodInvoke(object obj, string methodName, params object[] parameters)
        {
            return MethodInvoke(obj.GetType(), obj, methodName, parameters);
        }

        public static object MethodInvoke(Type type, object obj, string methodName, params object[] parameters)
        {
            IInvoke invoker = InvokerFactory.GetInvoker(type);
            return invoker.MethodInvoke(obj, methodName, parameters);
        }

        #endregion

        #region Property

        public static string GetPropertyNameIgnoreCase(Type type, string propertyName)
        {
            IInvoke invoker = InvokerFactory.GetInvoker(type);
            return invoker.GetPropertyNameIgnoreCase(propertyName);
        }

        #region 判断Property是否存在

        public static bool ExistProperty(Type type, string propertyName)
        {
            return ExistProperty(type, propertyName, false);
        }

        public static bool ExistProperty(Type type, string propertyName, bool ignoreCase)
        {
            IInvoke invoker = InvokerFactory.GetInvoker(type);
            if (ignoreCase)
            {
                propertyName = GetPropertyNameIgnoreCase(type, propertyName);
            }
            if (propertyName == null || propertyName.Trim().Length <= 0 || propertyName == "Item")
            {
                return false;
            }
            return invoker.ExistPropertyOrIndexerGet(propertyName) || invoker.ExistPropertyOrIndexerSet(propertyName);
        }

        public static bool ExistPropertySet(Type type, string propertyName)
        {
            return ExistPropertySet(type, propertyName, false);
        }

        public static bool ExistPropertySet(Type type, string propertyName, bool ignoreCase)
        {
            IInvoke invoker = InvokerFactory.GetInvoker(type);
            if (ignoreCase)
            {
                propertyName = GetPropertyNameIgnoreCase(type, propertyName);
            }
            if (propertyName == null || propertyName.Trim().Length <= 0 || propertyName == "Item")
            {
                return false;
            }
            return invoker.ExistPropertyOrIndexerSet(propertyName);
        }

        public static bool ExistPropertyGet(Type type, string propertyName)
        {
            return ExistPropertyGet(type, propertyName, false);
        }

        public static bool ExistPropertyGet(Type type, string propertyName, bool ignoreCase)
        {
            IInvoke invoker = InvokerFactory.GetInvoker(type);
            if (ignoreCase)
            {
                propertyName = GetPropertyNameIgnoreCase(type, propertyName);
            }
            if (propertyName == null || propertyName.Trim().Length <= 0 || propertyName == "Item")
            {
                return false;
            }
            return invoker.ExistPropertyOrIndexerGet(propertyName);
        }

        #endregion

        #region 获取属性的值

        public static object PropertyGet(object obj, string propertyName)
        {
            return PropertyGet(obj, propertyName, false);
        }

        public static object PropertyGet(object obj, string propertyName, bool ignoreCase)
        {
            return PropertyGet(obj, propertyName, ignoreCase, true);
        }

        public static object PropertyGet(object obj, string propertyName, bool ignoreCase, bool throwOnPropertyNotExist)
        {
            return PropertyGet(obj.GetType(), obj, propertyName, ignoreCase, throwOnPropertyNotExist);
        }

        public static object PropertyGet(Type type, object obj, string propertyName, bool ignoreCase, bool throwOnPropertyNotExist)
        {
            IInvoke invoker = InvokerFactory.GetInvoker(type);
            if (ignoreCase)
            {
                propertyName = GetPropertyNameIgnoreCase(type, propertyName);
            }
            if (propertyName == null || propertyName.Trim().Length <= 0 || !ExistPropertyGet(type, propertyName))
            {
                if (throwOnPropertyNotExist)
                {
                    throw new ApplicationException("无法找到类型'" + type.FullName + "'的公开实例属性" + propertyName + ".");
                }
                return null;
            }
            return invoker.PropertyGet(obj, propertyName);
        }

        #endregion

        #region 给属性赋值

        public static void PropertySet(object obj, string propertyName, object value)
        {
            PropertySet(obj, propertyName, value, false);
        }

        public static void PropertySet(object obj, string propertyName, object value, bool ignoreCase)
        {
            PropertySet(obj, propertyName, value, ignoreCase, true);
        }

        public static void PropertySet(object obj, string propertyName, object value, bool ignoreCase, bool throwOnPropertyNotExist)
        {
            PropertySet(obj.GetType(), obj, propertyName, value, ignoreCase, throwOnPropertyNotExist);
        }

        public static void PropertySet(Type type, object obj, string propertyName, object value, bool ignoreCase, bool throwOnPropertyNotExist)
        {
            IInvoke invoker = InvokerFactory.GetInvoker(type);
            if (ignoreCase)
            {
                propertyName = GetPropertyNameIgnoreCase(type, propertyName);
            }
            if (propertyName == null || propertyName.Trim().Length <= 0 || !ExistPropertySet(type, propertyName))
            {
                if (throwOnPropertyNotExist)
                {
                    throw new ApplicationException("无法找到类型'" + type.FullName + "'的公开实例属性" + propertyName + ".");
                }
                return;
            }
            invoker.PropertySet(obj, propertyName, value);
        }

        #endregion

        #region 获取属性类型

        // 默认区分属性名的大小写
        public static Type GetPropertyType(Type type, string propertyName)
        {
            return GetPropertyType(type, propertyName, false);
        }

        public static Type GetPropertyType(Type type, string propertyName, bool ignoreCase)
        {
            return GetPropertyType(type, propertyName, ignoreCase, true);
        }

        public static Type GetPropertyType(Type type, string propertyName, bool ignoreCase, bool throwOnPropertyNotExist)
        {
            IInvoke invoker = InvokerFactory.GetInvoker(type);
            if (ignoreCase)
            {
                propertyName = GetPropertyNameIgnoreCase(type, propertyName);
            }
            Type rst = (propertyName == null || propertyName.Trim().Length <= 0) ? null : invoker.GetPropertyType(propertyName);
            if (rst == null && throwOnPropertyNotExist)
            {
                throw new ApplicationException("无法找到类型'" + type.FullName + "'的公开实例属性" + propertyName + ".");
            }
            return rst;
        }

        #endregion

        #endregion

        #region Indexer

        public static bool ExistIndexer(Type type, params Type[] indexerParamTypes)
        {
            if (indexerParamTypes == null || indexerParamTypes.Length <= 0)
            {
                throw new ArgumentException("索引器必须要有参数.", "indexerParamTypes");
            }
            IInvoke invoker = InvokerFactory.GetInvoker(type);
            return invoker.ExistPropertyOrIndexerGet("Item", indexerParamTypes) || invoker.ExistPropertyOrIndexerSet("Item", indexerParamTypes);
        }

        public static bool ExistIndexerGet(Type type, params Type[] indexerParamTypes)
        {
            if (indexerParamTypes == null || indexerParamTypes.Length <= 0)
            {
                throw new ArgumentException("索引器必须要有参数.", "indexerParamTypes");
            }
            IInvoke invoker = InvokerFactory.GetInvoker(type);
            return invoker.ExistPropertyOrIndexerGet("Item", indexerParamTypes);
        }

        public static bool ExistIndexerSet(Type type, params Type[] indexerParamTypes)
        {
            if (indexerParamTypes == null || indexerParamTypes.Length <= 0)
            {
                throw new ArgumentException("索引器必须要有参数.", "indexerParamTypes");
            }
            IInvoke invoker = InvokerFactory.GetInvoker(type);
            return invoker.ExistPropertyOrIndexerSet("Item", indexerParamTypes);
        }

        public static Type GetIndexerType(Type type, params Type[] indexerParamTypes)
        {
            return GetIndexerType(type, true, indexerParamTypes);
        }

        public static Type GetIndexerType(Type type, bool throwOnIndexerNotExist, params Type[] indexerParamTypes)
        {
            if (indexerParamTypes == null || indexerParamTypes.Length <= 0)
            {
                throw new ArgumentException("索引器必须要有参数.", "indexerParamTypes");
            }
            IInvoke invoker = InvokerFactory.GetInvoker(type);
            Type rst = invoker.GetIndexerType(indexerParamTypes);
            if (rst == null && throwOnIndexerNotExist)
            {
                string tmp = string.Empty;
                foreach (Type t in indexerParamTypes)
                {
                    if (tmp.Length > 0)
                    {
                        tmp += ", ";
                    }
                    tmp += t.FullName;
                }
                throw new ApplicationException("无法找到类型'" + type.FullName + "'中参数为'" + tmp + "'的public索引器.");
            }
            return rst;
        }

        #region 获取索引器的值

        public static object IndexerGet(object obj, params object[] indexerParameters)
        {
            return IndexerGet(obj, true, indexerParameters);
        }

        public static object IndexerGet(object obj, bool throwOnPropertyNotExist, params object[] indexerParameters)
        {
            return PropertyGet(obj.GetType(), obj, throwOnPropertyNotExist, indexerParameters);
        }

        public static object PropertyGet(Type type, object obj, bool throwOnPropertyNotExist, params object[] indexerParameters)
        {
            if (indexerParameters == null || indexerParameters.Length <= 0)
            {
                throw new ArgumentException("索引器必须要有参数.", "indexerParamTypes");
            }
            IInvoke invoker = InvokerFactory.GetInvoker(type);
            Type[] indexerParamTypes = new Type[indexerParameters.Length];
            for (int i = 0; i < indexerParameters.Length; i++)
            {
                indexerParamTypes[i] = indexerParameters[i].GetType();
            }
            if (!ExistIndexerGet(type, indexerParamTypes))
            {
                if (throwOnPropertyNotExist)
                {
                    string tmp = string.Empty;
                    foreach (Type t in indexerParamTypes)
                    {
                        if (tmp.Length > 0)
                        {
                            tmp += ", ";
                        }
                        tmp += t.FullName;
                    }
                    throw new ApplicationException("无法找到类型'" + type.FullName + "'的public的可读的索引器[" + tmp + "].");
                }
                return null;
            }
            return invoker.IndexerGet(obj, indexerParameters);
        }

        #endregion

        #region 给索引器赋值

        public static void IndexerSet(object obj, object value, params object[] indexerParameters)
        {
            IndexerSet(obj, value, true, indexerParameters);
        }

        public static void IndexerSet(object obj, object value, bool throwOnPropertyNotExist, params object[] indexerParameters)
        {
            IndexerSet(obj.GetType(), obj, value, throwOnPropertyNotExist, indexerParameters);
        }

        public static void IndexerSet(Type type, object obj, object value, bool throwOnPropertyNotExist, params object[] indexerParameters)
        {
            if (indexerParameters == null || indexerParameters.Length <= 0)
            {
                throw new ArgumentException("索引器必须要有参数.", "indexerParamTypes");
            }
            IInvoke invoker = InvokerFactory.GetInvoker(type);
            Type[] indexerParamTypes = new Type[indexerParameters.Length];
            for (int i = 0; i < indexerParameters.Length; i++)
            {
                indexerParamTypes[i] = indexerParameters[i].GetType();
            }
            if (!ExistIndexerSet(type, indexerParamTypes))
            {
                if (throwOnPropertyNotExist)
                {
                    string tmp = string.Empty;
                    foreach (Type t in indexerParamTypes)
                    {
                        if (tmp.Length > 0)
                        {
                            tmp += ", ";
                        }
                        tmp += t.FullName;
                    }
                    throw new ApplicationException("无法找到类型'" + type.FullName + "'的public的可读的索引器[" + tmp + "].");
                }
                return;
            }
            invoker.IndexerSet(obj, value, indexerParameters);
        }

        #endregion

        #endregion
    }
}
