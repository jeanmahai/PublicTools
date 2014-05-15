using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;

namespace Soho.Utility
{
    public static class CacheStatisticManager
    {
        private static Dictionary<string, CacheStatistic> s_CacheStatistic = new Dictionary<string, CacheStatistic>();
        private static object s_SyncObj = new object();

        private static string GetKey(MethodBase method)
        {
            if (method.DeclaringType != null)
            {
                return method.DeclaringType.FullName + "." + method.Name;
            }
            return method.Name;
        }

        private static void Execute(MethodBase method, bool isHit, string cacheName, string group)
        {
            string methodName = GetKey(method);
            CacheStatistic statistic = null;
            bool lockWasSuccessful = false;
            do
            {
                lock (s_SyncObj)
                {
                    if (!s_CacheStatistic.ContainsKey(methodName))
                    {
                        statistic = new CacheStatistic(cacheName, group);
                        s_CacheStatistic.Add(methodName, statistic);
                    }
                    else
                    {
                        statistic = s_CacheStatistic[methodName];
                    }
                    lockWasSuccessful = Monitor.TryEnter(statistic);
                }
                if (lockWasSuccessful == false)
                {
                    Thread.Sleep(0);
                }
            }
            while (lockWasSuccessful == false);
            try
            {
                if (isHit)
                {
                    statistic.Hit();
                }
                else
                {
                    statistic.NotHit();
                }
            }
            finally
            {
                Monitor.Exit(statistic);
            }
        }

        public static void Hit(MethodBase method, string cacheName, string group)
        {
            if (cacheName == null)
            {
                cacheName = CacheSection.GetSetting().DefaultCacheName;
            }
            Execute(method, true, cacheName, group);
        }

        public static void NotHit(MethodBase method, string cacheName, string group)
        {
            if (cacheName == null)
            {
                cacheName = CacheSection.GetSetting().DefaultCacheName;
            }
            Execute(method, false, cacheName, group);
        }

        public static CacheStatistic GetStatisticByMethod(string methodName)
        {
            if (s_CacheStatistic.ContainsKey(methodName))
            {
                return s_CacheStatistic[methodName];
            }
            return null;
        }

        public static Dictionary<string, CacheStatistic> GetAllStatistic()
        {
            return new Dictionary<string, CacheStatistic>(s_CacheStatistic);
        }
    }

    public class CacheStatistic
    {
        public int HitCount
        {
            get;
            set;
        }

        public int TotalExecuteTimes
        {
            get;
            set;
        }

        public string CacheName
        {
            get;
            set;
        }

        public string GroupName
        {
            get;
            set;
        }

        public CacheStatistic(string cacheName, string group)
        {
            this.CacheName = cacheName;
            this.GroupName = group;
        }

        public void Hit()
        {
            HitCount = HitCount + 1;
            TotalExecuteTimes = TotalExecuteTimes + 1;
        }

        public void NotHit()
        {
            TotalExecuteTimes = TotalExecuteTimes + 1;
        }

        public double GetHitRate()
        {
            if (TotalExecuteTimes <= 0)
            {
                return -1;
            }
            return (double)HitCount / (double)TotalExecuteTimes;
        }
    }
}
