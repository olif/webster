using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Webster.Server
{
    public interface IWebSocketConnection
    {
        Guid Id { get; }

        Task Send(string message);

        Task CloseConnection();
    }
}
