using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Shared.Infrastructure.Security.Hashing
{
    public class BcryptPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            ThrowIfNullOrEmpty(password, nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool PasswordMatches(string providedPassword, string passwordHash)
        {
            ThrowIfNullOrEmpty(providedPassword, nameof(providedPassword));
            ThrowIfNullOrEmpty(passwordHash, nameof(passwordHash));

            return BCrypt.Net.BCrypt.Verify(providedPassword, passwordHash);
        }
    }
}
