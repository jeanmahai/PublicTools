using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using System.Threading;

namespace Common.Utility
{
    public static class AppSettingManager
    {
        private const string NODE_NAME = "AppSettingsPath";
        private static readonly string s_BaseFolderPath = GetBaseFolderPath();
        private static Dictionary<string, Dictionary<string, string>> s_Cache = new Dictionary<string, Dictionary<string, string>>();
        private static object s_SyncObj = new object();

        public static string BaseFolderPath
        {
            get { return AppSettingManager.s_BaseFolderPath; }
        }

        private static string GetBaseFolderPath()
        {
            string path = ConfigurationManager.AppSettings[NODE_NAME];
            if (path == null || path.Trim().Length <= 0)
            {
                return Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Configuration/AppSettings");
            }
            string p = Path.GetPathRoot(path);
            if (p == null || p.Trim().Length <= 0) // 说明是相对路径
            {
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
            }
            return path;
        }

        private static Dictionary<string, string> LoadSettings(string domainName)
        {
            string filePath = Path.Combine(s_BaseFolderPath, domainName + ".config");
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Can't find the AppSettings config file: " + filePath + "!");
            }
            domainName = domainName.ToUpper();
            Dictionary<string, string> rst = new Dictionary<string, string>();
            XElement doc = XElement.Load(filePath);
            foreach (var x in doc.Descendants("add"))
            {
                if (x.Attribute("key") == null || x.Attribute("key").Value == null || x.Attribute("key").Value.Trim().Length <= 0)
                {
                    throw new ApplicationException("There are some 'add' node without attribute 'key' in the AppSettings config file: " + filePath + ", please check it!");
                }
                string key = x.Attribute("key").Value.Trim().ToUpper();
                if (rst.ContainsKey(key))
                {
                    throw new ApplicationException("Duplicated value '" + x.Attribute("key").Value.Trim() + "' of attribute 'key' in 'add' node in the AppSettings config file: " + filePath + ", please check it (ex: ignore case)!");
                }
                string value = x.Attribute("value") == null ? null : (x.Attribute("value").Value == null ? null : x.Attribute("value").Value.Trim());
                value = value ?? x.Value;
                rst.Add(key, EnvironmentVariableManager.ReplaceVariable(value));
            }
            return rst;
        }

        public static string GetSetting(string domainName, string key)
        {
            key = key.ToUpper();
            Dictionary<string, string> data = GetAllSettings(domainName);
            if (data.ContainsKey(key))
            {
                return data[key];
            }
            return null;
        }

        public static Dictionary<string, string> GetAllSettings(string domainName)
        {
            domainName = domainName.ToUpper();
            Dictionary<string, string> data;
            if (!s_Cache.TryGetValue(domainName, out data))
            {
                lock (s_SyncObj)
                {
                    if (!s_Cache.TryGetValue(domainName, out data))
                    {
                        data = LoadSettings(domainName);
                        s_Cache.Add(domainName, data);
                    }
                }
            }
            return new Dictionary<string,string>(data);
        }

        internal static void ClearAllSettings(string domainName)
        {
            domainName = domainName.ToUpper();
            if (s_Cache.ContainsKey(domainName))
            {
                lock (s_SyncObj)
                {
                    if (s_Cache.ContainsKey(domainName))
                    {
                        s_Cache.Remove(domainName);
                    }
                }
            }
        }
    }

    public class AppSettingQueryFilter
    {
        public string DomainName
        {
            get;
            set;
        }

        public string Key
        {
            get;
            set;
        }
    }

    public class AppSettingFileWatcherAutorun : IStartup, IShutdown
    {
        public void Start()
        {
            AppSettingFileWatcher.BeginWatch();
        }

        public void Shut()
        {
            AppSettingFileWatcher.EndWatch();
        }
    }

    public static class AppSettingFileWatcher
    {
        private static FileSystemWatcher m_Watcher;

        public static void BeginWatch()
        {
            if (m_Watcher != null)
            {
                EndWatch();
            }
            string path = AppSettingManager.BaseFolderPath;
            if (!File.Exists(path))
            {
                return;
            }
            m_Watcher = new FileSystemWatcher(path, "*.config");
            m_Watcher.IncludeSubdirectories = false;
            m_Watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size | NotifyFilters.FileName;
            m_Watcher.Changed += FileChanged;
            m_Watcher.Deleted += FileChanged;
            m_Watcher.Renamed += FileRenamed;
            m_Watcher.EnableRaisingEvents = true;
        }

        public static void EndWatch()
        {
            if (m_Watcher != null)
            {
                m_Watcher.EnableRaisingEvents = false;
                m_Watcher.Changed -= FileChanged;
                m_Watcher.Deleted -= FileChanged;
                m_Watcher.Renamed -= FileRenamed;
                m_Watcher = null;
            }
        }

        private static void FileChanged(object sender, FileSystemEventArgs target)
        {
            string name = Path.GetFileNameWithoutExtension(target.FullPath);
            AppSettingManager.ClearAllSettings(name);
        }

        private static void FileRenamed(object sender, RenamedEventArgs target)
        {
            string name = Path.GetFileNameWithoutExtension(target.OldFullPath);
            AppSettingManager.ClearAllSettings(name);
        }
    }
}
