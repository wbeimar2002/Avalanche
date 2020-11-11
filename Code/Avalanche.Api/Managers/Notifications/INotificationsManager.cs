using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Notifications
{
    public interface INotificationsManager
    {
        void SendQueuedMessage(Ism.Broadcaster.Models.MessageRequest message);
        void SendDirectMessage(Ism.Broadcaster.Models.MessageRequest message);
    }
}
