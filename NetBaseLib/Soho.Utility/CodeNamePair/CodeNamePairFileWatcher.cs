using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Soho.Utility
{
    public class CodeNamePairFileWatcherAutorun : IStartup, IShutdown
    {
        public void Start()
        {
            CodeNamePairFileWatcher.BeginWatch();
        }

        public void Shut()
        {
            CodeNamePairFileWatcher.EndWatch();
        }
    }

    public static class CodeNamePairFileWatcher
    {
        private static FileSystemWatcher m_Watcher;

        public static void BeginWatch()
        {
            if (m_Watcher != null)
            {
                EndWatch();
            }
            var setting = CodeNamePairSection.GetSetting();
            if (setting == null)
            {
                return;
            }
            string path = setting.BaseAbsoluteFolder;
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
            CodeNamePairManager.ClearCache(name);
        }

        private static void FileRenamed(object sender, RenamedEventArgs target)
        {
            string name = Path.GetFileNameWithoutExtension(target.OldFullPath);
            CodeNamePairManager.ClearCache(name);
        }
    }
}
