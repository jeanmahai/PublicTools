using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Configuration;

namespace Common.Utility
{
    public static class ResouceFileWatcher
    {
        private static FileSystemWatcher s_Watcher;

        private static Timer s_Timer = new Timer(new TimerCallback(FileChanged_TimerChanged), null, Timeout.Infinite, Timeout.Infinite);
        private const int TimeoutMillis = 500; // 0.5 second
        private static List<string> s_LoadFileList = new List<string>();
        private static List<string> s_RemoveFileList = new List<string>();
        private static object s_SyncLoadObj = new object();
        private static object s_SyncRemoveObj = new object();

        private static string s_ResouceBaseFolder;

        public static void BeginWatch()
        {
            string path = ConfigurationManager.AppSettings["MessageResourcesPath"];
            if (path == null || path.Trim().Length <= 0) // 没有配置，使用默认路径
            {
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Configuration/MessageResources");
            }
            else if (path.IndexOf(':') < 0) // 配置的不是绝对路径
            {
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path.TrimStart('~', '/', '\\'));
            }
            BeginWatch(path);
        }

        public static void BeginWatch(string folderPath)
        {
            BeginWatch(folderPath, "*.xml");
        }

        public static void BeginWatch(string folderPath, string filter)
        {
            if (s_Watcher != null)
            {
                DisposeWatcher(s_Watcher);
            }
            folderPath = new DirectoryInfo(folderPath).FullName;
            s_ResouceBaseFolder = folderPath.ToUpper();
            s_Watcher = new FileSystemWatcher();

            s_Watcher.Deleted += new FileSystemEventHandler(Watcher_Deleted);
            s_Watcher.Renamed += new RenamedEventHandler(Watcher_Renamed);
            s_Watcher.Changed += new FileSystemEventHandler(Watcher_Changed);
            s_Watcher.Created += new FileSystemEventHandler(Watcher_Created);

            //监控路径（文件夹）
            s_Watcher.Path = folderPath;
            //是否包含子目录
            s_Watcher.IncludeSubdirectories = true;
            //如果filter为文件名称则表示监控该文件，如果为*.txt则表示要监控指定目录当中的所有.txt文件,如果为*.*表示所有的文件
            s_Watcher.Filter = filter;
            s_Watcher.NotifyFilter = NotifyFilters.LastWrite 
                                            | NotifyFilters.FileName 
                                            | NotifyFilters.Size;

            //开始初始化所有需要监控的文件
            InitAllMessages(folderPath, filter);

            //begin watching.
            s_Watcher.EnableRaisingEvents = true;
        }

        private static void InitAllMessages(string path, string filter)
        {
            string[] files = Directory.GetFiles(path, filter, SearchOption.AllDirectories);
            ResouceManager.ClearResouceCache();
            foreach (string file in files)
            {
                ResouceManager.LoadResouce(GetResouceCacheKey(file), file);
            }
        }

        private static string GetResouceCacheKey(string filePath)
        {
            return new FileInfo(filePath.ToUpper()).FullName.Replace(s_ResouceBaseFolder, string.Empty).Replace('\\', '.').Trim('.');
        }

        private static void FileChanged_TimerChanged(object state)
        {
            // 处理待加载入内存的资源文件列表
            if (s_LoadFileList.Count > 0)
            {
                lock (s_SyncLoadObj)
                {
                    if (s_LoadFileList.Count > 0)
                    {
                        List<string> tmp = new List<string>(s_LoadFileList);
                        foreach (var filePath in tmp)
                        {
                            ResouceManager.LoadResouce(GetResouceCacheKey(filePath), filePath);
                        }
                        s_LoadFileList.Clear();
                    }
                }
            }

            // 处理待从内存移除的资源文件列表
            if (s_RemoveFileList.Count > 0)
            {
                lock (s_SyncRemoveObj)
                {
                    if (s_RemoveFileList.Count > 0)
                    {
                        List<string> tmp = new List<string>(s_RemoveFileList);
                        foreach (var filePath in tmp)
                        {
                            ResouceManager.RemoveResouce(GetResouceCacheKey(filePath));
                        }
                        s_RemoveFileList.Clear();
                    }
                }
            }
        }

        private static void AddLoadFile(string filePath)
        {
            string path = filePath.ToUpper();
            if (!s_LoadFileList.Contains(path))
            {
                lock (s_SyncLoadObj)
                {
                    if (!s_LoadFileList.Contains(path))
                    {
                        s_LoadFileList.Add(path);
                    }
                }
            }
        }

        private static void AddRemoveFile(string filePath)
        {
            string path = filePath.ToUpper();
            if (!s_RemoveFileList.Contains(path))
            {
                lock (s_SyncRemoveObj)
                {
                    if (!s_RemoveFileList.Contains(path))
                    {
                        s_RemoveFileList.Add(path);
                    }
                }
            }
        }

        private static void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            AddLoadFile(e.FullPath);
            s_Timer.Change(TimeoutMillis, Timeout.Infinite);
        }

        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            AddLoadFile(e.FullPath);
            s_Timer.Change(TimeoutMillis, Timeout.Infinite);
        }

        private static void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            AddLoadFile(e.FullPath);
            AddRemoveFile(e.OldFullPath);
            s_Timer.Change(TimeoutMillis, Timeout.Infinite);
        }

        private static void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            AddRemoveFile(e.FullPath);
            s_Timer.Change(TimeoutMillis, Timeout.Infinite);
        }

        public static void EndWatch() 
        {
            if (s_Watcher != null)
            {
                DisposeWatcher(s_Watcher);
                s_Watcher = null;
            }
        }

        private static void DisposeWatcher(FileSystemWatcher watcher)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Deleted -= Watcher_Deleted;
            watcher.Renamed -= Watcher_Renamed;
            watcher.Changed -= Watcher_Changed;
            watcher.Created -= Watcher_Created;
            watcher.Dispose();
        }
    }
}
