using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Models;
using Ism.PgsTimeout.Common.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public interface IMediaService
    {
        Ism.Streaming.V1.Protos.WebRtcStreamer.WebRtcStreamerClient WebRtcStreamerClient { get; set; }
        PgsTimeout.PgsTimeoutClient PgsTimeoutClient { get; set; }

        //General
        Task<Ism.Streaming.V1.Protos.GetSourceStreamsResponse> GetSourceStreamsAsync();

        //Video
        Task<Google.Protobuf.WellKnownTypes.Empty> HandleMessageAsync(Ism.Streaming.V1.Protos.HandleMessageRequest handleMessageRequest);
        Task<Ism.Streaming.V1.Protos.InitSessionResponse> InitSessionAsync(Ism.Streaming.V1.Protos.InitSessionRequest initSessionRequest);
        Task<Google.Protobuf.WellKnownTypes.Empty> DeInitSessionAsync(Ism.Streaming.V1.Protos.DeInitSessionRequest deInitSessionRequest);

        //Timeout PDF
        Task<Google.Protobuf.WellKnownTypes.Empty> SetPgsTimeoutModeAsync(SetPgsTimeoutModeRequest setPgsTimeoutModeRequest);
        Task<Google.Protobuf.WellKnownTypes.Empty> SetTimeoutPageAsync(SetTimeoutPageRequest setTimeoutPageRequest);
        Task<Google.Protobuf.WellKnownTypes.Empty> NextPageAsync(Command command);
        Task<Google.Protobuf.WellKnownTypes.Empty> PreviousPageAsync(Command command);
    }
}
