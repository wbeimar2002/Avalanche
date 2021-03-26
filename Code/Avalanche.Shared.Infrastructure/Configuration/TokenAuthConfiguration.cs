using Avalanche.Shared.Infrastructure.Constants;

using Ism.Common.Core.Configuration;

namespace Avalanche.Shared.Infrastructure.Options
{
    public class TokenAuthConfiguration : IConfigurationPoco
    {
        public string Audience { get; private set; }
        public string Issuer { get; private set; }
        public long ExpirationSeconds { get; private set; } = AuthenticationDurations.DefaultTokenDuration;
        public long RefreshExpirationSeconds { get; private set; } = AuthenticationDurations.DefaultSessionDuration;

        public bool Validate()
        {
            AuthenticationDurations.Validate(ExpirationSeconds);
            AuthenticationDurations.Validate(RefreshExpirationSeconds);
            return true;
        }
    }
}