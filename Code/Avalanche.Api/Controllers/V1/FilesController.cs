using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public FilesController(ILogger<FilesController> appLoggerService)
        {
            _appLoggerService = appLoggerService;
        }

#warning TODO: This is entirely wrong and intended only for a workflow demo. Replace.
        // TODO: Need to define and implement correct image retrieval patterns. Not in scope of current work.  
        //      - Library ID needs to come with request, so we can determine the correct root path.
        //      - Need some sort of "local" vs "vss" status so we know if we need to proxy the request to the vss.
        // NOTE: A separate endpoint is probably best for video files as well, since those need to support range headers / chunking
        [HttpGet("DemoGetImageFile")]
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