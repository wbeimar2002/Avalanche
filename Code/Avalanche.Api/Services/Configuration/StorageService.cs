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
using Ism.Storage.Core.Configuration.V1.Protos;

namespace Avalanche.Api.Services.Configuration
{
    public class StorageService : IStorageService
    {
        readonly IConfigurationService _configurationService;
        readonly string _hostIpAddress;

        public ConfigurationService.ConfigurationServiceClient ConfigurationStorageService { get; set; }

        public StorageService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;

            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");

            var storageServiceGrpcPort = _configurationService.GetEnvironmentVariable("storageServiceGrpcPort");
            var grpcCertificate = _configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = _configurationService.GetEnvironmentVariable("grpcPassword");

            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(grpcCertificate, grpcPassword);

            //Client = ClientHelper.GetSecureClient<WebRtcStreamer.WebRtcStreamerClient>($"https://{hostIpAddress}:{WebRTCGrpcPort}", certificate);
            ConfigurationStorageService = ClientHelper.GetInsecureClient<ConfigurationService.ConfigurationServiceClient>($"https://{_hostIpAddress}:{storageServiceGrpcPort}");
        }

        public async Task<T> GetJson<T>(string configurationKey, int version)
        {
            var request = new GetConfigurationRequest()
            {
                Version = Convert.ToUInt32(version),
                Section = configurationKey,
                Context = new ConfigurationContextMessage() //TODO: Assign correct values
                {
                    DepartmentId = "Unknown",
                    UserId = "Unknown",
                    SystemId = "Unknown",
                    SiteId = "Unknown",
                    IdnId = "Unknown",
                }
            };

            var actionResponse = await ConfigurationStorageService.GetConfigurationAsync(request);

            return actionResponse.ConfigurationJson.Get<T>();
        }
    }
}
