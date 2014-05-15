using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace Soho.Utility.DataAccess.Database.DbProvider
{
    internal class SqlServerFactory : IDbFactory
    {
        private static SqlServerFactory s_Instance = new SqlServerFactory();

        public static SqlServerFactory Instance
        {
            get
            {
                return s_Instance;
            }
        }

        private SqlServerFactory()
        {

        }

        #region IDbFactory Members

        public System.Data.Common.DbCommand CreateCommand()
        {
            return new SqlCommand();
        }

        public System.Data.Common.DbConnection CreateConnection()
        {
            return new SqlConnection();
        }

        public System.Data.Common.DbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public System.Data.Common.DbDataAdapter CreateDataAdapter()
        {
            return new SqlDataAdapter();
        }

        public System.Data.Common.DbParameter CreateParameter()
        {
            return new SqlParameter();
        }

        #endregion
    }
}
