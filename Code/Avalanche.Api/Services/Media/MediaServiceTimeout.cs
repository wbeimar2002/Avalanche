using Avalanche.Shared.Domain.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Ism.PgsTimeout.Common.Core;
using Ism.Streaming.V1.Protos;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public partial class MediaService : IMediaService
    {
        public PgsTimeout.PgsTimeoutClient PgsTimeoutClient { get; set; }

        #region PgsTimeout

        public async Task<Empty> SetPgsTimeoutModeAsync(SetPgsTimeoutModeRequest setPgsTimeoutModeRequest)
        {
            return await PgsTimeoutClient.SetPgsTimeoutModeAsync(setPgsTimeoutModeRequest);
        }

        public async Task<Empty> SetTimeoutPageAsync(SetTimeoutPageRequest setTimeoutPageRequest)
        {
            return await PgsTimeoutClient.SetTimeoutPageAsync(setTimeoutPageRequest);
        }

        public async Task<Empty> NextPageAsync(Command command)
        {
            return await PgsTimeoutClient.NextPageAsync(new Empty());
        }

        public async Task<Empty> PreviousPageAsync(Command command)
        {
            return await PgsTimeoutClient.PreviousPageAsync(new Empty());
        }

        #endregion PgsTimeout
    }
}
