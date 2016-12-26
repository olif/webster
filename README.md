# Webster - A tiny .net core websocket server

![](https://nikolofs.visualstudio.com/_apis/public/build/definitions/a13d4681-6b82-49bd-86cb-55a6a8c7aae5/2/badge)

---
A small websocket server.

## Usage

1. Implement the abstract class `WebSocketServer`.

		public class DummyServer : WebSocketServer
		{
			protected override void OnMessageReceived(IWebSocketConnection conn, string message)
			{
				throw new NotImplementedException();
			}

			protected override void OnConnectionClosed(IWebSocketConnection conn)
			{
				throw new NotImplementedException();
			}

			protected override void OnConnectionOpened(IWebSocketConnection conn, HttpContext context)
			{
				throw new NotImplementedException();
			}
		}

2. Register websockets and the websocket server

		app.UseWebSockets();
		app.UseWebSocketServer(new DummyServer());

