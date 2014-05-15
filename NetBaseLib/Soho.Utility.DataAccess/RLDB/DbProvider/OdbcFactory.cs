using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Odbc;

namespace Soho.Utility.DataAccess.Database.DbProvider
{
    internal class OdbcFactory : IDbFactory
    {
        private static OdbcFactory s_Instance = new OdbcFactory();

        public static OdbcFactory Instance
        {
            get
            {
                return s_Instance;
            }
        }

        private OdbcFactory()
        {

        }

        #region IDbFactory Members

        public System.Data.Common.DbCommand CreateCommand()
        {
            return new OdbcCommand();
        }

        public System.Data.Common.DbConnection CreateConnection()
        {
            return new OdbcConnection();
        }

        public System.Data.Common.DbConnection CreateConnection(string connectionString)
        {
            return new OdbcConnection(connectionString);
        }

        public System.Data.Common.DbDataAdapter CreateDataAdapter()
        {
            return new OdbcDataAdapter();
        }

        public System.Data.Common.DbParameter CreateParameter()
        {
            return new OdbcParameter();
        }

        #endregion
    }
}
