using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Webster.Server
{
    public abstract class WebSocketServer
    {
        private readonly IDictionary<Guid, IWebSocketConnection> _activeConnections = 
            new ConcurrentDictionary<Guid, IWebSocketConnection>();

        protected abstract void OnTextMessageReceived(IWebSocketConnection conn, string message);

        protected abstract void OnBinaryMessageReceived(IWebSocketConnection conn, byte[] message);

        protected abstract void OnConnectionClosed(IWebSocketConnection conn);

        protected abstract void OnConnectionOpened(IWebSocketConnection conn);

        protected abstract void OnError(IWebSocketConnection conn, Exception e);

        private void OnCloseInternal(IWebSocketConnection connection)
        {
            _activeConnections.Remove(connection.Id);
            OnConnectionClosed(connection);
        }

        internal async Task ProcessRequest(HttpContext context)
        {
            var socket = await context.WebSockets.AcceptWebSocketAsync(subProtocol: null);

            var connection = new WebSocketConnection(socket, context, CancellationToken.None);
            connection.OnOpen = () => OnConnectionOpened(connection);
            connection.OnTextMessage = (msg) => OnTextMessageReceived(connection, msg);
            connection.OnBinaryMessage = (msg) => OnBinaryMessageReceived(connection, msg);
            connection.OnError = (exception) => OnError(connection, exception);
            connection.OnClose = () => OnCloseInternal(connection);

            _activeConnections.Add(connection.Id, connection);
            await connection.ProcessRequest(CancellationToken.None);
        }
    }
}
