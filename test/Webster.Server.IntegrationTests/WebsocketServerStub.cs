using System;
using Microsoft.AspNetCore.Http;

namespace Webster.Server.IntegrationTests
{
    public class WebSocketServerStub : WebSocketServer
    {
        public Action<IWebSocketConnection, string> TextMessageReceived;

        public Action<IWebSocketConnection, byte[]> BinaryMessageReceived;

        public Action<IWebSocketConnection> ConnectionClosed;

        public Action<IWebSocketConnection> ConnectionOpened;

        public Action<IWebSocketConnection, Exception> OnSocketError;

        public WebSocketServerStub()
        {
            TextMessageReceived = (conn, msg) => { };
            BinaryMessageReceived = (conn, msg) => { };
            ConnectionClosed = (conn) => { };
            ConnectionOpened = (conn) => { };
            OnSocketError = (conn, err) => {};
        }

        protected override void OnTextMessageReceived(IWebSocketConnection conn, string message)
        {
            TextMessageReceived(conn, message);
        }

        protected override void OnBinaryMessageReceived(IWebSocketConnection conn, byte[] message)
        {
            BinaryMessageReceived(conn, message);
        }

        protected override void OnError(IWebSocketConnection conn, Exception e)
        {
            OnSocketError(conn, e);
        }

        protected override void OnConnectionClosed(IWebSocketConnection conn)
        {
            ConnectionClosed(conn);
        }

        protected override void OnConnectionOpened(IWebSocketConnection conn)
        {
            ConnectionOpened(conn);
        }
    }
}
