using Avalanche.Security.Server.Core.Interfaces;
using Avalanche.Security.Server.Core.Models;
using Avalanche.Security.Server.Core.Services;
using Avalanche.Security.Server.Core.Services.Communication;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Avalanche.Security.Server.Core
{
    public class UserManager : IUserManager
    {
        private readonly ILogger<UserManager> _logger;
        private readonly IAuthenticationService _authenticationService;

        public UserManager(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public async Task<TokenResponse> Login(LoginInfo loginInfo)
        {
            var response = await _authenticationService.CreateAccessTokenAsync(loginInfo.UserName, loginInfo.UserPassword).ConfigureAwait(false);

            return response;
        }
    }
}