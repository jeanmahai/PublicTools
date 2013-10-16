using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Utility.DataAccess.Database.DbProvider;
using System.Data;
using System.Configuration;

namespace Common.Utility.DataAccess.KeyValue
{
    public class SQLDBProvider : IKeyValueDataProvider
    {
        private static string m_DBConnectionName = null;
        private static string DBConnectionName
        {
            get
            {
                if (string.IsNullOrEmpty(m_DBConnectionName))
                {
                    m_DBConnectionName = ConfigurationManager.AppSettings["WebKeyValueDataDBConnectionName"];
                    if (string.IsNullOrEmpty(m_DBConnectionName))
                    {
                        m_DBConnectionName = "WEBQuery";
                    }
                }
                return m_DBConnectionName;
            }
        }

        public T GetKeyValueData<T>(string dataCategory, string key) where T : class, new()
        {
            List<KeyValueDataAccessSetting> settingList = KeyValueDataAccessSettingManager.GetAllSettings();
            KeyValueDataAccessSetting setting = settingList.Find(f => f.DataCategory.ToUpper().Trim() == dataCategory.Trim().ToUpper());
            if (setting == null)
            {
                throw new NullReferenceException("没有找到" + dataCategory + "在KeyValueDataAccessSetting.config中的配置！");
            }
            string sql = string.Empty;
            if (string.IsNullOrEmpty(setting.DataCategoryPath))
            {
                sql = @"SELECT [SysNo]
                              ,[DataCategory]
                              ,[BizKey]
                              ,[BizValue]
                              ,[Status]
                              ,[InDate]
                              ,[LastEditDate]
                          FROM [WebKeyValueData].[dbo].[DefaultKVD] WITH(NOLOCK) WHERE DataCategory='{0}' AND BizKey='{1}'";
                sql = string.Format(sql, dataCategory.Replace("'", "''"), key.Replace("'", "''"));
            }
            else
            {
                sql = @"SELECT [SysNo]
                              ,[DataCategory]
                              ,[BizKey]
                              ,[BizValue]
                              ,[Status]
                              ,[InDate]
                              ,[LastEditDate]
                          FROM {0} WITH(NOLOCK) WHERE DataCategory='{1}' AND BizKey='{2}'";
                sql = string.Format(sql, setting.DataCategoryPath, dataCategory.Replace("'", "''"), key.Replace("'", "''"));
            }

            string dbConnectionName=SQLDBProvider.DBConnectionName;
            CustomDataCommand cmd = DataCommandManager.CreateCustomDataCommandFromSql(sql, dbConnectionName);
            DataTable dt = cmd.ExecuteDataTable();
            T t = null;
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                string bizValue = row["BizValue"] != null ? row["BizValue"].ToString().Trim() : null;
                if (!string.IsNullOrEmpty(bizValue) && bizValue.Length >=4)
                {
                    t = SerializationUtility.XmlDeserialize<T>(bizValue);
                }
            }
            return t;

        }


        public List<T> GetKeyValueData<T>(string dataCategory) where T : class, new()
        {
            List<KeyValueDataAccessSetting> settingList = KeyValueDataAccessSettingManager.GetAllSettings();
            KeyValueDataAccessSetting setting = settingList.Find(f => f.DataCategory.ToUpper().Trim() == dataCategory.Trim().ToUpper());
            if (setting == null)
            {
                throw new NullReferenceException("没有找到" + dataCategory + "在KeyValueDataAccessSetting.config中的配置！");
            }
            string sql = string.Empty;
            if (string.IsNullOrEmpty(setting.DataCategoryPath))
            {
                sql = @"SELECT [SysNo]
                              ,[DataCategory]
                              ,[BizKey]
                              ,[BizValue]
                              ,[Status]
                              ,[InDate]
                              ,[LastEditDate]
                          FROM [WebKeyValueData].[dbo].[DefaultKVD] WITH(NOLOCK) WHERE DataCategory='{0}'";
                sql = string.Format(sql, dataCategory.Replace("'", "''"));
            }
            else
            {
                sql = @"SELECT [SysNo]
                              ,[DataCategory]
                              ,[BizKey]
                              ,[BizValue]
                              ,[Status]
                              ,[InDate]
                              ,[LastEditDate]
                          FROM {0} WITH(NOLOCK) WHERE DataCategory='{1}'";
                sql = string.Format(sql, setting.DataCategoryPath, dataCategory.Replace("'", "''"));
            }

            string dbConnectionName = SQLDBProvider.DBConnectionName;
            CustomDataCommand cmd = DataCommandManager.CreateCustomDataCommandFromSql(sql, dbConnectionName);
            DataTable dt = cmd.ExecuteDataTable();
            List<T> list = new List<T>();
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {

                    string bizValue = row["BizValue"] != null ? row["BizValue"].ToString().Trim() : null;
                    if (!string.IsNullOrEmpty(bizValue) && bizValue.Length >= 4)
                    {
                        T t = SerializationUtility.XmlDeserialize<T>(bizValue);
                        list.Add(t);
                    }
                }
            }
            return list;
        }
    }
}
