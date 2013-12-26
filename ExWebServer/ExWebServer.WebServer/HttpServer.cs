using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using ExWebServer.WebServer.Comet;
using ExWebServer.WebServer.HttpLib;
using ExWebServer.SocketBase.Client;
using ExWebServer.SocketBase.Server;
using ExWebServer.SocketBase.Protocals;
using ExWebServer.WebServer.Module.OnlineUser;
using ExWebServer.Logic.Auth;
using ExWebServer.WebServer.Utility;
//using WebCommon.Messaging.MQHelper;

namespace ExWebServer.WebServer
{
    public class HttpServer : SocketServerBase
    {
        #region 预设参数部分

        #region Server参数
        /// <summary>
        /// Post内容最大长度
        /// </summary>
        public long MaxPostLength { get; set; }
        /// <summary>
        /// Header最大个数
        /// </summary>
        public int MaxHeaderLines { get; set; }
        /// <summary>
        /// 每个客户端的请求队列最大条目数
        /// </summary>
        public int MaxQueuedRequests { get; set; }
        public new HttpServerConfigure _Config = null;
        private Hashtable _ContextTable = Hashtable.Synchronized(new Hashtable(10000));
        private Dictionary<Comet.CometCommandID, Comet.CometCommandHandlerPipeline> _CometCommands = new Dictionary<WebServer.Comet.CometCommandID, WebServer.Comet.CometCommandHandlerPipeline>(50);
        #endregion

        #region 服务器状态和用户参数
        private WebServer.Module.ServerStat.ServerStatManager _ServerStatManager = null;
        public Hashtable _Users = Hashtable.Synchronized(new Hashtable(10000));
        #endregion

        #region Console参数
        Timer _tmWorker = null;
        TimerCallback _tmWorkerCallback = null;
        Timer _tmStatSaver = null;
        TimerCallback _tmStatSaverCallback = null;
        private long _MonitorCheckSuccessTimes = 0;
        private long _MonitorCheckErrorTimes = 0;
        private long _RequestTimes = 0;
        #endregion

        #endregion

        #region HttpServer部分

        /// <summary>
        /// 初始化
        /// </summary>
        public HttpServer()
        {
            _Config = new HttpServerConfigure();
        }
        /// <summary>
        /// 启动Server
        /// </summary>
        /// <returns></returns>
        public bool StartHttpServer()
        {
            LoadConfig();
            LoadCometCommands();
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

        #endregion

        #region 监控部分

        /// <summary>
        /// 监控线程
        /// </summary>
        /// <param name="stateInfo"></param>
        internal virtual void StartWorkerProcessing(Object stateInfo)
        {
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
                                if (!onlineUser.IsOnline)
                                {
                                    offLineUIDList.Add(uid);
                                    onlineUser.SetLogoff();
                                }
                                //  收集在线时长
                                onlineTimePlus = onlineUser.CollectOnlineTime();
                                //if (onlineTimePlus > 0)
                                //    _ServerStatManager.SaveUserOnlineTime(uid, onlineTimePlus);
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
                _MonitorCheckErrorTimes++;
                Console.WriteLine(string.Format("监测线程工作出现异常({0}):{1}", DateTime.Now.ToString(), ex.Message));
            }
            finally
            { }
            Console.Write("\rConn:{0}  Users:{1}    Check:{2}    Req:{3}          \r", _Clients.Count, _Users.Count, _MonitorCheckSuccessTimes, _RequestTimes);
        }
        /// <summary>
        /// 用户和服务器在线数据保存线程
        /// </summary>
        /// <param name="stateInfo"></param>
        internal virtual void StartStatSaverProcessing(Object stateInfo)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("保存在线数据线程工作出现异常({0}):{1}", DateTime.Now.ToString(), ex.Message));
            }
            finally
            { }
        }

        #endregion

        public override bool LoadConfig()
        {
            var configs =  ConfigHelper.LoadServerConfig();
            _Config = new HttpServerConfigure();

            //设置主机头
            _Config.HostList = configs.HostList;
            _Config.Port = configs.Port;
            _Config.SendThreads = configs.SendThreads;

            MaxPostLength = configs.MaxPostLength;
            MaxHeaderLines = configs.MaxHeaderLines;
            MaxQueuedRequests = configs.MaxQueuedRequests;

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
                            RegistCommandHandler(cmd, new WebServer.Handler.Default(this, cmd));
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
            command.RequireKeepAlive = true;
            RegistCommandHandler(command, new WebServer.Handler.Default(this, command));

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

        #region 用户处理
        /// <summary>
        /// 注册用户
        /// </summary>
        /// <param name="clmngr"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 注册用户请求
        /// </summary>
        /// <param name="clmngr"></param>
        /// <param name="request"></param>
        private void RegistUser(ClientManager clmngr, HttpRequest request)
        {
            if (clmngr == null || request == null)
                return;
            int userID = UserAuthLogic.AnalysisUserIDFromRequestCookies(request.Cookies);
            if (userID > 0)
            {
                RegistUser(clmngr, userID);
            }
        }
        /// <summary>
        /// 卸载用户
        /// </summary>
        /// <param name="clmngr"></param>
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
        /// <summary>
        /// 获取用户在线状态码
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public OnlineCodeValue GetUserOnlineCode(int uid)
        {
            UserOnlineInfo user = GetUserOnlineInfo(uid);
            return user != null ? user.OnlineStatusCode : OnlineCodeValue.Offline;
        }
        /// <summary>
        /// 获取用户在线信息
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public UserOnlineInfo GetUserOnlineInfo(int uid)
        {
            if (!_Users.ContainsKey(uid))
                return null;

            return _Users[uid] as UserOnlineInfo;
        }
        #endregion

        public override void ShutdownClient(ClientManager clmngr)
        {
            if (clmngr == null)
                return;

            if (clmngr.IsAuthedUser)
                UnRegistUser(clmngr);

            base.ShutdownClient(clmngr);

            //Console.Write("\r(Shut)Conn:{0}  Users:{1}    Check:{2}    Req:{3}          \r", _Clients.Count, _Users.Count, _MonitorCheckSuccessTimes, _RequestTimes);
        }

        /// <summary>
        /// 注册Handler
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

        /// <summary>
        /// 处理用户请求
        /// </summary>
        /// <param name="clmngr"></param>
        /// <param name="message"></param>
        /// <returns></returns>
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
    }
}
