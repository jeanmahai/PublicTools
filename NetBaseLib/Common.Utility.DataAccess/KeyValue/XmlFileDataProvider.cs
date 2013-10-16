using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Xml.Serialization;
using System.Configuration;

namespace Common.Utility.DataAccess.KeyValue
{
    public class XmlFileDataProvider : IKeyValueDataProvider
    {
        public T GetKeyValueData<T>(string dataCategory, string key) where T : class, new()
        {
            return XmlFileDataManager.GetKeyValueData<T>(dataCategory,key);
        }


        public List<T> GetKeyValueData<T>(string dataCategory) where T : class, new()
        {
            return XmlFileDataManager.GetKeyValueData<T>(dataCategory);
        }

        
    }
}
