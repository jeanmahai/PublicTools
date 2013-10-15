using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace AutoCheck
{
    internal class SignCardHelper
    {
        //随机数0-100,GET
        private static string URL_SIGN_CARD = "http://10.16.174.155/NesoftHRMSATTEvent/common/Function.aspx?Action=SignCard&ID{0}";
        //POST
        private static string URL_GET_RECORD = "http://10.16.174.155/NesoftHRMSATTEvent/common/OringinDataForm.aspx";
        private ManualResetEvent _callback;

        public SignCardHelper(ManualResetEvent callback)
        {
            _callback = callback;
        }

        public void ThreadPoolCallback(Object threadContext)
        {
            DateTime reserveDate = (DateTime)threadContext;
            LogFormat("thread started.reserve date:{0}", reserveDate);
            if (reserveDate < DateTime.Now)
                return;
            var ds = reserveDate - DateTime.Now;
            LogFormat("sleepling {0} days {1} hours {2} minutes {3} seconds", ds.Days, ds.Hours, ds.Minutes, ds.Seconds);
            Thread.Sleep(ds);
            SignCard();
            LogFormat("thread stop.reserve date:{0}", reserveDate);
            _callback.Reset();
        }

        void SignCard()
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
                        LogFormat("response result[{1}]:{0}", result, DateTime.Now);
                        return;
                    }
            }
            LogFormat("no response.{0}", DateTime.Now);
        }

        string GenerateSignCardUrl()
        {
            return string.Format(URL_SIGN_CARD, new Random().Next(0, 100));
        }

        void Log(object msg)
        {
            Console.WriteLine(msg);
        }
        void LogFormat(string str, params object[] args)
        {
            Console.WriteLine(str, args);
        }
    }
}
