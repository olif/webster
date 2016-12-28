using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Webster.Server
{
    public interface IWebSocketConnection
    {
        /// <summary>
        /// Unique id of this connection
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// The websocket request context
        /// </summary>
        HttpContext HttpContext { get; }

        Task Send(string message);

        Task CloseConnection();
    }
}
