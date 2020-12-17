using Avalanche.Shared.Infrastructure.Models;
using Ism.Common.Core.Configuration.Models;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Maintenance
{
    public interface ISettingsService
    {
        Task<TimeoutSettings> GetTimeoutSettings(ConfigurationContext context);
        Task<SetupSettings> GetSetupSettings(ConfigurationContext context);
        Task<RoutingSettings> GetRoutingSettings(ConfigurationContext context);
        Task<PgsSettings> GetPgsSettings(ConfigurationContext context);
    }
}
