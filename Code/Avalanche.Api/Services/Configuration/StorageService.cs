using Avalanche.Shared.Infrastructure.Services.Settings;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core.Testing;
using Ism.Security.Grpc.Helpers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Avalanche.Shared.Infrastructure.Extensions;
using Newtonsoft.Json;
using System.IO;
using Ism.Storage.Common.Core.Configuration.Protos;

namespace Avalanche.Api.Services.Configuration
{
    public class StorageService : IStorageService
    {
        readonly IConfigurationService _configurationService;
        readonly string _hostIpAddress;

        public bool IgnoreGrpcServicesMocks { get; set; }

        public ConfigurationService.ConfigurationServiceClient ConfigurationStorageService { get; set; }

        public StorageService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;

            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");

            var PatientListStoragePort = _configurationService.GetEnvironmentVariable("PatientListStoragePort");
            var grpcCertificate = _configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = _configurationService.GetEnvironmentVariable("grpcPassword");

            IgnoreGrpcServicesMocks = Convert.ToBoolean(_configurationService.GetEnvironmentVariable("IgnoreGrpcServicesMocks"));

            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(grpcCertificate, grpcPassword);

            //Client = ClientHelper.GetSecureClient<WebRtcStreamer.WebRtcStreamerClient>($"https://{hostIpAddress}:{WebRTCGrpcPort}", certificate);
            ConfigurationStorageService = ClientHelper.GetInsecureClient<ConfigurationService.ConfigurationServiceClient>($"https://{_hostIpAddress}:{PatientListStoragePort}");
        }

        public async Task<T> GetJson<T>(string configurationKey, int version)
        {
            var request = new GetConfigurationRequest()
            {
                Version = Convert.ToUInt32(version),
                Section = configurationKey
            };
            
            //Faking calls while I have the real server
            if (!IgnoreGrpcServicesMocks)
            {
                Mock<ConfigurationService.ConfigurationServiceClient> mockGrpcClient = new Mock<ConfigurationService.ConfigurationServiceClient>();
                var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new GetConfigurationResponse() { ConfigurationJson = string.Empty }), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
                mockGrpcClient.Setup(mock => mock.GetConfigurationAsync(request, null, null, CancellationToken.None)).Returns(fakeCall);

                ConfigurationStorageService = mockGrpcClient.Object;
            }

            var actionResponse = await ConfigurationStorageService.GetConfigurationAsync(request);

            return actionResponse.ConfigurationJson.Get<T>();
        }

        public async Task SaveJson<T>(string configurationKey, T jsonObject)
        {
            //Faking calls while I have the real server
            if (!IgnoreGrpcServicesMocks)
            {
                Mock<ConfigurationService.ConfigurationServiceClient> mockGrpcClient = new Mock<ConfigurationService.ConfigurationServiceClient>();
                var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
                mockGrpcClient.Setup(mock => mock.SaveConfigurationAsync(Moq.It.IsAny<SaveConfigurationRequest>(), null, null, CancellationToken.None)).Returns(fakeCall);

                ConfigurationStorageService = mockGrpcClient.Object;
            }

            var actionResponse = await ConfigurationStorageService.SaveConfigurationAsync(new SaveConfigurationRequest()
            {
                Section = configurationKey,
                ConfigurationJson = jsonObject.Json()
            });
        }
    }
}
