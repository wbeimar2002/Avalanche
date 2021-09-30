using System.Threading.Tasks;
using Avalanche.Shared.Infrastructure.Configuration;
using Avalanche.Shared.Infrastructure.Enumerations;
using Microsoft.FeatureManagement;

namespace Avalanche.Api.Helpers
{
    public static class FeaturesHelper
    {
        public static async Task<FeaturesConfiguration> GetFeatures(IFeatureManager featureManager)
        {
            if (featureManager == null)
            {
                return new FeaturesConfiguration();
            }

            return new FeaturesConfiguration
            {
                IsDevice = await featureManager.IsEnabledAsync(FeatureFlags.IsDevice),
                ActiveProcedure = await featureManager.IsEnabledAsync(FeatureFlags.ActiveProcedure),
                Devices = await featureManager.IsEnabledAsync(FeatureFlags.Devices),
                Media = await featureManager.IsEnabledAsync(FeatureFlags.Media),
                Presets = await featureManager.IsEnabledAsync(FeatureFlags.Presets),
                Recording = await featureManager.IsEnabledAsync(FeatureFlags.Recording),
                StreamSessions = await featureManager.IsEnabledAsync(FeatureFlags.StreamSessions),
            };
        }
    }
}
