using Ism.Broadcaster.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ism.Broadcaster.Services
{
    public interface IBroadcasterClientService
    {
        Task<bool> Send(Uri endpoint, MessageRequest message);
    }
}
