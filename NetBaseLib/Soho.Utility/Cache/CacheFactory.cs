using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Soho.Utility
{
    public static class CacheFactory
    {
        private static Dictionary<string, ICache> s_CacheProviders = new Dictionary<string, ICache>();
        private static object s_SyncObj = new object();

        public static ICache GetInstance()
        {
            return GetInstance(null);
        }

        public static ICache GetInstance(string name)
        {
            CacheSetting setting = CacheSection.GetSetting();
            if (name == null || name.Trim().Length <= 0)
            {
                name = setting.DefaultCacheName;
            }
            if (name == null)
            {
                throw new ConfigurationErrorsException("The default cache name is not configured in config file.");
            }
            if (s_CacheProviders.ContainsKey(name))
            {
                return s_CacheProviders[name];
            }
            lock (s_SyncObj)
            {
                if (s_CacheProviders.ContainsKey(name))
                {
                    return s_CacheProviders[name];
                }
                if (!setting.ContainsKey(name))
                {
                    throw new ConfigurationErrorsException("The cache named '" + name + "' is not be found in config file.");
                }
                CacheItemConfig item = setting[name];
                if (item.Type == null || item.Type.Trim().Length <= 0)
                {
                    throw new ConfigurationErrorsException("The type of cache '" + name + "' cannot be empty.");
                }
                Type p = Type.GetType(item.Type, true);
                if (!typeof(ICache).IsAssignableFrom(p))
                {
                    throw new ConfigurationErrorsException("The type '" + p.AssemblyQualifiedName + "' of cache '" + name + "' dosen't implement the interface '" + typeof(ICache).AssemblyQualifiedName + "'.");
                }
                ICache rst = (ICache)Activator.CreateInstance(p);
                rst.InitFromConfig(name, item.Parameters);
                s_CacheProviders.Add(name, rst);
                return rst;
            }
        }
    }
}
