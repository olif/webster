using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Webster.Server.IntegrationTests
{
    public class WebSocketServerTests
    {
        private readonly WebSocketClient _client;
        private readonly WebSocketServerStub _stub;

        public WebSocketServerTests()
        {
            var testServer = new TestServer(new WebHostBuilder().UseStartup<TestStartup>());
            _client = testServer.CreateWebSocketClient();
            _stub = TestStartup.Server;
        }

        [Fact]
        public async Task ConnectionOpened_IsCalled_WhenConnectionHasEstablished()
        {
            bool onOpenCalled = false;
            _stub.ConnectionOpened = (conn, query) => onOpenCalled = true;
            await _client.ConnectAsync(new Uri("http://localhost"), CancellationToken.None);

            await Task.Delay(100);
            onOpenCalled.Should().BeTrue();
        }

        [Fact]
        public async Task TextMessageRecieved_IsCalledWithTheMessage_WhenMessageIsReceived()
        {
            var semaphore = new SemaphoreSlim(0);
            var msg = "hello";
            var recievedMsg = "";
            _stub.TextMessageReceived = (conn, rcvdMsg) =>
            {
                recievedMsg = rcvdMsg;
                semaphore.Release(1);
            };
            var socket = await _client.ConnectAsync(new Uri("http://localhost"), CancellationToken.None);
            await socket.SendAsync(GetWebsocketMsg(msg), WebSocketMessageType.Text, true, CancellationToken.None);

            await Task.WhenAny(semaphore.WaitAsync(), Task.Delay(200));
            recievedMsg.Should().Be(msg);
        }

        [Fact]
        public async Task BinaryMessageRecieved_IsCalledWithTheMessage_WhenMessageIsReceived()
        {
            var semaphore = new SemaphoreSlim(0);
            var msg = "hello";
            byte[] recievedMsg = null;
            _stub.BinaryMessageReceived = (conn, rcvdMsg) =>
            {
                recievedMsg = rcvdMsg;
                semaphore.Release(1);
            };
            var socket = await _client.ConnectAsync(new Uri("http://localhost"), CancellationToken.None);
            await socket.SendAsync(GetWebsocketMsg(msg), WebSocketMessageType.Binary, true, CancellationToken.None);

            await Task.WhenAny(semaphore.WaitAsync(), Task.Delay(200));
            var receivedText = Encoding.UTF8.GetString(recievedMsg);
            receivedText.Should().Be(msg);
        }

        [Fact]
        public async Task ConnectionClosed_IsCalled_WhenConnectionHasBeenClosed()
        {
            var openedSem = new SemaphoreSlim(0);
            var closedSem = new SemaphoreSlim(0);
            var connectionClosedCalled = false;
            _stub.ConnectionClosed = (_) =>
            {
                connectionClosedCalled = true;
                closedSem.Release(1);
            };
            _stub.ConnectionOpened = (_, __) => openedSem.Release(1);
            var socket = await _client.ConnectAsync(new Uri("http://localhost"), CancellationToken.None);

            await Task.WhenAny(openedSem.WaitAsync(), Task.Delay(200));
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);

            await Task.WhenAny(closedSem.WaitAsync(), Task.Delay(200));
            connectionClosedCalled.Should().BeTrue();
        }

        [Fact]
        public async Task CloseInitiatedByClient_InvokesOnCloseOnServer()
        {
            var connClosedSem = new SemaphoreSlim(0);
            var connHasBeenClosed = false;
            _stub.TextMessageReceived = async (conn, _) =>
            {
                await Task.Delay(200);
                await conn.CloseConnection();
            };

            var socket = await _client.ConnectAsync(new Uri("http://localhost"), CancellationToken.None);
            var clientConn = new WebSocketConnection(socket, null, CancellationToken.None)
            {
                OnClose = () =>
                {
                    connHasBeenClosed = true;
                    connClosedSem.Release(1);
                }
            };

            var thread = new Thread(async () => await clientConn.ProcessRequest(CancellationToken.None));
            thread.Start();

            await socket.SendAsync(GetWebsocketMsg("close"), WebSocketMessageType.Text, true, CancellationToken.None);

            // This delay must be increased if running test in debug-mode
            await Task.WhenAny(connClosedSem.WaitAsync(), Task.Delay(400));
            connHasBeenClosed.Should().BeTrue();
        }

        private ArraySegment<byte> GetWebsocketMsg(string msg) => new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
    }
}