using Ism.Common.Core.Configuration.Models;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Maintenance
{
    public interface IStorageService
    {
        Task<T> GetObject<T>(string configurationKey, int version, ConfigurationContext context);
        Task<dynamic> GetDynamic(string configurationKey, int version, ConfigurationContext context);
        Task SaveJson(string configurationKey, string json, int version, ConfigurationContext context);
    }
}
