using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Transactions;
using System.Xml;
using System.Xml.Serialization;
using Soho.Utility.Log;

namespace Soho.Utility
{
    internal class SqlDbEmitter : ILogEmitter
    {
        private string m_Sql;
        private string m_ConnStrKey;
        private bool m_Encrypted = true;
        private ConnectionStringConfigType m_ConnectionStringConfigType = ConnectionStringConfigType.DatabaseListFile;

        public void Init(Dictionary<string, string> param)
        {
            if (!param.ContainsKey("sql"))
            {
                throw new ConfigurationErrorsException("Not config 'sql' for sqlDb emitter.");
            }
            if (!param.ContainsKey("connectionStringKey"))
            {
                throw new ConfigurationErrorsException("Not config 'connectionStringKey' for sqlDb emitter.");
            }
            this.m_ConnStrKey = param["connectionStringKey"];
            this.m_Sql = param["sql"];
            bool tmp;
            if (param.ContainsKey("encrypted") && bool.TryParse(param["encrypted"], out tmp))
            {
                m_Encrypted = tmp;
            }
            ConnectionStringConfigType c_type;
            if (param.ContainsKey("connectionStringConfigType") 
                && Enum.TryParse<ConnectionStringConfigType>(param["connectionStringConfigType"], out c_type)
                && Enum.IsDefined(typeof(ConnectionStringConfigType), c_type))
            {
                m_ConnectionStringConfigType = c_type;
            }
        }

        public void EmitLog(LogEntry log)
        {
            string provider;
            string connStr = ConnStringMgr.GetConnectionString(m_ConnStrKey, out provider, m_Encrypted, m_ConnectionStringConfigType);
            InsertLog(log, connStr, m_Sql, provider);
        }

        private DbConnection CreateDbConnection(string dbProviderName)
        {
            string dp = dbProviderName.Trim().ToLower();
            switch (dp)
            {
                case "odbc":
                    return new OdbcConnection();
                case "oledb":
                    return new OleDbConnection();
                case "sqlserver":
                default:
                    return new SqlConnection();
            }
        }

        private DbCommand CreateDbCommand(string dbProviderName, string sql, DbConnection conn)
        {
            DbCommand command;
            string dp = dbProviderName.Trim().ToLower();
            switch (dp)
            {
                case "odbc":
                    command = new OdbcCommand();
                    break;
                case "oledb":
                    command = new OleDbCommand();
                    break;
                case "sqlserver":
                default:
                    command = new SqlCommand();
                    break;
            }
            command.Connection = conn;
            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 300;
            return command;
        }

        private DbParameter CreateDbParameter(string dbProviderName, string name, object value, DbType? dbType = null, int? size = null)
        {
            DbParameter p;
            string dp = dbProviderName.Trim().ToLower();
            switch (dp)
            {
                case "odbc":
                    p = new OdbcParameter();
                    break;
                case "oledb":
                    p = new OleDbParameter();
                    break;
                case "sqlserver":
                default:
                    p = new SqlParameter();
                    break;
            }
            p.ParameterName = name;
            p.Value = value == null ? (object)DBNull.Value : value;
            if (dbType.HasValue)
            {
                p.DbType = dbType.Value;
            }
            if (size.HasValue)
            {
                p.Size = size.Value;
            }
            return p;
        }

        private void InsertLog(LogEntry log, string connStr, string sql, string dbProviderName)
        {
            using (ConnectionWrapper<DbConnection> connection =
                TransactionScopeConnections.GetOpenConnection(connStr, () => { return CreateDbConnection(dbProviderName); }))
            {
                object pro = log.ExtendedProperties == null ? (object)DBNull.Value : (object)log.ExtendedProperties.Serialize();
                List<DbParameter> pList = new List<DbParameter>()
                {
                    CreateDbParameter(dbProviderName, "@LogID", log.LogID, DbType.Guid),    // 1
                    CreateDbParameter(dbProviderName, "@Source", log.Source, DbType.AnsiString),    // 2
                    CreateDbParameter(dbProviderName, "@Category", log.Category, DbType.AnsiString),    // 3
                    CreateDbParameter(dbProviderName, "@RequestUrl", log.RequestUrl, DbType.AnsiString),    // 4
                    CreateDbParameter(dbProviderName, "@ServerName", log.ServerName, DbType.AnsiString),    // 5
                    CreateDbParameter(dbProviderName, "@ServerTime", log.ServerTime, DbType.DateTime),    // 6
                    CreateDbParameter(dbProviderName, "@ServerIP", log.ServerIP, DbType.AnsiString),    // 7
                    CreateDbParameter(dbProviderName, "@ReferenceKey", log.ReferenceKey, DbType.String),    // 8
                    CreateDbParameter(dbProviderName, "@UserHostName", log.UserHostName, DbType.String),    // 9
                    CreateDbParameter(dbProviderName, "@UserHostAddress", log.UserHostAddress, DbType.String),  // 10
                    CreateDbParameter(dbProviderName, "@Content", log.Content, DbType.String),  // 11
                    CreateDbParameter(dbProviderName, "@ProcessID", log.ProcessID, DbType.Int32),  // 12
                    CreateDbParameter(dbProviderName, "@ProcessName", log.ProcessName, DbType.String),  // 13
                    CreateDbParameter(dbProviderName, "@ThreadID", log.ThreadID, DbType.Int32), // 14
                    CreateDbParameter(dbProviderName, "@ExtendedProperties", pro, DbType.String)    // 15
                };
                using (DbCommand command = CreateDbCommand(dbProviderName, sql, connection.Connection))
                {
                    command.Parameters.AddRange(pList.ToArray());
                    command.ExecuteNonQuery();
                }
            }
        }

        private static class TransactionScopeConnections
        {
            [ThreadStatic]
            private static Dictionary<Transaction, Dictionary<string, DbConnection>> s_Dictionary = null;
            private static Dictionary<Transaction, Dictionary<string, DbConnection>> GetTransactionConnectionDictionary()
            {
                string name = ConfigurationManager.AppSettings["Transaction_Scope_Name"];
                if (name == null || name.Trim().Length <= 0)
                {
                    name = "Transaction_Scope";
                }
                LocalDataStoreSlot slot = Thread.GetNamedDataSlot(name);
                if (slot == null)
                {
                    slot = Thread.AllocateNamedDataSlot(name);
                }
                var transactionConnections = Thread.GetData(slot) as Dictionary<Transaction, Dictionary<string, DbConnection>>;
                if (transactionConnections == null)
                {
                    transactionConnections = new Dictionary<Transaction, Dictionary<string, DbConnection>>();
                    Thread.SetData(slot, transactionConnections);
                }
                return transactionConnections;
            }

            /// <summary>
            ///		Returns a connection for the current transaction. This will be an existing <see cref="DbConnection"/>
            ///		instance or a new one if there is a <see cref="TransactionScope"/> active. Otherwise this method
            ///		returns null.
            /// </summary>
            /// <param name="db"></param>
            /// <returns>Either a <see cref="DbConnection"/> instance or null.</returns>
            private static DbConnection GetConnection(string connectionString, Func<DbConnection> creator)
            {
                Transaction currentTransaction = Transaction.Current;

                if (currentTransaction == null)
                {
                    return null;
                }

                if (s_Dictionary == null)
                {
                    s_Dictionary = GetTransactionConnectionDictionary();
                }
                Dictionary<string, DbConnection> connectionList;
                s_Dictionary.TryGetValue(currentTransaction, out connectionList);

                DbConnection connection;
                if (connectionList != null)
                {
                    connectionList.TryGetValue(connectionString, out connection);
                    if (connection != null)
                    {
                        return connection;
                    }
                }
                else
                {
                    // We don't have a list for this transaction, so create a new one
                    connectionList = new Dictionary<string, DbConnection>();
                    s_Dictionary.Add(currentTransaction, connectionList);
                }

                //
                // Next we'll see if there is already a connection. If not, we'll create a new connection and add it
                // to the transaction's list of connections.
                //
                if (connectionList.ContainsKey(connectionString))
                {
                    connection = connectionList[connectionString];
                }
                else
                {
                    connection = creator();
                    connection.ConnectionString = connectionString;
                    connection.Open();
                    currentTransaction.TransactionCompleted += new TransactionCompletedEventHandler(OnTransactionCompleted);
                    connectionList.Add(connectionString, connection);
                }

                return connection;
            }

            /// <summary>
            ///		This event handler is called whenever a transaction is about to be disposed, which allows
            ///		us to remove the transaction from our list and dispose the connection instance we created.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private static void OnTransactionCompleted(object sender, TransactionEventArgs e)
            {
                Dictionary<string, DbConnection> connectionList; // = connections[e.Transaction];
                s_Dictionary.TryGetValue(e.Transaction, out connectionList);
                if (connectionList == null)
                {
                    return;
                }
                s_Dictionary.Remove(e.Transaction);
                foreach (DbConnection conneciton in connectionList.Values)
                {
                    try
                    {
                        conneciton.Dispose();
                    }
                    catch { }
                }
            }

            public static ConnectionWrapper<DbConnection> GetOpenConnection(string connectionString, Func<DbConnection> creator,
                bool disposeInnerConnection = true)
            {
                DbConnection connection = TransactionScopeConnections.GetConnection(connectionString, creator);
                if (connection != null)
                {
                    return new ConnectionWrapper<DbConnection>(connection, false);
                }
                else
                {
                    try
                    {
                        connection = creator();
                        connection.ConnectionString = connectionString;
                        connection.Open();
                    }
                    catch
                    {
                        if (connection != null)
                        {
                            connection.Close();
                        }
                        throw;
                    }
                    return new ConnectionWrapper<DbConnection>(connection, disposeInnerConnection);
                }
            }

            public static ConnectionWrapper<T> GetOpenConnection<T>(string connectionString, bool disposeInnerConnection = true)
                where T : DbConnection, new()
            {
                T connection = TransactionScopeConnections.GetConnection(connectionString, () => new T()) as T;
                if (connection != null)
                {
                    return new ConnectionWrapper<T>(connection, false);
                }
                else
                {
                    try
                    {
                        connection = new T();
                        connection.ConnectionString = connectionString;
                        connection.Open();
                    }
                    catch
                    {
                        if (connection != null)
                        {
                            connection.Close();
                        }
                        throw;
                    }
                    return new ConnectionWrapper<T>(connection, disposeInnerConnection);
                }
            }
        }

        private class ConnectionWrapper<T> : IDisposable where T : DbConnection
        {
            private T m_Connection;
            private bool m_DisposeConnection;

            /// <summary>
            ///		Create a new "lifetime" container for a <see cref="DbConnection"/> instance.
            /// </summary>
            /// <param name="connection">The connection</param>
            /// <param name="disposeConnection">
            ///		Whether or not to dispose of the connection when this class is disposed.
            ///	</param>
            public ConnectionWrapper(T connection, bool disposeConnection)
            {
                this.m_Connection = connection;
                this.m_DisposeConnection = disposeConnection;
            }

            /// <summary>
            ///		Gets the actual connection.
            /// </summary>
            public T Connection
            {
                get { return m_Connection; }
            }

            #region IDisposable Members

            /// <summary>
            ///		Dispose the wrapped connection, if appropriate.
            /// </summary>
            public void Dispose()
            {
                if (m_DisposeConnection)
                {
                    try
                    {
                        m_Connection.Close();
                        m_Connection.Dispose();
                    }
                    catch { }
                }
            }

            #endregion
        }

        private class ConnStringMgr
        {
            #region Private Member

            private const string NODE_NAME = "dataAccess";
            private const string DEFAULT_DATABASE_LIST_FILE_PATH = "Configs/Data/Database.config";
            private static object s_Setting = ConfigurationManager.GetSection(NODE_NAME);

            private static string GetPropertyValue(object obj, string propertyName)
            {
                Type type = obj.GetType();
                PropertyInfo p = type.GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (p == null || p.CanRead == false)
                {
                    return null;
                }
                var tmp = p.GetGetMethod().Invoke(obj, null);
                if (tmp == null)
                {
                    return null;
                }
                return tmp.ToString();
            }

            private static string DatabaseListFilePath
            {
                get
                {
                    string path = s_Setting == null ? null : GetPropertyValue(s_Setting, "DatabaseListFilePath");
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

            private static DatabaseList LoadDatabaseListFile()
            {
                return LoadFromXml<DatabaseList>(DatabaseListFilePath);
            }

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

            #endregion

            public static string GetConnectionString(string name, bool needDecrypt = true, ConnectionStringConfigType configType = ConnectionStringConfigType.DatabaseListFile)
            {
                string dbType;
                return GetConnectionString(name, out dbType, needDecrypt, configType);
            }

            public static string GetConnectionString(string name, out string dbType, bool needDecrypt = true, ConnectionStringConfigType configType = ConnectionStringConfigType.DatabaseListFile)
            {
                if (configType == ConnectionStringConfigType.AppConfig)
                {
                    var t = ConfigurationManager.ConnectionStrings[name];
                    if (t == null)
                    {
                        dbType = null;
                        return null;
                    }
                    dbType = t.ProviderName;
                    if (needDecrypt)
                    {
                        return Decrypt(t.ConnectionString);
                    }
                    return t.ConnectionString;
                }
                DatabaseList dbList = MemoryCache.Default.Get("dataAccess_connstr_list") as DatabaseList;
                if (dbList == null)
                {
                    dbList = LoadDatabaseListFile();
                    var policy = new CacheItemPolicy();
                    policy.AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddMinutes(10));
                    MemoryCache.Default.Set("dataAccess_connstr_list", dbList, policy);
                }
                if (dbList != null && dbList.DatabaseInstances != null && dbList.DatabaseInstances.Length > 0)
                {
                    foreach (var db in dbList.DatabaseInstances)
                    {
                        if (db.Name == name)
                        {
                            dbType = db.Type;
                            if (needDecrypt)
                            {
                                return Decrypt(db.ConnectionString);
                            }
                            return db.ConnectionString;
                        }
                    }
                }
                dbType = null;
                return null;
            }
        }

        private enum ConnectionStringConfigType
        {
            // 使用ECC中的数据库连接字符串配置方式
            DatabaseListFile,

            // 使用直接在app.config或web.config中数据库连接字符串配置方式
            AppConfig
        }
    }
}

namespace Soho.Utility.Log
{

    #region Config Entity

    [XmlRoot("databaseList", Namespace = "http://www.common.com/DatabaseList")]
    public class DatabaseList
    {
        [XmlElement("database")]
        public DatabaseInstance[] DatabaseInstances
        {
            get;
            set;
        }
    }

    [XmlRoot("database")]
    public class DatabaseInstance
    {
        [XmlAttribute("name")]
        public string Name
        {
            get;
            set;
        }

        [XmlAttribute("type")]
        public string Type
        {
            get;
            set;
        }

        [XmlElement("connectionString")]
        public string ConnectionString
        {
            get;
            set;
        }
    }

    public class DataAccessSection : IConfigurationSectionHandler
    {
        private const string DATABASE_LIST_FILE_ATTR = "databaseListFile";

        public object Create(object parent, object configContext, XmlNode section)
        {
            DataAccessSetting da = new DataAccessSetting();
            if (section != null && section.Attributes != null)
            {
                if (section.Attributes[DATABASE_LIST_FILE_ATTR] != null
                    && section.Attributes[DATABASE_LIST_FILE_ATTR].Value != null
                    && section.Attributes[DATABASE_LIST_FILE_ATTR].Value.Trim().Length > 0)
                {
                    da.DatabaseListFilePath = section.Attributes[DATABASE_LIST_FILE_ATTR].Value.Trim();
                }
            }
            return da;
        }
    }

    public class DataAccessSetting
    {
        public string DatabaseListFilePath
        {
            get;
            set;
        }
    }

    #endregion
}
