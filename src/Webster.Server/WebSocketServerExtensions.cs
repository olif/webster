using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Webster.Server
{
    public static class WebSocketServerExtensions
    {
        public static void UseWebSocketServer(this IApplicationBuilder builder, WebSocketServer server)
        {
            builder.UseMiddleware<WebSocketServerMiddleware>(server);
        }
    }
}
