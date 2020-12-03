using Avalanche.Shared.Infrastructure.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Core.Testing;
using Ism.Common.Core.Configuration.Models;
using Ism.PgsTimeout.Client.V1;
using Ism.PgsTimeout.V1.Protos;
using Ism.Security.Grpc.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using static Ism.PgsTimeout.V1.Protos.PgsTimeout;

namespace Avalanche.Api.Services.Configuration
{
    [ExcludeFromCodeCoverage]
    public class SettingsService : ISettingsService
    {
        readonly IConfigurationService _configurationService;
        readonly IStorageService _storageService;

        readonly string _hostIpAddress;

        public PgsTimeoutSecureClient PgsTimeoutClient { get; set; }
        public bool UseMocks { get; set; }

        public SettingsService(IConfigurationService configurationService, IStorageService storageService, IGrpcClientFactory<PgsTimeoutClient> grpcPgsClientFactory, ICertificateProvider certificateProvider)
        {
            _configurationService = configurationService;
            _storageService = storageService;

            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");

            var WebRTCGrpcPort = _configurationService.GetEnvironmentVariable("WebRTCGrpcPort");
            var PgsTimeoutGrpcPort = _configurationService.GetEnvironmentVariable("PgsTimeoutGrpcPort");

            UseMocks = true;

            if (UseMocks)
            {
                var mockResponseForPdf = new GetTimeoutPdfPathResponse()
                {
                    PdfPath = @"C:\Olympus\apps\config\AvalancheApi\safety_checklist.pdf"
                };

                var mockResponseForAlwaysOn = new GetPgsAlwaysOnSettingResponse()
                {
                    IsAlwaysOn = true
                };

                var fakeCallForPdf = TestCalls.AsyncUnaryCall(Task.FromResult(mockResponseForPdf), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
                var fakeCallForAlwaysOn = TestCalls.AsyncUnaryCall(Task.FromResult(mockResponseForAlwaysOn), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });

                Mock<PgsTimeoutClient> mockGrpcClient = new Mock<PgsTimeoutClient>();
                mockGrpcClient.Setup(mock => mock.GetTimeoutPdfPathAsync(Moq.It.IsAny<Empty>(), null, null, CancellationToken.None)).Returns(fakeCallForPdf);
                mockGrpcClient.Setup(mock => mock.GetPgsAlwaysOnSettingAsync(Moq.It.IsAny<Empty>(), null, null, CancellationToken.None)).Returns(fakeCallForAlwaysOn);

                var mockPgs = new Mock<IGrpcClientFactory<PgsTimeoutClient>>();
                mockPgs.Setup(m => m.GetSecureClient(It.IsAny<string>(), It.IsAny<X509Certificate2>(), It.IsAny<X509Certificate2>(), It.IsAny<string>(), It.IsAny<List<Interceptor>>(), It.IsAny<List<Func<Metadata, Metadata>>>()))
                        .Returns(mockGrpcClient.Object);

                grpcPgsClientFactory = mockPgs.Object;
            }

            PgsTimeoutClient = new PgsTimeoutSecureClient(grpcPgsClientFactory, _hostIpAddress, PgsTimeoutGrpcPort, certificateProvider); 
        }

        public async Task<TimeoutSettings> GetTimeoutSettings()
        {
            var actionResponseForPdf = await PgsTimeoutClient.GetTimeoutPdfPath();
            var actionResponseForAlwaysOn = await PgsTimeoutClient.GetPgsAlwaysOnSetting();

            return new TimeoutSettings
            {
                CheckListFileName = actionResponseForPdf.PdfPath,
                PgsVideoAlwaysOn = actionResponseForAlwaysOn.IsAlwaysOn
            };
        }

        public Task<SetupSettings> GetSetupSettings(ConfigurationContext context)
        {
            //TODO: Complete with the new dynamic settings
            return Task.FromResult(new SetupSettings());
        }

        public Task<RoutingSettings> GetVideoRoutingSettings(ConfigurationContext context)
        {
            //TODO: Complete with the new dynamic settings
            return Task.FromResult(new RoutingSettings());
        }
    }
}
