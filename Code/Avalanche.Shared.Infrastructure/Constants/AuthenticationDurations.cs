using System.ComponentModel.DataAnnotations;

namespace Avalanche.Shared.Infrastructure.Constants
{
    public static class AuthenticationDurations
    {
        public const long MaxSessionDuration = 86400;
        public const long MinSessionDuration = 60;
        public const long DefaultSessionDuration = 900;

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
