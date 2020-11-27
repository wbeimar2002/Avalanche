using Ism.PgsTimeout.V1.Protos;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public interface IMediaService
    {
        //General
        Task<Ism.Streaming.V1.Protos.GetSourceStreamsResponse> GetSourceStreamsAsync();

        //Video
        Task HandleMessageAsync(Ism.Streaming.V1.Protos.HandleMessageRequest handleMessageRequest);
        Task<Ism.Streaming.V1.Protos.InitSessionResponse> InitSessionAsync(Ism.Streaming.V1.Protos.InitSessionRequest initSessionRequest);
        Task DeInitSessionAsync(Ism.Streaming.V1.Protos.DeInitSessionRequest deInitSessionRequest);

        //Timeout PDF
        Task SetPgsTimeoutModeAsync(SetPgsTimeoutModeRequest setPgsTimeoutModeRequest);
        Task SetTimeoutPageAsync(SetTimeoutPageRequest setTimeoutPageRequest);
        Task NextPageAsync();
        Task PreviousPageAsync();
    }
}
