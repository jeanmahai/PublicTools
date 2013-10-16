using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;

namespace Common.Utility.DataAccess.Database.Config
{
    public class DataAccessSection : IConfigurationSectionHandler
    {
        private const string SQL_CONFIG_LIST_FILE_ATTR = "sqlConfigListFile";
        private const string DATABASE_LIST_FILE_ATTR = "databaseListFile";

        public object Create(object parent, object configContext, XmlNode section)
        {
            DataAccessSetting da = new DataAccessSetting();
            if (section != null && section.Attributes != null)
            {
                if (section.Attributes[SQL_CONFIG_LIST_FILE_ATTR] != null
                    && section.Attributes[SQL_CONFIG_LIST_FILE_ATTR].Value != null
                    && section.Attributes[SQL_CONFIG_LIST_FILE_ATTR].Value.Trim().Length > 0)
                {
                    da.SqlConfigListFilePath = section.Attributes[SQL_CONFIG_LIST_FILE_ATTR].Value.Trim();
                }
                if (section.Attributes[DATABASE_LIST_FILE_ATTR] != null
                    && section.Attributes[DATABASE_LIST_FILE_ATTR].Value != null
                    && section.Attributes[DATABASE_LIST_FILE_ATTR].Value.Trim().Length > 0)
                {
                    da.DatabaseListFilePath = section.Attributes[DATABASE_LIST_FILE_ATTR].Value.Trim();
                }
            }
            return da;
        }
    }

    public class DataAccessSetting
    {
        public string SqlConfigListFilePath
        {
            get;
            set;
        }

        public string DatabaseListFilePath
        {
            get;
            set;
        }
    }
}
