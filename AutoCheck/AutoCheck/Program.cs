using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AutoCheck
{
    class ConsoleWin32Helper
    {
        [DllImport("User32.dll", EntryPoint = "ShowWindow")]   //
        private static extern bool ShowWindow(IntPtr hWnd, int type);
        static ConsoleWin32Helper()
        {
            var iconPath = Path.Combine(Environment.CurrentDirectory, "user_check.ico");
            _NotifyIcon.Icon = new Icon(iconPath);
            _NotifyIcon.Visible = false;
            _NotifyIcon.Text = "tray";

            ContextMenu menu = new ContextMenu();
            MenuItem item = new MenuItem();
            //item.Text = "右键菜单，还没有添加事件";
            //item.Index = 0;

            //menu.MenuItems.Add(item);
            _NotifyIcon.ContextMenu = menu;

            _NotifyIcon.MouseDoubleClick += new MouseEventHandler(_NotifyIcon_MouseDoubleClick);

        }

        static void _NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            IntPtr ParenthWnd = FindWindow(null, Console.Title);
            ShowWindow(ParenthWnd, 1);
        }

        #region 禁用关闭按钮
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "GetSystemMenu")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, IntPtr bRevert);

        [DllImport("user32.dll", EntryPoint = "RemoveMenu")]
        static extern IntPtr RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        /// <summary>
        /// 禁用关闭按钮
        /// </summary>
        /// <param name="consoleName">控制台名字</param>
        public static void DisableCloseButton(string title)
        {
            //线程睡眠，确保closebtn中能够正常FindWindow，否则有时会Find失败。。
            Thread.Sleep(100);

            IntPtr windowHandle = FindWindow(null, title);
            IntPtr closeMenu = GetSystemMenu(windowHandle, IntPtr.Zero);
            uint SC_CLOSE = 0xF060;
            RemoveMenu(closeMenu, SC_CLOSE, 0x0);
        }
        public static bool IsExistsConsole(string title)
        {
            IntPtr windowHandle = FindWindow(null, title);
            if (windowHandle.Equals(IntPtr.Zero)) return false;

            return true;
        }
        #endregion

        #region 托盘图标
        static NotifyIcon _NotifyIcon = new NotifyIcon();
        public static void ShowNotifyIcon()
        {
            _NotifyIcon.Visible = true;
            //_NotifyIcon.ShowBalloonTip(3000, "", "我是托盘图标，用右键点击我试试，还可以双击看看。", ToolTipIcon.None);
        }
        public static void HideNotifyIcon()
        {
            _NotifyIcon.Visible = false;
        }

        #endregion
    }
    class Program
    {
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]   //找子窗体   
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]   //用于发送信息给窗体   
        private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);

        [DllImport("User32.dll", EntryPoint = "ShowWindow")]   //
        private static extern bool ShowWindow(IntPtr hWnd, int type);
        //随机数0-100,GET
        private static string URL_SIGN_CARD = "http://10.16.174.155/NesoftHRMSATTEvent/common/Function.aspx?Action=SignCard&ID{0}";
        //POST
        private static string URL_GET_RECORD = "http://10.16.174.155/NesoftHRMSATTEvent/common/OringinDataForm.aspx";

        private static bool _IsExit { get; set; }

        private static DateTime LastSign
        {
            get
            {
                var str = ConfigurationManager.AppSettings["LastSign"];
                if (string.IsNullOrEmpty(str))
                {
                    LogFormat("no sign time -->{0}", DateTime.Now);
                    return DateTime.Now;
                }
                LogFormat("last sign time -->{0}", ConfigurationManager.AppSettings["LastSign"]);
                return DateTime.Parse(ConfigurationManager.AppSettings["LastSign"]);
            }
            set { ConfigurationManager.AppSettings["LastSign"] = value.ToString(CultureInfo.InvariantCulture); }
        }

        private static List<Thread> Threads { get; set; }

        static void Show()
        {
            IntPtr ParenthWnd = FindWindow(null, Console.Title);
            ShowWindow(ParenthWnd, 1);
        }
        static void Hide()
        {
            IntPtr ParenthWnd = FindWindow(null, Console.Title);
            ShowWindow(ParenthWnd, 0);
        }

        static void Main(string[] args)
        {
            //ShowHelp();
            //Console.Title = "AC";
            //Helpers = new List<ManualResetEvent>();
            //ConsoleWin32Helper.ShowNotifyIcon();
            //var threadInput = new Thread(MonitorInput);
            //threadInput.Name = "input monitor";
            //threadInput.Start();
            //while (true)
            //{
            //    //if (Helpers.Count > 0)
            //    //    WaitHandle.WaitAny(Helpers.ToArray());
            //    Application.DoEvents();
            //}
            SignCard();
            Console.ReadKey();
        }

        static void Exit()
        {
            foreach (var thread in Threads)
            {
                thread.Abort();
                thread.Join();
            }
        }

        static void ShowHelp()
        {
            using (var fs = new FileStream(Path.Combine(Environment.CurrentDirectory, "readme.txt"), FileMode.Open))
            using (var sr = new StreamReader(fs))
            {
                Log(sr.ReadToEnd());
            }
        }

        static void ShowAllThread()
        {
            var msg = new StringBuilder();
            foreach (var thread in Threads)
            {
                msg.AppendFormat("{0}:{1}\n", thread.Name, thread.ThreadState);
            }
            Log(msg.ToString());
        }

        private static List<ManualResetEvent> Helpers { get; set; }

        static void MonitorInput()
        {
            while (true)
            {
                int workerTotal, completionTotal;
                ThreadPool.GetAvailableThreads(out workerTotal, out completionTotal);
                LogFormat("worker threads:{0},completion port threads:{1}", workerTotal, completionTotal);
                string input = Console.ReadLine();
                if(input.Equals("-hide"))
                {
                    Hide();
                    continue;
                }
                DateTime reserveDate;
                if (DateTime.TryParse(input, out reserveDate))
                {
                    var signHelper = new SignCardHelper(new ManualResetEvent(false));
                    //Helpers.Add(mre);
                    //ThreadPool.QueueUserWorkItem(signHelper.ThreadPoolCallback, reserveDate);
                    new Thread(signHelper.ThreadPoolCallback).Start(reserveDate);
                }

            }
        }

        static void AutoSign()
        {
            while (true)
            {
                try
                {
                    var curDate = DateTime.Now;
                    LogFormat("current system time:{0}", curDate);
                    var targetDate = CaculateNextSignTime();
                    LogFormat("target sign card time:{0}", targetDate);
                    var dur = targetDate - curDate;
                    LogFormat("sleep {0}days {1}hours {2}minutes", dur.Days, dur.Hours, dur.Minutes);
                    Log("sleeping ....");
                    Thread.Sleep(dur);
                    Log("time out");
                    LogFormat("current system time:{0}", DateTime.Now);
                    LogFormat("beginning sign card ...");
                    SignCard(int.Parse(ConfigurationManager.AppSettings["Retry"]));
                    LogFormat("sign card success.{0}", DateTime.Now);
                    LogFormat("==========================");
                }
                catch (Exception ex)
                {
                    LogFormat("error:{0}", ex.Message);
                }
            }
        }

        static string GenerateSignCardUrl()
        {
            return string.Format(URL_SIGN_CARD, new Random().Next(0, 100));
        }

        static bool SignCard()
        {
            var request = (HttpWebRequest)WebRequest.Create(GenerateSignCardUrl());
            request.UseDefaultCredentials = true;
            using (var respose = (HttpWebResponse)request.GetResponse())
            {
                var rs = respose.GetResponseStream();
                if (rs != null)
                    using (var stream = new StreamReader(rs))
                    {
                        var result = stream.ReadToEnd().Trim();
                        LogFormat("response result:{0}", result);
                        if (result.Equals("打卡成功"))
                        {
                            Console.WriteLine("success");
                            LogFormat("sign card success.{0}", DateTime.Now);
                            LastSign = DateTime.Now;
                            return true;
                        }
                    }
            }
            LogFormat("sign card failure.{0}", DateTime.Now);
            return false;
        }
        static bool SignCard(int retry)
        {
            var num = 0;
            var result = SignCard();
            while (result == false && num < retry)
            {
                result = SignCard();
                num++;
                LogFormat("retry {0} times", num);
            }
            return result;
        }


        static string GetRecord(DateTime? start, DateTime? end)
        {
            if (!start.HasValue) start = DateTime.Now;
            if (!end.HasValue) end = DateTime.Now;
            var request = (HttpWebRequest)WebRequest.Create(URL_GET_RECORD);
            request.UseDefaultCredentials = true;
            request.Method = "POST";
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows CE)";
            request.Accept = "*/*";
            request.ContentType = "application/x-www-form-urlencoded";
            //request.Headers.Add("","");
            var data = new StringBuilder();
            data.Append("__EVENTTARGET=");
            data.Append("&__EVENTARGUMENT=");
            data.Append(
                "&__VIEWSTATE=dDwtMTU0NDM2NDM4MDt0PDtsPGk8MT47PjtsPHQ8O2w8aTw1PjtpPDc+Oz47bDx0PDtsPGk8MD47PjtsPHQ8O2w8aTwxPjtpPDM+Oz47bDx0PDtsPGk8MT47PjtsPHQ8QDA8O0AwPHA8bDxWYWx1ZTs+O2w8U3lzdGVtLkRhdGVUaW1lLCBtc2NvcmxpYiwgVmVyc2lvbj0xLjAuNTAwMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkzNGUwODk8MjAxMy0wOS0wMT47Pj47Ozs7Oz47cDxsPFU7PjtsPDUwPDA4LzAyLzIwMTMgMTM6MDY6MTY+Oz4+O0AwPDs7Pjs7PjtsPGk8MD47PjtsPHQ8O2w8aTwwPjs+O2w8dDxAMDw7cDxsPFU7PjtsPDUwPDA4LzAyLzIwMTMgMTM6MDY6MTY+Oz4+Oz47Oz47Pj47Pj47Pj47dDw7bDxpPDE+Oz47bDx0PEAwPDtAMDxwPGw8VmFsdWU7PjtsPDUwPDA5LzAyLzIwMTMgMTM6MDY6MTY+Oz4+Ozs7Ozs+O3A8bDxVOz47bDw1MDwwOS8wMi8yMDEzIDEzOjA2OjE2Pjs+PjtAMDw7Oz47Oz47bDxpPDA+Oz47bDx0PDtsPGk8MD47PjtsPHQ8QDA8O3A8bDxVOz47bDw1MDwwOS8wMi8yMDEzIDEzOjA2OjE2Pjs+Pjs+Ozs+Oz4+Oz4+Oz4+Oz4+Oz4+O3Q8QDA8O0AwPHA8bDxzZXJBY3RpdmVDZWxsU3RyaW5nO3NlckFjdGl2ZVJvd1N0cmluZzs+O2w8Ozs+Pjs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7O0AwPGw8U3lzdGVtLlVJbnQ2NCwgbXNjb3JsaWIsIFZlcnNpb249MS4wLjUwMDAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iNzdhNWM1NjE5MzRlMDg5PDA+OzUxPDE+OzUxPDI+OzUxPDM+OzUxPDQ+OzUxPDU+OzUxPDY+OzUxPDc+OzUxPDg+OzUxPDk+OzUxPDEwPjs1MTwxMT47NTE8MTI+OzUxPDEzPjs1MTwxND47NTE8MTU+OzUxPDE2Pjs1MTwxNz47NTE8MTg+OzUxPDE5Pjs1MTwyMD47NTE8MjE+OzUxPDIyPjs1MTwyMz47NTE8MjQ+OzUxPDI1Pjs1MTwyNj47NTE8Mjc+OzUxPDI4Pjs1MTwyOT47NTE8MzA+OzUxPDMxPjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTw0Mjk0OTY3Mjk2Pjs1MTwwPjs1MTwxPjs+O2w8QDA8Ozs7O0AwPGw8NTE8MD47NTE8MT47PjtsPEAwPHA8bDxWYWx1ZTs+O2w8OS8yLzIwMTM7Pj47Oz47QDA8cDxsPFZhbHVlOz47bDwwODo0OTs+Pjs7Pjs+Oz47PjtAMDw7Ozs7QDA8bDw1MTwwPjs1MTwxPjs+O2w8QDA8cDxsPFZhbHVlOz47bDw5LzIvMjAxMzs+Pjs7PjtAMDxwPGw8VmFsdWU7PjtsPDExOjEyOz4+Ozs+Oz47Pjs+Oz47Oz47Ozs7Pjs+Ozs+Oz4+Oz4+O2w8d2ViZGNoRnJvbTt3ZWJkY2hGcm9tOkRycFBubDpDYWxlbmRhcjE7d2ViZGNoVG87d2ViZGNoVG86RHJwUG5sOkNhbGVuZGFyMTt3ZWJHaXJkRGF0YV9MaXN0Oz4+TZHo1tApWxJZghogmzFsEUiOAfI=");
            data.Append("&webdchFrom_hidden=");
            data.AppendFormat("&webdchFrom_input={0}", start.Value.ToString("MM/dd/yyyy"));
            data.Append("&webdchFrom_DrpPnl_Calendar1=");
            data.Append("&webdchTo_hidden=");
            data.AppendFormat("&webdchTo_input={0}", end.Value.ToString("MM/dd/yyyy"));
            data.Append("&webdchTo_DrpPnl_Calendar1=");
            data.Append("&btnSearch=查询");
            data.Append("&webGirdDataxList=%3CDisplayLayout%3E%3CStateChanges%3E%3C/StateChanges%3E%3C/DisplayLayout%3E");
            var dataBytes = Encoding.UTF8.GetBytes(data.ToString());
            request.ContentLength = dataBytes.Count();

            var requestStream = request.GetRequestStream();
            requestStream.Write(dataBytes, 0, dataBytes.Count());

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var rs = response.GetResponseStream();
                if (rs != null)
                {
                    using (var stream = new StreamReader(rs))
                    {
                        return stream.ReadToEnd();
                    }
                }
            }

            return "";
        }

        static DateTime CaculateNextSignTime()
        {
            var curDate = DateTime.Now;
            var r = new Random();
            var minute = r.Next(int.Parse(ConfigurationManager.AppSettings["MinMinute"]),
                int.Parse(ConfigurationManager.AppSettings["MaxMinute"]));
            var second = r.Next(0, 59);
            var target = DateTime.Parse(
                string.Format("{0} 08:{1}:{2}", LastSign.AddDays(1).ToString("yyyy-MM-dd"), minute, second));
            if (target.DayOfWeek == DayOfWeek.Saturday)
            {
                target = target.AddDays(2);
            }
            if (target.DayOfWeek == DayOfWeek.Sunday)
            {
                target = target.AddDays(1);
            }
            return target;
        }

        static void Log(object msg)
        {
            Console.WriteLine(msg);
        }
        static void LogFormat(string str, params object[] args)
        {
            Console.WriteLine(str, args);
        }

    }
}
