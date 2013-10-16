using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Utility;
using System.Runtime.Caching;
using System.Collections.Specialized;

namespace Common.Utility
{
    [Serializable]
    public class LocalMemoryCache : ICache
    {
        private static class LocalCacheKeyStorage
        {
            private static object s_SyncRoot = new object();

            private static Dictionary<string, string> s_KeyList = new Dictionary<string, string>();

            public static void Set(string cacheKey, string groupName)
            {
                lock (s_SyncRoot)
                {
                    if (s_KeyList.ContainsKey(cacheKey))
                    {
                        s_KeyList[cacheKey] = groupName;
                    }
                    else
                    {
                        s_KeyList.Add(cacheKey, groupName);
                    }
                }
            }

            public static void RemoveByGroup(string groupName)
            {
                List<string> list = GetKeys(groupName);
                lock (s_SyncRoot)
                {
                    foreach (string s in list)
                    {
                        s_KeyList.Remove(s);
                    }
                }
            }

            public static void RemoveByKey(string cacheKey)
            {
                lock (s_SyncRoot)
                {
                    s_KeyList.Remove(cacheKey);
                }
            }

            public static List<string> GetKeys(string groupName)
            {
                List<string> result = new List<string>();
                lock (s_SyncRoot)
                {
                    foreach (KeyValuePair<string, string> kvp in s_KeyList)
                    {
                        if (kvp.Value == groupName)
                        {
                            result.Add(kvp.Key);
                        }
                    }
                }
                return result;
            }
        }

        private ObjectCache s_Cache = null;

        public void InitFromConfig(string cacheName, NameValueCollection parameters)
        {
            if (parameters != null && parameters.Count <= 0)
            {
                parameters = null;
            }
            s_Cache = new MemoryCache("ECCentral-" + cacheName, parameters);
        }

        public bool Set(string key, object value, string groupName = null)
        {
            if (key == null || value == null)
            {
                return false;
            }

            var policy = new CacheItemPolicy();
            policy.Priority = CacheItemPriority.NotRemovable;
            if (groupName != null)
            {
                policy.RemovedCallback += a =>
                {
                    LocalCacheKeyStorage.RemoveByKey(a.CacheItem.Key);
                };
            }
            s_Cache.Set(new CacheItem(key, value), policy);
            if (groupName != null)
            {
                LocalCacheKeyStorage.Set(key, groupName);
            }
            return true;
        }

        public bool Set(string key, object value, TimeSpan slidingExpiration, string groupName = null)
        {
            if (key == null || value == null)
            {
                return false;
            }

            var policy = new CacheItemPolicy();
            policy.Priority = CacheItemPriority.Default;
            policy.SlidingExpiration = slidingExpiration;
            if (groupName != null)
            {
                policy.RemovedCallback += a =>
                {
                    LocalCacheKeyStorage.RemoveByKey(a.CacheItem.Key);
                };
            }
            s_Cache.Set(new CacheItem(key, value), policy);
            if (groupName != null)
            {
                LocalCacheKeyStorage.Set(key, groupName);
            }
            return true;
        }

        public bool Set(string key, object value, DateTime absoluteExpiration, string groupName = null)
        {
            if (key == null || value == null)
            {
                return false;
            }

            var policy = new CacheItemPolicy();
            policy.Priority = CacheItemPriority.Default;
            policy.AbsoluteExpiration = absoluteExpiration;
            if (groupName != null)
            {
                policy.RemovedCallback += a =>
                {
                    LocalCacheKeyStorage.RemoveByKey(a.CacheItem.Key);
                };
            }
            s_Cache.Set(new CacheItem(key, value), policy);
            if (groupName != null)
            {
                LocalCacheKeyStorage.Set(key, groupName);
            }
            return true;
        }

        public object Get(string key)
        {
            return s_Cache[key];
        }

        public T Get<T>(string key)
        {
            object cache = s_Cache[key];
            if (cache != null)
            {
                return (T)cache;
            }
            else
            {
                return default(T);
            }
        }

        public List<object> Get(string[] keys)
        {
            List<object> result = new List<object>(keys.Length);
            foreach (string key in keys)
            {
                result.Add(Get(key));
            }
            return result;
        }

        public List<T> Get<T>(string[] keys)
        {
            List<T> result = new List<T>(keys.Length);
            foreach (string key in keys)
            {
                result.Add(Get<T>(key));
            }
            return result;
        }

        public List<object> GetByGroup(string groupName)
        {
            List<string> keys = GetKeysByGroup(groupName);
            List<object> result = new List<object>(keys.Count);
            foreach (string key in keys)
            {
                object data = Get(key);
                if (data != null)
                {
                    result.Add(data);
                }
            }
            return result;
        }

        public List<T> GetByGroup<T>(string groupName)
        {
            List<string> keys = GetKeysByGroup(groupName);
            List<T> result = new List<T>(keys.Count);
            foreach (string key in keys)
            {
                T data = Get<T>(key);
                if (data != null)
                {
                    result.Add(data);
                }
            }
            return result;
        }

        public bool Remove(string key)
        {
            s_Cache.Remove(key);
            return true;
        }

        public bool RemoveByGroup(string groupName)
        {
            List<string> keys = GetKeysByGroup(groupName);
            foreach (string key in keys)
            {
                Remove(key);
            }
            return true;
        }

        public List<string> GetKeysByGroup(string groupName)
        {
            return LocalCacheKeyStorage.GetKeys(groupName);
        }

        public void FlushAll()
        {
            foreach (var item in s_Cache)
            {
                Remove(item.Key);
            }
        }
    }
}
