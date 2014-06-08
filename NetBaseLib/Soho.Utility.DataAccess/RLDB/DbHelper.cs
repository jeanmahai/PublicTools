using System;
using System.Configuration;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using System.Data.Common;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using System.ComponentModel;
using System.Text.RegularExpressions;

using Soho.Utility.DataAccess.Database.DbProvider;

namespace Soho.Utility.DataAccess
{
    public static partial class DbHelper
    {
        internal static void GetConnectionInfo(string databaseName, out string connectionString, out IDbFactory factory)
        {
            ProviderType type;
            ConnectionStringManager.GetConnectionInfo(databaseName, out connectionString, out type);
            factory = DbFactories.GetFactory(type);
        }

        private static ConnectionWrapper<DbConnection> GetOpenConnection(string connectionString, IDbFactory factory)
        {
            return GetOpenConnection(connectionString, factory, true);
        }

        private static ConnectionWrapper<DbConnection> GetOpenConnection(string connectionString, IDbFactory factory,
            bool disposeInnerConnection)
        {
            return TransactionScopeConnections.GetOpenConnection(connectionString, () => factory.CreateConnection(), disposeInnerConnection);
        }

        public static int ExecuteNonQuery(string databaseName, CommandType cmdType, string cmdText, int timeout, params DbParameter[] commandParameters)
        {
            IDbFactory dbFactory;
            string connectionString;
            GetConnectionInfo(databaseName, out connectionString, out dbFactory);
            DbCommand cmd = dbFactory.CreateCommand();
            ConnectionWrapper<DbConnection> wrapper = null;
            try
            {
                wrapper = GetOpenConnection(connectionString, dbFactory);
                PrepareCommand(cmd, wrapper.Connection, null, cmdType, cmdText, timeout, commandParameters);
                int val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                return val;
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex, connectionString, cmdText, commandParameters);
            }
            finally
            {
                if (wrapper != null)
                {
                    wrapper.Dispose();
                }
            }
        }

        public static DbDataReader ExecuteReader(string databaseName, CommandType cmdType, string cmdText, int timeout, params DbParameter[] commandParameters)
        {
            IDbFactory dbFactory;
            string connectionString;
            GetConnectionInfo(databaseName, out connectionString, out dbFactory);
            DbCommand cmd = dbFactory.CreateCommand();

            CommandBehavior cmdBehavior;
            if (Transaction.Current != null)
            {
                cmdBehavior = CommandBehavior.Default;
            }
            else
            {
                cmdBehavior = CommandBehavior.CloseConnection;
            }

            ConnectionWrapper<DbConnection> wrapper = null;
            try
            {
                wrapper = GetOpenConnection(connectionString, dbFactory);
                PrepareCommand(cmd, wrapper.Connection, null, cmdType, cmdText, timeout, commandParameters);
                DbDataReader rdr = cmd.ExecuteReader(cmdBehavior);
                // cmd.Parameters.Clear();
                return rdr;
            }
            catch (Exception ex)
            {
                if (wrapper != null)
                {
                    wrapper.Dispose();
                }
                throw new DataAccessException(ex, connectionString, cmdText, commandParameters);
            }
        }

        public static object ExecuteScalar(string databaseName, CommandType cmdType, string cmdText, int timeout, params DbParameter[] commandParameters)
        {
            IDbFactory dbFactory;
            string connectionString;
            GetConnectionInfo(databaseName, out connectionString, out dbFactory);
            DbCommand cmd = dbFactory.CreateCommand();

            ConnectionWrapper<DbConnection> wrapper = null;
            try
            {
                wrapper = GetOpenConnection(connectionString, dbFactory);
                PrepareCommand(cmd, wrapper.Connection, null, cmdType, cmdText, timeout, commandParameters);
                object val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return val;
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex, connectionString, cmdText, commandParameters);
            }
            finally
            {
                if (wrapper != null)
                {
                    wrapper.Dispose();
                }
            }
        }

        public static DataSet ExecuteDataSet(string databaseName, CommandType cmdType, string cmdText, int timeout, params DbParameter[] commandParameters)
        {
            IDbFactory dbFactory;
            string connectionString;
            GetConnectionInfo(databaseName, out connectionString, out dbFactory);

            DbCommand cmd = dbFactory.CreateCommand();
            DataSet ds = new DataSet();
            ConnectionWrapper<DbConnection> wrapper = null;
            try
            {
                wrapper = GetOpenConnection(connectionString, dbFactory);
                PrepareCommand(cmd, wrapper.Connection, null, cmdType, cmdText, timeout, commandParameters);
                DbDataAdapter sda = dbFactory.CreateDataAdapter();
                sda.SelectCommand = cmd;
                sda.Fill(ds);
                cmd.Parameters.Clear();
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex, connectionString, cmdText, commandParameters);
            }
            finally
            {
                if (wrapper != null)
                {
                    wrapper.Dispose();
                }
            }
            return ds;
        }

        public static DataTable ExecuteDataTable(string databaseName, CommandType cmdType, string cmdText, int timeout, params DbParameter[] commandParameters)
        {
            IDbFactory dbFactory;
            string connectionString;
            GetConnectionInfo(databaseName, out connectionString, out dbFactory);

            DbCommand cmd = dbFactory.CreateCommand();
            DataTable table = new DataTable();
            ConnectionWrapper<DbConnection> wrapper = null;
            try
            {
                wrapper = GetOpenConnection(connectionString, dbFactory);
                PrepareCommand(cmd, wrapper.Connection, null, cmdType, cmdText, timeout, commandParameters);
                DbDataAdapter sda = dbFactory.CreateDataAdapter();
                sda.SelectCommand = cmd;
                sda.Fill(table);
                cmd.Parameters.Clear();
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex, connectionString, cmdText, commandParameters);
            }
            finally
            {
                if (wrapper != null)
                {
                    wrapper.Dispose();
                }
            }
            return table;
        }

        public static DataRow ExecuteDataRow(string databaseName, CommandType cmdType, string cmdText, int timeout, params DbParameter[] commandParameters)
        {
            DataTable table = ExecuteDataTable(databaseName, cmdType, cmdText, timeout, commandParameters);
            if (table.Rows.Count == 0)
            {
                return null;
            }
            return table.Rows[0];
        }

        private static void PrepareCommand(DbCommand cmd, DbConnection conn, DbTransaction trans, CommandType cmdType, string cmdText, int timeout, DbParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            cmd.CommandTimeout = timeout;

            if (trans != null)
            {
                cmd.Transaction = trans;
            }

            cmd.CommandType = cmdType;

            if (cmdParms != null)
            {
                foreach (DbParameter parm in cmdParms)
                {
                    cmd.Parameters.Add(parm);
                }
            }
        }

        public static ArrayList DatatableConvertArrayList(DataTable dt)
        {
            ArrayList result = new ArrayList();
            DateTime dtNow = DateTime.Now;
            foreach (DataRow dr in dt.Rows)
            {
                Dictionary<string, object> drow = new Dictionary<string, object>();
                foreach (DataColumn dc in dt.Columns)
                {
                    drow.Add(dc.ColumnName, (DateTime.TryParse(dr[dc.ColumnName].ToString(), out dtNow) ? dr[dc.ColumnName].ToString() : dr[dc.ColumnName]));
                }
                result.Add(drow);
            }
            return result;
        }
    }
}
