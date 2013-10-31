using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace ExWebServer.WebServer.HttpLib
{
    public class HttpUtility
    {

        static string[] httpDateTimeFormats = new string[]
		{
			"ddd, d MMM yyyy H:m:s GMT",
			"dddd, d-MMM-yy H:m:s GMT",
			"ddd MMM d H:mm:s yy"
		};

        public static DateTime ParseHttpTime(string str)
        {
            DateTime dt;
            try
            {
                dt = DateTime.ParseExact(str, httpDateTimeFormats, System.Globalization.DateTimeFormatInfo.InvariantInfo,
                    System.Globalization.DateTimeStyles.AllowWhiteSpaces | System.Globalization.DateTimeStyles.AdjustToUniversal);
            }
            catch (FormatException)
            {
                dt = DateTime.Parse(str, CultureInfo.InvariantCulture);
            }
            return dt;
        }

        public static string GetStatusCodeDescriptions(int code)
        {
            switch (code)
            { 
                case 100:
                    return "Continue";
                case 101:
                    return "Switching Protocols";
                case 200:
                    return "OK";
                case 201:
                    return "Created";
                case 202:
                    return "Accepted";
                case 203:
                    return "Non-Authoritative Information";
                case 204:
                    return "No Content";
                case 205:
                    return "Reset Content";
                case 206:
                    return "Partial Content";
                case 300:
                    return "Multiple Choices";
                case 301:
                    return "Moved Permanently";
                case 302:
                    return "Found";
                case 303:
                    return "See Other";
                case 304:
                    return "Not Modified";
                case 305:
                    return "Use Proxy";
                case 307:
                    return "Temporary Redirect";
                case 400:
                    return "Bad Request";
                case 401:
                    return "Unauthorized";
                case 402:
                    return "Payment Required";
                case 403:
                    return "Forbidden";
                case 404:
                    return "Not Found";
                case 405:
                    return "Method Not Allowed";
                case 406:
                    return "Not Acceptable";
                case 407:
                    return "Proxy Authentication Required";
                case 408:
                    return "Request Time-out";
                case 409:
                    return "Conflict";
                case 410:
                    return "Gone";
                case 411:
                    return "Length Required";
                case 412:
                    return "Precondition Failed";
                case 413:
                    return "Request Entity Too Large";
                case 414:
                    return "Request-URI Too Large";
                case 415:
                    return "Unsupported Media Type";
                case 416:
                    return "Requested range not satisfiable";
                case 417:
                    return "Expectation Failed";
                case 500:
                    return "Internal Server Error";
                case 501:
                    return "Not Implemented";
                case 502:
                    return "Bad Gateway";
                case 503:
                    return "Service Unavailable";
                case 504:
                    return "Gateway Time-out";
                case 505:
                    return "HTTP Version not supported";
                default:
                    return "Continue"; 
            }
        }

        public static NameValueCollection ParseCookiesFromCookieString(string cookieString)
        {
            NameValueCollection cookies = new NameValueCollection();

            if (string.IsNullOrEmpty(cookieString))
                return cookies;

            cookieString = cookieString.Trim();

            int pos = 0, paramSplitor = 0, len = cookieString.Length;
            string item = string.Empty;
            string name = string.Empty, value = string.Empty;
            char[] nameValueSplitor = new char[] { '=' };

            if (len == 0)
                return cookies;

            while (pos < len)
            {
                paramSplitor = cookieString.IndexOf(';', pos);
                if (paramSplitor > pos)
                {
                    item = cookieString.Substring(pos, paramSplitor - pos);
                    pos = paramSplitor + 1;
                }
                else
                {
                    item = cookieString.Substring(pos, len - pos);
                    pos = len;
                }
                string[] para = item.Split(nameValueSplitor);
                if (para.Length != 2)
                    continue;
                name = para[0].Trim();
                value = para[1].Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    cookies[name] = value;
                }
            }

            return cookies;
        }

        public static NameValueCollection ParseParametersFromQueryString(string query)
        {
            NameValueCollection parameters = new NameValueCollection();
            
            if (string.IsNullOrEmpty(query))
                return parameters;

            query = query.Trim();

            int pos = 0, paramSplitor = 0, len = query.Length;
            string item = string.Empty;
            string name = string.Empty, value = string.Empty;
            char[] nameValueSplitor = new char[]{'='};
            if (len == 0)
                return parameters;

            while (query.Substring(pos, 1) == "?" && pos < len)
                pos++;

            while (pos < len)
            {
                paramSplitor = query.IndexOf('&', pos);
                if (paramSplitor > pos)
                {
                    item = query.Substring(pos, paramSplitor - pos);
                    pos = paramSplitor + 1;
                }
                else
                {
                    item = query.Substring(pos, len - pos);
                    pos = len;
                }
                string[] para = item.Split(nameValueSplitor);
                if (para.Length != 2)
                    continue;
                name = para[0].Trim();
                value = para[1].Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    parameters[name] = value;
                }
            }

            return parameters;
        }
    }
}
