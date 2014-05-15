using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Configuration;
using System.Xml.Serialization;

namespace Soho.Utility.DataAccess.KeyValue
{
    internal static class XmlFileDataManager
    {
        private const string NODE_NAME = "XmlFileDataFolder";
        private const string DEFAULT_FOLDER = @"App_Data\XmlData";
        private static object m_SyncObj = new object();

        private static Dictionary<string, object> m_CacheData = new Dictionary<string, object>();

        public static T GetKeyValueData<T>(string dataCategory, string key) where T : class, new()
        {
            List<KeyValueDataAccessSetting> list = KeyValueDataAccessSettingManager.GetAllSettings();
            KeyValueDataAccessSetting setting = list.Find(f => f.DataCategory.ToUpper().Trim() == dataCategory.Trim().ToUpper());
            if (setting == null)
            {
                throw new NullReferenceException("没有找到" + dataCategory + "在KeyValueDataAccessSetting.config中的配置！");
            }

            if (!m_CacheData.ContainsKey(dataCategory.ToUpper().Trim()))
            {
                string xmlFileFullPath = Path.Combine(GetBaseFolderPath(), setting.DataCategoryPath, setting.DataCategory + ".xml");
                T t= DeserializeToObject<T>(xmlFileFullPath);
                m_CacheData.Add(dataCategory.ToUpper().Trim(), t);
                return t;
            }
            else
            {
                lock (m_SyncObj)
                {
                    return (T)m_CacheData[dataCategory.ToUpper().Trim()];
                }
            }
        }


        public static List<T> GetKeyValueData<T>(string dataCategory) where T : class, new()
        {
            List<KeyValueDataAccessSetting> list = KeyValueDataAccessSettingManager.GetAllSettings();
            KeyValueDataAccessSetting setting = list.Find(f => f.DataCategory.ToUpper().Trim() == dataCategory.Trim().ToUpper());
            if (setting == null)
            {
                throw new NullReferenceException("没有找到" + dataCategory + "在KeyValueDataAccessSetting.config中的配置！");
            }

            if (!m_CacheData.ContainsKey(dataCategory.ToUpper().Trim()) )
            {

                string xmlFileFullPath = Path.Combine(GetBaseFolderPath(), setting.DataCategoryPath, setting.DataCategory + ".xml");
                if (!File.Exists(xmlFileFullPath))
                {
                    return null;
                }
                DataTable dt = null;
                try
                {
                    DataSet ds = new DataSet();
                    XmlReadMode mode = ds.ReadXml(xmlFileFullPath, XmlReadMode.ReadSchema);
                    dt = ds.Tables[0];
                }
                catch (Exception)
                {
                    return null;
                }
                if (dt != null)
                {
                    List<T> tlist = BuildEntityListFromDataTable<T>(dt);
                    m_CacheData.Add(dataCategory.ToUpper().Trim(), tlist);
                    return tlist;
                }
                return null;
            }
            else
            {
                lock (m_SyncObj)
                {
                    return (List<T>)m_CacheData[dataCategory.ToUpper().Trim()];
                }
            }
        }

        public static string GetBaseFolderPath()
        {
            string path = ConfigurationManager.AppSettings[NODE_NAME];
            if (path == null || path.Trim().Length <= 0)
            {
                return Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, DEFAULT_FOLDER);
            }
            string p = Path.GetPathRoot(path);
            if (p == null || p.Trim().Length <= 0) // 说明是相对路径
            {
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
            }
            return path;
        }

        

        internal static void ClearCacheData(string dataCategory)
        {
            dataCategory = dataCategory.ToUpper().Trim();
            lock (m_SyncObj)
            {
                if (m_CacheData.ContainsKey(dataCategory))
                {
                    m_CacheData.Remove(dataCategory);
                }
            }

        }



        private static List<T> BuildEntityListFromDataTable<T>(DataTable dt) where T : class, new()
        {
            List<T> list = new List<T>();
            Type type = typeof(T);
            foreach (DataRow row in dt.Rows)
            {
                T t = new T();
                foreach (DataColumn conl in dt.Columns)
                {
                    System.Reflection.PropertyInfo pro = t.GetType().GetProperty(conl.ColumnName);
                    if (pro != null && pro.CanWrite && row[conl.ColumnName].GetType() != typeof(DBNull))
                    {
                        pro.SetValue(t, row[conl.ColumnName], null);
                    }
                }
                list.Add(t);
            }
            return list;
        }

        private static T DeserializeToObject<T>(string filename) where T : class, new()
        {
            T t = default(T);
            lock (m_SyncObj)
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open,FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    XmlSerializer formatter = new XmlSerializer(typeof(T));
                    t = (T)formatter.Deserialize(fs);
                }
            }
            return t;
        }
        
       
    }

    public class XmlFileDataWatcherAutorun : IStartup, IShutdown
    {

        public void Start()
        {
            XmlFileDataWatcher.BeginWatch();
        }

        public void Shut()
        {
            XmlFileDataWatcher.EndWatch();
        }
    }


    public class XmlFileDataWatcher
    {
        private static FileSystemWatcher m_Watcher;

        public static void BeginWatch()
        {
            if (m_Watcher != null)
            {
                EndWatch();
            }
            string folder = XmlFileDataManager.GetBaseFolderPath();
            if (!Directory.Exists(folder))
            {
                return;
            }
            m_Watcher = new FileSystemWatcher(folder, "*.xml");
            m_Watcher.IncludeSubdirectories = true;
            m_Watcher.NotifyFilter =  NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size | NotifyFilters.FileName;
            m_Watcher.Changed += new FileSystemEventHandler(m_Watcher_Changed);
            m_Watcher.Deleted += new FileSystemEventHandler(m_Watcher_Deleted);
            m_Watcher.Renamed += new RenamedEventHandler(m_Watcher_Renamed);
            m_Watcher.EnableRaisingEvents = true;
        }

        static void m_Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            string name = Path.GetFileNameWithoutExtension(e.OldFullPath);
            XmlFileDataManager.ClearCacheData(name);
        }

        static void m_Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            string name = Path.GetFileNameWithoutExtension(e.FullPath);
            XmlFileDataManager.ClearCacheData(name);
        }

        static void m_Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            string name = Path.GetFileNameWithoutExtension(e.FullPath);
            XmlFileDataManager.ClearCacheData(name);
        }

         

        public static void EndWatch()
        {
            if (m_Watcher != null)
            {
                m_Watcher.EnableRaisingEvents = false;
                m_Watcher.Changed -= m_Watcher_Changed;
                m_Watcher.Deleted -= m_Watcher_Deleted;
                m_Watcher.Renamed -= m_Watcher_Renamed;
                m_Watcher = null;
            }
        }

         
    }


}
