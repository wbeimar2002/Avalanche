using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalanche.Host.Service.Clients;
using Avalanche.Host.Service.Enumerations;
using Avalanche.Host.Service.Helpers;
using Avalanche.Host.Service.Models;
using Avalanche.Host.Service.Services.Logging;
using Grpc.Core;

namespace Avalanche.Host.Service.Services.Security
{
    public class AuthorizationService : AuthorizationServiceProto.AuthorizationServiceProtoBase
    {
        #region private fields

        readonly IAppLoggerService _logger;
        readonly ISecurityService _securityService;

        #endregion

        #region ctor

        public AuthorizationService(IAppLoggerService logger, ISecurityService authSvc)
        {
            _logger = logger;
            _securityService = authSvc;

            _logger.Log(LogType.Information, "AuthorizationService is now running.");
        }

        #endregion
        
        #region public functions
        public override Task<AuthorizationResult> AuthenticateUser(Clients.ApplicationUser request, ServerCallContext context)
        {
            try
            {
                _logger.Log(LogType.Debug, LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var success = _securityService.AuthenticateUser(request.Username, request.Password);

                if (success)
                {
                    return Task.FromResult(new AuthorizationResult { Success = true, Username = request.Username });
                }
                else
                {
                    _logger.Log(LogType.Warning, $"User {request.Username} with the supplied password is not authenticated.", null, request.Username);
                    return Task.FromResult(new AuthorizationResult { Success = false, Username = request.Username });
                }
            }
            catch (System.Exception exception)
            {
                _logger.Log(LogType.Error, LoggerHelper.GetLogMessage(DebugLogType.Exception), exception);
                return Task.FromResult(new AuthorizationResult()
                {
                    Message = exception.Message,
                    Success = false,
                    Username = request.Username
                });
            }
            finally
            {
                _logger.Log(LogType.Debug, LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }
        #endregion
    }
}