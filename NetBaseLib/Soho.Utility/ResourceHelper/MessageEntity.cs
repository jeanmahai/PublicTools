using System.Xml.Serialization;

namespace Soho.Utility
{
    public class MessageEntity
    {
        [XmlAttribute("name")]
        public string KeyName;

        [XmlText]
        public string Text;
    }
}
