using Ism.Common.Core.Configuration;

using System.ComponentModel.DataAnnotations;

namespace Avalanche.Shared.Infrastructure.Models
{
    public class AuthConfiguration : IConfigurationPoco
    {
        private const int MinKeyLength = 16;

        public string SecretKey { get; private set; }

        public bool Validate()
        {
        if (string.IsNullOrWhiteSpace(SecretKey))
        {
            throw new ValidationException($"{nameof(SecretKey)} cannot be null, empty or whitespace");
        }

            if (SecretKey.Length < MinKeyLength)
            {
                throw new ValidationException($"{nameof(SecretKey)} must be longer than {MinKeyLength} characters");
            }

            return true;
        }
    }
}
