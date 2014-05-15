using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Soho.Utility;
using System.Collections.Specialized;
using System.Configuration;
using System.Threading;

namespace Soho.Utility
{
    [Serializable]
    public class MemcachedCache : ICache
    {
        #region CacheItem

        [Serializable]
        public abstract class CacheItem
        {
            public string Key { get; private set; }

            public object Data { get; private set; }

            public string GroupName { get; private set; }

            public CacheItem(string key, object data, string groupName)
            {
                Key = key;
                Data = data;
                GroupName = groupName;
            }

            public virtual DateTime GetLocalCacheExpiry(TimeSpan defaultLocalCacheExpiry)
            {
                return DateTime.Now.Add(defaultLocalCacheExpiry);
            }
        }

        [Serializable]
        public class NeverCacheItem : CacheItem
        {
            public NeverCacheItem(string key, object data, string groupName)
                : base(key, data, groupName)
            {

            }
        }

        [Serializable]
        public class SlidingCacheItem : CacheItem
        {
            public TimeSpan SlidingExpiration { get; private set; }

            public SlidingCacheItem(string key, object data, string groupName, TimeSpan slidingExpiration)
                : base(key, data, groupName)
            {
                SlidingExpiration = slidingExpiration;
            }

            public override DateTime GetLocalCacheExpiry(TimeSpan defaultLocalCacheExpiry)
            {
                TimeSpan x = defaultLocalCacheExpiry < SlidingExpiration ? defaultLocalCacheExpiry : SlidingExpiration;
                return DateTime.Now.Add(x);
            }
        }

        [Serializable]
        public class AbsoluteCacheItem : CacheItem
        {
            public DateTime AbsoluteExpiration { get; private set; }

            public AbsoluteCacheItem(string key, object data, string groupName, DateTime absoluteExpiration)
                : base(key, data, groupName)
            {
                AbsoluteExpiration = absoluteExpiration;
            }

            public override DateTime GetLocalCacheExpiry(TimeSpan defaultLocalCacheExpiry)
            {
                DateTime x = DateTime.Now.Add(defaultLocalCacheExpiry);
                x = x < AbsoluteExpiration ? x : AbsoluteExpiration;
                return x;
            }
        }

        #endregion

        private class DistributedKeyStorage
        {
            private MemcachedClient m_Cacher;
            private const char FLAG = ';';

            public DistributedKeyStorage(MemcachedClient cacher)
            {
                m_Cacher = cacher;
            }

            private string GenerateGroupCacheKey(string groupName)
            {
                return "g:" + groupName;
            }

            public void Add(string groupName, string cacheKey)
            {
                groupName = GenerateGroupCacheKey(groupName);
                bool sucess = m_Cacher.Append(groupName, cacheKey + FLAG);
                if (!sucess)
                {
                    m_Cacher.Add(groupName, cacheKey + FLAG, TimeSpan.FromDays(30));
                }
            }

            public void RemoveByGroup(string groupName)
            {
                m_Cacher.Delete(GenerateGroupCacheKey(groupName));
            }

            public List<string> GetKeys(string groupName)
            {
                object cachedData = m_Cacher.Get(GenerateGroupCacheKey(groupName));
                if (cachedData != null)
                {
                    string s = cachedData as string;
                    if (s != null)
                    {
                        string[] list = s.Split(new char[] { FLAG }, StringSplitOptions.RemoveEmptyEntries);
                        List<string> result = new List<string>(list.Length);
                        for (int i = 0; i < list.Length; i++)
                        {
                            if (!result.Contains(list[i]) && list[i] != null && list[i].Trim().Length > 0)
                            {
                                result.Add(list[i].Trim());
                            }
                        }
                        return result;
                    }
                }
                return new List<string>(0);
            }
        }

        private const string PARTITION_NAME = "ECCentral_Service_{0}";
        private MemcachedClient m_CacheManager;
        private DistributedKeyStorage m_DistributedKeyStorage;

        private List<SlidingCacheItem> m_ReActiveCacheTask = new List<SlidingCacheItem>();
        private object m_SyncObj = new object();
        private Timer m_Timer;

        #region Local Cache

        private TimeSpan m_LocalCacheExpiry = new TimeSpan(0, 1, 0); // default 1 min
        private ICache m_LocalCache = null;

        private void FlushLocalCache()
        {
            if (m_LocalCache == null)
            {
                return;
            }
            m_LocalCache.RemoveByGroup(m_CacheManager.Name);
        }

        private string GetLocalCacheKey(string key)
        {
            return "l:" + key;
        }

        private void SetLocalCache(CacheItem data)
        {
            if (m_LocalCache == null)
            {
                return;
            }
            m_LocalCache.Set(GetLocalCacheKey(data.Key), data, data.GetLocalCacheExpiry(m_LocalCacheExpiry), m_CacheManager.Name);
        }

        private CacheItem GetFromLocalCache(string key)
        {
            if (m_LocalCache == null)
            {
                return null;
            }
            return m_LocalCache.Get(GetLocalCacheKey(key)) as CacheItem;
        }

        private CacheItem[] GetFromLocalCache(string[] keys, out List<int> notFoundKeyIndexList, out List<string> notFoundKeys)
        {
            CacheItem[] list = new CacheItem[keys.Length];
            notFoundKeyIndexList = new List<int>(keys.Length);
            notFoundKeys = new List<string>(keys.Length);
            if (m_LocalCache == null)
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    notFoundKeyIndexList.Add(i);
                    notFoundKeys.Add(keys[i]);
                }
                return list;
            }
            for (int i = 0; i < keys.Length; i++)
            {
                CacheItem tmp = GetFromLocalCache(keys[i]);
                if (tmp == null)
                {
                    notFoundKeyIndexList.Add(i);
                    notFoundKeys.Add(keys[i]);
                }
                list[i] = tmp;
            }
            return list;
        }

        private void RemoveLocalCache(string key)
        {
            if (m_LocalCache == null)
            {
                return;
            }
            m_LocalCache.Remove(GetLocalCacheKey(key));
        }

        #endregion

        internal MemcachedClient CacheManager
        {
            get
            {
                return m_CacheManager;
            }
        }

        private void RealHitSlidingCacheItem(object state)
        {
            List<SlidingCacheItem> taskList = null;
            if (m_ReActiveCacheTask.Count > 0)
            {
                lock (m_SyncObj)
                {
                    if (m_ReActiveCacheTask.Count > 0)
                    {
                        taskList = new List<SlidingCacheItem>(m_ReActiveCacheTask);
                        m_ReActiveCacheTask.Clear();
                    }
                }
            }
            if (taskList != null)
            {
                foreach (var task in taskList)
                {
                    m_CacheManager.Replace(task.Key, task.Data, task.SlidingExpiration);
                }
            }
        }

        private void HitSlidingCacheItem(SlidingCacheItem item)
        {
            if (item == null)
            {
                return;
            }
            if (!m_ReActiveCacheTask.Exists(t => t.Key == item.Key))
            {
                lock (m_SyncObj)
                {
                    if (!m_ReActiveCacheTask.Exists(t => t.Key == item.Key))
                    {
                        m_ReActiveCacheTask.Add(item);
                    }
                }
            }
        }

        private bool Set(CacheItem item)
        {
            if (item == null || item.Data == null)
            {
                return false;
            }
            if (item.GroupName != null)
            {
                m_DistributedKeyStorage.Add(item.GroupName, item.Key);
            }
            bool rst = false;
            if (item is NeverCacheItem)
            {
                rst = m_CacheManager.Set(item.Key, (NeverCacheItem)item);
            }
            else if (item is AbsoluteCacheItem)
            {
                rst = m_CacheManager.Set(item.Key, (AbsoluteCacheItem)item, ((AbsoluteCacheItem)item).AbsoluteExpiration);
            }
            else if (item is SlidingCacheItem)
            {
                if (!m_ReActiveCacheTask.Exists(t => t.Key == item.Key))
                {
                    lock (m_SyncObj)
                    {
                        int index = m_ReActiveCacheTask.FindIndex(t => t.Key == item.Key);
                        if (index >= 0 && index < m_ReActiveCacheTask.Count)
                        {
                            m_ReActiveCacheTask.RemoveAt(index);
                        }
                    }
                }
                rst = m_CacheManager.Set(item.Key, (SlidingCacheItem)item, ((SlidingCacheItem)item).SlidingExpiration);
            }
            if (rst)
            {
                SetLocalCache(item);
            }
            return rst;
        }

        public void InitFromConfig(string cacheName, NameValueCollection parameters)
        {
            if (!parameters.AllKeys.Contains("serverList") || parameters["serverList"] == null || parameters["serverList"].Trim().Length <= 0)
            {
                throw new ConfigurationErrorsException("Please config the 'serverList' in parameters for cache type '" + typeof(MemcachedCache).AssemblyQualifiedName + "'");
            }
            string pname = string.Format(PARTITION_NAME, cacheName);
            if (!MemcachedClient.Exists(pname))
            {
                MemcachedClient.Setup(pname, parameters["serverList"].Trim().Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries));
            }

            m_CacheManager = MemcachedClient.GetInstance(pname);
            m_DistributedKeyStorage = new DistributedKeyStorage(m_CacheManager);

            if (parameters.AllKeys.Contains("maxPoolSize") && parameters["maxPoolSize"] != null && parameters["maxPoolSize"].Trim().Length > 0)
            {
                m_CacheManager.MaxPoolSize = Convert.ToUInt32(parameters["maxPoolSize"].Trim());
            }
            if (parameters.AllKeys.Contains("minPoolSize") && parameters["minPoolSize"] != null && parameters["minPoolSize"].Trim().Length > 0)
            {
                m_CacheManager.MinPoolSize = Convert.ToUInt32(parameters["minPoolSize"].Trim());
            }
            if (parameters.AllKeys.Contains("sendReceiveTimeout") && parameters["sendReceiveTimeout"] != null && parameters["sendReceiveTimeout"].Trim().Length > 0)
            {
                m_CacheManager.SendReceiveTimeout = (int)(TimeSpan.Parse(parameters["sendReceiveTimeout"].Trim()).TotalMilliseconds);
            }
            if (parameters.AllKeys.Contains("socketRecycleAge") && parameters["socketRecycleAge"] != null && parameters["socketRecycleAge"].Trim().Length > 0)
            {
                m_CacheManager.SocketRecycleAge = TimeSpan.Parse(parameters["socketRecycleAge"].Trim());
            }
            if (parameters.AllKeys.Contains("keyPrefix") && parameters["keyPrefix"] != null && parameters["keyPrefix"].Trim().Length > 0)
            {
                m_CacheManager.KeyPrefix = parameters["keyPrefix"].Trim();
            }
            if (parameters.AllKeys.Contains("connectTimeout") && parameters["connectTimeout"] != null && parameters["connectTimeout"].Trim().Length > 0)
            {
                m_CacheManager.ConnectTimeout = (int)(TimeSpan.Parse(parameters["connectTimeout"].Trim()).TotalMilliseconds);
            }
            if (parameters.AllKeys.Contains("compressionThreshold") && parameters["compressionThreshold"] != null && parameters["compressionThreshold"].Trim().Length > 0)
            {
                m_CacheManager.CompressionThreshold = Convert.ToUInt32(parameters["compressionThreshold"].Trim());
            }
            if (parameters.AllKeys.Contains("localCacheExpiry") && parameters["localCacheExpiry"] != null && parameters["localCacheExpiry"].Trim().Length > 0)
            {
                m_LocalCacheExpiry = TimeSpan.Parse(parameters["localCacheExpiry"].Trim());
            }
            if (parameters.AllKeys.Contains("localCacheName") && parameters["localCacheName"] != null && parameters["localCacheName"].Trim().Length > 0)
            {
                m_LocalCache = CacheFactory.GetInstance(parameters["localCacheName"].Trim());
            }
            m_Timer = new Timer(new TimerCallback(RealHitSlidingCacheItem), null, 1000, 1000);
        }

        public bool Set(string key, object value, string groupName = null)
        {
            return Set(new NeverCacheItem(key, value, groupName));
        }

        public bool Set(string key, object value, TimeSpan slidingExpiration, string groupName = null)
        {
            return Set(new SlidingCacheItem(key, value, groupName, slidingExpiration));
        }

        public bool Set(string key, object value, DateTime absoluteExpiration, string groupName = null)
        {
            return Set(new AbsoluteCacheItem(key, value, groupName, absoluteExpiration));
        }

        public object Get(string key)
        {
            CacheItem item = GetFromLocalCache(key);
            bool IsHitLocalCache = true;
            if (item == null)
            {
                IsHitLocalCache = false;
                item = m_CacheManager.Get(key) as CacheItem;
            }
            if (item != null)
            {
                HitSlidingCacheItem(item as SlidingCacheItem);
                if (IsHitLocalCache == false)
                {
                    SetLocalCache(item);
                }
                return item.Data;
            }
            return null;
        }

        public T Get<T>(string key)
        {
            object cache = Get(key);
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
            List<int> indexArray;
            List<string> emptyKeys;
            CacheItem[] itemArray = GetFromLocalCache(keys, out indexArray, out emptyKeys);
            if (emptyKeys.Count > 0)
            {
                object[] cachedData = m_CacheManager.Get(emptyKeys.ToArray());
                for (int i = 0; i < cachedData.Length; i++)
                {
                    int index = indexArray[i];
                    CacheItem tmp = cachedData[i] as CacheItem;
                    itemArray[index] = tmp;
                    if (tmp != null)
                    {
                        SetLocalCache(tmp);
                    }
                }
            }

            List<object> result = new List<object>(itemArray.Length);
            for (int i = 0; i < itemArray.Length; i++)
            {
                if (itemArray[i] == null)
                {
                    result.Add(null);
                }
                else
                {
                    HitSlidingCacheItem(itemArray[i] as SlidingCacheItem);
                    result.Add(itemArray[i].Data);
                }
            }
            return result;
        }

        public List<T> Get<T>(string[] keys)
        {
            List<object> list = Get(keys);
            List<T> rst = new List<T>(list.Count);
            list.ForEach(o => rst.Add((T)o));
            return rst;
        }

        public List<object> GetByGroup(string groupName)
        {
            List<string> keys = m_DistributedKeyStorage.GetKeys(groupName);
            return Get(keys.ToArray());
        }

        public List<T> GetByGroup<T>(string groupName)
        {
            List<string> keys = m_DistributedKeyStorage.GetKeys(groupName);
            return Get<T>(keys.ToArray());
        }

        public bool Remove(string key)
        {
            RemoveLocalCache(key);
            return m_CacheManager.Delete(key);
        }

        public bool RemoveByGroup(string groupName)
        {
            bool result = true;
            List<string> keys = m_DistributedKeyStorage.GetKeys(groupName);
            if (keys != null && keys.Count > 0)
            {
                foreach (string key in keys)
                {
                    bool tmpResult = this.Remove(key);
                    if (tmpResult == false)
                    {
                        result = false;
                    }
                }
            }
            if (result)
            {
                m_DistributedKeyStorage.RemoveByGroup(groupName);
            }
            return result;
        }

        public void FlushAll()
        {
            FlushLocalCache();
            m_CacheManager.FlushAll();
        }

        public List<string> GetKeysByGroup(string groupName)
        {
            return m_DistributedKeyStorage.GetKeys(groupName);
        }
    }
}
