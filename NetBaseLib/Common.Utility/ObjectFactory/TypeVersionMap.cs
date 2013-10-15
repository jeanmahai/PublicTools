using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Common.Utility
{
    [XmlRoot("map")]
    public class TypeVersionMap
    {
        [XmlAttribute("type")]
        public string Type
        {
            get;
            set;
        }
        [XmlAttribute("version")]
        public string Version
        {
            get;
            set;
        }
    }

}
