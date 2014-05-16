using System;
using System.IO;
using System.Text;
using System.Runtime.Caching;
using System.Collections.Generic;

namespace Soho.Utility
{
    public static class CacheManager
    {
        private const string CACHE_LOCKER_PREFIX = "C_L_";

        public static void RemoveFromCache(string cacheName, string cacheKey)
        {
            ICache c = CacheFactory.GetInstance(cacheName);
            string locker = CACHE_LOCKER_PREFIX + "N:" + cacheName + cacheKey;
            lock (locker)
            {
                c.Remove(cacheKey);
            }
        }

        public static void RemoveFromLocalCache(string cacheKey)
        {
            string locker = CACHE_LOCKER_PREFIX + cacheKey;
            lock (locker)
            {
                MemoryCache.Default.Remove(cacheKey);
            }
        }

        public static T GetWithCache<T>(string cacheName, string cacheKey, Func<T> getter, bool absoluteExpiration = true, int cacheExpirationMinutes = 30)
            where T : class
        {
            ICache c = CacheFactory.GetInstance(cacheName);
            T rst = c.Get(cacheKey) as T;
            if (rst != null)
            {
                return rst;
            }
            string locker = CACHE_LOCKER_PREFIX + "N:" + cacheName + cacheKey;
            lock (locker)
            {
                rst = c.Get(cacheKey) as T;
                if (rst != null)
                {
                    return rst;
                }
                rst = getter();
                if (absoluteExpiration)
                {
                    c.Set(cacheKey, rst, DateTime.Now.AddMinutes(cacheExpirationMinutes));
                }
                else
                {
                    c.Set(cacheKey, rst, TimeSpan.FromMinutes(cacheExpirationMinutes));
                }
                return rst;
            }
        }

        public static T GetWithLocalCache<T>(string cacheKey, Func<T> getter, bool absoluteExpiration = true, int cacheExpirationMinutes = 30)
            where T : class
        {
            T rst = MemoryCache.Default.Get(cacheKey) as T;
            if (rst != null)
            {
                return rst;
            }
            string locker = CACHE_LOCKER_PREFIX + cacheKey;
            lock (locker)
            {
                rst = MemoryCache.Default.Get(cacheKey) as T;
                if (rst != null)
                {
                    return rst;
                }
                rst = getter();
                CacheItemPolicy cp = new CacheItemPolicy();
                if (absoluteExpiration)
                {
                    cp.AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddMinutes(cacheExpirationMinutes));
                }
                else
                {
                    cp.SlidingExpiration = TimeSpan.FromMinutes(cacheExpirationMinutes);
                }
                MemoryCache.Default.Set(cacheKey, rst, cp);
                return rst;
            }
        }

        public static T GetWithLocalCache<T>(string cacheKey, Func<T> getter, params string[] filePathList)
            where T : class
        {
            T rst = MemoryCache.Default.Get(cacheKey) as T;
            if (rst != null)
            {
                return rst;
            }
            string locker = CACHE_LOCKER_PREFIX + cacheKey;
            lock (locker)
            {
                rst = MemoryCache.Default.Get(cacheKey) as T;
                if (rst != null)
                {
                    return rst;
                }
                rst = getter();
                List<string> list = new List<string>(filePathList.Length);
                foreach (var file in filePathList)
                {
                    if (File.Exists(file))
                    {
                        list.Add(file);
                    }
                }
                if (list.Count > 0)
                {
                    CacheItemPolicy cp = new CacheItemPolicy();
                    cp.ChangeMonitors.Add(new HostFileChangeMonitor(list));
                    MemoryCache.Default.Set(cacheKey, rst, cp);
                }
                return rst;
            }
        }

        public static string ReadTextFileWithLocalCache(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }
            FileInfo f = new FileInfo(filePath);
            string key = f.FullName.ToUpper().GetHashCode().ToString();
            return GetWithLocalCache<string>(key, () => LoadRawString(filePath), filePath);
        }

        public static T ReadXmlFileWithLocalCache<T>(string filePath)
            where T : class
        {
            if (!File.Exists(filePath))
            {
                return null;
            }
            FileInfo f = new FileInfo(filePath);
            string key = "ReadXmlFileWithCache_" + f.FullName.ToUpper().GetHashCode().ToString();
            return GetWithLocalCache<T>(key, () => SerializationUtility.LoadFromXml<T>(filePath), filePath);
        }

        public static T ReadJsonFileWithLocalCache<T>(string filePath)
            where T : class
        {
            if (!File.Exists(filePath))
            {
                return null;
            }
            FileInfo f = new FileInfo(filePath);
            string key = "ReadJsonFileWithCache_" + f.FullName.ToUpper().GetHashCode().ToString();
            return GetWithLocalCache<T>(key, () => SerializationUtility.JsonDeserialize<T>(LoadRawString(filePath)), filePath);
        }

        private static string LoadRawString(string filePath)
        {
            using (StreamReader sr = new StreamReader(filePath, Encoding.GetEncoding("gb2312"), true))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
