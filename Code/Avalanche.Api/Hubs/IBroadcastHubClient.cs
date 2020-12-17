using Ism.SystemState.Models.Procedure;
using System.Threading.Tasks;

namespace Avalanche.Api.Hubs
{
    public interface IBroadcastHubClient
    {
        Task SendGeneric(string eventName, string content);

        Task OnImageCapture(ImageCapturedEvent imageCapturedEvent);
    }
}
