using Ism.Common.Core.Configuration.Models;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Maintenance
{
    public interface IStorageService
    {
        Task<string> GetJson(string configurationKey, int version, ConfigurationContext context);
        Task<T> GetJsonFullObject<T>(string configurationKey, int version, ConfigurationContext context);
        Task<T> GetJsonObject<T>(string configurationKey, int version, ConfigurationContext context);
        Task<dynamic> GetJsonDynamic(string configurationKey, int version, ConfigurationContext context);
        Task SaveJsonObject(string configurationKey, string json, int version, ConfigurationContext context);
        Task SaveJsonMetadata(string configurationKey, string json, int version, ConfigurationContext context);
        Task<dynamic> GetJsonFullDynamic(string configurationKey, int version, ConfigurationContext context);
        Task UpdateConfiguration<TData>(string configurationKey, int version, ConfigurationContext context, Action<JsonPatchDocument<TData>> update) where TData : class, new();
    }
}
