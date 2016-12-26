using System;
using Microsoft.AspNetCore.Http;

namespace Webster.Server.IntegrationTests
{
    public class WebSocketServerStub : WebSocketServer
    {
        public Action<IWebSocketConnection, string> MessageReceived;

        public Action<IWebSocketConnection> ConnectionClosed;

        public Action<IWebSocketConnection, HttpContext> ConnectionOpened;

        public WebSocketServerStub()
        {
            MessageReceived = (conn, msg) => { };
            ConnectionClosed = (conn) => { };
            ConnectionOpened = (conn, query) => { };
        }

        protected override void OnMessageReceived(IWebSocketConnection conn, string message)
        {
            MessageReceived(conn, message);
        }

        protected override void OnConnectionClosed(IWebSocketConnection conn)
        {
            ConnectionClosed(conn);
        }

        protected override void OnConnectionOpened(IWebSocketConnection conn, HttpContext context)
        {
            ConnectionOpened(conn, context);
        }
    }
}
