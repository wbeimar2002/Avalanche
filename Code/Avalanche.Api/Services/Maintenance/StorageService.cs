using Avalanche.Shared.Infrastructure.Extensions;

using Ism.Common.Core.Configuration.Models;
using Ism.Storage.Configuration.Client.V1;

using Newtonsoft.Json.Linq;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalanche.Api.Extensions;
using System.Collections.Generic;
using System.Linq;

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

        public async Task<string> GetJson(string configurationKey, int version, ConfigurationContext context)
        {
            var json = await _client.GetConfiguration(configurationKey, Convert.ToUInt32(version), context);

            if (json != null)
            {
                var jObject = JObject.Parse(json);
                JObject child = (JObject)jObject[configurationKey];

                return child.ToString();
            }
            else
                return null;
        }

        public async Task<dynamic> GetJsonFullDynamic(string configurationKey, int version, ConfigurationContext context)
        {
            var json = await _client.GetConfiguration(configurationKey, Convert.ToUInt32(version), context);
            return json == null ? null : JObject.Parse(json);
        }

        public async Task<List<dynamic>> GetJsonDynamicList(string configurationKey, int version, ConfigurationContext context)
        {
            var json = await _client.GetConfiguration(configurationKey, Convert.ToUInt32(version), context);

            if (string.IsNullOrEmpty(json))
                return null;
            else
            {
                var jObject = JObject.Parse(json);

                JToken jToken = null;
                jObject.TryGetValue(configurationKey, out jToken);

                if (jToken != null)
                {
                    if (jToken is JArray)
                    {
                        JArray child = (JArray)jObject[configurationKey];
                        return child == null ? null : child.Select(d => (dynamic)d).ToList();
                    }
                }

                return null;

            }
        }

        public async Task<dynamic> GetJsonDynamic(string configurationKey, int version, ConfigurationContext context)
        {
            var json = await _client.GetConfiguration(configurationKey, Convert.ToUInt32(version), context);

            if (string.IsNullOrEmpty(json))
                return null;
            else
            {
                var jObject = JObject.Parse(json);

                JToken jToken = null;
                jObject.TryGetValue(configurationKey, out jToken);

                if (jToken != null)
                {
                    if (jToken is JObject)
                    {
                        JObject child = (JObject)jObject[configurationKey];
                        return child == null ? null : child;
                    }
                }

                return null;
                
            }
        }

        public async Task<T> GetJsonObject<T>(string configurationKey, int version, ConfigurationContext context)
        {
            var json = await _client.GetConfiguration(configurationKey, Convert.ToUInt32(version), context);

            var jObject = JObject.Parse(json);

            JToken jToken = null;

            jObject.TryGetValue(configurationKey, out jToken);

            if (jToken != null)
            {
                if (jToken is JObject)
                {
                    JObject child = (JObject)jObject[configurationKey];
                    json = child.ToString();
                }

                if (jToken is JArray)
                {
                    JArray child = (JArray)jObject[configurationKey];
                    json = child.ToString();
                }
            }

            return json.Get<T>();
        }

        public async Task SaveJsonObject(string configurationKey, string json, int version, ConfigurationContext context)
        {
            var kind = await _client.GetConfigurationKinds();
            var kindId = context.SiteId;

            string jsonWrapper = @"{}";
            JObject jsonRoot = JObject.Parse(jsonWrapper);

            jsonRoot.Add(new JProperty(configurationKey, JObject.Parse(json)));

            string finalJson = jsonRoot.ToString(Newtonsoft.Json.Formatting.None);
            await _client.SaveConfiguration(configurationKey, Convert.ToUInt32(version), finalJson, "Site", kindId);
        }

        public async Task SaveJsonMetadata(string configurationKey, string json, int version, ConfigurationContext context)
        {
            var kind = await _client.GetConfigurationKinds();
            var kindId = context.SiteId;
            await _client.SaveConfiguration(configurationKey, Convert.ToUInt32(version), json, "Site", kindId);
        }
    }
}
