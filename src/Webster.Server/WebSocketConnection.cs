using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Webster.Server
{
    internal class WebSocketConnection : IWebSocketConnection
    {
        private CancellationToken _disconnectToken;
        private readonly WebSocket _socket;
        private readonly TaskQueue _taskQueue;
        internal Action OnOpen { get; set; }
        internal Action<string> OnTextMessage { get; set; }
        internal Action<byte[]> OnBinaryMessage { get; set; } 
        internal Action OnClose { get; set; }
        internal Action<Exception> OnError { get; set; }

        public Guid Id { get; }

        public HttpContext HttpContext { get; }

        public WebSocketConnection(WebSocket socket, HttpContext context, CancellationToken disconnectToken)
        {
            _socket = socket;
            HttpContext = context;
            _disconnectToken = disconnectToken;
            _taskQueue = new TaskQueue();

            Id = Guid.NewGuid();
            OnOpen = () => { };
            OnClose = () => { };
            OnError = (ex) => { };
            OnTextMessage = (msg) => { };
            OnBinaryMessage = (msg) => { };
        }

        public async Task Send(string message)
        {
            var data = Encoding.UTF8.GetBytes(message);
            await _taskQueue.Enqueue(async () =>
            {
                if (_socket.State != WebSocketState.Open)
                {
                    return;
                }

                await _socket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, _disconnectToken);
            });
        }

        public async Task CloseConnection()
        {
            await _taskQueue.Enqueue(async () =>
            {
                if (_socket.State == WebSocketState.Open)
                {
                    try
                    {
                        await _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    }
                    catch
                    {
                        ; // Swallow
                    }
                }
            });
        }

        public async Task ProcessRequest(CancellationToken disconnecToken)
        {
            var closeReceived = false;
            OnOpen();
            try
            {
                while (!closeReceived && !_disconnectToken.IsCancellationRequested)
                {
                    var result = await ReadMessageAsync(_socket);

                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Close:
                            closeReceived = true;
                            // Give the close response some time to be received
                            await Task.WhenAny(SendCloseResponse(), Task.Delay(1000));
                            OnClose();
                            break;
                        case WebSocketMessageType.Text:
                            OnTextMessage((string) result.Data);
                            break;
                        case WebSocketMessageType.Binary:
                            OnBinaryMessage((byte[]) result.Data);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private async Task SendCloseResponse()
        {
            if (_socket.State == WebSocketState.CloseReceived)
            {
                await _taskQueue.Enqueue(() => _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None));
            }
        }

        private async Task<WebSocketMessage> ReadMessageAsync(WebSocket socket)
        {
            if (socket.State != WebSocketState.Open)
            {
                return WebSocketMessage.CloseMessage;
            }

            var buffer = new byte[1024];

            var ms = new MemoryStream();

            while (true)
            {
                var receiveResult = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), _disconnectToken);
                await ms.WriteAsync(buffer, 0, receiveResult.Count, CancellationToken.None);

                if (receiveResult.EndOfMessage)
                {
                    switch (receiveResult.MessageType)
                    {
                        case WebSocketMessageType.Close:
                            return WebSocketMessage.CloseMessage;
                        case WebSocketMessageType.Binary:
                            return new WebSocketMessage(WebSocketMessageType.Binary, ms.ToArray());
                        case WebSocketMessageType.Text:
                            return new WebSocketMessage(WebSocketMessageType.Text, Encoding.UTF8.GetString(ms.ToArray()));
                        default:
                            throw new ArgumentOutOfRangeException(nameof(receiveResult.MessageType), null, "Invalid message type");
                    }
                }
            }
        }
    }
}

