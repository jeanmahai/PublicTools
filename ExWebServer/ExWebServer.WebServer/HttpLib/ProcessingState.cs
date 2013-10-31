using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExWebServer.WebServer.HttpLib
{
    public enum ProcessingState
    {
        NA = 0,
        RequestLine = 1,
        RequestHeaders,
        RequestProcCompleted,
        ResponsePreparing,
        ResponseFlushed
    }
}
