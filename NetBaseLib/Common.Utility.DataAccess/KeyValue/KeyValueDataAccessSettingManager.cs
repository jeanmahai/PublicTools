using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using System.Xml.Linq;

namespace Common.Utility.DataAccess.KeyValue
{
    public static class KeyValueDataAccessSettingManager
    {
        private const string NODE_NAME = "KeyValueDataAccessSettingFolder";
        private const string DEFAULT_FOLDER = "Configuration";
        public const string CONFIG_FILE = "KeyValueDataAccessSetting.config";
        private static readonly string m_SettingFolderPath = GetBaseFolderPath();

        private static List<KeyValueDataAccessSetting> m_KeyValueDataAccessSettingList = null;

        private static object m_SyncObj = new object();

        public static string SettingFolderPath
        {
            get { return KeyValueDataAccessSettingManager.m_SettingFolderPath; }
        }

        private static string GetBaseFolderPath()
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


        public static List<KeyValueDataAccessSetting> GetAllSettings()
        {
            if (m_KeyValueDataAccessSettingList == null)
            {
                lock (m_SyncObj)
                {
                    string filePath = Path.Combine(m_SettingFolderPath, CONFIG_FILE);
                    if (!File.Exists(filePath))
                    {
                        throw new FileNotFoundException("没有找到文件: " + filePath + "!");
                    }

                    m_KeyValueDataAccessSettingList = new List<KeyValueDataAccessSetting>();

                    XElement doc = XElement.Load(filePath);
                    if (doc == null 
                        || doc.Descendants("KVDataAccessItem") == null 
                        || doc.Descendants("KVDataAccessItem").Count() == 0)
                    {
                        return m_KeyValueDataAccessSettingList;
                    }

                    foreach (var x in doc.Descendants("KVDataAccessItem"))
                    {
                        if (x.Attribute("DataCategory") == null || x.Attribute("DataCategory").Value.Trim() == string.Empty
                            || x.Attribute("AccessMode") == null || x.Attribute("AccessMode").Value.Trim() == string.Empty)
                        {
                            throw new ApplicationException("DataCategory和AccessMode两个属性必须存在并且有值！");
                        }

                        KeyValueDataAccessSetting setting = new KeyValueDataAccessSetting();
                        setting.DataCategory = x.Attribute("DataCategory").Value.Trim();
                        string mode=x.Attribute("AccessMode").Value.Trim();
                        setting.AccessMode = (KeyValueDataAccessMode)Enum.Parse(typeof(KeyValueDataAccessMode), mode, true);
                        setting.DataCategoryPath = x.Attribute("DataCategoryPath") == null ? null : x.Attribute("DataCategoryPath").Value.Trim();
                        m_KeyValueDataAccessSettingList.Add(setting);
                    }
                }
            }

            return m_KeyValueDataAccessSettingList;
        }

        internal static void ResetSetting()
        {
            m_KeyValueDataAccessSettingList = null;
            GetAllSettings();
        }
    }

    public class KeyValueDataAccessSettingFileWatcherAutorun : IStartup, IShutdown
    {

        public void Start()
        {
            KeyValueDataAccessSettingFileWatcher.BeginWatch();
        }

        public void Shut()
        {
            KeyValueDataAccessSettingFileWatcher.EndWatch();
        }
    }


    public static class KeyValueDataAccessSettingFileWatcher
    {
        

        private static FileSystemWatcher m_Watcher;

        public static void BeginWatch()
        {
            if (m_Watcher != null)
            {
                EndWatch();
            }

            string path = KeyValueDataAccessSettingManager.SettingFolderPath;

            if (!Directory.Exists(path))
            {
                return;
            }
            m_Watcher = new FileSystemWatcher(path, KeyValueDataAccessSettingManager.CONFIG_FILE);
            m_Watcher.IncludeSubdirectories = false;
            m_Watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size | NotifyFilters.FileName;
            m_Watcher.Changed += FileChanged;
            m_Watcher.Deleted += FileChanged; 
            m_Watcher.EnableRaisingEvents = true;
        }

        public static void EndWatch()
        {
            if (m_Watcher != null)
            {
                m_Watcher.EnableRaisingEvents = false;
                m_Watcher.Changed -= FileChanged;
                m_Watcher.Deleted -= FileChanged; 
                m_Watcher = null;
            }
        }

        private static void FileChanged(object sender, FileSystemEventArgs target)
        {
            KeyValueDataAccessSettingManager.ResetSetting();
        }

         
    }
}
