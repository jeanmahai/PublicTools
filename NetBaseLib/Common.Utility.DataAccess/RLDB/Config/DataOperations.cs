using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Data;

namespace Common.Utility.DataAccess.Database.Config
{
    [XmlRoot("dataOperations", Namespace = "http://oversea.newegg.com/DataOperation")]
    public partial class DataOperations
    {
        [XmlElement("dataCommand")]
        public DataCommandConfig[] DataCommand
        {
            get;
            set;
        }
    }

    [XmlRoot("dataCommand")]
    public partial class DataCommandConfig
    {
        private CommandType m_CommandType = CommandType.Text;
        private int m_TimeOut = 300;

        [XmlElement("commandText")]
        public string CommandText
        {
            get;
            set;
        }

        [XmlElement("parameters")]
        public Parameters Parameters
        {
            get;
            set;
        }

        [XmlAttribute("name")]
        public string Name
        {
            get;
            set;
        }

        [XmlAttributeAttribute("database")]
        public string Database
        {
            get;
            set;
        }

        [XmlAttributeAttribute("commandType")]
        [DefaultValueAttribute(CommandType.Text)]
        public CommandType CommandType
        {
            get
            {
                return this.m_CommandType;
            }
            set
            {
                this.m_CommandType = value;
            }
        }

        [XmlAttributeAttribute("timeOut")]
        [DefaultValueAttribute(300)]
        public int TimeOut
        {
            get
            {
                return this.m_TimeOut;
            }
            set
            {
                this.m_TimeOut = value;
            }
        }

        public DataCommandConfig Clone()
        {
            DataCommandConfig config = new DataCommandConfig();
            config.CommandText = this.CommandText;
            config.CommandType = this.CommandType;
            config.Database = this.Database;
            config.Name = this.Name;
            config.Parameters = this.Parameters == null ? null : this.Parameters.Clone();
            config.TimeOut = this.TimeOut;
            return config;
        }
    }

    [XmlRoot("parameters")]
    public partial class Parameters
    {
        [XmlElement("param")]
        public Param[] Param
        {
            get;
            set;
        }

        public Parameters Clone()
        {
            Parameters p = new Parameters();
            if (this.Param != null)
            {
                p.Param = new Param[this.Param.Length];
                for (int i = 0; i < this.Param.Length; i++)
                {
                    p.Param[i] = this.Param[i].Clone();
                }
            }
            return p;
        }
    }

    [XmlRoot("param")]
    public partial class Param
    {
        private ParameterDirection m_Direction = ParameterDirection.Input;
        private int m_Size = -1;
        private byte m_Scale = 0;
        private byte m_Precision = 0;

        public Param Clone()
        {
            Param p = new Param();
            p.DbType = this.DbType;
            p.Direction = this.Direction;
            p.Name = this.Name;
            p.Precision = this.Precision;
            p.Property = this.Property;
            p.Scale = this.Scale;
            p.Size = this.Size;
            return p;
        }

        [XmlAttribute("name")]
        public string Name
        {
            get;
            set;
        }

        [XmlAttribute("dbType")]
        public DbType DbType
        {
            get;
            set;
        }

        [XmlAttribute("direction")]
        [DefaultValue(ParameterDirection.Input)]
        public ParameterDirection Direction
        {
            get
            {
                return this.m_Direction;
            }
            set
            {
                this.m_Direction = value;
            }
        }

        [XmlAttribute("size")]
        [DefaultValue(-1)]
        public int Size
        {
            get
            {
                return this.m_Size;
            }
            set
            {
                this.m_Size = value;
            }
        }

        [XmlAttribute("scale")]
        [DefaultValue(0)]
        public byte Scale
        {
            get
            {
                return this.m_Scale;
            }
            set
            {
                this.m_Scale = value;
            }
        }

        [XmlAttribute("precision")]
        [DefaultValue(0)]
        public byte Precision
        {
            get
            {
                return this.m_Precision;
            }
            set
            {
                this.m_Precision = value;
            }
        }

        [XmlAttribute("property")]
        public string Property
        {
            get;
            set;
        }
    }
}
