using System;
using System.Threading.Tasks;
using Avalanche.Api.Managers.Media;
using Avalanche.Shared.Domain.Models.Media;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement.Mvc;

namespace Avalanche.Api.Controllers.V1
{
    // TODOs:
    // use strongly typed request-response models
    // such as don't return VideoDeviceModel for GetSourceStreams

    // where does source selection logic go?
    // where does Preview logic go?

    [Route("[controller]")]
    [ApiController]
#warning uncomment this before PR
    //[Authorize]
    [FeatureGate(FeatureFlags.WebRtc)]
    public class WebRtcController : ControllerBase
    {
        private readonly ILogger<WebRtcController> _logger;
        private readonly IWebRtcManager _webRTCManager;
        private readonly IWebHostEnvironment _environment;

        public WebRtcController(ILogger<WebRtcController> logger, IWebRtcManager webRTCManager, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
            _webRTCManager = webRTCManager;
        }

        /// <summary>
        /// Get source strems for WebRTC
        /// </summary>
        /// <returns></returns>
        [HttpGet("sources")]
        [Produces(typeof(GetWebRtcStreamsResponse))]
        public async Task<IActionResult> GetSourceStreams()
        {
            try
            {
                _logger.LogRequested();
                var result = await _webRTCManager.GetSourceStreams().ConfigureAwait(false);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogCompleted();
            }
        }

        /// <summary>
        /// Play video with WebRTC
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("")]
        [Produces(typeof(InitWebRtcSessionResponse))]
        public async Task<IActionResult> InitSession(InitWebRtcSessionRequest request)
        {
            try
            {
                _logger.LogRequested();
                var result = await _webRTCManager.InitSession(request).ConfigureAwait(false);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogCompleted();
            }
        }

        /// <summary>
        /// Handle WebRTC message
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> HandleMessage(HandleWebRtcMessageRequest request)
        {
            try
            {
                _logger.LogRequested();
                await _webRTCManager.HandleMessage(request).ConfigureAwait(false);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogCompleted();
            }
        }

        /// <summary>
        /// Stop WebRTC video reproduction
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete("")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeInitSession(DeInitWebRtcSessionRequest request)
        {
            try
            {
                _logger.LogRequested();
                await _webRTCManager.DeInitSession(request).ConfigureAwait(false);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogCompleted();
            }
        }
    }
}
