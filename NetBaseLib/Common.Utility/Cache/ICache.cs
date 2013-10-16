using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace Common.Utility
{
    public interface ICache
    {
        void InitFromConfig(string cacheName, NameValueCollection parameters);

        bool Set(string key, object value, string groupName = null);

        bool Set(string key, object value, TimeSpan slidingExpiration, string groupName = null);

        bool Set(string key, object value, DateTime absoluteExpiration, string groupName = null);

        object Get(string key);

        T Get<T>(string key);

        List<object> Get(string[] keys);

        List<T> Get<T>(string[] keys);

        List<object> GetByGroup(string groupName);

        List<T> GetByGroup<T>(string groupName);

        bool Remove(string key);

        bool RemoveByGroup(string groupName);

        void FlushAll();

        List<string> GetKeysByGroup(string groupName);
    }
}
