using Ism.Common.Core.Configuration.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Maintenance
{
    public interface IStorageService
    {
        Task<string> GetJson(string configurationKey, int version, ConfigurationContext context);
        
        Task<T> GetJsonObject<T>(string configurationKey, int version, ConfigurationContext context);

        Task<dynamic> GetJsonFullDynamic(string configurationKey, int version, ConfigurationContext context);
        Task<dynamic> GetJsonDynamic(string configurationKey, int version, ConfigurationContext context);
        Task<List<dynamic>> GetJsonDynamicList(string configurationKey, int version, ConfigurationContext context);

        Task SaveJsonObject(string configurationKey, string json, int version, ConfigurationContext context);
        Task SaveJsonMetadata(string configurationKey, string json, int version, ConfigurationContext context);
    }
}
