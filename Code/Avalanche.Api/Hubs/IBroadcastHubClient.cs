using Ism.SystemState.Models.Procedure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Hubs
{
    public interface IBroadcastHubClient
    {
        Task SendGeneric(string eventName, string content);

        Task OnImageCapture(ImageCapturedEvent imageCapturedEvent);
    }
}
