using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Soho.Utility.DataAccess.KeyValue
{
    public class KeyValueDataAccessSetting
    {
        public string DataCategory { get; set; }

        public KeyValueDataAccessMode AccessMode { get; set; }

        public string DataCategoryPath { get; set; }
    }

    public enum KeyValueDataAccessMode
    {
        XMLFile,
        SQLDB,
        MongoDB
    }
}
