using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Common.Utility.DataAccess.Database.Config
{
    [XmlRoot("databaseList", Namespace = "http://www.common.com/DatabaseList")]
    public class DatabaseList
    {
        [XmlElement("database")]
        public DatabaseInstance[] DatabaseInstances
        {
            get;
            set;
        }
    }

    [XmlRoot("database")]
    public class DatabaseInstance
    {
        [XmlAttribute("name")]
        public string Name
        {
            get;
            set;
        }

        [XmlAttribute("type")]
        public string Type
        {
            get;
            set;
        }

        [XmlElement("connectionString")]
        public string ConnectionString
        {
            get;
            set;
        }
    }
}
