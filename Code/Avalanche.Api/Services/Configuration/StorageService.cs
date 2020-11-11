using Avalanche.Shared.Infrastructure.Services.Settings;
using Moq;
using System;
using System.Threading.Tasks;
using Avalanche.Shared.Infrastructure.Extensions;
using Newtonsoft.Json;
using System.IO;
using Ism.Storage.Core.Configuration.V1.Protos;
using Ism.Storage.Configuration.Client.V1;
using Ism.Security.Grpc.Interfaces;
using static Ism.Storage.Core.Configuration.V1.Protos.ConfigurationService;
using Ism.Storage.Core.Configuration.V1.Protos;
using Ism.Common.Core.Configuration.Models;

namespace Avalanche.Api.Services.Configuration
{
    public class StorageService : IStorageService
    {
        readonly IConfigurationService _configurationService;
        readonly string _hostIpAddress;

        public ConfigurationServiceSecureClient ConfigurationStorageService { get; set; }

        public StorageService(IConfigurationService configurationService, IGrpcClientFactory<ConfigurationServiceClient> grpcClientFactory, ICertificateProvider certificateProvider)
        {
            _configurationService = configurationService;

            var hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");

            var storageServiceGrpcPort = _configurationService.GetEnvironmentVariable("storageServiceGrpcPort");

            //Client = ClientHelper.GetSecureClient<WebRtcStreamer.WebRtcStreamerClient>($"https://{hostIpAddress}:{WebRTCGrpcPort}", certificate);
            //ConfigurationStorageService = ClientHelper.GetInsecureClient<ConfigurationService.ConfigurationServiceClient>($"https://{_hostIpAddress}:{storageServiceGrpcPort}");
            ConfigurationStorageService = new ConfigurationServiceSecureClient(grpcClientFactory, hostIpAddress, storageServiceGrpcPort, certificateProvider);
        }

        public async Task<T> GetJson<T>(string configurationKey, int version)
        {
            var context = new ConfigurationContext //TODO: Assign correct values
            {
                DepartmentId = "Unknown",
                UserId = "Unknown",
                SystemId = "Unknown",
                SiteId = "Unknown",
                IdnId = "Unknown",
            };

            var actionResponse = await ConfigurationStorageService.GetConfiguration(configurationKey, Convert.ToUInt32(version), context);

            return actionResponse.Get<T>();
        }
    }
}
