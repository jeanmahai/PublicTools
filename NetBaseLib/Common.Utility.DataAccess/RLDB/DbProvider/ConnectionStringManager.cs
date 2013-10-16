using System;
using System.Text;
using System.Configuration;
using System.Collections.Generic;

namespace Common.Utility.DataAccess.Database.DbProvider
{
    public static class ConnectionStringManager
    {
        private static Dictionary<string, ConnStrSetting> s_ConnStrMaps = new Dictionary<string, ConnStrSetting>();
        private static object s_SyncObject = new object();

        internal static void GetConnectionInfo(string databaseName, out string connectionString, out ProviderType providerType)
        {
            if (!s_ConnStrMaps.ContainsKey(databaseName))
            {
                throw new ApplicationException("Can't find the info of database '" + databaseName + "'. It hasn't been configurated.");
            }
            ConnStrSetting conn = s_ConnStrMaps[databaseName];
            connectionString = conn.ConnectionString;
            providerType = conn.ProviderType;
        }

        internal static void ClearAllConnectionString()
        {
            if (s_ConnStrMaps != null)
            {
                s_ConnStrMaps.Clear();
            }
        }

        public static int ConnectionStringCount
        {
            get
            {
                if (s_ConnStrMaps == null)
                {
                    return 0;
                }
                return s_ConnStrMaps.Count;
            }
        }

        public static void SetConnectionString(string databaseName, string connStr, string providerName)
        {
            ProviderType providerType = ConvertProviderNameToType(providerName, databaseName, connStr);
            SetConnectionString(databaseName, connStr, providerType);
        }

        public static void SetConnectionString(string databaseName, string connStr, ProviderType providerType)
        {
            ConnStrSetting tmp = new ConnStrSetting(databaseName, connStr, providerType);
            if (s_ConnStrMaps.ContainsKey(databaseName))
            {
                s_ConnStrMaps[databaseName] = tmp;
            }
            else
            {
                s_ConnStrMaps.Add(databaseName, tmp);
            }
        }

        public static void SetSqlServerConnectionString(string databaseName, string connStr)
        {
            SetConnectionString(databaseName, connStr, ProviderType.SqlServer);
        }

        private static ProviderType ConvertProviderNameToType(string providerName, string databaseName, string connStr)
        {
            if (providerName == null || providerName.Trim().Length <= 0)
            {
                return ProviderType.SqlServer;
            }
            string name = providerName.Trim().ToLower();
            switch (name)
            {
                case "sqlserver":
                    return ProviderType.SqlServer;
                case "odbc":
                    return ProviderType.Odbc;
                case "oledb":
                    return ProviderType.OleDb;
                default:
                    throw new ConfigurationErrorsException("Not support this database provider '" + providerName + "' for database whose name is '" + databaseName + "' and connection string is '" + connStr + "'.");
            }
        }

        private class ConnStrSetting
        {
            private string m_Name;
            private string m_ConnectionString;
            private ProviderType m_ProviderType;

            public ConnStrSetting(string name, string connStr, ProviderType proType)
            {
                m_Name = name;
                m_ConnectionString = connStr;
                m_ProviderType = proType;
            }

            public string Name
            {
                get
                {
                    return m_Name;
                }
            }

            public string ConnectionString
            {
                get
                {
                    return m_ConnectionString;
                }
            }

            public ProviderType ProviderType
            {
                get
                {
                    return m_ProviderType;
                }
            }
        }
    }
}
