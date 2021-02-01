using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.PgsTimeout.Client.V1;
using Ism.Security.Grpc.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Ism.Utility.Core.Preconditions;
using static Ism.PgsTimeout.V1.Protos.PgsTimeout;
using Ism.PgsTimeout.V1.Protos;

namespace Avalanche.Api.Services.Media
{
    public class PgsTimeoutService : IPgsTimeoutService
    {
        private readonly IConfigurationService _configurationService;
        private readonly PgsTimeoutSecureClient _client;

        public PgsTimeoutService(
            IConfigurationService configurationService, 
            IGrpcClientFactory<PgsTimeoutClient> grpcClientFactory, 
            ICertificateProvider certificateProvider)
        {
            _configurationService = ThrowIfNullOrReturn(nameof(configurationService), configurationService);
            ThrowIfNull(nameof(grpcClientFactory), grpcClientFactory);
            ThrowIfNull(nameof(certificateProvider), certificateProvider);

            var ip = _configurationService.GetEnvironmentVariable("hostIpAddress");
            var port = _configurationService.GetEnvironmentVariable("PgsTimeoutGrpcPort");

            _client = new PgsTimeoutSecureClient(grpcClientFactory, ip, port, certificateProvider);
        }

        public Task SetPgsTimeoutMode(SetPgsTimeoutModeRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetPgsTimeoutModeResponse> GetPgsTimeoutMode()
        {
            throw new NotImplementedException();
        }

        public Task<GetPgsVideoFileResponse> GetPgsVideoFile()
        {
            throw new NotImplementedException();
        }

        public Task<GetPgsVideoListResponse> GetPgsVideoFileList()
        {
            throw new NotImplementedException();
        }

        public Task SetPgsVideoFile(SetPgsVideoFileRequest request)
        {
            throw new NotImplementedException();
        }

        public Task SetCurrentVideoToRandomTime(RandomPosRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetTimeoutPageResponse> GetTimeoutPage()
        {
            throw new NotImplementedException();
        }

        public Task SetTimeoutPage(SetTimeoutPageRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetTimeoutPageCountResponse> GetTimeoutPageCount()
        {
            throw new NotImplementedException();
        }

        public Task<GetTimeoutPdfPathResponse> GetTimeoutPdfPath()
        {
            throw new NotImplementedException();
        }

        public Task NextPage()
        {
            throw new NotImplementedException();
        }

        public Task PreviousPage()
        {
            throw new NotImplementedException();
        }
    }
}
