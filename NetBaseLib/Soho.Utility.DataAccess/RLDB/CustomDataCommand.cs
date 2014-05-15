using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Soho.Utility.DataAccess.Database.Config;
using System.Data;

namespace Soho.Utility.DataAccess
{
    public class CustomDataCommand : DataCommand
    {
        internal CustomDataCommand(DataCommandConfig config)
            : base(config)
        {

        }

        #region Add Parameter

        public void AddInputParameter(string name, DbType dbType, object value)
        {
            this.InnerAddInputParameter(name, dbType, value);
        }

        public void AddInputParameter(string name, DbType dbType)
        {
            this.InnerAddInputParameter(name, dbType);
        }

        public void AddOutParameter(string name, DbType dbType, int size)
        {
            this.InnerAddOutParameter(name, dbType, size);
        }

        #endregion

        public CommandType CommandType
        {
            get { return Config.CommandType; }
            set { Config.CommandType = value; }
        }

        public string CommandText
        {
            get { return Config.CommandText; }
            set { Config.CommandText = value; }
        }

        public int CommandTimeout
        {
            get { return Config.TimeOut; }
            set { Config.TimeOut = value; }
        }

        public string DatabaseAliasName
        {
            get { return Config.Database; }
            set { Config.Database = value.ToString(); }
        }
    }
}
