using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Webster.Server
{
    public static class ServerExtensions
    {
        public static void UseSimpleServer(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<SimpleServerMiddleware>();
        }
    }

    public class SimpleServerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketServer _server;

        public SimpleServerMiddleware(RequestDelegate next, WebSocketServer server)
        {
            _next = next;
            _server = server;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                await _server.ProcessRequest(context);
            }
            else
            {
                await _next(context);
            }
        }
    }
}