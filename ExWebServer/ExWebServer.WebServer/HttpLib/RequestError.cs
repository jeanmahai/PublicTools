using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExWebServer.WebServer.HttpLib
{
    public class RequestError
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public RequestError(int statusCode)
        {
            StatusCode = statusCode;
            Message = HttpUtility.GetStatusCodeDescriptions(statusCode);
        }
    }
}
