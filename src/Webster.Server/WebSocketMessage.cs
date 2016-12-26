using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Webster.Server
{
    internal class WebSocketMessage
    {
        public static WebSocketMessage CloseMessage = new WebSocketMessage(WebSocketMessageType.Close, null);
        public static WebSocketMessage EmptyMessage = new WebSocketMessage(WebSocketMessageType.Binary, null);

        public WebSocketMessageType MessageType { get; }

        public object Data { get; }

        public WebSocketMessage(WebSocketMessageType type, object data)
        {
            MessageType = type;
            Data = data;
        }
    }
}
