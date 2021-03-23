using Avalanche.Shared.Infrastructure.Constants;

using Ism.Common.Core.Configuration;

namespace Avalanche.Shared.Infrastructure.Models
{
    public class CookieAuthConfiguration : IConfigurationPoco
    {
        public string Path { get; private set; } = "/files";
        public long ExpirationSeconds { get; private set; } = AuthenticationDurations.DefaultSessionDuration;

        public bool Validate()
        {
            AuthenticationDurations.Validate(ExpirationSeconds);
            return true;
        }
    }
}
