using System;
using System.Collections.ObjectModel;
using Avalanche.Security.Server.Core.Models;
using Avalanche.Security.Server.Core.Security.Hashing;
using Avalanche.Security.Server.Core.Security.Tokens;
using Avalanche.Security.Server.Entities;
using Avalanche.Security.Server.Security.Tokens;
using Avalanche.Shared.Infrastructure.Options;
using Moq;
using Xunit;

namespace Avalanche.Security.Tests.Security.Tokens
{
    public class TokenHandlerTests
    {
        private TokenAuthConfiguration _tokenConfig;
        private Mock<IPasswordHasher> _passwordHasher;
        private SigningOptions _signingOptions;
        private UserEntity _user;

        private readonly ITokenHandler _tokenHandler;

        public TokenHandlerTests()
        {
            SetupMocks();
            _tokenHandler = new TokenHandler(_tokenConfig, _signingOptions, _passwordHasher.Object);
        }

        private void SetupMocks()
        {
            _tokenConfig = new TokenAuthConfiguration
            (
                "Testing",
                "Testing",
                30,
                60
            );

            _passwordHasher = new Mock<IPasswordHasher>();
            _passwordHasher.Setup(ph => ph.HashPassword(It.IsAny<string>())).Returns("123");

            _signingOptions = new SigningOptions();

            _user = new UserEntity
            {
                Id = 1,
                LoginName = "test@test.com",
                Password = "123",
                FirstName = "Some",
                LastName = "User",
                UserRoles = new Collection<UserRole>
                {
                    new UserRole
                    {
                        Role = new Role
                        {
                            Id = 1,
                            Name = nameof(ERole.Common)
                        }
                    }
                }
            };
        }

        [Fact]
        public void Should_Create_Access_Token()
        {
            var accessToken = _tokenHandler.CreateAccessToken(_user);

            Assert.NotNull(accessToken);
            Assert.NotNull(accessToken.RefreshToken);
            Assert.NotEmpty(accessToken.Token);
            Assert.NotEmpty(accessToken.RefreshToken.Token);
            Assert.True(accessToken.Expiration > DateTime.UtcNow.Ticks);
            Assert.True(accessToken.RefreshToken.Expiration > DateTime.UtcNow.Ticks);
            Assert.True(accessToken.RefreshToken.Expiration > accessToken.Expiration);
        }

        [Fact]
        public void Should_Take_Existing_Refresh_Token()
        {
            var accessToken = _tokenHandler.CreateAccessToken(_user);
            var refreshToken = _tokenHandler.TakeRefreshToken(accessToken.RefreshToken.Token);

            Assert.NotNull(refreshToken);
            Assert.Equal(accessToken.RefreshToken.Token, refreshToken.Token);
            Assert.Equal(accessToken.RefreshToken.Expiration, refreshToken.Expiration);
        }

        [Fact]
        public void Should_Return_Null_For_Empty_Refresh_Token_When_Trying_To_Take()
        {
            var refreshToken = _tokenHandler.TakeRefreshToken("");
            Assert.Null(refreshToken);
        }

        [Fact]
        public void Should_Return_Null_For_Invalid_Refresh_Token_When_Trying_To_Take()
        {
            var refreshToken = _tokenHandler.TakeRefreshToken("invalid_token");
            Assert.Null(refreshToken);
        }

        [Fact]
        public void Should_Not_Take_Refresh_Token_That_Was_Already_Taken()
        {
            var accessToken = _tokenHandler.CreateAccessToken(_user);
            var refreshToken = _tokenHandler.TakeRefreshToken(accessToken.RefreshToken.Token);
            var refreshTokenSecondTime = _tokenHandler.TakeRefreshToken(accessToken.RefreshToken.Token);

            Assert.NotNull(refreshToken);
            Assert.Null(refreshTokenSecondTime);
        }

        [Fact]
        public void Should_Revoke_Refresh_Token()
        {
            var accessToken = _tokenHandler.CreateAccessToken(_user);
            _tokenHandler.RevokeRefreshToken(accessToken.RefreshToken.Token);
            var refreshToken = _tokenHandler.TakeRefreshToken(accessToken.RefreshToken.Token);

            Assert.Null(refreshToken);
        }
    }
}
