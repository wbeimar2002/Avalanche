using Avalanche.Shared.Infrastructure.Extensions;

using Ism.Common.Core.Configuration.Models;
using Ism.Storage.Configuration.Client.V1;

using Newtonsoft.Json.Linq;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Maintenance
{
    [ExcludeFromCodeCoverage]
    public class StorageService : IStorageService
    {
        const string _siteId = "Avalanche"; //Temporary hardcoded

        private readonly ConfigurationServiceSecureClient _client;

        public StorageService(ConfigurationServiceSecureClient client)
        {
            _client = client;
        }

        public async Task<dynamic> GetJsonDynamic(string configurationKey, int version, ConfigurationContext context)
        {
            context.SiteId = _siteId;
            var json = await _client.GetConfiguration(configurationKey, Convert.ToUInt32(version), context);
            return json == null ? null : JObject.Parse(json);
        }

        public async Task<T> GetJsonObject<T>(string configurationKey, int version, ConfigurationContext context)
        {
            context.SiteId = _siteId;
            var actionResponse = await _client.GetConfiguration(configurationKey, Convert.ToUInt32(version), context);
           return actionResponse.Get<T>();
        }

        public async Task SaveJson(string configurationKey, string json, int version, ConfigurationContext context)
        {
            var kind = await _client.GetConfigurationKinds();
            var kindId = _siteId; 
            await _client.SaveConfiguration(configurationKey, Convert.ToUInt32(version), json, "Site", kindId);
        }
    }
}
