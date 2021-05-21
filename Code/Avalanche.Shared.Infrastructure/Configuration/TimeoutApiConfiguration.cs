using Avalanche.Shared.Domain.Enumerations;

namespace Avalanche.Shared.Infrastructure.Configuration
{
    public class TimeoutApiConfiguration : SingleSourceMultipleSinksConfiguration
    {
        /// <summary>
        /// Is timeout a pdf file or fullscreen video source?
        /// </summary>
        public TimeoutModes Mode { get; set; }
    }
}
