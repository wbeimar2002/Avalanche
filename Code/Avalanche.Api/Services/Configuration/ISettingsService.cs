using Avalanche.Shared.Infrastructure.Models;
using Ism.Common.Core.Configuration.Models;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Configuration
{
    public interface ISettingsService
    {
        Task<TimeoutSettings> GetTimeoutSettings();
        Task<SetupSettings> GetSetupSettings(ConfigurationContext context);
        Task<RoutingSettings> GetVideoRoutingSettings(ConfigurationContext context);
    }
}
