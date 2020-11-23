using Avalanche.Shared.Domain.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Ism.PgsTimeout.Client.V1;
using Ism.PgsTimeout.V1.Protos;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public partial class MediaService : IMediaService
    {
        public bool UseMocks { get; set; }

        PgsTimeoutSecureClient PgsTimeoutClient { get; set; }

        #region PgsTimeout

        public Task SetPgsTimeoutModeAsync(SetPgsTimeoutModeRequest setPgsTimeoutModeRequest) => PgsTimeoutClient.SetPgsTimeoutMode(setPgsTimeoutModeRequest);

        public Task SetTimeoutPageAsync(SetTimeoutPageRequest setTimeoutPageRequest) => PgsTimeoutClient.SetTimeoutPage(setTimeoutPageRequest);

        public Task NextPageAsync() => PgsTimeoutClient.NextPage();

        public Task PreviousPageAsync() => PgsTimeoutClient.PreviousPage();

        #endregion PgsTimeout
    }
}
