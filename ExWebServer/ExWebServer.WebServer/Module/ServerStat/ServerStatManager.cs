using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.IO;
using System.Text;
//using WebCommon.Database;
//using WebCommon.Utility;

namespace ExWebServer.WebServer.Module.ServerStat
{
    internal class ServerStatManager
    {
        const int ONLINESTATS_DEFAULT_CAPACITY = 50;
        const int ONLINESTATS_DEFAULT_SAVE_INTERVALS = 300;
        //const int ONLINESTATS_DEFAULT_SAVE_INTERVALS = 10;
        const int ONLINESTATS_BULCKSAVE_CAPACITY = 128;

        const int ONLINETIME_DEFAULT_CAPACITY = 50000;
        const int ONLINETIME_DEFAULT_SAVE_INTERVALS = 450;
        //const int ONLINETIME_DEFAULT_SAVE_INTERVALS = 15;

        private Hashtable _OnlineStats { get; set; }
        private Hashtable _UserOnlineTimeStats { get; set; }
        private Timer _tmCCUStatSaver = null;
        private TimerCallback _tmCCUStatSaverCallback = null;

        private Timer _tmOnlineTimeSaver = null;
        private TimerCallback _tmOnlineTimeSaverCallback = null;

        //public bool IsSaveTime { get { return _LastSaveTime.AddSeconds(_SaveIntervals) < DateTime.Now ? true : false; } }

        public ServerStatManager() 
        {
            Init();
        }

        private void Init()
        {

            InitOnlineStatsHashtable(ONLINESTATS_DEFAULT_CAPACITY);
            
            InitGameTimeStatsHashtable(ONLINETIME_DEFAULT_CAPACITY);

            AutoResetEvent autoEvent = new AutoResetEvent(false);

            _tmCCUStatSaverCallback = new TimerCallback(this.CheckAndSaveCCUStat);
            _tmCCUStatSaver = new Timer(_tmCCUStatSaverCallback, autoEvent, 1000, ONLINESTATS_DEFAULT_SAVE_INTERVALS * 1000);

            _tmOnlineTimeSaverCallback = new TimerCallback(this.CollectAndSaveOnlineTime);
            _tmOnlineTimeSaver = new Timer(_tmOnlineTimeSaverCallback, autoEvent, 1000, ONLINETIME_DEFAULT_SAVE_INTERVALS * 1000);
            
        }

        private void InitOnlineStatsHashtable() { InitOnlineStatsHashtable(ONLINESTATS_DEFAULT_CAPACITY); }
        private void InitOnlineStatsHashtable(int capacity)
        {
            capacity = capacity > ONLINESTATS_DEFAULT_CAPACITY ? capacity : ONLINESTATS_DEFAULT_CAPACITY;
            if (_OnlineStats != null)
            {
                try { _OnlineStats.Clear(); }
                catch { }
                finally { _OnlineStats = null; }
            }
            _OnlineStats = Hashtable.Synchronized(new Hashtable(capacity));
        }

        private void InitGameTimeStatsHashtable() { InitGameTimeStatsHashtable(ONLINETIME_DEFAULT_CAPACITY); }
        private void InitGameTimeStatsHashtable(int capacity)
        {
            capacity = capacity > ONLINETIME_DEFAULT_CAPACITY ? capacity : ONLINETIME_DEFAULT_CAPACITY;
            if (_UserOnlineTimeStats != null)
            {
                try { _UserOnlineTimeStats.Clear(); }
                catch { }
                finally { _UserOnlineTimeStats = null; }
            }
            _UserOnlineTimeStats = Hashtable.Synchronized(new Hashtable(capacity));
        }

        private SqlConnection GetDBConnection()
        {
            return GetDBConnection(0);
        }

        private SqlConnection GetDBConnection(int idx)
        {
            string dbName = "WGLog"; ;
            SqlConnection conn = null;
            try
            {
                //conn = MSSQLHelper.GetConnection(dbName, idx);
            }
            catch { }
            finally { }

            return conn;
        }

        private string GetSqlStatement(string sqlSection, string sqlItem)
        {
            string sqlFile = "sqlstatement";
            string sql = "";
            //SubConfigure config = null;

            try
            {
                //config = SubConfigureManager.GetConfigure(sqlFile);
                //if (config == null)
                //    return sql;

                //sql = config.GetItem(sqlSection, sqlItem);
            }
            catch { }
            finally
            {
            }

            return sql;
        }

        public static long GetHashKeyOfOnlineStat(int siteID,int posID)
        {
            long siteIDtemp = (long)siteID;
            long posIDtemp = (long)posID;
            long key = (siteIDtemp << 32) | posIDtemp;

            return key;
        }

        public static long GetHashKeyOfOnlineStat(SiteOnlineStat stat)
        {
            if (stat == null)
                throw new Exception("SiteOnlineStat Object cannot be null.");

            return GetHashKeyOfOnlineStat(stat.SiteID, stat.PosID);
        }

        private bool BulkSaveUserOnlineTimeToDataBase(List<UserOnlineTimeInfo> onlineTimeList)
        {
            bool result = false;

            if (onlineTimeList == null || onlineTimeList.Count < 1)
                return result;

            string sqlSection = "OnlineServer";
            string sqlItem = "BulkSaveUserOnlineTime";
            string sql = "";
            SqlConnection conn = null;
            SqlCommand cmd = null;

            byte[] buf = null;
            try
            {
                conn = GetDBConnection();
                if (conn == null)
                {
                    //Console.WriteLine("SaveStateToDatabase result:{0}", "no connection");
                    return result;
                }


                sql = GetSqlStatement(sqlSection, sqlItem);

                if (string.IsNullOrEmpty(sql))
                {
                    //Console.WriteLine("SaveStateToDatabase result:{0}", "no sqlstatement");
                    return result;
                }

                //  格式：0-3 = uid,4-7 = gameTimePlus
                buf = new byte[sizeof(int) * onlineTimeList.Count * 2];
                byte[] intBytes = null;
                int uid = 0, timePlus = 0;
                int bytePos = 0;
                foreach (UserOnlineTimeInfo stat in onlineTimeList)
                {
                    uid = stat.UserID;
                    timePlus = stat.OnlineTimePlus;

                    intBytes = BitConverter.GetBytes(uid);
                    Array.Reverse(intBytes);
                    Array.Copy(intBytes, 0, buf, bytePos, intBytes.Length);
                    bytePos += 4;

                    intBytes = BitConverter.GetBytes(timePlus);
                    Array.Reverse(intBytes);
                    Array.Copy(intBytes, 0, buf, bytePos, intBytes.Length);
                    bytePos += 4;

                }

                cmd = new SqlCommand(sql, conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@OnlineTimeData", buf);
                cmd.Parameters.Add(new SqlParameter("@Return", SqlDbType.Int));
                cmd.Parameters["@Return"].Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();
                int returnValue = Convert.ToInt32(cmd.Parameters["@Return"].Value);
                result = returnValue == 0 ? true : false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("BulkSaveUserOnlineTimeToDatabase error:{0}", ex.Message);
            }
            finally
            {
                if (cmd != null)
                    cmd = null;
                if (conn != null)
                {
                    try { conn.Close(); }
                    catch { }
                    finally { conn = null; }
                }
            }

            return result;
        }

        private bool BulkSaveOnlineStatToDataBase(List<SiteOnlineStat> statList)
        {
            bool result = false;

            if (statList == null || statList.Count < 1)
                return result;

            string sqlSection = "OnlineServer";
            string sqlItem = "BulkSaveOnlineStat";
            string sql = "";
            SqlConnection conn = null;
            SqlCommand cmd = null;

            byte[] buf = null;
            try
            {
                conn = GetDBConnection();
                if (conn == null)
                {
                    //Console.WriteLine("SaveStateToDatabase result:{0}", "no connection");
                    return result;
                }


                sql = GetSqlStatement(sqlSection, sqlItem);

                if (string.IsNullOrEmpty(sql))
                {
                    //Console.WriteLine("SaveStateToDatabase result:{0}", "no sqlstatement");
                    return result;
                }

                //  格式：0-3 = siteID,4-7 = posID,8-11 = ccu,12-15 = maxCCU
                buf = new byte[sizeof(int) * statList.Count * 4];
                byte[] intBytes = null;
                int siteID = 0, posID = 0, ccu = 0, maxCCU = 0;
                int bytePos = 0;
                foreach (SiteOnlineStat stat in statList)
                {
                    siteID = stat.SiteID;
                    posID = stat.PosID;
                    ccu = stat.CCU;
                    maxCCU = stat.MaxCCU;

                    intBytes = BitConverter.GetBytes(siteID);
                    Array.Reverse(intBytes);
                    Array.Copy(intBytes, 0, buf, bytePos, intBytes.Length);
                    bytePos += 4;

                    intBytes = BitConverter.GetBytes(posID);
                    Array.Reverse(intBytes);
                    Array.Copy(intBytes, 0, buf, bytePos, intBytes.Length);
                    bytePos += 4;

                    intBytes = BitConverter.GetBytes(ccu);
                    Array.Reverse(intBytes);
                    Array.Copy(intBytes, 0, buf, bytePos, intBytes.Length);
                    bytePos += 4;

                    intBytes = BitConverter.GetBytes(maxCCU);
                    Array.Reverse(intBytes);
                    Array.Copy(intBytes, 0, buf, bytePos, intBytes.Length);
                    bytePos += 4;

                }

                cmd = new SqlCommand(sql, conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CCUData",buf);
                cmd.Parameters.Add(new SqlParameter("@Return", SqlDbType.Int));
                cmd.Parameters["@Return"].Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();
                int returnValue = Convert.ToInt32(cmd.Parameters["@Return"].Value);
                result = returnValue == 0 ? true : false;
                //Console.WriteLine("BulkSaveStateToDatabase result:{0}", returnValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine("BulkSaveStateToDatabase error:{0}", ex.Message);
            }
            finally
            {
                if (cmd != null)
                    cmd = null;
                if (conn != null)
                {
                    try { conn.Close(); }
                    catch { }
                    finally { conn = null; }
                }
            }

            return result;
        }

        

        private bool SaveOnlineStatToDataBase(SiteOnlineStat stat)
        {
            bool result = false;

            if (stat == null)
                return result;

            string sqlSection = "OnlineServer";
            string sqlItem = "SaveOnlineStat";
            string sql = "";
            SqlConnection conn = null;
            SqlCommand cmd = null;

            try
            {
                conn = GetDBConnection();
                if (conn == null)
                {
                    //Console.WriteLine("SaveStateToDatabase result:{0}", "no connection");
                    return result;
                }
                    

                sql = GetSqlStatement(sqlSection, sqlItem);

                if (string.IsNullOrEmpty(sql))
                {
                    //Console.WriteLine("SaveStateToDatabase result:{0}", "no sqlstatement");
                    return result;
                }
                    

                cmd = new SqlCommand(sql, conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SiteID", stat.SiteID);
                cmd.Parameters.AddWithValue("@PosID", stat.PosID);
                cmd.Parameters.AddWithValue("@CCU", stat.CCU);
                cmd.Parameters.AddWithValue("@MaxCCU", stat.MaxCCU);
                cmd.Parameters.Add(new SqlParameter("@Return", SqlDbType.Int));
                cmd.Parameters["@Return"].Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();
                int returnValue = Convert.ToInt32(cmd.Parameters["@Return"].Value);
                result = returnValue == 0 ? true : false;
                //Console.WriteLine("SaveStateToDatabase result:{0}", returnValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SaveStateToDatabase error:{0}",ex.Message);
            }
            finally
            {
                if (cmd != null)
                    cmd = null;
                if (conn != null)
                {
                    try { conn.Close(); }
                    catch { }
                    finally { conn = null; }
                }
            }

            return result;

        }

        public void SaveOnlineStat(int siteID,int posID,int ccu)
        {
            if (ccu < 0)
                return;

            if (_OnlineStats == null)
                InitOnlineStatsHashtable();

            SiteOnlineStat statTemp = null;

            try
            {
                long key = GetHashKeyOfOnlineStat(siteID, posID);

                if (_OnlineStats.ContainsKey(key))
                {
                    statTemp = (SiteOnlineStat)_OnlineStats[key];
                    statTemp.CCU = ccu;
                }
                else
                {
                    lock (_OnlineStats.SyncRoot)
                    {
                        statTemp = new SiteOnlineStat(siteID, posID, ccu);
                        _OnlineStats[key] = statTemp;
                    }
                }
            }
            catch { }
        }

        private void SaveCCUStateToDisk(List<SiteOnlineStat> toBeSavedStats,string file)
        {
            //if (toBeSavedStats == null || toBeSavedStats.Count < 1 || string.IsNullOrEmpty(file))
            //    return;

            //StringBuilder sb = new StringBuilder();

            //try 
            //{
            //    foreach (SiteOnlineStat stat in toBeSavedStats)
            //    {
            //        sb.Append(string.Format("{0};{1};{2};{3}\r\n", stat.SiteID, stat.PosID, stat.MaxCCU, stat.CCU));
            //    }

            //    string rootPath = System.Environment.CurrentDirectory;
            //    string fullPath = string.Format("{0}\\{1}.log", rootPath,file);

            //    using(FileStream fs = new FileStream(fullPath, FileMode.Create))
            //    {
            //        //获得字节数组

            //        byte[] data = new UTF8Encoding().GetBytes(sb.ToString());
            //        //开始写入
            //        fs.Write(data, 0, data.Length);

            //        //清空缓冲区、关闭流
            //        fs.Flush();
            //        fs.Close();
            //    }
            //}
            //catch { }
            //finally { }

        }

        internal virtual void CheckAndSaveCCUStat(Object stateInfo)
        {
            if (_OnlineStats == null || _OnlineStats.Count < 1)
                return;


            List<SiteOnlineStat> toBeSavedStats = new List<SiteOnlineStat>(_OnlineStats.Count);

            try
            {
                lock (_OnlineStats.SyncRoot)
                {
                    foreach (DictionaryEntry statItem in _OnlineStats)
                    {
                        toBeSavedStats.Add((SiteOnlineStat)statItem.Value);
                    }
                }

                if (toBeSavedStats.Count > 0)
                {
                    SaveCCUStateToDisk(toBeSavedStats,"ccu_save2db");

                    if (toBeSavedStats.Count <= ONLINESTATS_BULCKSAVE_CAPACITY)
                    {
                        //  
                        BulkSaveOnlineStatToDataBase(toBeSavedStats);
                    }
                    else
                    {
                        List<SiteOnlineStat> tempList = new List<SiteOnlineStat>(ONLINESTATS_BULCKSAVE_CAPACITY);
                        int pos = 0,currSaved = 0;
                        while (pos < toBeSavedStats.Count)
                        {
                            currSaved = (toBeSavedStats.Count - pos) <= ONLINESTATS_BULCKSAVE_CAPACITY ? (toBeSavedStats.Count - pos) : ONLINESTATS_BULCKSAVE_CAPACITY;
                            tempList = toBeSavedStats.GetRange(pos, currSaved);
                            BulkSaveOnlineStatToDataBase(tempList);
                            pos += currSaved;
                        }
                    }
                    
                    foreach (SiteOnlineStat stat in toBeSavedStats)
                    {
                        //  假定全部成功
                        stat.OnlineStatSaved();
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine("CheckAndSaveCCUStat error:{0}", ex.Message); }
            finally {  }
            
        }

        internal virtual void CollectAndSaveOnlineTime(Object stateInfo)
        {
            if (_UserOnlineTimeStats == null || _UserOnlineTimeStats.Count < 1)
                return;

            List<UserOnlineTimeInfo> onlineTimeBuf = null;

            int[] uidList = null;
            try
            {
                lock (_UserOnlineTimeStats.SyncRoot)
                {
                    if (_UserOnlineTimeStats.Count > 0)
                    { 
                        uidList = new int[_UserOnlineTimeStats.Count];
                        _UserOnlineTimeStats.Keys.CopyTo(uidList, 0);
                    }
                }

                if (uidList != null && uidList.Length > 0)
                {
                    int bulkSize = 500;
                    onlineTimeBuf = new List<UserOnlineTimeInfo>(bulkSize);
                    UserOnlineTimeInfo onlineTime = null;

                    foreach (int uid in uidList)
                    {
                        if (!_UserOnlineTimeStats.ContainsKey(uid))
                            continue;

                        onlineTime = (UserOnlineTimeInfo)_UserOnlineTimeStats[uid];
                        if (onlineTime.OnlineTimePlus < 1)
                            continue;

                        if (onlineTimeBuf.Count < bulkSize)
                            onlineTimeBuf.Add(new UserOnlineTimeInfo(uid,onlineTime.OnlineTimePlus));
                        else
                        {
                            BulkSaveUserOnlineTimeToDataBase(onlineTimeBuf);
                            //  保存完本批数据后从hashtable里移除用户记录
                            lock (_UserOnlineTimeStats.SyncRoot)
                            {
                                foreach (UserOnlineTimeInfo user in onlineTimeBuf)
                                {
                                    _UserOnlineTimeStats.Remove(user.UserID);
                                }
                            }
                            onlineTimeBuf.Clear();
                            onlineTimeBuf.Add(new UserOnlineTimeInfo(uid, onlineTime.OnlineTimePlus));
                        }
                        onlineTime.OnlineTimePlus = 0;
                    }

                    if (onlineTimeBuf.Count > 0)
                    {
                        BulkSaveUserOnlineTimeToDataBase(onlineTimeBuf);
                        lock (_UserOnlineTimeStats.SyncRoot)
                        {
                            foreach (UserOnlineTimeInfo user in onlineTimeBuf)
                            {
                                _UserOnlineTimeStats.Remove(user.UserID);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("CheckAndSaveOnlineTime error:{0}", ex.Message);
            }
            finally { }

        }

        public void BulkSaveOnlineStat(Dictionary<long, SiteOnlineStat> siteStatsTable)
        {
            if (_OnlineStats == null)
                InitOnlineStatsHashtable();

            if (siteStatsTable == null || siteStatsTable.Count < 1)
                return;

            SiteOnlineStat statTemp = null;
            List<SiteOnlineStat> toBeSavedStats = new List<SiteOnlineStat>(siteStatsTable.Count);

            try
            {
                long key = 0;

                foreach (KeyValuePair<long, SiteOnlineStat> siteStat in siteStatsTable)
                {
                    toBeSavedStats.Add(siteStat.Value);

                    key = GetHashKeyOfOnlineStat(siteStat.Value);

                    if (_OnlineStats.ContainsKey(key))
                    {
                        statTemp = (SiteOnlineStat)_OnlineStats[key];
                        statTemp.CCU = siteStat.Value.CCU;
                    }
                    else
                    {
                        lock (_OnlineStats.SyncRoot)
                        {
                            _OnlineStats[key] = siteStat.Value;
                            statTemp = siteStat.Value;
                        }
                    }
                }
                SaveCCUStateToDisk(toBeSavedStats,"ccu_save2mem");

            }
            catch(Exception ex)
            {
                Console.WriteLine("BulkSaveOnlineStat error:{0}",ex.Message);
            }
        }

        public void SaveOnlineStat(SiteOnlineStat stat)
        {
            if (_OnlineStats == null)
                InitOnlineStatsHashtable();

            if (stat == null)
                return;

            SiteOnlineStat statTemp = null;

            try
            {
                long key = GetHashKeyOfOnlineStat(stat);

                if (_OnlineStats.ContainsKey(key))
                {
                    statTemp = (SiteOnlineStat)_OnlineStats[key];
                    statTemp.CCU = stat.CCU;
                }
                else
                {
                    lock (_OnlineStats.SyncRoot)
                    {
                        _OnlineStats[key] = stat;
                        statTemp = stat;
                    }
                }
            }
            catch { }
        }

        public void SaveUserOnlineTime(int uid, int gameTimePlus)
        {
            if (_UserOnlineTimeStats == null)
                InitGameTimeStatsHashtable();


            if (gameTimePlus < 1)
                return;
            
            try 
            {
                if (_UserOnlineTimeStats.ContainsKey(uid))
                {
                    UserOnlineTimeInfo gameTimeInfo = (UserOnlineTimeInfo)_UserOnlineTimeStats[uid];
                    gameTimeInfo.OnlineTimePlus += gameTimePlus;
                }
                else
                {
                    lock (_UserOnlineTimeStats.SyncRoot)
                    {
                        _UserOnlineTimeStats[uid] = new UserOnlineTimeInfo(uid, gameTimePlus);
                    }
                }
            }
            catch { }
        }
    }
}
