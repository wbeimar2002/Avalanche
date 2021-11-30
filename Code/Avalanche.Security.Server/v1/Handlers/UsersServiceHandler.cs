using System;
using System.Threading.Tasks;
using Avalanche.Security.Server.Core.Interfaces;
using Avalanche.Security.Server.Core.Models;
using Avalanche.Security.Server.v1.Protos;
using Grpc.Core;
using Ism.Common.Core.Grpc.Extensions;
using Microsoft.Extensions.Logging;
using static Avalanche.Security.Server.v1.Protos.UserServer;

namespace Avalanche.Security.Server.v1.Handlers
{
    public class UsersServiceHandler : UserServerBase
    {
        private readonly ILogger<UsersServiceHandler> _logger;
        private readonly IUserManager _userManager;

        public UsersServiceHandler(ILogger<UsersServiceHandler> logger, IUserManager userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public override async Task<LoginUserResponse> LoginUser(LoginUserRequest request, ServerCallContext context)
        {
            try
            {
                //TODO: Import package for this validations
                //ThrowIfNull(nameof(request), request);

                var result = await _userManager.Login(new LoginInfo()
                {
                    UserName = request.UserName, UserPassword = request.UserPassword
                }).ConfigureAwait(false);

                //TODO: Improve this mapping
                return await Task.FromResult(new LoginUserResponse { IsSuccess = result.Success, AccessToken = result.Token.Token, Expiration = result.Token.Expiration, RefreshToken = result.Token.RefreshToken.Token }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex.LogAndReturnGrpcException(_logger);
            }
        }
    }
}
