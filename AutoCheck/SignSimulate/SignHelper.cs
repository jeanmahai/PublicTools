using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CsQuery;

namespace SignSimulate
{
    internal class SignHelper
    {
        public static string URL_TOP = "http://10.16.174.155/NesoftHRMSATTEvent/common/TopPan.htm";
        public static string URL_LEFT = "http://10.16.174.155/NesoftHRMSATTEvent/common/ControlPanel.aspx";
        public static string URL_SIGN = "http://10.16.174.155/NesoftHRMSATTEvent/common/Function.aspx?Action=SignCard&Crypt={0}&ID{1}";
        public static string URL_MAIN = "http://10.16.174.155/NesoftHRMSATTEvent/common/MainWin.aspx";
        
        public static string GetHtml(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.UseDefaultCredentials = true;
            string result = string.Empty;
            using (var respose = (HttpWebResponse)request.GetResponse())
            {
                var rs = respose.GetResponseStream();
                if (rs != null)
                    using (var stream = new StreamReader(rs))
                    {
                        result = stream.ReadToEnd().Trim();
                    }
            }
            return result;
        }

        public static string GetSessionId()
        {
            var request = (HttpWebRequest)WebRequest.Create(URL_MAIN);
            request.UseDefaultCredentials = true;
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var vals = response.Headers.GetValues("set-cookie");
                if(vals != null && vals.Any())
                {
                    var strs = vals[0].Split(new char[] {';'});
                    if(strs.Any())
                    {
                        var ids = strs[0].Split(new char[] {'='});
                        if(ids.Any())
                        {
                            return ids[1];
                        }
                    }
                }
                return "";
            }
        }

        public static string GetControlPanelHtml(string sessionId)
        {
            var request = (HttpWebRequest) WebRequest.Create(URL_LEFT);
            request.UseDefaultCredentials = true;
            request.Headers.Add("cookie", string.Format("ASP.NET_SessionId={0}", sessionId));
            request.Host = "10.16.174.155";
            request.Accept = "*/*";
            request.Headers.Add("Accept-Encoding", "gzip, deflate");
            request.Headers.Add("Accept-Language", "zh-CN");
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.3; .NET4.0C; .NET4.0E)";
            request.KeepAlive = true;
            string result = string.Empty;
            using(var response=(HttpWebResponse)request.GetResponse())
            {
                var stream = response.GetResponseStream();
                if(stream!=null)
                {
                    using(var reader=new StreamReader(stream))
                    {
                        result = reader.ReadToEnd();
                    }
                }
            }
            return result;
        }

        public static string GetSignCrypt(string html)
        {
            var reg = new Regex("var enCrypt\\s*=\\s*\"([0-9a-zA-Z]*)\";");
            var m=reg.Matches(html);
            if(m.Count>0)
            {
                var crypt = m[0];
                if (crypt.Groups.Count >= 2)
                {
                    return crypt.Groups[1].ToString();
                }
            }
            return "";
        }

        public static void Sign(DateTime date)
        {
            var curDate = DateTime.Now;
            Console.WriteLine("current datetime:{0}", curDate);
            Console.WriteLine("sign datettime:{0}",date);
            var ds = date - curDate;
            Console.WriteLine("sleep times:{0} {1}:{2}:{3}",ds.Days,ds.Hours,ds.Minutes,ds.Seconds);
            Console.WriteLine("sleeping...");
            Thread.Sleep(ds);
            
            var sessionId = GetSessionId();
            Console.WriteLine("session id:{0}",sessionId);

            var html = GetControlPanelHtml(sessionId);
            var crypt = GetSignCrypt(html);
            Console.WriteLine("crypt:{0}",crypt);

            var r = new Random();
            var num = r.NextDouble()*100;
            var url = string.Format(URL_SIGN, crypt, num);
            Console.WriteLine("request url:{0}",url);
            var request = (HttpWebRequest) WebRequest.Create(url);
            request.UseDefaultCredentials = true;
            using(var response=(HttpWebResponse)request.GetResponse())
            {
                var stream = response.GetResponseStream();
                if(stream!=null)
                {
                    using(var reader=new StreamReader(stream))
                    {
                        Console.WriteLine("response result:\n{0}",reader.ReadToEnd());
                        return;
                    }
                }
            }
            Console.WriteLine("no response");
        }
    }
}
