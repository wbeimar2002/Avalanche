using System;
using System.Linq;
using System.Threading.Tasks;
using Avalanche.Api.Managers.Media;
using Avalanche.Shared.Domain.Models.Media;
using Avalanche.Shared.Infrastructure.Configuration;
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
using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    [FeatureGate(FeatureFlags.LiveStream)]
    public class LiveStreamController : ControllerBase
    {
        private readonly ILogger<LiveStreamController> _logger;
        private readonly IWebRtcManager _webRTCManager;
        private readonly IRoutingManager _routingManager;
        private readonly IWebHostEnvironment _environment;
        private readonly WebRtcApiConfiguration _webRtcConfig;

        public LiveStreamController(
            ILogger<LiveStreamController> logger,
            IWebRtcManager webRtcManager,
            IRoutingManager routingManager,
            IWebHostEnvironment environment,
            WebRtcApiConfiguration webRtcConfig)
        {
            _environment = ThrowIfNullOrReturn(nameof(environment), environment);
            _logger = ThrowIfNullOrReturn(nameof(logger), logger);
            _webRTCManager = ThrowIfNullOrReturn(nameof(webRtcManager), webRtcManager);
            _routingManager = ThrowIfNullOrReturn(nameof(routingManager), routingManager);
            _webRtcConfig = ThrowIfNullOrReturn(nameof(webRtcConfig), webRtcConfig);
        }

        #region WebRtc Core

        /// <summary>
        /// Initializes a webrtc session with the specified stream name
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("webrtc")]
        [Produces(typeof(InitWebRtcSessionResponse))]
        public async Task<IActionResult> InitSession(InitWebRtcSessionRequest request)
        {
            try
            {
                _logger.LogRequested();

                // this method is intended for "public" streams only
                var streamConfig = _webRtcConfig.ViewableStreams.SingleOrDefault(x => string.Equals(x.StreamName, request.StreamName, StringComparison.OrdinalIgnoreCase));
                if (streamConfig == null)
                {
                    throw new InvalidOperationException($"Invalid stream name: {request.StreamName}");
                }

                // TODO: pass in audio stream when needed for lsp and whatnot
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
        /// Initializes a webrtc session using the preview stream name
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("webrtcpreview")]
        [Produces(typeof(InitWebRtcSessionResponse))]
        public async Task<IActionResult> InitPreviewSession(InitWebRtcSessionRequest request)
        {
            try
            {
                _logger.LogRequested();
                request.StreamName = _webRtcConfig.PreviewStreamName;
                request.Offer.BypassMaxStreamRestrictions = true; // always want preview to work
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
        /// Handle a WebRtc message, used for stream negotiation and keep-alive
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("webrtc")]
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
        /// Stops a webrtc session
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete("webrtc")]
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

        #endregion WebRtc Core

        #region LSP and MedPresence related endpoints

        /// <summary>
        /// Get wbertc streams usable in MP, lsp, etc
        /// </summary>
        /// <returns></returns>
        [HttpGet("sources")]
        [Produces(typeof(GetLiveStreamsResponse))]
        public async Task<IActionResult> GetLiveStreams()
        {
            try
            {
                _logger.LogRequested();
                var streamNames = (await _webRTCManager.GetSourceStreams().ConfigureAwait(false)).StreamNames;
                var validStreams = _webRtcConfig.ViewableStreams.Where(x => streamNames.Contains(x.StreamName, StringComparer.OrdinalIgnoreCase));

                var sourceSelectionConfig = _webRtcConfig.SourceSelectionConfiguration;

                var result = new GetLiveStreamsResponse
                {
                    SourceSelectionStreamName = sourceSelectionConfig.Enabled ? sourceSelectionConfig.StreamName : string.Empty,
                    SourceSelectionSink = sourceSelectionConfig.Enabled ? sourceSelectionConfig.VideoSink : new AliasIndexModel(),
                    Streams = validStreams.Select(x => new LiveStreamInfo { StreamName = x.StreamName, DisplayName = x.DisplayName }).ToList()
                };
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
        /// Performs source selection routing
        /// </summary>
        /// <returns></returns>
        [HttpPut("sources/selectedsource")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> RouteSourceSelection(AliasIndexModel source)
        {
            try
            {
                _logger.LogRequested();

                var config = _webRtcConfig.SourceSelectionConfiguration;

                if (!config.Enabled)
                {
                    throw new InvalidOperationException("Source selection is disabled");
                }

                await _routingManager.RouteVideoSource(new RouteModel { Sink = config.VideoSink, Source = source }).ConfigureAwait(false);

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

        #endregion LSP and MedPresence related endpoints
    }
}
