using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;

namespace Soho.Utility.DataAccess.KeyValue
{
    /// <summary>
    /// KeyValueData模式获取数据的管理器
    /// </summary>
    public static class KeyValueDataManager
    {
        /// <summary>
        /// 根据数据类别，数据主键获取数据
        /// XMLFileData模式：将dataCategory（对应xml的文件名）中的数据取出，反序列化为T类型的数据实体
        /// SQLDA/MonoDB模式：取出dataCategory下，主键值等于key的数据，反序列化为T类型的数据实体
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="dataCategory">数据类别</param>
        /// <param name="key">数据主键,如果是XMLFile模式，key不起任何作用</param>
        /// <returns>返回的数据，KeyValueData中的Value</returns>
        public static T GetKeyValueData<T>(string dataCategory, string key) where T : class,new()
        {
            List<KeyValueDataAccessSetting> settingList = KeyValueDataAccessSettingManager.GetAllSettings();
            KeyValueDataAccessSetting setting = settingList.Find(f => f.DataCategory.ToUpper().Trim() == dataCategory.ToUpper().Trim());
            if (setting == null)
            {
                return null;
            }
            IKeyValueDataProvider dataProvider = GetProvider(setting.AccessMode);
            T t = dataProvider.GetKeyValueData<T>(dataCategory, key);
            return t;
        }


        /// <summary>
        /// 本方法只适用于XMLMaker制造的XMFileData模式，根据dataCategory（对应xml的文件名）取出xml文件中的数据，转换为类型为T的数据列表
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="dataCategory">数据类别</param>
        /// <returns>返回的数据列表</returns>
        public static List<T> GetKeyValueData<T>(string dataCategory) where T : class,new()
        {
            List<KeyValueDataAccessSetting> settingList = KeyValueDataAccessSettingManager.GetAllSettings();
            KeyValueDataAccessSetting setting = settingList.Find(f => f.DataCategory.ToUpper().Trim() == dataCategory.ToUpper().Trim());
            if (setting == null)
            {
                return null;
            }
            IKeyValueDataProvider dataProvider = GetProvider(setting.AccessMode);
            List<T> list = dataProvider.GetKeyValueData<T>(dataCategory);
            return list;
        }

        private static IKeyValueDataProvider GetProvider(KeyValueDataAccessMode mode)
        {
            switch (mode)
            {
                case KeyValueDataAccessMode.XMLFile:
                    return new XmlFileDataProvider();
                case KeyValueDataAccessMode.SQLDB:
                    return new SQLDBProvider();
                case KeyValueDataAccessMode.MongoDB:
                    return new MongoDBProvider();
                default:
                    return new XmlFileDataProvider();
            }
        }

    }
    
    
}
