using System.Xml.Serialization;

namespace Common.Utility
{
    public class MessageEntity
    {
        [XmlAttribute("name")]
        public string KeyName;

        [XmlText]
        public string Text;
    }
}
