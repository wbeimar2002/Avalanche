using AutoMapper;
using NUnit.Framework;
using Avalanche.Api.Mapping;
using System.Threading.Tasks;
using Avalanche.Api.Managers.AutoLogin;
using Avalanche.Api.Handlers.Security.Tokens;
using Moq;
using Avalanche.Shared.Infrastructure.Configuration;
using Avalanche.Api.ViewModels.Security;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Avalanche.Shared.Infrastructure.Constants;

namespace Avalanche.Api.Test.Managers
{
    [TestFixture()]
    public class AutoLoginManagerTests
    {
        private IMapper _mapper;
        private IAutoLoginManager _autoLoginManager;
        private Mock<ITokenHandler> _tokenHandler;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile(new SecurityMappingConfiguration()));

            _tokenHandler = new Mock<ITokenHandler>();
            _mapper = config.CreateMapper();
            _autoLoginManager = new AutoLoginManager(_tokenHandler.Object, new AutoLoginConfiguration { AutoLogin = true });
        }

        [Test]
        public async Task AutoLoginEnabledDoesNotThrow()
        {
            var result = await _autoLoginManager.AutoLoginEnabled().ConfigureAwait(false);

            Assert.True(result);
        }

        [Test]
        public async Task AutoLoginTokenNotNull()
        {
            var token = CreateAccessToken(new UserModel { AutoLogin = true });
            _tokenHandler.Setup(s => s.CreateAccessToken(It.IsAny<UserModel>())).Returns(token);

            var result = await _autoLoginManager.CreateAccessTokenAsync().ConfigureAwait(false);
            Assert.NotNull(result);

            var accessTokenResource = _mapper.Map<AccessToken, AccessTokenViewModel>(result.Token);
            Assert.NotNull(accessTokenResource);
            Assert.True(result.Success);
        }

        [Test]
        public async Task RefreshTokenDoesNotThrow()
        {
            var result = await _autoLoginManager.RefreshTokenAsync("test").ConfigureAwait(false);
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Test]
        public async Task RevokeTokenDoesNotThrow()
        {
            _autoLoginManager.RevokeRefreshToken("test");
        }


        private AccessToken CreateAccessToken(UserModel user)
        {
            var refreshToken = BuildRefreshToken();
            var accessToken = BuildAccessToken(user, refreshToken);
            return accessToken;
        }

        private RefreshToken BuildRefreshToken()
        {
            var refreshToken = new RefreshToken
            (
                token: Guid.NewGuid().ToString(),
                expiration: DateTime.UtcNow.Ticks
            );

            return refreshToken;
        }

        private AccessToken BuildAccessToken(UserModel user, RefreshToken refreshToken)
        {
            var accessTokenExpiration = DateTime.UtcNow;

            var securityToken = new JwtSecurityToken
            (
                issuer: "test",
                audience: "test",
                claims: GetClaims(user),
                expires: accessTokenExpiration
            );

            var handler = new JwtSecurityTokenHandler();
            var accessToken = handler.WriteToken(securityToken);

            return new AccessToken(accessToken, accessTokenExpiration.Ticks, refreshToken);
        }

        private IEnumerable<Claim> GetClaims(UserModel user)
        {
            var claims = new List<Claim>();
            if (user.AutoLogin == true)
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                claims.Add(new Claim(AvalancheClaimTypes.AutoLogin, "true", ClaimValueTypes.Boolean));
            }
            else
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.UserName));
                claims.Add(new Claim(AvalancheClaimTypes.Id, user.Id.ToString()));
                claims.Add(new Claim(AvalancheClaimTypes.FirstName, user.FirstName));
                claims.Add(new Claim(AvalancheClaimTypes.LastName, user.LastName));
                claims.Add(new Claim(AvalancheClaimTypes.IsAdmin, user.IsAdmin?.ToString() ?? false.ToString(), ClaimValueTypes.Boolean));
            }
            return claims;
        }
    }
}
