using Ism.Common.Core.Configuration.Models;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Maintenance
{
    public interface IStorageService
    {
        Task<T> GetJson<T>(string configurationKey, int version, ConfigurationContext context);
        Task SaveJson(string configurationKey, string json, int version, ConfigurationContext context);
    }
}
