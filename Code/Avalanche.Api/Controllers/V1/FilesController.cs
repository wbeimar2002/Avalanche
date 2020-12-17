using Avalanche.Api.Managers.Devices;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    [ExcludeFromCodeCoverage]
    public class FilesController : ControllerBase
    {
        readonly ILogger _appLoggerService;
        readonly IMediaManager _mediaManager;

        public FilesController(IMediaManager mediaManager, ILogger<FilesController> appLoggerService)
        {
            _appLoggerService = appLoggerService;
            _mediaManager = mediaManager;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestFormLimits(MultipartBodyLengthLimit = 209715200)]
        [RequestSizeLimit(209715200)]
        public IActionResult Upload([FromForm(Name = "file")]IFormFile file, [FromServices]IWebHostEnvironment env)
        {
            try
            {
                _appLoggerService.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                _mediaManager.SaveFileAsync(file);

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

#warning TODO: This is entirely wrong and intended only for a workflow demo. Remove.
        // Need to define and implement correct image retrieval patterns. Not in scope of current work, but the following is not at all correct.
        [HttpGet("DemoGetImageFile")]
        [AllowAnonymous]
        public IActionResult DemoGetImageFile([FromQuery]string path)
        {
            try
            {
                var libraryRoot = Environment.GetEnvironmentVariable("demoLibraryFolder");
                var translated = path.Replace('\\', '/').TrimStart('/');
                var fullPath = System.IO.Path.Combine(libraryRoot, translated);
                return PhysicalFile(fullPath, "image/jpeg");
            }
            catch (Exception ex)
            {
                _appLoggerService.LogError(LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return BadRequest();
            }
        }
    }
}