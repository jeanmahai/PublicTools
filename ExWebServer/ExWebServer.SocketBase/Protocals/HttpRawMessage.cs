using System.Text;

namespace ExWebServer.SocketBase.Protocals
{
    public class HttpRawMessage : ISocketMessage
    {
        private string _Message = string.Empty;

        public HttpRawMessage() { }
        public HttpRawMessage(string msg)
        {
            _Message = !string.IsNullOrEmpty(msg) ? msg : string.Empty;
        }

        public int Length { get { return !string.IsNullOrEmpty(_Message) ? _Message.Length : 0; } }

        public override string ToString()
        {
            return _Message;
        }

        public byte[] GetBytes()
        {
            return Encoding.UTF8.GetBytes(_Message);
        }

        public static HttpRawMessage ParseFromBytes(byte[] buf)
        {
            return ParseFromBytes(buf, 0, buf.Length);
        }

        public static HttpRawMessage ParseFromBytes(byte[] buf, int index, int count)
        {
            if (buf == null || count < 1 || index < 0 || index + count > buf.Length)
                return new HttpRawMessage();

            return new HttpRawMessage(Encoding.UTF8.GetString(buf, index, count));
        }
    }
}
