using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;

namespace Soho.Utility.DataAccess.Database.DbProvider
{
    internal class OleDbFactory : IDbFactory
    {
        private static OleDbFactory s_Instance = new OleDbFactory();

        public static OleDbFactory Instance
        {
            get
            {
                return s_Instance;
            }
        }

        private OleDbFactory()
        {

        }

        #region IDbFactory Members

        public System.Data.Common.DbCommand CreateCommand()
        {
            return new OleDbCommand();
        }

        public System.Data.Common.DbConnection CreateConnection()
        {
            return new OleDbConnection();
        }

        public System.Data.Common.DbConnection CreateConnection(string connectionString)
        {
            return new OleDbConnection(connectionString);
        }

        public System.Data.Common.DbDataAdapter CreateDataAdapter()
        {
            return new OleDbDataAdapter();
        }

        public System.Data.Common.DbParameter CreateParameter()
        {
            return new OleDbParameter();
        }

        #endregion
    }
}
