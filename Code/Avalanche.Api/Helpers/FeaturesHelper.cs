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
                IsDevice = await featureManager.IsEnabledAsync(FeatureFlags.IsDevice).ConfigureAwait(false),
                ActiveProcedure = await featureManager.IsEnabledAsync(FeatureFlags.ActiveProcedure).ConfigureAwait(false),
                Devices = await featureManager.IsEnabledAsync(FeatureFlags.Devices).ConfigureAwait(false),
                Media = await featureManager.IsEnabledAsync(FeatureFlags.Media).ConfigureAwait(false),
                Presets = await featureManager.IsEnabledAsync(FeatureFlags.Presets).ConfigureAwait(false),
                Recording = await featureManager.IsEnabledAsync(FeatureFlags.Recording).ConfigureAwait(false),
                WebRtc = await featureManager.IsEnabledAsync(FeatureFlags.WebRtc).ConfigureAwait(false),
            };
        }
    }
}
