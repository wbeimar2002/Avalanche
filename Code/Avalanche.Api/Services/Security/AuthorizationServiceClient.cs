using Avalanche.Host.Service.Clients;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Helpers;
using Avalanche.Shared.Infrastructure.Services.Configuration;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Security
{
    public class AuthorizationServiceClient : IAuthorizationServiceClient
    {
        readonly Grpc.Core.Channel _channel;
        readonly AuthorizationServiceProto.AuthorizationServiceProtoClient _client;
        readonly ILogger _appLoggerService;

        public AuthorizationServiceClient(
            IConfigurationService configuration,
            ILogger<AuthorizationServiceClient> appLogger)
        {
            string hostIpAddress = Environment.GetEnvironmentVariable("hostIpAddress");
            var endpoint = string.IsNullOrEmpty(hostIpAddress) ?
                configuration.GetValue<string>("HostService:EndpointAddress") : hostIpAddress;

            _appLoggerService = appLogger;
            _channel = new Grpc.Core.Channel(endpoint, ChannelCredentials.Insecure);
            _client = new AuthorizationServiceProto.AuthorizationServiceProtoClient(_channel);
        }

        public async Task<bool> AuthenticateUserAsync(User user)
        {
            try
            {
                var userRequest = new ApplicationUser()
                {
                    Username = user.Username,
                    Password = user.Password
                };

                var result = await _client.AuthenticateUserAsync(userRequest);
                return result.Success;
            }
            catch (System.Exception exception)
            {
                _appLoggerService.LogError(LoggerHelper.GetLogMessage(DebugLogType.Exception), exception);
                return false;
            }
        }
    }
}
