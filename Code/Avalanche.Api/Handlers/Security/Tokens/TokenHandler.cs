using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Avalanche.Api.Handlers.Security.Hashing;
using Avalanche.Api.ViewModels.Security;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Options;

namespace Avalanche.Api.Handlers.Security.Tokens
{
    public class TokenHandler : ITokenHandler
    {
        private readonly ISet<RefreshToken> _refreshTokens = new HashSet<RefreshToken>();

        private readonly TokenAuthConfiguration _tokenConfiguration;
        private readonly SigningOptions _signingConfigurations;
        private readonly IPasswordHasher _passwordHaser;

        public TokenHandler(TokenAuthConfiguration tokenConfiguration, SigningOptions signingConfigurations, IPasswordHasher passwordHaser)
        {
            _passwordHaser = passwordHaser;
            _tokenConfiguration = tokenConfiguration;
            _signingConfigurations = signingConfigurations;
        }

        public AccessToken CreateAccessToken(UserModel user)
        {
            var refreshToken = BuildRefreshToken();
            var accessToken = BuildAccessToken(user, refreshToken);
            _refreshTokens.Add(refreshToken);

            return accessToken;
        }

        public RefreshToken TakeRefreshToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var refreshToken = _refreshTokens.SingleOrDefault(t => t.Token == token);
            if (refreshToken != null)
                _refreshTokens.Remove(refreshToken);

            return refreshToken;
        }

        public void RevokeRefreshToken(string token)
        {
            TakeRefreshToken(token);
        }

        private RefreshToken BuildRefreshToken()
        {
            var refreshToken = new RefreshToken
            (
                token: _passwordHaser.HashPassword(Guid.NewGuid().ToString()),
                expiration: DateTime.UtcNow.AddSeconds(_tokenConfiguration.RefreshExpirationSeconds).Ticks
            );

            return refreshToken;
        }

        private AccessToken BuildAccessToken(UserModel user, RefreshToken refreshToken)
        {
            var accessTokenExpiration = DateTime.UtcNow.AddSeconds(_tokenConfiguration.ExpirationSeconds);

            var securityToken = new JwtSecurityToken
            (
                issuer: _tokenConfiguration.Issuer,
                audience: _tokenConfiguration.Audience,
                claims: GetClaims(user),
                expires: accessTokenExpiration,
                notBefore: DateTime.UtcNow,
                signingCredentials: _signingConfigurations.SigningCredentials
            );

            var handler = new JwtSecurityTokenHandler();
            var accessToken = handler.WriteToken(securityToken);

            return new AccessToken(accessToken, accessTokenExpiration.Ticks, refreshToken);
        }

        private IEnumerable<Claim> GetClaims(UserModel user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim("Id", user.Id.ToString()),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName),
            };

            //TODO: Don't delete this need to be added in the future
            //foreach (var userRole in user.UserRoles)
            //{
            //    claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
            //}

            return claims;
        }
    }
}
