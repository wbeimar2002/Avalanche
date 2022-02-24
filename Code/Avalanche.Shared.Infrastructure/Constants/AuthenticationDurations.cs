using FluentValidation;

namespace Avalanche.Shared.Infrastructure.Constants
{
    public static class AuthenticationDurations
    {
        public const long DefaultSessionDuration = 900;
        public const long DefaultTokenDuration = 600;
        public const long MaxSessionDuration = 86400;
        public const long MinSessionDuration = 60;
        public static bool Validate(long sessionDuration)
        {
            if (sessionDuration > MaxSessionDuration || sessionDuration < MinSessionDuration)
            {
                throw new ValidationException($"{nameof(sessionDuration)} of {sessionDuration} in invalid.  Must be between {MinSessionDuration} and {MaxSessionDuration}");
            }
            return true;
        }
    }
}
