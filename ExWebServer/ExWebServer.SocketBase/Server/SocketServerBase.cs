using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using ExWebServer.SocketBase.Client;
using ExWebServer.SocketBase.Protocals;
using Common.Utility;

namespace ExWebServer.SocketBase.Server
{
    public class SocketServerBase
    {
        public ISocketServerConfigure _Config = null;
        public Hashtable _Clients = Hashtable.Synchronized(new Hashtable(10000));

        //Listening Socket 
        protected Socket _ServerSock;
        protected long _LocalhostIP = IPUtility.GetIPIntValue("127.0.0.1");

        Timer _bwHeartListenerTimer = null;
        TimerCallback _bwHeartListener = null;

        public bool IfLoadSettings { get; set; }

        private MessageSender[] MsgDeliver = null;
        private static readonly object OutingMsgLock = new object();

        /// <summary>
        /// 使用默认的config.xml来加载服务器设置
        /// </summary>
        /// <returns></returns>
        public virtual bool LoadConfig()
        {
            string configFile = "config.xml";
            _Config = new SocketServerConfigure();
            if (!_Config.LoadConfig(configFile))
                return false;

            return true;
        }

        public virtual void DebugMessage(string msg)
        {
            try
            {
                //DebugLogger.Debug(msg);
            }
            catch { }
        }

        public virtual void BackupMessage(string msg)
        {
            try
            {
                //BackLogger.Debug(msg);
            }
            catch { }
        }

        public virtual void CloseAll()
        {
            try { if (_bwHeartListenerTimer != null) _bwHeartListenerTimer.Dispose(); }
            catch { }
            finally { _bwHeartListenerTimer = null; }

            lock (this._Clients.SyncRoot)
            {
                if (this._Clients.Keys.Count > 0)
                {
                    int[] keys = new int[this._Clients.Keys.Count];
                    this._Clients.Keys.CopyTo(keys, 0);
                    ClientManager clmngr = null;
                    for (int i = 0; i < keys.Length; i++)
                    {
                        if (this._Clients.ContainsKey(keys[i]))
                        {
                            clmngr = this._Clients[keys[i]] as ClientManager;
                            if (clmngr != null && clmngr.Socket.Connected)
                            {
                                try
                                {
                                    clmngr.Socket.Shutdown(SocketShutdown.Both);
                                }
                                finally { clmngr.Socket.Close(); }
                            }
                        }
                    }
                    this._Clients.Clear();
                }
            }

            if (_ServerSock != null)
            {
                try { _ServerSock.Shutdown(SocketShutdown.Both); }
                catch { }
                finally { _ServerSock.Close(); }
            }
            GC.Collect();
        }

        protected virtual void Start()
        {
            Start(null);
        }
        protected virtual void Start(ISocketServerConfigure config)
        {
            if (config != null)
                this._Config = config;

            if (this._Config == null)
                return;

            _ServerSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _ServerSock.Bind(new IPEndPoint(IPAddress.Any, this._Config.Port));	    //  监听全部IP
            _ServerSock.Listen(_Config.Backlog);                                                    //  排队数

            //建立socket连接的callback调用
            _ServerSock.BeginAccept(new AsyncCallback(OnConnectRequest), _ServerSock);

            AutoResetEvent autoEvent = new AutoResetEvent(false);
            _bwHeartListener = new TimerCallback(this.StartHeartListening);
            _bwHeartListenerTimer = new Timer(_bwHeartListener, autoEvent, 1000, 500);

            if (MsgDeliver == null)
                MsgDeliver = new MessageSender[this._Config.SendThreads];

            for (int i = 0; i < MsgDeliver.Length; i++)
            {
                MsgDeliver[i] = new MessageSender();
                MsgDeliver[i].ShutDownClient += new ShutDownClientEventHandler(ShutdownClientEventHanlder);
            }

            Console.WriteLine("*** {0}(Port:{1}) Started {2} *** ", this._Config.ServerName, this._Config.Port, DateTime.Now.ToString("G"));
        }

        private bool AppendToOutMessageQueue(QueuedOutMessage msg)
        {
            try
            {
                if (msg == null || msg.Client == null || MsgDeliver == null || MsgDeliver.Length < 1)
                    return false;
                //int idx = msg.Client.nUID % MESSAGE_DELIVER_THREADS;
                int idx = (int)(msg.Client.nUID % this._Config.SendThreads);
                if (idx >= 0 && idx < MsgDeliver.Length && MsgDeliver[idx] != null)
                {
                    MsgDeliver[idx].SendAsc(msg);
                }
            }
            catch { }
            return true;
        }

        private bool AppendToOutMessageQueue(QueuedOutMessage[] msgs)
        {
            if (msgs != null && msgs.Length > 0)
            {
                foreach (QueuedOutMessage msg in msgs)
                {
                    AppendToOutMessageQueue(msg);
                }
                return true;
            }
            return false;
        }

        internal virtual void StartHeartListening(Object stateInfo)
        {
            const int HEARTBEAT_INTERVAL = 5;
            try
            {
                if (this._Clients.Count > 0)
                {
                    List<ClientManager> expiredClient = null;
                    lock (this._Clients.SyncRoot)
                    {
                        if (this._Clients.Count > 0)
                        {
                            expiredClient = new List<ClientManager>(this._Clients.Count / 5 + 10);

                            foreach (DictionaryEntry item in this._Clients)
                            {
                                ClientManager clmngr = item.Value as ClientManager;
                                if (clmngr.dtLastHeart.AddSeconds(HEARTBEAT_INTERVAL) < DateTime.Now)
                                {
                                    expiredClient.Add(clmngr);
                                }
                            }
                        }
                    }

                    if (expiredClient != null && expiredClient.Count > 0)
                    {
                        foreach (ClientManager clmngr in expiredClient)
                        {
                            ShutdownClient(clmngr);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string strError = ex.Message;
                DebugMessage(string.Format("心跳监测线程1故障:{0}", ex.Message));
            }
            finally
            {

            }
        }

        protected virtual void OnConnectRequest(IAsyncResult ar)
        {
            Socket listener = null;
            try
            {
                listener = (Socket)ar.AsyncState;
                CreateNewClient(listener.EndAccept(ar));
            }

            catch (Exception ex)
            {
                Console.WriteLine(string.Format("OnConnectRequest Error:{0}", ex.Message));
            }
            finally
            {
                try
                {
                    listener.BeginAccept(new AsyncCallback(OnConnectRequest), listener);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("BeginAccept err:{0}", ex.Message));
                }
                finally { }

            }
        }

        protected virtual bool RegistTrustIP(ClientManager clmngr)
        {
            if (clmngr == null)
                return false;

            string ipAddr = ((IPEndPoint)clmngr.Socket.RemoteEndPoint).Address.ToString();
            long remoteIP = IPUtility.GetIPIntValue(ipAddr);
            try
            {
                if (remoteIP == _LocalhostIP)
                    return false;

                if (_Config.TrustRemoteIP != null && _Config.TrustRemoteIP.Count > 0)
                {
                    foreach (long ip in _Config.TrustRemoteIP)
                    {
                        if (ip == remoteIP)
                        {
                            clmngr.IsTrustIP = true;
                            break;
                        }
                    }
                }
            }
            catch { }

            return clmngr.IsTrustIP;
        }

        protected virtual bool RegistUser(ClientManager clmngr, int uid)
        {
            if (clmngr == null || uid < 1 || !this._Clients.ContainsKey(clmngr.nSessionID))
                return false;

            clmngr.IsAuthedUser = true;
            clmngr.nUID = uid;

            return true;
        }

        /// <summary>
        /// 登记连接到临时队列中
        /// </summary>
        /// <param name="clientSock"></param>
        protected virtual void CreateNewClient(Socket clientSock)
        {
            if (clientSock == null)
                return;
            try
            {
                //将该Socket挂入管理池
                ClientManager clmngr = new ClientManager(clientSock);
                RegistTrustIP(clmngr);

                lock (this._Clients.SyncRoot)
                {
                    this._Clients[clmngr.nSessionID] = clmngr;
                }
                SetupReceiveCallback(clmngr);
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error occured when CreateNewClient:{0} ", ex.Message));
            }
        }

        /// <summary>
        /// 从socket队列中移除某项
        /// </summary>
        /// <param name="clmngr"></param>
        protected void RemoveClientItem(ClientManager clmngr)
        {
            if (clmngr == null)
                return;

            try
            {
                lock (this._Clients.SyncRoot)
                {
                    this._Clients.Remove(clmngr.nSessionID);
                }
            }
            catch { }
            finally { clmngr = null; }
        }

        public virtual void ShutdownClient(int sessionID)
        {
            if (this._Clients.ContainsKey(sessionID))
            {
                ShutdownClient(this._Clients[sessionID] as ClientManager);
            }
        }

        private void ShutdownClientEventHanlder(object sender, ClientShutDownEventArgs e)
        {
            if (e == null || e.Client == null)
                return;
            try
            {
                ClientManager clmngr = (ClientManager)e.Client;
                ShutdownClient(clmngr);
            }
            catch { }
        }

        public virtual void ShutdownClient(ClientManager clmngr)
        {
            if (clmngr == null)
                return;
            try
            {
                clmngr.Socket.Shutdown(SocketShutdown.Both);
                clmngr.Socket.Close();
            }
            catch (Exception ex) { string strError = ex.Message; }
            finally
            {
                RemoveClientItem(clmngr);
            }
        }

        protected void SetupReceiveCallback(ClientManager clmngr)
        {
            if (clmngr == null)
                return;
            try
            {
                AsyncCallback recieveData = new AsyncCallback(OnReceivedData);
                clmngr.Socket.BeginReceive(clmngr._szBuffer, 0, clmngr._szBuffer.Length, SocketFlags.None, recieveData, clmngr);
            }
            catch
            {
                ShutdownClient(clmngr);
            }
        }

        protected virtual void OnReceivedData(IAsyncResult ar)
        {
            ClientManager clmngr = null;
            Socket sock = null;
            bool recieving = false;
            try
            {
                clmngr = (ClientManager)ar.AsyncState;
                if (clmngr.Socket == null || !clmngr.Socket.Connected)
                {
                    ShutdownClient(clmngr);
                    return;
                }

                sock = clmngr.Socket;

                SocketError err;

                int nBytesRec = sock.EndReceive(ar, out err);

                if (nBytesRec == 0)
                {
                    ShutdownClient(clmngr);
                    return;
                }

                clmngr.dtLastHeart = DateTime.Now.AddSeconds(60);

                HttpRawMessage request = clmngr.GetHttpRawMessage(nBytesRec);
                if (request != null && request.Length > 0)
                {
                    recieving = HandleRequest(clmngr, request);

                }
                if (recieving)
                    SetupReceiveCallback(clmngr);
            }
            catch (Exception ex)
            {
                string strError = ex.Message;
                ShutdownClient(clmngr);
                Console.WriteLine(string.Format("Unusual error druing Recieve!({0})", ex.Message));
            }
            finally
            {
            }
        }

        public virtual ClientManager GetClientManagerByUID(object sender, int uid)
        {
            if (this._Clients.ContainsKey(uid))
                return this._Clients[uid] as ClientManager;

            return null;
        }

        public virtual CommonResult BroadCastToClient(object sender, ClientNotifEventArgs e)
        {
            CommonResult result = CommonResult.InvalidParams;
            ISocketMessage msg = e.Command;
            int senderUID = e.Sender;
            int[] keys = null;

            try
            {
                if (this._Clients.Keys.Count > 0)
                {
                    //  如果指定了广播受众
                    if (e.Recvs != null && e.Recvs.Length > 0)
                        keys = e.Recvs;
                    else
                    {
                        //  否则群发
                        lock (this._Clients.SyncRoot)
                        {
                            if (this._Clients.Keys.Count < 1)
                                return CommonResult.Success;
                            keys = new int[this._Clients.Keys.Count];
                            this._Clients.Keys.CopyTo(keys, 0);
                        }
                    }

                    if (keys != null && keys.Length > 0)
                    {
                        ClientManager clmngr = null;
                        foreach (int uid in keys)
                        {
                            //  如果发送人是普通用户,则检查其是否在某个用户的黑名单里
                            if (this._Clients.ContainsKey(uid))
                            {
                                clmngr = this._Clients[uid] as ClientManager;
                                AppendToOutMessageQueue(new QueuedOutMessage(clmngr, msg));
                            }
                        }
                    }
                }
                result = CommonResult.Success;

            }
            catch (Exception ex)
            {
                DebugMessage(string.Format("BroadCastToClient Error:{0};Source:{1}", ex.Message, ex.Source));
                result = CommonResult.SystemError;
            }
            finally { }

            return result;
        }

        /// <summary>
        /// 向客户端下发消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public virtual CommonResult NotifyClient(object sender, ClientNotifEventArgs e)
        {
            int[] sessionIDs = e.Recvs;
            ISocketMessage msg = e.Command;
            ClientManager clmngr = null;
            CommonResult result = CommonResult.OtherError;

            try
            {
                if (sessionIDs == null || sessionIDs.Length < 1 || msg == null)
                    return CommonResult.NoData;
                foreach (int sessionID in sessionIDs)
                {
                    if (!_Clients.ContainsKey(sessionID))
                        continue;
                    clmngr = this._Clients[sessionID] as ClientManager;
                    AppendToOutMessageQueue(new QueuedOutMessage(clmngr, msg));
                }

                result = CommonResult.Success;
            }
            catch (Exception ex)
            {
                DebugMessage(string.Format("NotifiClient Error:{0};Source:{1}", ex.Message, ex.Source));
                result = CommonResult.SystemError;
            }
            finally { }

            return result;
        }

        public virtual void SendToClient(int sessionID, ISocketMessage msg)
        {
            if (sessionID <= 0 || msg == null)
                return;

            if (this._Clients.ContainsKey(sessionID))
            {
                ClientManager clmngr = this._Clients[sessionID] as ClientManager;
                SendToClient(clmngr, msg);
            }

        }

        public virtual void SendToClient(ClientManager clmngr, ISocketMessage msg)
        {
            try
            {
                if (clmngr == null || msg == null || msg.Length == 0)
                    return;

                byte[] buf = msg.GetBytes();
                if (buf == null || buf.Length < 1)
                    return;
                int i = clmngr.Socket.Send(buf, buf.Length, SocketFlags.None);
            }
            catch (Exception ex)
            {
                string strError = ex.Message;
                DebugMessage(string.Format("SendToClient Error:{0};Source:{1}", ex.Message, ex.Source));
            }
            finally { }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clmngr"></param>
        /// <param name="request"></param>
        /// <returns>true = 继续接收数据</returns>
        public virtual bool HandleRequest(ClientManager clmngr, ISocketMessage request)
        {
            return true;
        }
    }
}
