using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Common.Utility
{
    public static class ObjectFactory<T> where T : class
    {
        private static RealObjectFactory<T> m_Factory = new RealObjectFactory<T>();

        #region Private Helper

        private static string GetVersion()
        {
            string version = TypeVersionConfig.Instance.GlobalDefaultVersion;
            if (version == null || version.Trim().Length <= 0)
            {
                version = TypeVersionConfig.VERSION_DEFAULT;
            }

            if (TypeVersionConfig.ExistVersionMap(typeof(T)))
            {
                version = TypeVersionConfig.GetVersion(typeof(T));
            }
            return version;
        }

        private static T LocateInstance(RealObjectFactory<T> factory, string version, string[] filters, bool throwOnError)
        {
            T instance = factory.GetService(version, filters);
            if (instance == null && throwOnError)
            {
                StringBuilder sb = new StringBuilder();
                if (filters != null && filters.Length > 0)
                {
                    sb.Append(" with the filter : {");
                    foreach (var f in filters)
                    {
                        sb.Append(" \"" + f + "\",");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append(" }");
                }
                throw new ApplicationException("Can't load the implementation instance of type'" + typeof(T).AssemblyQualifiedName + "' for version '" + version + "'" + sb.ToString() + ".");
            }
            return instance;
        }

        #endregion

        #region Get Singleton Instance

        public static T Instance
        {
            get
            {
                return GetInstance(true);
            }
        }

        public static T GetInstance(bool throwOnError)
        {
            return GetInstance(GetVersion(), throwOnError);
        }

        public static T GetInstance(string version)
        {
            return GetInstance(version, true);
        }

        public static T GetInstance(string[] filters)
        {
            return GetInstance(filters, true);
        }

        public static T GetInstance(string version, bool throwOnError)
        {
            return GetInstance(version, null, throwOnError);
        }

        public static T GetInstance(string[] filters, bool throwOnError)
        {
            return GetInstance(GetVersion(), filters, throwOnError);
        }

        public static T GetInstance(string version, string[] filters)
        {
            return GetInstance(version, filters, true);
        }

        public static T GetInstance(string version, string[] filters, bool throwOnError)
        {
            return LocateInstance(m_Factory, version, filters, throwOnError);
        }

        #endregion

        #region Create New Instance

        public static T NewInstance()
        {
            return NewInstance(true);
        }

        public static T NewInstance(bool throwOnError)
        {
            return NewInstance(GetVersion(), throwOnError);
        }

        public static T NewInstance(string version)
        {
            return NewInstance(version, true);
        }

        public static T NewInstance(string version, bool throwOnError)
        {
            return NewInstance(version, null, throwOnError);
        }

        public static T NewInstance(string[] filters)
        {
            return NewInstance(filters, true);
        }

        public static T NewInstance(string[] filters, bool throwOnError)
        {
            return NewInstance(GetVersion(), filters, throwOnError);
        }

        public static T NewInstance(string version, string[] filters)
        {
            return NewInstance(version, filters, true);
        }

        public static T NewInstance(string version, string[] filters, bool throwOnError)
        {
            RealObjectFactory<T> factory;
            if (throwOnError)
            {
                factory = new RealObjectFactory<T>();
            }
            else
            {
                try
                {
                    factory = new RealObjectFactory<T>();
                }
                catch
                {
                    return null;
                }
            }
            return LocateInstance(factory, version, filters, throwOnError);
        }

        #endregion

        public static List<KeyValuePair<string, Type>> GetAllVersions()
        {
            return m_Factory.GetAllVersions();
        }

 
    }

    public static class ObjectFactory
    {
        public static List<KeyValuePair<string, Type>> GetAllVersions(Type type)
        {
            if (!type.IsClass)
            {
                return new List<KeyValuePair<string, Type>>(0);
            }
            MethodInfo m = typeof(ObjectFactory<>).MakeGenericType(type).GetMethod("GetAllVersions", BindingFlags.Public | BindingFlags.Static);
            return (List<KeyValuePair<string, Type>>)m.Invoke(null, null);
        }
    }
}
