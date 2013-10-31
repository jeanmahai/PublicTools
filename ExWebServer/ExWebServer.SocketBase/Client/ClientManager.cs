using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using ExWebServer.SocketBase.Protocals;

namespace ExWebServer.SocketBase.Client
{
    public class ClientManager : IClient
    {
        public int nUID { get; set; }
        public int nSessionID { get; set; }
        private static int _SessionIDSeed;
        private ClientManagerType _Status = ClientManagerType.Disconnected;
        private Socket _Socket = null;
        private DateTime _dtLastHeartBeat = DateTime.MinValue;
        public byte[] _szBuffer = new byte[4096];
        public ClientManagerType Status
        {
            get { return this._Status; }
            set { this._Status = value; }
        }
        public DateTime dtLastHeart
        {
            get { return _dtLastHeartBeat; }
            set { _dtLastHeartBeat = value; }
        }
        public Socket Socket
        {
            get { return _Socket; }
        }

        /// <summary>
        /// 信任连接
        /// </summary>
        public bool IsTrustIP { get; set; }
        /// <summary>
        /// 经过用户身份验证,此时nUID真实有效
        /// </summary>
        public bool IsAuthedUser { get; set; }
        /// <summary>
        /// 保持长连接
        /// </summary>
        public bool IsKeepAlive { get; set; }

        private int GetSessionID()
        {
            if (_SessionIDSeed < 1900000000 && _SessionIDSeed > 0)
                return Interlocked.Increment(ref _SessionIDSeed);
            else
                return Interlocked.Exchange(ref _SessionIDSeed, 1);
        }

        public IPAddress IP
        {
            get
            {
                if (this._Socket != null)
                    return ((IPEndPoint)this._Socket.RemoteEndPoint).Address;
                else
                    return IPAddress.None;
            }
        }
        public int Port
        {
            get
            {
                if (this._Socket != null)
                    return ((IPEndPoint)this._Socket.RemoteEndPoint).Port;
                else
                    return -1;
            }
        }

        public ClientManager()
        {
            this.nSessionID = GetSessionID();
            this.nUID = this.nSessionID;

            this.dtLastHeart = DateTime.Now;

            this.IsTrustIP = this.IsAuthedUser = this.IsKeepAlive = false;
        }

        public ClientManager(Socket socket)
        {
            this.nSessionID = socket.RemoteEndPoint.Serialize().ToString().GetHashCode();
            this.nUID = this.nSessionID;

            this.Status = ClientManagerType.Disconnected;
            this.dtLastHeart = DateTime.Now;
            this._Socket = socket;

            this.IsTrustIP = this.IsAuthedUser = this.IsKeepAlive = false;
            this.IsTrustIP = true;
        }

        public HttpRawMessage GetHttpRawMessage(int receivedBytes)
        {
            HttpRawMessage result = null;

            if (receivedBytes < 1)
                return result;

            return HttpRawMessage.ParseFromBytes(_szBuffer, 0, receivedBytes);
        }
    }
}
