using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExWebServer.WebServer.Module.OnlineUser
{
    public class UserOnlineInfo
    {
        const int TIMEOUT_SECONDS = 30;
        
        private DateTime _MinTime = new DateTime(1970, 1, 1);

        public int UserID { get; set; }
        public OnlineCodeValue OnlineStatusCode { get; set; }
        public int SiteID { get; set; }
        public int PosID { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime LastActTime { get; set; }

        public bool SocketOpen { get; set; }
        public bool IsTimeout { get { return LastActTime.AddSeconds(TIMEOUT_SECONDS) < DateTime.Now ? true : false; } }
        public bool IsOnline { get { return SocketOpen || !IsTimeout ? true : false; } }

        public DateTime LastCollectTime { get; set; }
        public int CurrentOnlineTimeSecs { get; set; }

        public DateTime LoginTime { get; set; }
        public DateTime LogoffTime { get; set; }

        public UserOnlineInfo() 
        {
            UserID = 0;
            SiteID = 0;
            PosID = 0;

            LastCollectTime = _MinTime;
            CurrentOnlineTimeSecs = 0;

            SocketOpen = false;
            CreateTime = LastActTime = DateTime.Now;
            LoginTime = LogoffTime = _MinTime; 
        }

        public UserOnlineInfo(int uid)
        {
            if (uid < 0)
                throw new Exception("UserID cannot be negative.");

            UserID = uid;
            SiteID = 0;
            PosID = 0;

            LastCollectTime = _MinTime;
            CurrentOnlineTimeSecs = 0;

            SocketOpen = false;
            CreateTime = LastActTime = DateTime.Now;
            LoginTime = LogoffTime = _MinTime;
        }

        public void SetPosition(int siteID,int posID)
        {
            SiteID = siteID;
            PosID = posID;
        }

        /// <summary>
        /// 设置为登录
        /// </summary>
        /// <returns>如果为再次登录,则返回上次的在线时长(秒);否则为0</returns>
        private int SetLogin_Discard()
        {
            int onlinePlus = 0;
            
            if (LogoffTime >= LoginTime)
                onlinePlus = (int)LogoffTime.Subtract(LoginTime).TotalSeconds;

            LoginTime = DateTime.Now;
            LogoffTime = _MinTime;

            SocketOpen = true;

            return onlinePlus;
        }

        /// <summary>
        /// 设置为登录
        /// </summary>
        /// <returns>如果为再次登录,则返回上次的在线时长(秒);否则为0</returns>
        public void SetLogin()
        {
            if (LogoffTime > LastCollectTime)
                CurrentOnlineTimeSecs += (int)LogoffTime.Subtract(LastCollectTime).TotalSeconds;

            LoginTime = DateTime.Now;
            LastCollectTime = DateTime.Now;
            LogoffTime = _MinTime;
        }

        /// <summary>
        /// 设置为登出
        /// </summary>
        /// <returns>返回本次在线时长</returns>
        public int SetLogoff_Discard()
        {
            int onlinePlus = 0;

            if (LogoffTime < LoginTime)
                LogoffTime = DateTime.Now;

            onlinePlus = (int)LogoffTime.Subtract(LoginTime).TotalSeconds;

            return onlinePlus;
            //SocketOpen = false;
        }

        /// <summary>
        /// 设置为登出
        /// </summary>
        /// <returns>返回本次在线时长</returns>
        public void SetLogoff()
        {
            if (LogoffTime < LoginTime)
                LogoffTime = DateTime.Now;
        }

        public int CollectOnlineTime()
        {
            int onlineTime = CurrentOnlineTimeSecs;
            CurrentOnlineTimeSecs = 0;

            //  首次收集
            if (LastCollectTime < LoginTime)
                LastCollectTime = LoginTime;

            if (LogoffTime < LoginTime)
            {
                //  用户在线
                onlineTime += (int)DateTime.Now.Subtract(LastCollectTime).TotalSeconds;
                LastCollectTime = DateTime.Now;
            }
            else
            {
                //  用户已经离线    
                onlineTime += (int)LogoffTime.Subtract(LastCollectTime).TotalSeconds;
                LastCollectTime = LogoffTime;
            }

            return onlineTime;
        }

        public void SetOnlineStatusCode(OnlineCodeValue onlineCode)
        {
            OnlineStatusCode = onlineCode;
        }

        public void RefreshActiveTime()
        {
            LastActTime = DateTime.Now;
        }
    }
}
