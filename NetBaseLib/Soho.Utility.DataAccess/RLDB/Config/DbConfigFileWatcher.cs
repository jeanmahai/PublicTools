using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Soho.Utility.DataAccess.Database.DbProvider;

namespace Soho.Utility.DataAccess.Database.Config
{
    public static class DbConfigFileWatcher
    {
        #region ConnStrFile Watcher

        private static FileSystemWatcher s_ConnStrFile_Watcher;

        private static Timer s_ConnStrFile_Timer = new Timer(new TimerCallback(ConnStrFileChanged_TimerChanged), null, Timeout.Infinite, Timeout.Infinite);
        private const int ConnStrFile_TimeoutMillis = 500; // 0.5 second

        public static void BeginConnStrFileWatch()
        {
            if (s_ConnStrFile_Watcher != null)
            {
                EndConnStrFileWatch();
            }
            string path = ConfigHelper.DatabaseListFilePath;
            if (!File.Exists(path))
            {
                return;
            }
            s_ConnStrFile_Watcher = new FileSystemWatcher(Path.GetDirectoryName(path), Path.GetFileName(path));
            s_ConnStrFile_Watcher.IncludeSubdirectories = false;
            s_ConnStrFile_Watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size;
            s_ConnStrFile_Watcher.Changed += ConnStrFileChanged;
            s_ConnStrFile_Watcher.EnableRaisingEvents = true;
        }

        public static void EndConnStrFileWatch()
        {
            if (s_ConnStrFile_Watcher != null)
            {
                s_ConnStrFile_Watcher.EnableRaisingEvents = false;
                s_ConnStrFile_Watcher.Changed -= ConnStrFileChanged;
                s_ConnStrFile_Watcher = null;
            }
        }

        private static void ConnStrFileChanged(object sender, FileSystemEventArgs target)
        {
            s_ConnStrFile_Timer.Change(ConnStrFile_TimeoutMillis, Timeout.Infinite);
        }

        private static void ConnStrFileChanged_TimerChanged(object state)
        {
            ConnectionStringManager.ClearAllConnectionString();
        }

        #endregion

        #region SqlFile Watcher

        private static FileSystemWatcher s_SqlFile_Watcher;

        private static Timer s_Timer = new Timer(new TimerCallback(FileChanged_TimerChanged), null, Timeout.Infinite, Timeout.Infinite);
        private const int TimeoutMillis = 500; // 0.5 second
        private static string s_ConnStrFileFolder = new DirectoryInfo(Path.GetDirectoryName(ConfigHelper.DatabaseListFilePath)).FullName.ToUpper();
        private static string s_ConnStrFileName = Path.GetFileName(ConfigHelper.DatabaseListFilePath).ToUpper();

        public static void BeginSqlFileWatch()
        {
            if (s_SqlFile_Watcher != null)
            {
                EndSqlFileWatch();
            }
            string folder = ConfigHelper.ConfigFolder;
            if (!Directory.Exists(folder))
            {
                return;
            }
            s_ConnStrFile_Watcher = new FileSystemWatcher(folder);
            s_ConnStrFile_Watcher.IncludeSubdirectories = true;
            s_ConnStrFile_Watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size | NotifyFilters.FileName;
            s_ConnStrFile_Watcher.Changed += SqlFileChanged;
            s_ConnStrFile_Watcher.Renamed += SqlFileRenamed;
            //s_ConnStrFile_Watcher.Created += SqlFileCreated;
            s_ConnStrFile_Watcher.EnableRaisingEvents = true;
        }

        public static void EndSqlFileWatch()
        {
            if (s_SqlFile_Watcher != null)
            {
                s_SqlFile_Watcher.EnableRaisingEvents = false;
                s_SqlFile_Watcher.Changed -= SqlFileChanged;
                s_ConnStrFile_Watcher.Renamed -= SqlFileRenamed;
                //s_ConnStrFile_Watcher.Created -= SqlFileCreated;
                s_SqlFile_Watcher = null;
            }
        }

        private static bool IsConnStrFile(string filePath)
        {
            string tFolder = new DirectoryInfo(Path.GetDirectoryName(filePath)).FullName.ToUpper();
            string tFile = Path.GetFileName(filePath).ToUpper();
            return (tFolder == s_ConnStrFileFolder && tFile == s_ConnStrFileName);
        }

        private static void SqlFileChanged(object sender, FileSystemEventArgs e)
        {
            if (IsConnStrFile(e.FullPath)) // 说明是Connection String配置文件，会有另外一个FileSystemWatcher来处理
            {
                return;
            }
            s_Timer.Change(TimeoutMillis, Timeout.Infinite);
        }

        private static void SqlFileRenamed(object sender, RenamedEventArgs e)
        {
            if (IsConnStrFile(e.FullPath) || IsConnStrFile(e.OldFullPath)) // 说明是Connection String配置文件，会有另外一个FileSystemWatcher来处理
            {
                return;
            }
            s_Timer.Change(TimeoutMillis, Timeout.Infinite);
        }

        //private static void SqlFileCreated(object sender, FileSystemEventArgs e)
        //{
        //    if (IsConnStrFile(e.FullPath)) // 说明是Connection String配置文件，会有另外一个FileSystemWatcher来处理
        //    {
        //        return;
        //    }
        //    s_Timer.Change(TimeoutMillis, Timeout.Infinite);
        //}

        private static void FileChanged_TimerChanged(object state)
        {
            DataCommandManager.ClearAllDataCommandFiles();
        }

        #endregion
    }

    public class DbConfigFileWatcherAutorun : IStartup, IShutdown
    {
        public void Start()
        {
            DbConfigFileWatcher.BeginConnStrFileWatch();
            DbConfigFileWatcher.BeginSqlFileWatch();
        }

        public void Shut()
        {
            DbConfigFileWatcher.EndSqlFileWatch();
            DbConfigFileWatcher.EndConnStrFileWatch();
        }
    }
}
