using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExWebServer.WebServer
{
    public class BussinessException : Exception
    {
        public BussinessException(string message) : base(message)
        {
        }
    }

    public class ExceptionResult
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
