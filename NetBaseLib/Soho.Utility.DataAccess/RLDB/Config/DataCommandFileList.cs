using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Soho.Utility.DataAccess.Database.Config
{
    /// <summary>
    /// configuration that contains the list of DataCommand configuration files.
    /// This class is for internal use only.
    /// </summary>
    [XmlRoot("dataCommandFiles", Namespace = "http://www.common.com/DbCommandFiles")]
    public class DataCommandFileList
    {
        public class DataCommandFile
        {
            [XmlAttribute("name")]
            public string FileName
            {
                get;
                set;
            }
        }

        [XmlElement("file")]
        public DataCommandFile[] FileList
        {
            get;
            set;
        }
    }
}
