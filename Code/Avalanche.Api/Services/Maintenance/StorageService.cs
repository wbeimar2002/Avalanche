using Avalanche.Shared.Infrastructure.Extensions;

using Ism.Common.Core.Configuration.Models;
using Ism.Storage.Configuration.Client.V1;

using Newtonsoft.Json.Linq;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalanche.Api.Extensions;

namespace Avalanche.Api.Services.Maintenance
{
    [ExcludeFromCodeCoverage]
    public class StorageService : IStorageService
    {
        private readonly ConfigurationServiceSecureClient _client;

        public StorageService(ConfigurationServiceSecureClient client)
        {
            _client = client;
        }

        public async Task<dynamic> GetJsonDynamic(string configurationKey, int version, ConfigurationContext context)
        {
            var json = await _client.GetConfiguration(configurationKey, Convert.ToUInt32(version), context);

            if (string.IsNullOrEmpty(json))
                return null;
            else
            {
                var jObject = JObject.Parse(json);
                JObject child = null;

                if (jObject.ContainsKey(configurationKey))
                    child = (JObject)jObject[configurationKey];

                return child == null ? null : child;
            }
        }

        public async Task<T> GetJsonFullObject<T>(string configurationKey, int version, ConfigurationContext context)
        {
            var json = await _client.GetConfiguration(configurationKey, Convert.ToUInt32(version), context);
            return json.Get<T>();
        }

        public async Task<T> GetJsonObject<T>(string configurationKey, int version, ConfigurationContext context)
        {
            var json = await _client.GetConfiguration(configurationKey, Convert.ToUInt32(version), context);

            var jObject = JObject.Parse(json);
            JObject child = (JObject)jObject[configurationKey];

            json = child.ToString();

            return json.Get<T>();
        }

        public async Task SaveJson(string configurationKey, string json, int version, ConfigurationContext context)
        {
            var kind = await _client.GetConfigurationKinds();
            var kindId = context.SiteId;

            string jsonWrapper = @"{}";
            JObject jsonRoot = JObject.Parse(jsonWrapper);

            jsonRoot.Add(new JProperty(configurationKey, JObject.Parse(json)));

            string finalJson = jsonRoot.ToString(Newtonsoft.Json.Formatting.None);
            await _client.SaveConfiguration(configurationKey, Convert.ToUInt32(version), finalJson, "Site", kindId);
        }
    }
}
