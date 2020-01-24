using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Security.Claims;
using Avalanche.Api.Services.Security;

namespace Avalanche.Api.Managers.Security
{
    public class SecurityManagerMock : ISecurityManager
    {
        #region private fields

        static readonly JsonSerializerSettings __serializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented };

        readonly JwtIssuerOptions _jwtOptions;
        readonly IAuthorizationServiceClient _authorizationServiceClient;

        #endregion


        #region ctor

        public SecurityManagerMock(IOptions<JwtIssuerOptions> jwtOptions, IAuthorizationServiceClient remoteClientService)
        {
            _jwtOptions = jwtOptions.Value;
            _authorizationServiceClient = remoteClientService;
        }

        #endregion


        #region ISecurityService implementation

        public async Task<string> Authenticate(User user)
        {
            var claimsIdentity = await GetClaimsIdentity(user);

            if (claimsIdentity == null)
            {
                return null;
            }

            var claims = claimsIdentity.Claims;

            var jwt = new JwtSecurityToken(_jwtOptions.Issuer, _jwtOptions.Audience, claims, _jwtOptions.NotBefore, _jwtOptions.NotBefore.Add(_jwtOptions.ValidFor), _jwtOptions.SigningCredentials);
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return JsonConvert.SerializeObject(new { access_token = encodedJwt, expires_in = (int)_jwtOptions.ValidFor.TotalSeconds }, __serializerSettings);
        }

        #endregion

        #region private helpers

        async Task<ClaimsIdentity> GetClaimsIdentity(User user)
        {
            var isValidUser = await _authorizationServiceClient.AuthenticateUserAsync(user);
            if (isValidUser)
            {
                return new ClaimsIdentity(new GenericIdentity(user.Username, "Token"), new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, _jwtOptions.JtiGenerator().Result),
                    new Claim(JwtRegisteredClaimNames.Iat,ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64),
                    new Claim("UserType", "AvalancheDevMode")
                });
            }
            else
                return null;
        }

        static long ToUnixEpochDate(DateTime date) =>
            (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);

        #endregion
    }
}