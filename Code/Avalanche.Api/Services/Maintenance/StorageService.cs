using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Avalanche.Shared.Infrastructure.Extensions;
using Ism.Common.Core.Configuration.Models;
using Ism.Storage.Configuration.Client.V1;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace Avalanche.Api.Services.Maintenance
{
    [ExcludeFromCodeCoverage]
    public class StorageService : IStorageService
    {
        private readonly ConfigurationServiceSecureClient _client;

        public StorageService(ConfigurationServiceSecureClient client) => _client = client;

        public async Task<bool> ValidateSchema(string schemaKey, string json, int version, ConfigurationContext configurationContext)
        {
            if (string.IsNullOrEmpty(schemaKey))
            {
                return true;
            }
            else
            {
                dynamic dynamicSchema = await GetJsonFullDynamic(schemaKey, version, configurationContext);

                if (dynamicSchema == null)
                    return true;
                else
                {
                    string schemaJson = JsonConvert.SerializeObject(dynamicSchema);
#pragma warning disable CS0618 // Type or member is obsolete
                    var schema = JsonSchema.Parse(schemaJson);
                    JObject jsonObject = JObject.Parse(json);
                    return jsonObject.IsValid(schema);
#pragma warning restore CS0618 // Type or member is obsolete
                }
            }
        }

        public async Task UpdateJsonProperty(string configurationKey, string jsonKey, string jsonValue, int version, ConfigurationContext context, bool isList = false)
        {
            var json = await _client.GetConfiguration(configurationKey, Convert.ToUInt32(version), context);

            var jsonRoot = JObject.Parse(json);
            jsonRoot = (JObject)jsonRoot[configurationKey];

            var jObject = jsonRoot;
            var keys = jsonKey.Split('.');

            for (var i = 0; i < keys.Length; i++)
            {
                if (i == keys.Length - 1)
                {
                    if (isList)
                    {
                        jObject[keys[i]] = JArray.Parse(jsonValue);
                    }
                    else
                    {
                        jObject[keys[i]] = JObject.Parse(jsonValue);
                    }
                }
                else
                {
                    jObject = (JObject)jObject[keys[i]];
                }
            }

            await SaveJsonObject(configurationKey, jsonRoot.ToString(), version, context);
        }

        public async Task<T> GetJsonObject<T>(string configurationKey, int version, ConfigurationContext context)
        {
            var json = await _client.GetConfiguration(configurationKey, Convert.ToUInt32(version), context);
            json = GetJson(configurationKey, json);
            return json.Get<T>();
        }

        public async Task<string> GetJson(string configurationKey, int version, ConfigurationContext context)
        {
            var json = await _client.GetConfiguration(configurationKey, Convert.ToUInt32(version), context);
            return GetJson(configurationKey, json);
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
            {
                return null;
            }
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
            {
                return null;
            }
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

        public async Task SaveJsonObject(string configurationKey, string json, int version, ConfigurationContext context, bool isList = false)
        {
            var kind = await _client.GetConfigurationKinds();
            var kindId = context.SiteId;

            var jsonWrapper = @"{}";
            var jsonRoot = JObject.Parse(jsonWrapper);

            if (isList)
            {
                jsonRoot.Add(new JProperty(configurationKey, JArray.Parse(json)));
            }
            else
            {
                jsonRoot.Add(new JProperty(configurationKey, JObject.Parse(json)));
            }

            var finalJson = jsonRoot.ToString(Formatting.None);
            await _client.SaveConfiguration(configurationKey, Convert.ToUInt32(version), finalJson, "Site", kindId);
        }

        public async Task SaveJsonMetadata(string configurationKey, string json, int version, ConfigurationContext context)
        {
            var kind = await _client.GetConfigurationKinds();
            var kindId = context.SiteId;
            await _client.SaveConfiguration(configurationKey, Convert.ToUInt32(version), json, "Site", kindId);
        }

        private string GetJson(string configurationKey, string json)
        {
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

            return json;
        }

        public async Task UpdateConfiguration<TData>(string configurationKey, int version, ConfigurationContext context, JsonPatchDocument<TData> update) where TData : class, new()
        {
            var patch = SerializeJsonPatch(update);

            var kindId = context.SiteId;
            await _client.UpdateConfiguration(configurationKey, Convert.ToUInt32(version), "Site", kindId, patch);
        }

        public Task UpdateConfiguration<TData>(string configurationKey, int version, ConfigurationContext context, Action<JsonPatchDocument<TData>> update) where TData : class, new()
        {
            var patch = new JsonPatchDocument<TData>();
            update(patch);

            return UpdateConfiguration(configurationKey, version, context, patch);
        }

        // json patch deserialize requires newtonsoft; no current plan for system.text.json support: https://github.com/dotnet/aspnetcore/issues/16968
        private string SerializeJsonPatch<TData>(JsonPatchDocument<TData> patch)
            where TData : class, new()
            => JsonConvert.SerializeObject(patch);
    }
}
