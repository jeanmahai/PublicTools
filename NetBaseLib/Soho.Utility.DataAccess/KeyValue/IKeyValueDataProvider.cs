using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Soho.Utility.DataAccess.KeyValue
{
    public interface IKeyValueDataProvider
    {
        T GetKeyValueData<T>(string dataCategory, string key) where T : class,new();

        List<T> GetKeyValueData<T>(string dataCategory) where T : class,new(); 
    }
}
