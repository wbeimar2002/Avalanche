using Ism.Common.Core.Configuration;

using System.ComponentModel.DataAnnotations;

namespace Avalanche.Shared.Infrastructure.Options
{
    public class TokenConfiguration : IConfigurationPoco
    {
        private const long MaxExpirationDuration = 3600;
        private const long MinExpirationDuration = 60;

        public string Audience { get; private set; }
        public string Issuer { get; private set; }
        public long AccessTokenExpiration { get; private set; }
        public long RefreshTokenExpiration { get; private set; }

        public bool Validate()
        {
            if (AccessTokenExpiration > MaxExpirationDuration || AccessTokenExpiration < MinExpirationDuration)
            {
                throw new ValidationException($"{nameof(AccessTokenExpiration)} of {AccessTokenExpiration} in invalid.  Must be between {MinExpirationDuration} and {MaxExpirationDuration}");
            }

            if (RefreshTokenExpiration > MaxExpirationDuration || RefreshTokenExpiration < MinExpirationDuration)
            {
                throw new ValidationException($"{nameof(RefreshTokenExpiration)} of {RefreshTokenExpiration} in invalid.  Must be between {MinExpirationDuration} and {MaxExpirationDuration}");
            }

            return true;
        }
    }
}