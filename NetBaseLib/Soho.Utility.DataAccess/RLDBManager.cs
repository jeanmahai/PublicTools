using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Configuration;
using System.Xml;

namespace Soho.Utility.DataAccess
{
    internal class RealtimeSection : IConfigurationSectionHandler
    {
        private const string ENABLE_ATTR = "enable";
        private const string DATABASE_NAME_ATTR = "databaseName";
        private const string TABLE_NAME_ATTR = "tableName";

        public object Create(object parent, object configContext, XmlNode section)
        {
            RealtimeSetting realtime = new RealtimeSetting();
            if (section != null && section.Attributes != null)
            {
                if (section.Attributes[ENABLE_ATTR] != null
                    && section.Attributes[ENABLE_ATTR].Value != null
                    && section.Attributes[ENABLE_ATTR].Value.Trim().Length > 0)
                {
                    bool enable;
                    if (Boolean.TryParse(section.Attributes[ENABLE_ATTR].Value.Trim(), out enable))
                    {
                        realtime.Enable = enable;
                    }
                    else
                    {
                        throw new ConfigurationErrorsException("Invalid realtime enable config.");
                    }
                }
                if (section.Attributes[DATABASE_NAME_ATTR] != null
                    && section.Attributes[DATABASE_NAME_ATTR].Value != null
                    && section.Attributes[DATABASE_NAME_ATTR].Value.Trim().Length > 0)
                {
                    realtime.DatabaseName = section.Attributes[DATABASE_NAME_ATTR].Value.Trim();
                }
                if (section.Attributes[TABLE_NAME_ATTR] != null
                    && section.Attributes[TABLE_NAME_ATTR].Value != null
                    && section.Attributes[TABLE_NAME_ATTR].Value.Trim().Length > 0)
                {
                    realtime.TableName = section.Attributes[TABLE_NAME_ATTR].Value.Trim();
                }

            }
            return realtime;
        }
    }
    internal class RealtimeSetting
    {
        public bool Enable
        {
            get;
            set;
        }

        public string DatabaseName
        {
            get;
            set;
        }

        public string TableName
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 关系数据库操作辅助类
    /// </summary>
    public static class RLDBManager
    {
        private const string NODE_NAME = "realTime";

        private static RealtimeSetting GetRealtimeSetting()
        {
            return ConfigurationManager.GetSection(NODE_NAME) as RealtimeSetting;
        }

        /// <summary>
        /// 查询分页数据(包括Realtime)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryDbCommand">查询QueryDB数据的Command</param>
        /// <param name="realtimeDbCommand">查询Realtime数据的Command</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页数据</param>
        /// <param name="sortField">排序字段</param>
        /// <param name="aliasName">数据主表别名</param>
        /// <param name="keyField">数据主表主键</param>
        /// <param name="totalCount">总记录</param>
        /// <returns></returns>
        public static List<T> Query<T>(CustomDataCommand queryDbCommand, CustomDataCommand realtimeDbCommand,
             string aliasName, string keyField, PagingInfoEntity pagingInfo, out int totalCount)
             where T : class, new()
        {
            var setting = GetRealtimeSetting();
            string pagingSql = @"
SELECT @TotalCount = COUNT(1) 
FROM (
    #InputSql#
    UNION ALL
    #RealtimeSql#
) result

SELECT
    *
FROM(
    SELECT TOP (@EndNumber)
        ROW_NUMBER() OVER(ORDER BY #SortColumnName#) AS RowNumber,
        *
    FROM 
    (
        #InputSql#
        UNION ALL
        #RealtimeSql#
    ) unionResult ) result
WHERE RowNumber > @StartNumber";
            DbParameter[] parameters = queryDbCommand.GetDbParameterList().ToArray();
            DbParameter[] realtimeParams = realtimeDbCommand.GetDbParameterList().ToArray();
            string inputSql = queryDbCommand.CommandText;
            inputSql += string.Format(" AND NOT EXISTS(SELECT TOP 1 1 FROM {0} AS r WHERE r.BusinessDataType = '{1}' AND r.BusinessKey = {2}.{3})", setting.TableName, typeof(T).FullName, aliasName, keyField);
            string realtimeSql, unionSql = string.Empty;

            realtimeSql = realtimeDbCommand.CommandText;

            pagingSql = pagingSql.Replace("#SortColumnName#", pagingInfo.SortField);
            pagingSql = pagingSql.Replace("#InputSql#", inputSql).Replace("#RealtimeSql#", realtimeSql);

            var command = DataCommandManager.CreateCustomDataCommandFromSql(pagingSql, setting.DatabaseName);

            command.AddInputParameter("@StartNumber", DbType.Int32, pagingInfo.StartRowIndex);
            command.AddInputParameter("@EndNumber", DbType.Int32, pagingInfo.StartRowIndex + pagingInfo.MaximumRows);
            command.AddOutParameter("@TotalCount", DbType.Int32, 4);

            //合并参数
            List<DbParameter> list = new List<DbParameter>();
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    if (list.FirstOrDefault(k => k.ParameterName.Trim().ToUpper() == p.ParameterName.Trim().ToUpper()) == null)
                    {
                        list.Add(p);
                    }
                }
            }
            if (realtimeParams != null)
            {
                foreach (var p in realtimeParams)
                {
                    if (list.FirstOrDefault(k => k.ParameterName.Trim().ToUpper() == p.ParameterName.Trim().ToUpper()) == null)
                    {
                        list.Add(p);
                    }
                }
            }
            list.ForEach(p =>
            {
                command.AddInputParameter(p.ParameterName, p.DbType, p.Value);
            });
            var result = command.ExecuteEntityList<T>();

            totalCount = int.Parse(command.GetParameterValue("@TotalCount").ToString());

            return result;
        }

        /// <summary>
        /// 查询数据(包括Realtime)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryDbCommand">查询QueryDB数据的Command</param>
        /// <param name="realtimeDbCommand">查询Realtime数据的Command</param>      
        /// <param name="aliasName">数据主表别名</param>
        /// <param name="keyField">数据主表主键</param>        
        /// <returns></returns>
        public static List<T> Query<T>(CustomDataCommand queryDbCommand, CustomDataCommand realtimeDbCommand, string aliasName, string keyField)
            where T : class, new()
        {
            var setting = GetRealtimeSetting();
            string sql = @"
        #InputSql#
        UNION ALL
        #RealtimeSql#";
            DbParameter[] parameters = queryDbCommand.GetDbParameterList().ToArray();
            DbParameter[] realtimeParams = realtimeDbCommand.GetDbParameterList().ToArray();
            string inputSql = queryDbCommand.CommandText;
            inputSql += string.Format(" AND NOT EXISTS(SELECT TOP 1 1 FROM {0} AS r WHERE r.BusinessDataType = '{1}' AND r.BusinessKey = {2}.{3})", setting.TableName, typeof(T).FullName, aliasName, keyField);
            string realtimeSql, unionSql = string.Empty;

            realtimeSql = realtimeDbCommand.CommandText;

            var command = DataCommandManager.CreateCustomDataCommandFromSql(sql, setting.DatabaseName);
            //合并参数
            List<DbParameter> list = new List<DbParameter>();
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    if (list.FirstOrDefault(k => k.ParameterName.Trim().ToUpper() == p.ParameterName.Trim().ToUpper()) == null)
                    {
                        list.Add(p);
                    }
                }
            }
            if (realtimeParams != null)
            {
                foreach (var p in realtimeParams)
                {
                    if (list.FirstOrDefault(k => k.ParameterName.Trim().ToUpper() == p.ParameterName.Trim().ToUpper()) == null)
                    {
                        list.Add(p);
                    }
                }
            }
            list.ForEach(p =>
            {
                command.AddInputParameter(p.ParameterName, p.DbType, p.Value);
            });

            return command.ExecuteEntityList<T>();
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryDbCommand">查询QueryDB数据的Command</param>                    
        /// <returns></returns>
        public static List<T> Query<T>(DataCommand queryDbCommand) where T : class, new()
        {
            return queryDbCommand.ExecuteEntityList<T>();
        }

        /// <summary>
        /// 根据主键加载数据,如果需要查询Realtime表，请使用包含参数id的重载方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryDbCommand"></param>
        /// <returns></returns>
        public static T Load<T>(DataCommand queryDbCommand) where T : class, new()
        {
            return Load<T>(queryDbCommand, null);
        }

        /// <summary>
        /// 根据主键加载数据,如果需要查询Realtime表，请传入参数id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">主键</param>
        /// <param name="queryDbCommand">查询QueryDB的DataCommand</param>
        /// <returns></returns>
        public static T Load<T>(DataCommand queryDbCommand, string id) where T : class, new()
        {
            if (!string.IsNullOrEmpty(id))
            {
                var setting = GetRealtimeSetting();
                string sql = @"SELECT 
        BusinessData 
    FROM #TableName# WITH(NOLOCK)
    WHERE BusinessKey = @BusinessKey
        AND BusinessDataType = @BusinessDataType";
                sql = sql.Replace("#TableName#", setting.TableName);
                var command = DataCommandManager.CreateCustomDataCommandFromSql(sql, setting.DatabaseName);
                string xml = command.ExecuteScalar<string>();
                if (!string.IsNullOrEmpty(xml))
                {
                    var result = SerializationUtility.XmlDeserialize<T>(xml);
                    return result;
                }
            }
            return queryDbCommand.ExecuteEntity<T>();
        }

        /// <summary>
        /// 写入Realtime数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">数据</param>
        /// <param name="keyGetter">获取数据主键的表达式，如订单数据的订单编号</param>
        public static void Insert<T>(T data, Func<T, string> keyGetter)
        {
            var setting = GetRealtimeSetting();
            string sql = @"
    INSERT INTO #TableName#
    (
        BusinessKey,        
        BusinessDataType,
        ChangeType,
        BusinessData,
        CreateDate        
    )
    VALUES
    (
        @BusinessKey,        
        @BusinessDataType,
        @ChangeType,        
        @BusinessData,
        GETDATE()
    )";
            sql = sql.Replace("#TableName#", setting.TableName);
            string businessData = SerializationUtility.XmlSerialize(data);
            var command = DataCommandManager.CreateCustomDataCommandFromSql(sql, setting.DatabaseName);
            command.AddInputParameter("@BusinessKey", DbType.String, keyGetter(data));
            command.AddInputParameter("@BusinessDataType", DbType.String, typeof(T).FullName);
            command.AddInputParameter("@BusinessData", DbType.String, businessData);
            command.AddInputParameter("@ChangeType", DbType.String, "A");
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// 修改Realtime数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">数据</param>
        /// <param name="keyGetter">获取数据主键的表达式，如订单数据的订单编号</param>
        public static void Update<T>(T data, Func<T, string> keyGetter)
        {
            var setting = GetRealtimeSetting();
            string sql = @"
DECLARE @TranCounterNew int
BEGIN TRY  
	BEGIN      
		SELECT @TranCounterNew = @@TRANCOUNT     
		IF @TranCounterNew = 0  
		BEGIN TRANSACTION  
			IF EXISTS(
				SELECT TOP 1 1
				FROM #TableName# WITH(NOLOCK)
				WHERE BusinessKey = @BusinessKey
					AND BusinessDataType = @BusinessDataType)
			BEGIN
				INSERT INTO #TableName#_History
				  (
					BusinessKey,        
					BusinessDataType,
					ChangeType,        
					BusinessData,
					CreateDate
				  )
				  SELECT 
					 BusinessKey        
					,BusinessDataType
					,ChangeType
					,BusinessData
					,GETDATE()
				  FROM #TableName# WITH(NOLOCK)
				  WHERE BusinessKey = @BusinessKey
					AND BusinessDataType = @BusinessDataType                 
			END

			UPDATE #TableName# SET            
					ChangeType = 'U',
					BusinessData = @BusinessData
				WHERE BusinessKey = @BusinessKey
					AND BusinessDataType = @BusinessDataType   
					
			IF XACT_STATE() = 1 AND @TranCounterNew = 0  
		COMMIT TRANSACTION;   
	END  
END TRY  
BEGIN CATCH  
	BEGIN  
		IF XACT_STATE() <> 0  
        BEGIN  
            IF @TranCounterNew = 0     
                ROLLBACK TRANSACTION  
        END                    
	END  
END Catch";
            sql = sql.Replace("#TableName#", setting.TableName);
            string businessKey = keyGetter(data);
            var command = DataCommandManager.CreateCustomDataCommandFromSql(sql, setting.DatabaseName);
            command.AddInputParameter("@BusinessKey", DbType.String, businessKey);
            command.AddInputParameter("@BusinessData", DbType.String, SerializationUtility.XmlSerialize(data));
            command.AddInputParameter("@BusinessDataType", DbType.String, typeof(T).FullName);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// 删除Realtime数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">数据主键</param>
        public static void Delete<T>(string id)
        {
            var setting = GetRealtimeSetting();
            string sql = @"
    UPDATE #TableName# SET            
        ChangeType = 'D'        
    WHERE BusinessKey = @BusinessKey
        AND BusinessDataType = @BusinessDataType";
            sql = sql.Replace("#TableName#", setting.TableName);
            var command = DataCommandManager.CreateCustomDataCommandFromSql(sql, setting.DatabaseName);
            command.AddInputParameter("@BusinessKey", DbType.String, id);
            command.AddInputParameter("@BusinessDataType", DbType.String, typeof(T).FullName);
            command.ExecuteNonQuery();
        }
    }
}
