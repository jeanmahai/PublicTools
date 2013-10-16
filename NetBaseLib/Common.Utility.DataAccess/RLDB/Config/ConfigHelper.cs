using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Configuration;
using System.ServiceModel.Web;
using System.Web;

namespace Common.Utility.DataAccess.Database.Config
{
    internal static class ConfigHelper
    {
        private const string NODE_NAME = "dataAccess";
        private const string DEFAULT_SQL_CONFIG_LIST_FILE_PATH = "Configuration/Data/DbCommandFiles.config";
        private const string DEFAULT_DATABASE_LIST_FILE_PATH = "Configuration/Data/Database.config";

        private static string s_ConfigFolder = null;
        private static DataAccessSetting s_Setting = ConfigurationManager.GetSection(NODE_NAME) as DataAccessSetting;

        public static string CurrentUserAcct
        {
            get
            {
                if (WebOperationContext.Current != null
                    && WebOperationContext.Current.IncomingRequest != null
                    && WebOperationContext.Current.IncomingRequest.Headers != null)
                {
                    string acct = WebOperationContext.Current.IncomingRequest.Headers["X-User-Acct"];
                    if (acct != null && acct.Trim().Length > 0)
                    {
                        return HttpUtility.UrlDecode(acct);
                    }
                }
                return string.Empty;
            }
        }

        public static string ConfigFolder
        {
            get
            {
                if (s_ConfigFolder == null)
                {
                    s_ConfigFolder = Path.GetDirectoryName(SqlConfigListFilePath);
                }
                return s_ConfigFolder;
            }
        }

        public static string SqlConfigListFilePath
        {
            get
            {
                string path = s_Setting == null ? null : s_Setting.SqlConfigListFilePath;
                if (path == null || path.Trim().Length <= 0)
                {
                    path = DEFAULT_SQL_CONFIG_LIST_FILE_PATH;
                }
                string p = Path.GetPathRoot(path);
                if (p == null || p.Trim().Length <= 0) // 说明是相对路径
                {
                    return Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
                }
                return path;
            }
        }

        public static string DatabaseListFilePath
        {
            get
            {
                string path = s_Setting == null ? null : s_Setting.DatabaseListFilePath;
                if (path == null || path.Trim().Length <= 0)
                {
                    path = DEFAULT_DATABASE_LIST_FILE_PATH;
                }
                string p = Path.GetPathRoot(path);
                if (p == null || p.Trim().Length <= 0) // 说明是相对路径
                {
                    return Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
                }
                return path;
            }
        }

        private static T LoadFromXml<T>(string fileName)
        {
            FileStream fs = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                return (T)serializer.Deserialize(fs);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        public static DataCommandFileList LoadSqlConfigListFile()
        {
            string p = SqlConfigListFilePath;
            if (!string.IsNullOrWhiteSpace(p) && File.Exists(p.Trim()))
            {
                return LoadFromXml<DataCommandFileList>(p.Trim());
            }
            return null;
        }

        public static DatabaseList LoadDatabaseListFile()
        {
            return LoadFromXml<DatabaseList>(DatabaseListFilePath);
        }

        public static DataOperations LoadDataCommandList(string filePath)
        {
            return LoadFromXml<DataOperations>(filePath);
        }

        public static bool IsKeyWord(string property, out ConfigKeyWord keyword)
        {
            keyword = ConfigKeyWord.Now;
            if (property == null)
            {
                return false;
            }
            property = property.Trim();
            if (property.Length <= 2)
            {
                return false;
            }
            if (property[0] != '[' || property[property.Length - 1] != ']')
            {
                return false;
            }
            property = property.Substring(1, property.Length - 2).ToUpper();
            if (property == "NOW")
            {
                keyword = ConfigKeyWord.Now;
                return true;
            }
            else if (property == "USERACCT")
            {
                keyword = ConfigKeyWord.UserAcct;
                return true;
            }
            else if (property == "USERSYSNO")
            {
                keyword = ConfigKeyWord.UserSysNo;
                return true;
            }
            return false;
        }
    }

    public enum ConfigKeyWord
    {
        UserSysNo,
        UserAcct,
        Now
    }
}
