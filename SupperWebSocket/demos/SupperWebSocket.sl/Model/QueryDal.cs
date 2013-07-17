using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocket.WebSocket.Server;

namespace Model
{
    public class QueryDal : IWebSocketHandler
    {
        public WebSocketResponse Analyze(WebSocketRequest request, WebSocketResponse response)
        {
            response.Data = "其他实体测试";
            return response;
        }
    }
}
