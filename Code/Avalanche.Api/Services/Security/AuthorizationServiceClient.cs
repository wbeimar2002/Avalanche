using Avalanche.Host.Service.Clients;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Helpers;
using Avalanche.Shared.Infrastructure.Services.Configuration;
using Avalanche.Shared.Infrastructure.Services.Logger;
using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Security
{
    public class AuthorizationServiceClient : IAuthorizationServiceClient
    {
        readonly Grpc.Core.Channel _channel;
        readonly AuthorizationServiceProto.AuthorizationServiceProtoClient _client;
        readonly IAppLoggerService _appLoggerService;

        public AuthorizationServiceClient(
            IConfigurationService configuration,
            IAppLoggerService appLogger)
        {
            string HostIpAddress = Environment.GetEnvironmentVariable("HOST_IP_ADDRESS");
            var endpoint = string.IsNullOrEmpty(HostIpAddress) ?
                configuration.GetValue<string>("HostService:EndpointAddress") : HostIpAddress;

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
                _appLoggerService.Log(LogType.Error, LoggerHelper.GetLogMessage(DebugLogType.Exception), exception);
                return false;
            }
        }
    }
}
