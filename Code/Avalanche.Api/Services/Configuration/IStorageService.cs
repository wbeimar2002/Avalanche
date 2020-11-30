using Ism.Common.Core.Configuration.Models;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Configuration
{
    public interface IStorageService
    {
        Task<T> GetJson<T>(string configurationKey, int version, ConfigurationContext context);
    }
}
