using Avalanche.Shared.Infrastructure.Constants;

using Ism.Common.Core.Configuration;

namespace Avalanche.Shared.Infrastructure.Options
{
    public class TokenAuthConfiguration : IConfigurationPoco
    {
        // Default constructor needed for binding
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TokenAuthConfiguration() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public TokenAuthConfiguration(string audience, string issuer, long expirationSeconds, long refreshSeconds)
        {
            Audience = audience;
            Issuer = issuer;
            ExpirationSeconds = expirationSeconds;
            RefreshExpirationSeconds = refreshSeconds;
        }

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
