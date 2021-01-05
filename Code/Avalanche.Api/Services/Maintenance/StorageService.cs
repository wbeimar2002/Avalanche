using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.Common.Core.Configuration.Models;
using Ism.Security.Grpc.Interfaces;
using Ism.Storage.Configuration.Client.V1;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using static Ism.Storage.Core.Configuration.V1.Protos.ConfigurationService;

namespace Avalanche.Api.Services.Maintenance
{
    [ExcludeFromCodeCoverage]
    public class StorageService : IStorageService
    {
        readonly IConfigurationService _configurationService;
        const string _siteId = "Avalanche"; //Temporary hardcoded

        ConfigurationServiceSecureClient ConfigurationStorageService { get; set; }

        public StorageService(IConfigurationService configurationService, IGrpcClientFactory<ConfigurationServiceClient> grpcClientFactory, ICertificateProvider certificateProvider)
        {
            _configurationService = configurationService;

            var hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");
            var storageServiceGrpcPort = _configurationService.GetEnvironmentVariable("storageServiceGrpcPort");

            ConfigurationStorageService = new ConfigurationServiceSecureClient(grpcClientFactory, hostIpAddress, storageServiceGrpcPort, certificateProvider);
        }

        public async Task<T> GetJsonObject<T>(string configurationKey, int version, ConfigurationContext context)
        {
            context.SiteId = _siteId;
            var actionResponse = await ConfigurationStorageService.GetConfiguration(configurationKey, Convert.ToUInt32(version), context);
            return actionResponse.Get<T>();
        }

        public async Task<dynamic> GetJsonDynamic(string configurationKey, int version, ConfigurationContext context)
        {
            context.SiteId = _siteId;
            var json = await ConfigurationStorageService.GetConfiguration(configurationKey, Convert.ToUInt32(version), context);
            return json == null ? null : JObject.Parse(json);
        }

        public async Task SaveJson(string configurationKey, string json, int version, ConfigurationContext context)
        {
            var kind = await ConfigurationStorageService.GetConfigurationKinds();
            var kindId = _siteId; 
            await ConfigurationStorageService.SaveConfiguration(configurationKey, Convert.ToUInt32(version), json, "Site", kindId);
        }
    }
}
