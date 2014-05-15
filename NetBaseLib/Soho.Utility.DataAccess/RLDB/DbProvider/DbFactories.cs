using System;
using System.Collections.Generic;
using System.Text;

namespace Soho.Utility.DataAccess.Database.DbProvider
{
    internal static class DbFactories
    {
        public static IDbFactory GetFactory(ProviderType providerType)
        {
            switch (providerType)
            {
                case ProviderType.SqlServer:
                    return SqlServerFactory.Instance;
                case ProviderType.Odbc:
                    return OdbcFactory.Instance;
                default:
                    return OleDbFactory.Instance;
            }
        }
    }

    public enum ProviderType
    {
        SqlServer,
        Odbc,
        OleDb
    }
}
