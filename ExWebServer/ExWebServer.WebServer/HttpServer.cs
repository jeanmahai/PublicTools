using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using ExWebServer.WebServer.Comet;
using ExWebServer.WebServer.HttpLib;
using ExWebServer.SocketBase.Client;
using ExWebServer.SocketBase.Server;
using ExWebServer.SocketBase.Protocals;
using ExWebServer.WebServer.Handler.SSO;
using ExWebServer.WebServer.Module.OnlineUser;
//using WebCommon.Messaging.MQHelper;

namespace ExWebServer.WebServer
{
    public class HttpServer : SocketServerBase
    {
        const int MAX_POST_LENGTH = 4 * 1024 * 1024;
        const int MAX_HEADER_LINES = 30;
        const int MAX_QUEUED_REQUESTS = 5;

        const string ONLINENOTIFICATION_MQNAME = "onlinenotification";

        private Hashtable _ContextTable = Hashtable.Synchronized(new Hashtable(10000));
        private Dictionary<Comet.CometCommandID, Comet.CometCommandHandlerPipeline> _CometCommands = new Dictionary<WebServer.Comet.CometCommandID, WebServer.Comet.CometCommandHandlerPipeline>(50);
        private long maxPostLength = MAX_POST_LENGTH;
        private int maxHeaders = MAX_HEADER_LINES;
        private int maxQueuedRequests = MAX_QUEUED_REQUESTS;
        //private MSMQReceiver _OnlineNotifySvc = null;
        private WebServer.Module.ServerStat.ServerStatManager _ServerStatManager = null;

        public new HttpServerConfigure _Config = null;

        public Hashtable _Users = Hashtable.Synchronized(new Hashtable(10000));
        public Queue mqBodys = Queue.Synchronized(new Queue(10000));

        Timer _tmWorker = null;
        TimerCallback _tmWorkerCallback = null;

        Timer _tmStatSaver = null;
        TimerCallback _tmStatSaverCallback = null;

        private long _MonitorCheckSuccessTimes = 0;
        private long _MonitorCheckErrorTimes = 0;
        private long _RequestTimes = 0;

        public override bool LoadConfig()
        {
            _Config = new HttpServerConfigure();

            //设置主机头
            _Config.HostList = new List<string>();
            _Config.HostList.Add("127.0.0.1:800");
            _Config.HostList.Add("svc.exwebserver.com:800");
            _Config.Port = 800;
            _Config.SendThreads = 10;

            return true;
        }

        /// <summary>
        /// 加载本服务器实例所支持的Command
        /// </summary>
        /// <returns></returns>
        private bool LoadCometCommands()
        {

            if (_CometCommands == null)
                _CometCommands = new Dictionary<WebServer.Comet.CometCommandID, WebServer.Comet.CometCommandHandlerPipeline>(50);
            else
                _CometCommands.Clear();

            if (_Config.ComandList != null && _Config.ComandList.Count > 0)
            {
                foreach (Comet.CometCommand cmd in _Config.ComandList)
                {
                    switch (cmd.CommandID)
                    {
                        case CometCommandID.Default:
                            RegistCommandHandler(cmd, new WebServer.Handler.SimpleInstantRequestHandler(this, cmd));
                            break;
                        case CometCommandID.QueryOnlineUser:
                            RegistCommandHandler(cmd, new WebServer.Handler.QueryUserOnline(this, cmd));
                            break;
                        case CometCommandID.QueryOnlineUserList:
                            RegistCommandHandler(cmd, new WebServer.Handler.QueryUserOnlineList(this, cmd));
                            break;
                    }
                }
            }

            Comet.CometCommand command = null;
            command = new WebServer.Comet.CometCommand(WebServer.Comet.CometCommandID.Default);
            command.Permissions = Comet.CometCommand.PERMISSION_ANONYMOUS;
            command.RequireKeepAlive = false;
            RegistCommandHandler(command, new WebServer.Handler.SimpleInstantRequestHandler(this, command));

            command = new CometCommand(WebServer.Comet.CometCommandID.QueryOnlineUser);
            command.Permissions = Comet.CometCommand.PERMISSION_ANONYMOUS;
            command.RequireKeepAlive = false;
            RegistCommandHandler(command, new WebServer.Handler.QueryUserOnline(this, command));

            command = new CometCommand(WebServer.Comet.CometCommandID.QueryOnlineUserList);
            command.Permissions = Comet.CometCommand.PERMISSION_ANONYMOUS;
            command.RequireKeepAlive = false;
            RegistCommandHandler(command, new WebServer.Handler.QueryUserOnlineList(this, command));

            return true;
        }

        /// <summary>
        /// 从Request中解析command
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private CometCommand GetCommandFromRequest(HttpRequest request)
        {
            CometCommandHandlerPipeline handlers = GetRegistedCommandHandlers(request);
            if (handlers != null)
                return handlers.Command;
            return null;
        }

        /// <summary>
        /// 找到与Request匹配的处理管道
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private CometCommandHandlerPipeline GetRegistedCommandHandlers(HttpRequest request)
        {
            CometCommand command = CometCommand.ParseFromRequest(request);

            return GetRegistedCommandHandlers(command.CommandID);
        }

        /// <summary>
        /// 找到与CommandID匹配的处理管道
        /// </summary>
        /// <param name="commandID"></param>
        /// <returns></returns>
        private CometCommandHandlerPipeline GetRegistedCommandHandlers(CometCommandID commandID)
        {
            if (_CometCommands.ContainsKey(commandID))
                return _CometCommands[commandID];
            else
                return null;
        }

        private bool CheckKeepAliveChannelExists(int uid)
        {
            return _ContextTable != null && _ContextTable.ContainsKey(uid) ? true : false;
        }

        /// <summary>
        /// 将本次KeepAlive的请求登记到处理队列中
        /// </summary>
        /// <param name="clmngr"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private HttpContext RegistKeepAliveRequestContext(ClientManager clmngr, HttpRequest request)
        {
            HttpContext context = null;

            if (clmngr == null || !clmngr.IsAuthedUser || !this._Clients.ContainsKey(clmngr.nSessionID) || request == null || request.IsRequestError)
                return context;

            clmngr.IsKeepAlive = true;

            //Context Table使用User ID作为Key
            int uid = clmngr.nUID;

            lock (_ContextTable.SyncRoot)
            {
                if (_ContextTable.ContainsKey(uid))
                {
                    context = _ContextTable[uid] as HttpContext;
                    //将请求添加到队列
                    context.AppendRequest(request);
                }
                else
                {
                    context = new HttpContext(this, clmngr, request);
                    //将请求添加到队列
                    _ContextTable[uid] = context;
                }
            }

            return context;
        }

        /// <summary>
        /// 注销长连接登记
        /// </summary>
        /// <param name="uid"></param>
        public void UnRegistKeepAliveRequestContext(int uid)
        {
            //Context Table使用User ID作为Key
            lock (_ContextTable.SyncRoot)
            {
                _ContextTable.Remove(uid);
            }
        }

        private HttpContext GetContext(int uid)
        {
            if (_ContextTable.ContainsKey(uid))
                return (HttpContext)_ContextTable[uid];

            return null;
        }

        private HttpContext GetContext(ClientManager clmngr)
        {
            if (clmngr != null)
                return GetContext(clmngr.nUID);

            return null;
        }

        private void HandleErrorRequest(ClientManager clmngr, HttpRequest request)
        {
            if (clmngr == null || request == null)
                return;

            HttpContext context = new HttpContext(this, clmngr, request);
            HttpResponse response = context.Response;
            response.Write(request.Error.Message);
        }

        private void RegistUser(ClientManager clmngr, HttpRequest request)
        {
            const string X_SSO_KEYNAME_USERID = "User_id";
            const string X_SSO_KEYNAME_NICKNAME = "User_Nick";
            const string X_SSO_KEYNAME_USERCOOKIEKEY = "UserCookieKey";

            RegistUser(clmngr, 100001);
            if (clmngr == null || request == null || request.Cookies == null)
                return;
            //  读取sso cookie

            string cookieUserID = request.Cookies[X_SSO_KEYNAME_USERID];
            string cookieUserNick = request.Cookies[X_SSO_KEYNAME_NICKNAME];
            string cookieKey = request.Cookies[X_SSO_KEYNAME_USERCOOKIEKEY];
            //string ssoCookie = request.Cookies[X_SSO_KEYNAME_S1];
            //SSOCore ssoHandler = new WebServer.Handler.SSO.SSOCore();
            JJSSOCore ssoHandler = new JJSSOCore();
            //  验证ｓｓｏ
            //int ssoResult = ssoHandler.CheckSSOCookie(ssoCookie);
            int ssoResult = ssoHandler.CheckSSOCookie(cookieUserID, cookieUserNick, cookieKey);

            if (ssoResult == 1 && ssoHandler.UserInfoFromCookie.nUID > 0)
            {
                RegistUser(clmngr, ssoHandler.UserInfoFromCookie.nUID);
            }
        }

        private bool CheckRequestHost(HttpRequest request)
        {
            if (request == null)
                return false;

            if (_Config == null || _Config.HostList == null || _Config.HostList.Count < 1)
                return true;

            foreach (string host in _Config.HostList)
            {
                if (string.Compare(request.Host, host, true) == 0)
                    return true;
            }
            return false;
        }

        protected override bool RegistUser(ClientManager clmngr, int uid)
        {
            if (clmngr == null || uid < 1 || !this._Clients.ContainsKey(clmngr.nSessionID))
                return false;

            clmngr.IsAuthedUser = true;
            clmngr.nUID = uid;

            if (_Users.ContainsKey(uid))
            {
                UserOnlineInfo onlineUser = _Users[uid] as UserOnlineInfo;
                if (!onlineUser.IsOnline)
                {
                    onlineUser.SetLogin();
                }
                onlineUser.RefreshActiveTime();
            }
            else
            {
                UserOnlineInfo onlineUser = new UserOnlineInfo(uid);
                onlineUser.SetLogin();
                lock (_Users.SyncRoot)
                {
                    _Users[uid] = onlineUser;
                }
            }
            return true;
        }

        public void RegistUserPosition(int uid, int siteID, int posID)
        {
            if (_Users.ContainsKey(uid))
            {
                UserOnlineInfo onlineUser = _Users[uid] as UserOnlineInfo;
                onlineUser.SiteID = siteID;
                onlineUser.PosID = posID;
            }
        }

        /// <summary>
        /// Post内容最大长度
        /// </summary>
        public long MaxPostLength
        {
            get
            {
                return maxPostLength;
            }
            set
            {
                maxPostLength = value > 0 && value < MAX_POST_LENGTH ? value : MAX_POST_LENGTH;
            }
        }

        /// <summary>
        /// Header最大个数
        /// </summary>
        public int MaxHeaderLines
        {
            get { return maxHeaders; }
            set { maxHeaders = value > 0 && value <= MAX_HEADER_LINES ? value : MAX_HEADER_LINES; }
        }

        /// <summary>
        /// 每个客户端的请求队列最大条目数
        /// </summary>
        public int MaxQueuedRequests
        {
            get { return maxQueuedRequests; }
            set { maxQueuedRequests = value > 0 && value <= MAX_QUEUED_REQUESTS ? value : MAX_QUEUED_REQUESTS; }
        }

        public HttpServer()
        {
            _Config = new HttpServerConfigure();
        }


        public bool StartHttpServer()
        {

            LoadConfig();

            LoadCometCommands();

            //  初始化MQ接收机
            //_OnlineNotifySvc = new MSMQReceiver(ONLINENOTIFICATION_MQNAME);
            //_OnlineNotifySvc.RegisterMessageHandle(new ProcessMessageHandle(HandleMQMessageArrived));
            //_OnlineNotifySvc.StartReceiver();

            base.Start(_Config);

            if (_Config.HostList != null && _Config.HostList.Count > 0)
            {
                Console.WriteLine("=============These hosts are running:=============");
                foreach (string host in _Config.HostList)
                {
                    Console.WriteLine(host);
                }
                Console.WriteLine("--------------------------------------------------");
            }
            _ServerStatManager = new WebServer.Module.ServerStat.ServerStatManager();

            AutoResetEvent autoEvent = new AutoResetEvent(false);

            _tmWorkerCallback = new TimerCallback(this.StartWorkerProcessing);
            _tmWorker = new Timer(_tmWorkerCallback, autoEvent, 1000, 5000);

            _tmStatSaverCallback = new TimerCallback(this.StartStatSaverProcessing);
            _tmStatSaver = new Timer(_tmStatSaverCallback, autoEvent, 1000, 1000);

            return true;
        }

        internal virtual void StartWorkerProcessing(Object stateInfo)
        {
            //const int HEARTBEAT_INTERVAL = 600;
            try
            {
                UserOnlineInfo onlineUser = null;
                int uid = 0;
                int onlineTimePlus = 0;
                List<int> offLineUIDList = null;

                _MonitorCheckSuccessTimes++;

                if (_Users.Count > 0)
                {
                    offLineUIDList = new List<int>(_Users.Count);

                    lock (_Users.SyncRoot)
                    {
                        if (_Users.Count > 0)
                        {
                            foreach (DictionaryEntry item in _Users)
                            {
                                onlineUser = (UserOnlineInfo)item.Value;
                                uid = (int)item.Key;
                                //if (onlineUser.IsOnline)
                                //    onlineCount++;
                                if (!onlineUser.IsOnline)
                                {
                                    offLineUIDList.Add(uid);
                                    onlineUser.SetLogoff();
                                }
                                //  收集在线时长
                                onlineTimePlus = onlineUser.CollectOnlineTime();
                                if (onlineTimePlus > 0)
                                    _ServerStatManager.SaveUserOnlineTime(uid, onlineTimePlus);

                                //_Users.Remove(uid);
                            }

                            if (offLineUIDList.Count > 0)
                            {
                                foreach (int offLineUID in offLineUIDList)
                                {
                                    _Users.Remove(offLineUID);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string strError = ex.Message;
                //DebugMessage(string.Format("心跳监测线程1故障:{0}", ex.Message));
                _MonitorCheckErrorTimes++;
                Console.WriteLine(string.Format("监测线程工作出现异常({0}):{1}", DateTime.Now.ToString(), ex.Message));
            }
            finally
            {

            }
            Console.Write("\rConn:{0}  Users:{1}    Check:{2}    Req:{3}          \r", _Clients.Count, _Users.Count, _MonitorCheckSuccessTimes, _RequestTimes);
        }

        internal virtual void StartStatSaverProcessing(Object stateInfo)
        {
            //const int HEARTBEAT_INTERVAL = 600;
            try
            {
                int ccu = _Users.Count;
                int[] uids = null;
                //_ServerStatManager.SaveOnlineStat(1, 0, ccu);
                if (_Users.Keys.Count > 0)
                {
                    lock (_Users.SyncRoot)
                    {
                        if (_Users.Keys.Count > 0)
                        {
                            uids = new int[_Users.Keys.Count];
                            _Users.Keys.CopyTo(uids, 0);
                        }
                    }
                    if (uids != null && uids.Length > 0)
                    {
                        Dictionary<long, WebServer.Module.ServerStat.SiteOnlineStat> siteStatsTable = new Dictionary<long, WebServer.Module.ServerStat.SiteOnlineStat>(uids.Length / 4 + 32);
                        long key = 0;
                        int siteID = 0, posID = 0;
                        UserOnlineInfo onlineUser = null;
                        WebServer.Module.ServerStat.SiteOnlineStat siteStat = null;
                        foreach (int uid in uids)
                        {
                            if (_Users.ContainsKey(uid))
                            {
                                onlineUser = (UserOnlineInfo)_Users[uid];

                                if (!onlineUser.IsOnline)
                                    continue;

                                siteID = onlineUser.SiteID;
                                posID = onlineUser.PosID;
                                key = WebServer.Module.ServerStat.ServerStatManager.GetHashKeyOfOnlineStat(siteID, posID);
                                if (siteStatsTable.ContainsKey(key))
                                {
                                    siteStat = siteStatsTable[key];
                                }
                                else
                                {
                                    siteStat = new WebServer.Module.ServerStat.SiteOnlineStat(siteID, posID, 0);
                                    siteStatsTable[key] = siteStat;
                                }
                                siteStat.CCU++;
                            }
                        }
                        key = WebServer.Module.ServerStat.ServerStatManager.GetHashKeyOfOnlineStat(1, 0);
                        siteStatsTable[key] = new WebServer.Module.ServerStat.SiteOnlineStat(1, 0, ccu);
                        _ServerStatManager.BulkSaveOnlineStat(siteStatsTable);
                    }
                }

            }
            catch (Exception ex)
            {
                string strError = ex.Message;
                //DebugMessage(string.Format("心跳监测线程1故障:{0}", ex.Message));
                Console.WriteLine(string.Format("保存在线数据线程工作出现异常({0}):{1}", DateTime.Now.ToString(), ex.Message));
            }
            finally
            {

            }
        }

        public void UnRegistUser(ClientManager clmngr)
        {
            if (clmngr == null || !clmngr.IsAuthedUser)
                return;

            int uid = clmngr.nUID;
            try
            {
                if (_Users.ContainsKey(uid))
                {
                    UserOnlineInfo user = _Users[uid] as UserOnlineInfo;
                    user.SocketOpen = false;
                }
            }
            catch { }
            finally
            {
                clmngr.IsAuthedUser = false;
            }

        }

        public override void ShutdownClient(ClientManager clmngr)
        {
            if (clmngr == null)
                return;

            if (clmngr.IsAuthedUser)
                UnRegistUser(clmngr);

            base.ShutdownClient(clmngr);

            //Console.Write("\r(Shut)Conn:{0}  Users:{1}    Check:{2}    Req:{3}          \r", _Clients.Count, _Users.Count, _MonitorCheckSuccessTimes, _RequestTimes);
        }

        public OnlineCodeValue GetUserOnlineCode(int uid)
        {
            UserOnlineInfo user = GetUserOnlineInfo(uid);
            return user != null ? user.OnlineStatusCode : OnlineCodeValue.Offline;
        }

        public UserOnlineInfo GetUserOnlineInfo(int uid)
        {
            if (!_Users.ContainsKey(uid))
                return null;

            return _Users[uid] as UserOnlineInfo;
        }


        /// <summary>
        /// 登记Handler
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cmdHandler"></param>
        public void RegistCommandHandler(CometCommand command, Handler.IHttpHandler cmdHandler)
        {
            if (command == null || cmdHandler == null || _CometCommands == null)
                return;

            CometCommandHandlerPipeline handlers = null;
            if (_CometCommands.ContainsKey(command.CommandID))
            {
                handlers = _CometCommands[command.CommandID];
                handlers.RegistHandler(cmdHandler);
            }
            else
            {
                handlers = new CometCommandHandlerPipeline(this, command);
                handlers.RegistHandler(cmdHandler);
                _CometCommands[command.CommandID] = handlers;
            }
        }

        public override bool HandleRequest(ClientManager clmngr, ISocketMessage message)
        {
            bool goRecieving = false;
            try
            {
                _RequestTimes++;
                HttpRequest request = HttpRequest.ParseFromRawMessage(this, message.ToString());

                if (!request.IsRequestError)
                {
                    //检查主机头是否符合设置
                    if (!CheckRequestHost(request))
                    {
                        //shutdown client
                        ShutdownClient(clmngr);
                        return goRecieving;
                    }
                    //获取该request的handlers
                    CometCommandHandlerPipeline handlePipeline = GetRegistedCommandHandlers(request);

                    if (handlePipeline == null)
                    {
                        //shutdown client
                        ShutdownClient(clmngr);
                        return goRecieving;
                    }

                    HttpContext context = new HttpContext(this, clmngr, request);

                    CometCommand cometCmd = handlePipeline.Command;
                    request.Command = cometCmd;

                    if (!clmngr.IsAuthedUser)
                    {
                        RegistUser(clmngr, request);
                    }
                    //如果要求长连接
                    if (cometCmd.RequireKeepAlive)
                    {
                        //登记本次请求事务
                        if (RegistKeepAliveRequestContext(clmngr, request) == null)
                        {
                            //shutdown client
                            ShutdownClient(clmngr);
                            return goRecieving;
                        }
                    }
                    //执行处理管道
                    if (handlePipeline.Count > 0)
                    {
                        foreach (Handler.IHttpHandler handler in handlePipeline.Handlers)
                        {
                            if (handler != null)
                            {
                                handler.HandleRequest(clmngr, context);
                            }
                        }
                    }
                    //TestResponse(context);
                }
                else
                {
                    HandleErrorRequest(clmngr, request);
                }
            }
            catch (Exception ex)
            {
                ShutdownClient(clmngr);
                Console.WriteLine(string.Format("Error occured when HandleRequest:{0} ", ex.Message));
            }
            finally { }
            return goRecieving;
        }

        public void TestResponse(HttpContext context)
        {
            if (context == null)
                return;

            HttpResponse response = context.Response;
            HttpRequest request = context.GetNextRequest();
            if (request == null)
                return;

            string query = string.Empty;
            foreach (string name in request.Parameters)
            {
                query += string.Format("{0} = {1}\r\n", name, request.Parameters[name]);
            }
            string path = request.PathUri.AbsolutePath;

            string output = string.Format("{0} \r\n {1} \r\n{2} \r\n{3}", query, path, request.Host, DateTime.Now.ToString());

            DateTime dtEnd = DateTime.Now.AddMilliseconds(3000);
            while (true)
            {
                if (DateTime.Now > dtEnd)
                    break;
                Thread.Sleep(50);
            }
            response.Write(output);
        }

        #region
        //public class TestMSMQ
        //{
        //    public void Run()
        //    {
        //        //模拟消息处理机
        //        MessageProcessor msgProc = new MessageProcessor();
        //        //消息接收机
        //        MSMQReceiver msgReceiver = new MSMQReceiver("Chat");
        //        //msgReceiver.OnMessageArrivedHandle += new ProcessMessageHandle(msgProc.ProcessMessage);
        //        msgReceiver.RegisterMessageHandle(new ProcessMessageHandle(msgProc.ProcessMessage));
        //        msgReceiver.StartReceiver();

        //        //消息发送机
        //        MSMQAgent agent = MSMQAgent.GetMSMQAgent("Chat");
        //        int n = 3;
        //        while (n > 0)
        //        {
        //            agent.SendMessage(n.ToString(), "hello");
        //            n--;
        //        }
        //        Console.ReadLine();
        //    }
        //}
        //public class MessageProcessor
        //{
        //    public void ProcessMessage(string lable, string body)
        //    {
        //        Console.WriteLine("{0} / {1} / {2}", DateTime.Now.ToString(), lable, body);
        //    }
        //}
//        <?xml version="1.0" encoding="UTF-8"?>
//<root>
//  <item name="Chat">
//    <bus0>FormatName:DIRECT=TCP:192.168.1.103\private$\chat0</bus0>
//  </item>
//  <item name="OnlineNotification">
//    <bus0>FormatName:DIRECT=TCP:192.168.10.92\private$\notification</bus0>
//  </item>
//  <item name="Mission">
//    <bus0>FormatName:DIRECT=TCP:192.168.10.92\private$\mission</bus0>
//  </item>
//  <item name="ShareClick">
//    <bus0>FormatName:DIRECT=TCP:192.168.10.92\private$\shareclick</bus0>
//  </item>
//</root>
        #endregion
    }
}
