using Avalanche.Api.Utilities;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Avalanche.Shared.Infrastructure.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    //[Authorize]
    [AllowAnonymous]
    [ExcludeFromCodeCoverage]
    public class SecurityController : ControllerBase
    {
        private readonly ILogger _appLoggerService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private SigningConfigurations _signingConfigurations;
        private IOptions<TokenOptions> _tokenOptions;

        public SecurityController(ILogger<SecurityController> appLoggerService, IHttpContextAccessor httpContextAccessor, SigningConfigurations signingConfigurations, IOptions<TokenOptions> tokenOptions)
        {
            _appLoggerService = appLoggerService;
            _httpContextAccessor = httpContextAccessor;
            _tokenOptions = tokenOptions;
            _signingConfigurations = signingConfigurations;
        }

        [HttpPost("acquireFileCookie")]
        public async Task<IActionResult> AcquireFileCookie([FromServices]IWebHostEnvironment env, [FromBody] string jwtToken)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var tokenUser = JwtUtilities.ValidateToken(jwtToken, JwtUtilities.GetDefaultJwtValidationParameters(_tokenOptions.Value, _signingConfigurations));

                var identity = new ClaimsIdentity(tokenUser.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                return Ok();
            }
            catch (Exception exception)
            {
                _appLoggerService.LogError(LoggerHelper.GetLogMessage(DebugLogType.Exception), exception);
                return new BadRequestObjectResult(exception.Get(env.IsDevelopment()));
            }
            finally
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }

        }
    }
}
