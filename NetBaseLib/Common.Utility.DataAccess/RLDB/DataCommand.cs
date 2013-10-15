using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Common.Utility.DataAccess.Database.Config;
using Common.Utility.DataAccess.Database.DbProvider;

namespace Common.Utility.DataAccess
{
    public class DataCommand
    {
        private DataCommandConfig m_Config;
        protected DataCommandConfig Config
        {
            get { return m_Config; }
        }

        private IDbFactory m_Factory;

        private List<DbParameter> m_DbParameterList;
        private string[] m_PropertyNameList;

        public List<DbParameter> GetDbParameterList()
        {
            return m_DbParameterList;
        }

        internal DataCommand(DataCommandConfig config)
        {
            m_Config = config.Clone();
            string[] list;
            IDbFactory factory;
            string conn;
            DbHelper.GetConnectionInfo(m_Config.Database, out conn, out factory);
            m_Factory = factory;
            m_DbParameterList = CreateParameters(out list);
            m_PropertyNameList = list;
            SetAutoValueParameter(m_DbParameterList, m_PropertyNameList);
        }

        private void SetAutoValueParameter(List<DbParameter> parameterList, string[] propertyNameList)
        {
            for (int i = 0; i < m_DbParameterList.Count; i++)
            {
                ConfigKeyWord keyword;
                string property = propertyNameList[i];
                if (ConfigHelper.IsKeyWord(property, out keyword))
                {
                    if (keyword == ConfigKeyWord.UserSysNo)
                    {
                        m_DbParameterList[i].Value = ServiceContext.Current.UserSysNo;
                    }
                    else if (keyword == ConfigKeyWord.UserAcct)
                    {
                        string acct = ConfigHelper.CurrentUserAcct;
                        m_DbParameterList[i].Value = (acct == null ? DBNull.Value : (object)acct);
                    }
                    else
                    {
                        m_DbParameterList[i].Value = DateTime.Now;
                    }
                }
            }
        }

        private object GetDbValue(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return DBNull.Value;
            }
            Type type = value.GetType();
            if (type.IsEnum ||
                (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                    && type.GetGenericArguments() != null
                    && type.GetGenericArguments().Length == 1 && type.GetGenericArguments()[0].IsEnum))
            {
                object code;
                if (EnumCodeMapper.TryGetCode(value, out code))
                {
                    return code;
                }
                else
                {
                    return (int)value;
                }
            }
            return value;
        }

        internal bool HasDefinedParameter(string name)
        {
            if (!name.StartsWith("@"))
            {
                name = "@" + name;
            }
            return m_DbParameterList.Exists(p => p.ParameterName == name);
        }

        protected void InnerAddInputParameter(string name, DbType dbType, object value)
        {
            DbParameter p = m_Factory.CreateParameter();
            p.ParameterName = name;
            p.DbType = dbType;
            p.Value = GetDbValue(value);
            p.Direction = ParameterDirection.Input;
            m_DbParameterList.Add(p);
        }

        protected void InnerAddInputParameter(string name, DbType dbType)
        {
            DbParameter p = m_Factory.CreateParameter();
            p.ParameterName = name;
            p.DbType = dbType;
            p.Direction = ParameterDirection.Input;
            m_DbParameterList.Add(p);
        }

        protected void InnerAddOutParameter(string name, DbType dbType, int size)
        {
            DbParameter p = m_Factory.CreateParameter();
            p.ParameterName = name;
            p.DbType = dbType;
            p.Size = size;
            p.Direction = ParameterDirection.Output;
            m_DbParameterList.Add(p);
        }

        private List<DbParameter> CreateParameters(out string[] propertyNameList)
        {
            if (m_Config == null || m_Config.Parameters == null || m_Config.Parameters.Param == null || m_Config.Parameters.Param.Length <= 0)
            {
                propertyNameList = new string[0];
                return new List<DbParameter>(0);
            }
            List<DbParameter> array = new List<DbParameter>(m_Config.Parameters.Param.Length);
            propertyNameList = new string[m_Config.Parameters.Param.Length];
            for (int i = 0; i < m_Config.Parameters.Param.Length; i++)
            {
                Param parm = m_Config.Parameters.Param[i];
                DbParameter pa = m_Factory.CreateParameter();
                array.Add(pa);
                pa.DbType = parm.DbType;
                pa.Direction = parm.Direction;
                pa.ParameterName = parm.Name;
                propertyNameList[i] = parm.Property == null || parm.Property.Trim().Length <= 0 ? parm.Name : parm.Property;
                if (propertyNameList[i].StartsWith("@"))
                {
                    propertyNameList[i] = propertyNameList[i].TrimStart('@');
                }
                if (m_Config.Parameters.Param[i].Size != -1)
                {
                    pa.Size = parm.Size;
                }
                if (pa is SqlParameter)
                {
                    SqlParameter s = (SqlParameter)pa;
                    if (parm.Scale != 0)
                    {
                        s.Scale = parm.Scale;
                    }
                    if (parm.Precision != 0)
                    {
                        s.Precision = parm.Precision;
                    }
                }
            }
            return array;
        }

        public void ReplaceParameterValue(string paramName, object paramValue)
        {
            object tmp = paramValue;
            if (tmp != null)
            {
                tmp = GetDbValue(tmp);
            }
            else
            {
                tmp = string.Empty;
            }
            m_Config.CommandText = m_Config.CommandText.Replace(paramName, tmp.ToString());
        }

        public void ReplaceParameterValueAsCurrentUserSysNo(string paramName)
        {
            m_Config.CommandText = m_Config.CommandText.Replace(paramName, ServiceContext.Current.UserSysNo.ToString());
        }

        public void ReplaceParameterValueAsCurrentUserAcct(string paramName)
        {
            string acct = ConfigHelper.CurrentUserAcct;
            m_Config.CommandText = m_Config.CommandText.Replace(paramName, (acct == null ? string.Empty : acct));
        }

        public object GetParameterValue(string paramName)
        {
            if (!paramName.StartsWith("@"))
            {
                paramName = "@" + paramName;
            }
            DbParameter parm = m_DbParameterList.Find(p => p.ParameterName == paramName);
            if (parm == null)
            {
                return null;
            }
            return parm.Value;
        }

        internal void SetParameterValue(string paramName, DbType dbType, object val)
        {
            if (!paramName.StartsWith("@"))
            {
                paramName = "@" + paramName;
            }
            object value = GetDbValue(val);
            foreach (var p in m_DbParameterList)
            {
                if (p.ParameterName == paramName)
                {
                    p.Value = value;
                    p.DbType = dbType;
                    break;
                }
            }
        }

        public void SetParameterValue(string paramName, object val)
        {
            if (!paramName.StartsWith("@"))
            {
                paramName = "@" + paramName;
            }
            object value = GetDbValue(val);
            foreach (var p in m_DbParameterList)
            {
                if (p.ParameterName == paramName)
                {
                    p.Value = value;
                    break;
                }
            }
        }

        public void SetParameterValueAsCurrentUserSysNo(string paramName)
        {
            SetParameterValue(paramName, ServiceContext.Current.UserSysNo.ToString());
        }

        public void SetParameterValueAsCurrentUserAcct(string paramName)
        {
            string acct = ConfigHelper.CurrentUserAcct;
            SetParameterValue(paramName, (acct == null ? DBNull.Value : (object)acct));
        }

        public void SetParameterValue<T>(T entity)
            where T : class
        {
            SetParameterValue<T>(entity, false, true);
        }

        public void SetParameterValue<T>(T entity, bool ignoreCase, bool throwOnPropertyNotExist)
            where T : class
        {
            SetParameterValue<T>(null, entity, ignoreCase, throwOnPropertyNotExist);
        }

        public void SetParameterValue<T>(InputMap<T> map, T entity)
            where T : class
        {
            SetParameterValue<T>(map, entity, false, true);
        }

        public void SetParameterValue<T>(InputMap<T> map, T entity, bool ignoreCase, bool throwOnPropertyNotExist)
            where T : class
        {
            SetParameterValue<T>(map, entity, '_', ignoreCase, throwOnPropertyNotExist);
        }

        public void SetParameterValue<T>(InputMap<T> map, T entity, char sqlParamSplitter)
            where T : class
        {
            SetParameterValue<T>(map, entity, sqlParamSplitter, false, true);
        }

        public void SetParameterValue<T>(InputMap<T> map, T entity, char sqlParamSplitter, bool ignoreCase, bool throwOnPropertyNotExist)
            where T : class
        {
            List<ParameterProperty<T>> maps = (map == null) ? null : map.GetMaps();
            if (maps == null)
            {
                maps = new List<ParameterProperty<T>>(0);
            }
            for (int i = 0; i < m_DbParameterList.Count; i++)
            {
                string property = m_PropertyNameList[i];
                ConfigKeyWord keyword;
                ParameterProperty<T> pp = maps.Find(p => p.Parameter == m_DbParameterList[i].ParameterName || ("@" + p.Parameter == m_DbParameterList[i].ParameterName));
                string[] proArray;
                if (pp != null)
                {
                    proArray = pp.PropertyList.ToArray();
                }
                else if (ConfigHelper.IsKeyWord(property, out keyword))
                {
                    // 构造函数的时候，就已经对这种自动赋值的param已经赋过值了，这里不用再赋了
                    //if (keyword == ConfigKeyWord.UserSysNo)
                    //{
                    //    m_DbParameterList[i].Value = ServiceContext.Current.UserSysNo;
                    //}
                    //else if (keyword == ConfigKeyWord.UserAcct)
                    //{
                    //    string acct = ConfigHelper.CurrentUserAcct;
                    //    m_DbParameterList[i].Value = (acct == null ? DBNull.Value : (object)acct);
                    //}
                    //else
                    //{
                    //    m_DbParameterList[i].Value = DateTime.Now;
                    //}
                    continue;
                }
                else
                {
                    char[] sep = sqlParamSplitter == '.' ? new char[] { sqlParamSplitter } : new char[] { sqlParamSplitter, '.' };
                    proArray = property.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                }
                object tmp = DataMapper.GetFieldValueAllowNull(entity, proArray, ignoreCase, throwOnPropertyNotExist);
                if (tmp != null)
                {
                    Type type = tmp.GetType();
                    if (type.IsEnum ||
                        (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                            && type.GetGenericArguments() != null
                            && type.GetGenericArguments().Length == 1 && type.GetGenericArguments()[0].IsEnum))
                    {
                        object code;
                        if (EnumCodeMapper.TryGetCode(tmp, out code))
                        {
                            tmp = code;
                        }
                        else if (m_DbParameterList[i].DbType == DbType.Int16
                            || m_DbParameterList[i].DbType == DbType.Int32
                            || m_DbParameterList[i].DbType == DbType.Int64)
                        {
                            tmp = (int)tmp;
                        }
                        else
                        {
                            tmp = tmp.ToString();
                        }
                    }
                }
                m_DbParameterList[i].Value = (tmp == null ? DBNull.Value : tmp);
            }
        }

        public int ExecuteNonQuery()
        {
            return DbHelper.ExecuteNonQuery(m_Config.Database, m_Config.CommandType, m_Config.CommandText, m_Config.TimeOut, m_DbParameterList.ToArray());
        }

        public object ExecuteScalar()
        {
            return DbHelper.ExecuteScalar(m_Config.Database, m_Config.CommandType, m_Config.CommandText, m_Config.TimeOut, m_DbParameterList.ToArray());
        }

        public T ExecuteScalar<T>()
        {
            object v = DataMapper.ConvertIfEnum(ExecuteScalar(), typeof(T));
            return DataConvertor.GetValue<T>(v, null, null);
        }

        public C ExecuteFirstColumn<T, C>()
            where C : ICollection<T>, new()
        {
            C list = new C();
            using (DbDataReader reader = ExecuteDataReader())
            {
                while (reader.Read())
                {
                    list.Add(DataConvertor.GetValue<T>(DataMapper.ConvertIfEnum(reader[0], typeof(T)), null, null));
                }
                reader.Close();
            }
            return list;
        }

        public List<T> ExecuteFirstColumn<T>()
        {
            return ExecuteFirstColumn<T, List<T>>();
        }

        public DbDataReader ExecuteDataReader()
        {
            return DbHelper.ExecuteReader(m_Config.Database, m_Config.CommandType, m_Config.CommandText, m_Config.TimeOut, m_DbParameterList.ToArray());
        }

        public DataSet ExecuteDataSet()
        {
            return DbHelper.ExecuteDataSet(m_Config.Database, m_Config.CommandType, m_Config.CommandText, m_Config.TimeOut, m_DbParameterList.ToArray());
        }

        #region ExecuteDataTable

        public DataTable ExecuteDataTable()
        {
            return DbHelper.ExecuteDataTable(m_Config.Database, m_Config.CommandType, m_Config.CommandText, m_Config.TimeOut, m_DbParameterList.ToArray());
        }

        public DataTable ExecuteDataTable<EnumType>(string enumColumnName) where EnumType : struct
        {
            DataTable table = ExecuteDataTable();
            ConvertEnumColumn<EnumType>(table, enumColumnName);
            return table;
        }

        public DataTable ExecuteDataTable<EnumType>(int enumColumnIndex) where EnumType : struct
        {
            DataTable table = ExecuteDataTable();
            ConvertEnumColumn<EnumType>(table, enumColumnIndex);
            return table;
        }

        public DataTable ExecuteDataTable<EnumType>(string enumColumnName, string newColumnNameForOriginalValue) where EnumType : struct
        {
            DataTable table = ExecuteDataTable();
            ConvertEnumColumn<EnumType>(table, enumColumnName, newColumnNameForOriginalValue);
            return table;
        }

        public DataTable ExecuteDataTable<EnumType>(int enumColumnIndex, string newColumnNameForOriginalValue) where EnumType : struct
        {
            DataTable table = ExecuteDataTable();
            ConvertEnumColumn<EnumType>(table, enumColumnIndex, newColumnNameForOriginalValue);
            return table;
        }

        public DataTable ExecuteDataTable(string enumColumnName, Type enumType)
        {
            DataTable table = ExecuteDataTable();
            ConvertEnumColumn(table, enumColumnName, enumType);
            return table;
        }

        public DataTable ExecuteDataTable(int enumColumnIndex, Type enumType)
        {
            DataTable table = ExecuteDataTable();
            ConvertEnumColumn(table, enumColumnIndex, enumType);
            return table;
        }

        public DataTable ExecuteDataTable(string enumColumnName, Type enumType, string newColumnNameForOriginalValue)
        {
            DataTable table = ExecuteDataTable();
            ConvertEnumColumn(table, enumColumnName, enumType, newColumnNameForOriginalValue);
            return table;
        }

        public DataTable ExecuteDataTable(int enumColumnIndex, Type enumType, string newColumnNameForOriginalValue)
        {
            DataTable table = ExecuteDataTable();
            ConvertEnumColumn(table, enumColumnIndex, enumType, newColumnNameForOriginalValue);
            return table;
        }

        public DataTable ExecuteDataTable(EnumColumnList enumColumns)
        {
            DataTable table = ExecuteDataTable();
            ConvertEnumColumn(table, enumColumns);
            return table;
        }

        public DataTable ExecuteDataTable(string codeNamePairColumnName, string domain, string key)
        {
            DataTable table = ExecuteDataTable();
            ConvertCodeNamePairColumn(table, codeNamePairColumnName, domain, key);
            return table;
        }

        public DataTable ExecuteDataTable(int codeNamePairColumnIndex, string domain, string key)
        {
            DataTable table = ExecuteDataTable();
            ConvertCodeNamePairColumn(table, codeNamePairColumnIndex, domain, key);
            return table;
        }

        public DataTable ExecuteDataTable(string codeNamePairColumnName, string domain, string key, string newColumnNameForOriginalValue)
        {
            DataTable table = ExecuteDataTable();
            ConvertCodeNamePairColumn(table, codeNamePairColumnName, domain, key, newColumnNameForOriginalValue);
            return table;
        }

        public DataTable ExecuteDataTable(int codeNamePairColumnIndex, string domain, string key, string newColumnNameForOriginalValue)
        {
            DataTable table = ExecuteDataTable();
            ConvertCodeNamePairColumn(table, codeNamePairColumnIndex, domain, key, newColumnNameForOriginalValue);
            return table;
        }

        public DataTable ExecuteDataTable(CodeNamePairColumnList codeNamePairColunms)
        {
            DataTable table = ExecuteDataTable();
            ConvertCodeNamePairColumn(table, codeNamePairColunms);
            return table;
        }

        public DataTable ExecuteDataTable(EnumColumnList enumColumns, CodeNamePairColumnList codeNamePairColunms)
        {
            DataTable table = ExecuteDataTable();
            ConvertColumn(table, enumColumns, codeNamePairColunms);
            return table;
        }

        #endregion

        public DataRow ExecuteDataRow()
        {
            return DbHelper.ExecuteDataRow(m_Config.Database, m_Config.CommandType, m_Config.CommandText, m_Config.TimeOut, m_DbParameterList.ToArray());
        }

        #region ExecuteEntity<T>

        public T ExecuteEntity<T>() where T : class, new()
        {
            return ExecuteEntity<T>(null, true, true);
        }

        public T ExecuteEntity<T>(Action<DbDataReader, T> manualMapper) where T : class, new()
        {
            return ExecuteEntity<T>(manualMapper, true, true);
        }

        public T ExecuteEntity<T>(Action<DbDataReader, T> manualMapper, bool propertyNameIgnoreCase, bool skipNotExistProperty) where T : class, new()
        {
            return ExecuteEntity<T>(manualMapper, propertyNameIgnoreCase, skipNotExistProperty, '.');
        }

        public T ExecuteEntity<T>(Action<DbDataReader, T> manualMapper, bool propertyNameIgnoreCase, bool skipNotExistProperty, char splitter) where T : class, new()
        {
            T entity = null;
            using (DbDataReader reader = ExecuteDataReader())
            {
                if (reader.Read())
                {
                    entity = new T();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string[] pNames = reader.GetName(i).Split(new char[] { splitter }, StringSplitOptions.RemoveEmptyEntries);
                        object val = DataMapper.ConvertIfEnum(reader[i], typeof(T), pNames, propertyNameIgnoreCase, skipNotExistProperty);
                        DataMapper.FillFieldValue(entity, pNames, val, propertyNameIgnoreCase, skipNotExistProperty);
                    }
                    if (manualMapper != null)
                    {
                        manualMapper(reader, entity);
                    }
                }
                reader.Close();
            }
            return entity;
        }

        #endregion

        #region ExecuteEntityList<T, C>

        public C ExecuteEntityList<T, C>()
            where T : class, new()
            where C : ICollection<T>, new()
        {
            return ExecuteEntityList<T, C>(null);
        }

        public C ExecuteEntityList<T, C>(Action<DbDataReader, T> manualMapper)
            where T : class, new()
            where C : ICollection<T>, new()
        {
            return ExecuteEntityList<T, C>(manualMapper, true, true);
        }

        public C ExecuteEntityList<T, C>(Action<DbDataReader, T> manualMapper, bool propertyNameIgnoreCase, bool skipNotExistProperty)
            where T : class, new()
            where C : ICollection<T>, new()
        {
            return ExecuteEntityList<T, C>(manualMapper, propertyNameIgnoreCase, skipNotExistProperty, '.');
        }

        public C ExecuteEntityList<T, C>(Action<DbDataReader, T> manualMapper, bool propertyNameIgnoreCase, bool skipNotExistProperty, char splitter)
            where T : class, new()
            where C : ICollection<T>, new()
        {
            C list = new C();
            using (DbDataReader reader = ExecuteDataReader())
            {
                while (reader.Read())
                {
                    T entity = new T();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string[] pNames = reader.GetName(i).Split(new char[] { splitter }, StringSplitOptions.RemoveEmptyEntries);
                        object val = DataMapper.ConvertIfEnum(reader[i], typeof(T), pNames, propertyNameIgnoreCase, skipNotExistProperty);
                        DataMapper.FillFieldValue(entity, pNames, val, propertyNameIgnoreCase, skipNotExistProperty);
                    }
                    if (manualMapper != null)
                    {
                        manualMapper(reader, entity);
                    }
                    list.Add(entity);
                }
                reader.Close();
            }
            return list;
        }

        public List<T> ExecuteEntityList<T>()
            where T : class, new()
        {
            return ExecuteEntityList<T>(null);
        }

        public List<T> ExecuteEntityList<T>(Action<DbDataReader, T> manualMapper)
            where T : class, new()
        {
            return ExecuteEntityList<T>(manualMapper, true, true);
        }

        public List<T> ExecuteEntityList<T>(Action<DbDataReader, T> manualMapper, bool propertyNameIgnoreCase, bool skipNotExistProperty)
            where T : class, new()
        {
            return ExecuteEntityList<T>(manualMapper, propertyNameIgnoreCase, skipNotExistProperty, '.');
        }

        public List<T> ExecuteEntityList<T>(Action<DbDataReader, T> manualMapper, bool propertyNameIgnoreCase, bool skipNotExistProperty, char splitter)
            where T : class, new()
        {
            return ExecuteEntityList<T, List<T>>(manualMapper, propertyNameIgnoreCase, skipNotExistProperty, splitter);
        }

        #endregion

        #region ConvertEnumColumn

        public void ConvertEnumColumn<EnumType>(DataTable table, string columnName) where EnumType : struct
        {
            ConvertEnumColumn(table, columnName, typeof(EnumType));
        }

        public void ConvertEnumColumn<EnumType>(DataTable table, int columnIndex) where EnumType : struct
        {
            ConvertEnumColumn(table, columnIndex, typeof(EnumType));
        }

        public void ConvertEnumColumn<EnumType>(DataTable table, string columnName, string newnewColumnNameForOriginalValue) where EnumType : struct
        {
            ConvertEnumColumn(table, columnName, typeof(EnumType), newnewColumnNameForOriginalValue);
        }

        public void ConvertEnumColumn<EnumType>(DataTable table, int columnIndex, string newnewColumnNameForOriginalValue) where EnumType : struct
        {
            ConvertEnumColumn(table, columnIndex, typeof(EnumType), newnewColumnNameForOriginalValue);
        }

        public void ConvertEnumColumn(DataTable table, string columnName, Type enumType)
        {
            ConvertEnumColumn(table, new EnumColumnList { { columnName, enumType } });
        }

        public void ConvertEnumColumn(DataTable table, int columnIndex, Type enumType)
        {
            ConvertEnumColumn(table, new EnumColumnList { { columnIndex, enumType } });
        }

        public void ConvertEnumColumn(DataTable table, string columnName, Type enumType, string newnewColumnNameForOriginalValue)
        {
            ConvertEnumColumn(table, new EnumColumnList { { columnName, enumType, newnewColumnNameForOriginalValue } });
        }

        public void ConvertEnumColumn(DataTable table, int columnIndex, Type enumType, string newnewColumnNameForOriginalValue)
        {
            ConvertEnumColumn(table, new EnumColumnList { { columnIndex, enumType, newnewColumnNameForOriginalValue } });
        }

        public void ConvertEnumColumn(DataTable table, EnumColumnList enumColumns)
        {
            ConvertColumn(table, enumColumns, null);
        }

        #endregion

        #region ConvertCodeNamePairColumn

        public void ConvertCodeNamePairColumn(DataTable table, string columnName, string domainName, string key)
        {
            ConvertCodeNamePairColumn(table, new CodeNamePairColumnList { { columnName, domainName, key } });
        }

        public void ConvertCodeNamePairColumn(DataTable table, int columnIndex, string domainName, string key)
        {
            ConvertCodeNamePairColumn(table, new CodeNamePairColumnList { { columnIndex, domainName, key } });
        }

        public void ConvertCodeNamePairColumn(DataTable table, string columnName, string domainName, string key, string newColumnNameForOriginalValue)
        {
            ConvertCodeNamePairColumn(table, new CodeNamePairColumnList { { columnName, domainName, key, newColumnNameForOriginalValue } });
        }

        public void ConvertCodeNamePairColumn(DataTable table, int columnIndex, string domainName, string key, string newColumnNameForOriginalValue)
        {
            ConvertCodeNamePairColumn(table, new CodeNamePairColumnList { { columnIndex, domainName, key, newColumnNameForOriginalValue } });
        }

        public void ConvertCodeNamePairColumn(DataTable table, CodeNamePairColumnList codeNamePairColunms)
        {
            ConvertColumn(table, null, codeNamePairColunms);
        }

        #endregion

        public void ConvertColumn(DataTable table, EnumColumnList enumColumns, CodeNamePairColumnList codeNamePairColunms)
        {
            if (table == null || table.Rows == null || table.Rows.Count <= 0)
            {
                return;
            }
            if (enumColumns != null && enumColumns.Count > 0)
            {
                foreach (var entry in enumColumns)
                {
                    Type enumType = entry.EnumType;
                    if (!enumType.IsEnum)
                    {
                        throw new ArgumentException("The type '" + enumType.AssemblyQualifiedName + "' is not enum.", "enumColumns");
                    }
                    string columnName = entry.ColumnIndex.HasValue ? table.Columns[entry.ColumnIndex.Value].ColumnName : entry.ColumnName;
                    int index = table.Columns.IndexOf(columnName);
                    if (index >= 0)
                    {
                        table.Columns[index].ColumnName = (entry.NewColumnNameForOriginalValue != null && entry.NewColumnNameForOriginalValue.Trim().Length > 0) ?
                            entry.NewColumnNameForOriginalValue : (columnName + "_ECCentral_Auto_Removed_820319");
                        table.Columns.Add(columnName, enumType);
                        entry.ColumnIndex = index;
                        entry.NewColumnIndex = table.Columns.Count - 1;
                    }
                }
            }
            if (codeNamePairColunms != null && codeNamePairColunms.Count > 0)
            {
                foreach (var entry in codeNamePairColunms)
                {
                    string columnName = entry.ColumnIndex.HasValue ? table.Columns[entry.ColumnIndex.Value].ColumnName : entry.ColumnName;
                    int index = table.Columns.IndexOf(columnName);
                    if (index >= 0)
                    {
                        table.Columns[index].ColumnName = (entry.NewColumnNameForOriginalValue != null && entry.NewColumnNameForOriginalValue.Trim().Length > 0) ?
                            entry.NewColumnNameForOriginalValue : (columnName + "_ECCentral_Auto_Removed_820319");
                        table.Columns.Add(columnName, typeof(string));
                        entry.ColumnIndex = index;
                        entry.NewColumnIndex = table.Columns.Count - 1;
                    }
                }
            }

            foreach (DataRow row in table.Rows)
            {
                ConvertColumnFromRow(row, enumColumns, codeNamePairColunms);
            }
        }

        private void ConvertColumnFromRow(DataRow row, EnumColumnList enumColumns, CodeNamePairColumnList codeNamePairColunms)
        {
            if (enumColumns != null && enumColumns.Count > 0)
            {
                foreach (var entry in enumColumns)
                {
                    Type enumType = entry.EnumType;
                    if (!enumType.IsEnum)
                    {
                        throw new ArgumentException("The type '" + enumType.AssemblyQualifiedName + "' is not enum.", "enumColumns");
                    }
                    int columnIndex = entry.ColumnIndex.HasValue ? entry.ColumnIndex.Value : row.Table.Columns.IndexOf(entry.ColumnName + "_ECCentral_Auto_Removed_820319");
                    if (columnIndex < 0)
                    {
                        continue;
                    }
                    if (row[columnIndex] == null || row[columnIndex] == DBNull.Value)
                    {
                        row[entry.NewColumnIndex] = DBNull.Value;
                        continue;
                    }
                    object orignalData = row[columnIndex];
                    object tmp;
                    if (orignalData == null || orignalData == DBNull.Value || orignalData.ToString().Trim().Length <= 0)
                    {
                        row[entry.NewColumnIndex] = DBNull.Value;
                    }
                    else if (EnumCodeMapper.TryGetEnum(orignalData, enumType, out tmp))
                    {
                        row[entry.NewColumnIndex] = tmp;
                    }
                    else
                    {
                        row[entry.NewColumnIndex] = Enum.Parse(enumType, orignalData.ToString(), true);
                    }
                }
            }

            if (codeNamePairColunms != null && codeNamePairColunms.Count > 0)
            {
                foreach (var entry in codeNamePairColunms)
                {
                    int columnIndex = entry.ColumnIndex.HasValue ? entry.ColumnIndex.Value : row.Table.Columns.IndexOf(entry.ColumnName + "_ECCentral_Auto_Removed_820319");
                    if (row[columnIndex] == null || row[columnIndex] == DBNull.Value)
                    {
                        row[entry.NewColumnIndex] = DBNull.Value;
                        continue;
                    }
                    List<CodeNamePair> list = CodeNamePairManager.GetList(entry.DomainName, entry.Key);
                    string code = row[columnIndex].ToString();
                    CodeNamePair option = list.Find(x => x.Code == code);
                    if (option != null)
                    {
                        row[entry.NewColumnIndex] = option.Name;
                    }
                }
            }
        }
    }

    #region Enum和CodeNamePair转换的辅助类

    public class EnumColumn
    {
        internal EnumColumn()
        {

        }

        public string ColumnName
        {
            get;
            set;
        }

        public int? ColumnIndex
        {
            get;
            set;
        }

        public Type EnumType
        {
            get;
            set;
        }

        public string NewColumnNameForOriginalValue
        {
            get;
            set;
        }

        internal int NewColumnIndex
        {
            get;
            set;
        }
    }

    public class EnumColumnList : List<EnumColumn>
    {
        public void Add(string columnName, Type enumType)
        {
            this.Add(new EnumColumn { ColumnName = columnName, EnumType = enumType });
        }

        public void Add(int columnIndex, Type enumType)
        {
            this.Add(new EnumColumn { ColumnIndex = columnIndex, EnumType = enumType });
        }

        public void Add(string columnName, Type enumType, string newColumnNameForOriginalValue)
        {
            this.Add(new EnumColumn { ColumnName = columnName, EnumType = enumType, NewColumnNameForOriginalValue = newColumnNameForOriginalValue });
        }

        public void Add(int columnIndex, Type enumType, string newColumnNameForOriginalValue)
        {
            this.Add(new EnumColumn { ColumnIndex = columnIndex, EnumType = enumType, NewColumnNameForOriginalValue = newColumnNameForOriginalValue });
        }
    }

    public class CodeNamePairColumn
    {
        internal CodeNamePairColumn()
        {

        }

        public string ColumnName
        {
            get;
            set;
        }

        public int? ColumnIndex
        {
            get;
            set;
        }

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

        public string NewColumnNameForOriginalValue
        {
            get;
            set;
        }

        internal int NewColumnIndex
        {
            get;
            set;
        }
    }

    public class CodeNamePairColumnList : List<CodeNamePairColumn>
    {
        public void Add(string columnName, string domainName, string key)
        {
            this.Add(new CodeNamePairColumn { ColumnName = columnName, DomainName = domainName, Key = key });
        }

        public void Add(int columnIndex, string domainName, string key)
        {
            this.Add(new CodeNamePairColumn { ColumnIndex = columnIndex, DomainName = domainName, Key = key });
        }

        public void Add(string columnName, string domainName, string key, string newColumnNameForOriginalValue)
        {
            this.Add(new CodeNamePairColumn { ColumnName = columnName, DomainName = domainName, Key = key, NewColumnNameForOriginalValue = newColumnNameForOriginalValue });
        }

        public void Add(int columnIndex, string domainName, string key, string newColumnNameForOriginalValue)
        {
            this.Add(new CodeNamePairColumn { ColumnIndex = columnIndex, DomainName = domainName, Key = key, NewColumnNameForOriginalValue = newColumnNameForOriginalValue });
        }
    }

    #endregion
}
