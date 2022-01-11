using System;
using Avalanche.Security.Server.Core.Security.Hashing;
using Avalanche.Security.Server.Security.Hashing;
using Moq;
using Xunit;

namespace Avalanche.Security.Tests.Security.Hashing
{
    public class PasswordHasherTests
    {        
        readonly IPasswordHasher _passwordHasher;
        readonly Mock<IPasswordHasher> _passwordHasherMock = new Mock<IPasswordHasher>();
        //readonly Mock<ILogginService> _logger;

        public PasswordHasherTests() => _passwordHasher = new PasswordHasher();

        [Fact]
        public void ShouldThrowExceptionForEmptyPasswordWhenHashing()
        {
            string password = "";
            _ = Assert.Throws<ArgumentNullException>(() => _passwordHasher.HashPassword(password));
        }

        [Fact]
        public void ShouldHashPasswords()
        {
            var firstPassword = "123456";
            var secondPassword = "123456";

            var firstPasswordAsHash = _passwordHasher.HashPassword(firstPassword);
            var secondPasswordAsHash = _passwordHasher.HashPassword(secondPassword);

            Assert.NotSame(firstPasswordAsHash, firstPassword);
            Assert.NotSame(secondPasswordAsHash, secondPassword);
            Assert.NotSame(firstPasswordAsHash, secondPasswordAsHash);
        }

        [Fact]
        public void ShouldMatchPasswordForValidHash()
        {
            var firstPassword = "123456";
            var firstPasswordAsHash = _passwordHasher.HashPassword(firstPassword);

            Assert.True(_passwordHasher.PasswordMatches(firstPassword, firstPasswordAsHash));
        }

        [Fact]
        public void ShouldReturnFalseForDifferentHasherPasswords()
        {
            var firstPassword = "123456";
            var secondPassword = "654321";

            var firstPasswordAsHash = _passwordHasher.HashPassword(firstPassword);
            var secondPasswordAsHash = _passwordHasher.HashPassword(secondPassword);

            Assert.False(_passwordHasher.PasswordMatches(firstPassword, secondPasswordAsHash));
            Assert.False(_passwordHasher.PasswordMatches(secondPassword, firstPasswordAsHash));
        }
    }
}
