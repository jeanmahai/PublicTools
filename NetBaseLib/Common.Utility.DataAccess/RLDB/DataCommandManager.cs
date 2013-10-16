using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Utility.DataAccess.Database.Config;
using System.IO;
using Common.Utility.DataAccess.Database.DbProvider;
using System.Security.Cryptography;
using System.Data.Common;

namespace Common.Utility.DataAccess
{
    public static class DataCommandManager
    {
        private static Dictionary<string, DataCommandConfig> s_ConfigCache = null;
        private static object s_SyncObj = new object();
        private static object s_SyncObj_For_ConnStr = new object();

        #region Encrypt & Decrypt

        private const string m_LongDefaultKey = "Nesc.Oversea";
        private const string m_LongDefaultIV = "Oversea3";

        private static string Encrypt(string encryptionText)
        {
            string result = string.Empty;

            if (encryptionText.Length > 0)
            {
                byte[] bytes = Encoding.Unicode.GetBytes(encryptionText);
                byte[] inArray = Encrypt(bytes);
                if (inArray.Length > 0)
                {
                    result = Convert.ToBase64String(inArray);
                }


            }
            return result;

        }

        private static string Decrypt(string encryptionText)
        {
            string result = string.Empty;

            if (encryptionText.Length > 0)
            {
                byte[] bytes = Convert.FromBase64String(encryptionText);
                byte[] inArray = Decrypt(bytes);
                if (inArray.Length > 0)
                {
                    result = Encoding.Unicode.GetString(inArray);
                }

            }
            return result;

        }

        private static byte[] Encrypt(byte[] bytesData)
        {
            byte[] result = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                ICryptoTransform cryptoServiceProvider = CreateAlgorithm().CreateEncryptor();
                using (CryptoStream stream2 = new CryptoStream(stream, cryptoServiceProvider, CryptoStreamMode.Write))
                {
                    stream2.Write(bytesData, 0, bytesData.Length);
                    stream2.FlushFinalBlock();
                    stream2.Close();
                    result = stream.ToArray();
                }
                stream.Close();
            }
            return result;
        }

        private static byte[] Decrypt(byte[] bytesData)
        {
            byte[] result = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                ICryptoTransform cryptoServiceProvider = CreateAlgorithm().CreateDecryptor();
                using (CryptoStream stream2 = new CryptoStream(stream, cryptoServiceProvider, CryptoStreamMode.Write))
                {
                    stream2.Write(bytesData, 0, bytesData.Length);
                    stream2.FlushFinalBlock();
                    stream2.Close();
                    result = stream.ToArray();
                }
                stream.Close();
            }
            return result;
        }

        private static Rijndael CreateAlgorithm()
        {
            Rijndael rijndael = new RijndaelManaged();
            rijndael.Mode = CipherMode.CBC;
            byte[] key = Encoding.Unicode.GetBytes(m_LongDefaultKey);
            byte[] iv = Encoding.Unicode.GetBytes(m_LongDefaultIV);
            rijndael.Key = key;
            rijndael.IV = iv;
            return rijndael;
        }

        #endregion

        internal static void ClearAllDataCommandFiles()
        {
            s_ConfigCache = null;
        }

        internal static void LoadAllDataCommandFiles()
        {
            DataCommandFileList fileList = ConfigHelper.LoadSqlConfigListFile();
            if (fileList == null || fileList.FileList == null || fileList.FileList.Length <= 0)
            {
                s_ConfigCache = new Dictionary<string, DataCommandConfig>(0);
                return;
            }
            Dictionary<string, DataCommandConfig> cache = new Dictionary<string, DataCommandConfig>();
            foreach (var file in fileList.FileList)
            {
                string path = file.FileName;
                string root = Path.GetPathRoot(path);
                if (root == null || root.Trim().Length <= 0)
                {
                    path = Path.Combine(ConfigHelper.ConfigFolder, path);
                }
                DataOperations op = ConfigHelper.LoadDataCommandList(path);
                if (op != null && op.DataCommand != null && op.DataCommand.Length > 0)
                {
                    foreach (var da in op.DataCommand)
                    {
                        if (cache.ContainsKey(da.Name))
                        {
                            throw new ApplicationException("Duplicate name '" + da.Name + "' for data command in file '" + path + "'.");
                        }
                        cache.Add(da.Name, da);
                    }
                }
            }
            s_ConfigCache = cache;
        }

        internal static void LoadDatabaseInfo()
        {
            DatabaseList dbList = ConfigHelper.LoadDatabaseListFile();
            if (dbList != null && dbList.DatabaseInstances != null && dbList.DatabaseInstances.Length > 0)
            {
                List<string> tmp = new List<string>(dbList.DatabaseInstances.Length);
                foreach (var db in dbList.DatabaseInstances)
                {
                    if (tmp.Contains(db.Name))
                    {
                        throw new ApplicationException("Duplidated database name '" + db.Name + "' in configuration file '" + ConfigHelper.DatabaseListFilePath + "'.");
                    }
                    tmp.Add(db.Name);
                    ConnectionStringManager.SetConnectionString(db.Name, Decrypt(db.ConnectionString), db.Type);
                }
            }
        }

        public static void InitConfig()
        {
            if (s_ConfigCache == null)
            {
                lock (s_SyncObj)
                {
                    if (s_ConfigCache == null)
                    {
                        LoadAllDataCommandFiles();
                    }
                }
            }
            if (ConnectionStringManager.ConnectionStringCount <= 0)
            {
                lock (s_SyncObj_For_ConnStr)
                {
                    if (ConnectionStringManager.ConnectionStringCount <= 0)
                    {
                        LoadDatabaseInfo();
                    }
                }
            }
        }

        public static DataCommand GetDataCommand(string sqlNameInConfig)
        {
            InitConfig();
            if (!s_ConfigCache.ContainsKey(sqlNameInConfig))
            {
                throw new KeyNotFoundException("Can't find the data command configuration of name '" + sqlNameInConfig + "'");
            }
            return new DataCommand(s_ConfigCache[sqlNameInConfig]);
        }

        public static CustomDataCommand CreateCustomDataCommandFromConfig(string sqlNameInConfig)
        {
            InitConfig();
            if (!s_ConfigCache.ContainsKey(sqlNameInConfig))
            {
                throw new KeyNotFoundException("Can't find the data command configuration of name '" + sqlNameInConfig + "'");
            }
            return new CustomDataCommand(s_ConfigCache[sqlNameInConfig]);
        }

        internal static CustomDataCommand CreateCustomDataCommandFromSql(string sql, string dataBase)
        {
            InitConfig();

            DataCommandConfig config = new DataCommandConfig();
            config.Database = dataBase;
            config.CommandText = sql;
            return new CustomDataCommand(config);
        }        
    }
}
