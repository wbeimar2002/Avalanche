using System;
using System.Linq;
using Avalanche.Shared.Infrastructure.Security.Hashing;
using Xunit;

namespace Avalanche.Security.Server.Test.Security.Hashing
{
#pragma warning disable CA1707 // Identifiers should not contain underscores
    public class PasswordHasherTests
    {
        [Fact]
        public void PasswordHasher_EmptyPassword_Throws()
        {
            // Arrange
            var hasher = new PasswordHasher();
            const string password = "";

            // Act & Assert
            _ = Assert.Throws<ArgumentNullException>(() => hasher.HashPassword(password));
        }

        [Fact]
        public void PasswordHasher_Succeeds()

        {
            // Arrange
            const string password = "123456";
            var hasher = new PasswordHasher();

            // Act
            var firstHash = hasher.HashPassword(password);
            var secondHash = hasher.HashPassword(password);

            // Assert
            Assert.NotSame(firstHash, password);
            Assert.NotSame(secondHash, password);
            Assert.NotSame(firstHash, secondHash);
        }

        [Fact]
        public void PasswordHasher_PasswordMatches_ValidHashSucceeds()
        {

            // Arrange
            const string password = "123456";
            var hasher = new PasswordHasher();

            // Act
            var hash = hasher.HashPassword(password);

            // Assert
            Assert.True(hasher.PasswordMatches(password, hash));
        }

        [Fact]
        public void PasswordHasher_PasswordMatches_InvalidHashFails()
        {

            // Arrange
            const string password = "123456";
            var hasher = new PasswordHasher();

            // Act
            var hash = hasher.HashPassword(password);

            var charToReplace = hash.First(x => x != 'a');
            var mutilatedHash = hash.Replace(charToReplace, 'a');

            // Assert
            Assert.False(hasher.PasswordMatches(password, mutilatedHash));
        }

        [Fact]
        public void PasswordHasher_PasswordMatches_InvalidPasswordFails()
        {

            // Arrange
            const string password = "123456";
            const string invalidPassword = "thispassworddoesntmatch";
            var hasher = new PasswordHasher();

            // Act
            var hash = hasher.HashPassword(password);

            // Assert
            Assert.False(hasher.PasswordMatches(invalidPassword, hash));
        }
    }
#pragma warning restore CA1707 // Identifiers should not contain underscores
}
