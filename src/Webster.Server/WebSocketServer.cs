using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        protected abstract void OnConnectionOpened(IWebSocketConnection conn, HttpContext context);

        private void OnCloseInternal(IWebSocketConnection connection)
        {
            _activeConnections.Remove(connection.Id);
            OnConnectionClosed(connection);
        }

        internal async Task ProcessRequest(HttpContext context)
        {
            try
            {
                var socket = await context.WebSockets.AcceptWebSocketAsync(subProtocol: null);

                var connection = new WebSocketConnection(socket, context.Request.Query, CancellationToken.None);
                connection.OnOpen = () => OnConnectionOpened(connection, context);
                connection.OnTextMessage = (msg) => OnTextMessageReceived(connection, msg);
                connection.OnBinaryMessage = (msg) => OnBinaryMessageReceived(connection, msg);
                connection.OnClose = () => OnCloseInternal(connection);

                _activeConnections.Add(connection.Id, connection);
                await connection.ProcessRequest(CancellationToken.None);
            }
            catch (Exception)
            {
                // If acceptwebsocketasync failed, do not remove from dictionary
                // If socket already added -> error
                // If processrequest failed -> error
                throw;
            }
        }
    }
}
