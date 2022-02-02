using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalanche.Api.Managers.Procedures;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
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
    [Route("procedures/active")]
    [ApiController]
    [Authorize]
    [FeatureGate(FeatureFlags.ActiveProcedure)]
    public class ActiveProcedureController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IActiveProcedureManager _activeProcedureManager;
        private readonly IWebHostEnvironment _environment;

        public ActiveProcedureController(ILogger<ActiveProcedureController> logger, IActiveProcedureManager _activeProcedureManager, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
            this._activeProcedureManager = _activeProcedureManager;
        }

        /// <summary>
        /// Load the active procedure (if exists)
        /// </summary>
        /// <returns>Active Procedure model or null</returns>
        [HttpGet("")]
        [ProducesResponseType(typeof(ActiveProcedureViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActive()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _activeProcedureManager.GetActiveProcedure();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception));
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Delete a content item (image/video) from the active procedure
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="contentId"></param>
        [HttpDelete("{contentType}/{contentId}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteActiveProcedureContentItem(ProcedureContentType contentType, Guid contentId)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _activeProcedureManager.DeleteActiveProcedureMediaItem(contentType, contentId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception));
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Delete content items (image/video) from the active procedure
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="contentIds"></param>
        [HttpDelete("contents/{contentType}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteActiveProcedureContentItems(ProcedureContentType contentType, [FromBody] IEnumerable<Guid> contentIds)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _activeProcedureManager.DeleteActiveProcedureMediaItems(contentType, contentIds);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception));
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Discard Active Procedure
        /// </summary>
        [HttpDelete("")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> DiscardActiveProcedure()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _activeProcedureManager.DiscardActiveProcedure();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception));
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Finish Active Procedure
        /// </summary>
        [HttpPut("")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> FinishActiveProcedure()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _activeProcedureManager.FinishActiveProcedure();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception));
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Set ActiveProcedure's "RequiresUserConfirmation" flag to false.
        /// </summary>
        [HttpDelete("confirmation")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> ConfirmActiveProcedure()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _activeProcedureManager.ConfirmActiveProcedure();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception));
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Apply label to image 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="label"></param>
        [HttpPut]
        [Route("{id}/label")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> ApplyLabelToImage(string id, [FromBody] string label)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _activeProcedureManager.ApplyLabelToActiveProcedure(new ContentViewModel()
                {
                    ContentId = new Guid(id),
                    Label = label,
                    ProcedureContentType = ProcedureContentType.Image
                });

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception));
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Apply label to video 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="label"></param>
        [HttpPut]
        [Route("videos/{id}/label")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> ApplyLabelToVideo(string id, [FromBody] string label)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _activeProcedureManager.ApplyLabelToActiveProcedure(new ContentViewModel()
                {
                    ContentId = new Guid(id),
                    Label = label,
                    ProcedureContentType = ProcedureContentType.Video
                });

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception));
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        [HttpPut]
        [Route("images/latest/label")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> ApplyLabelToLatestImages([FromBody] string label)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _activeProcedureManager.ApplyLabelToLatestImages(label);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception));
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Allocate new procedure for the process
        /// </summary>
        /// <param name="registrationMode"></param>
        [HttpPost]
        [Route("procedure/{registrationMode}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> AllocateNewProcedure(RegistrationMode registrationMode, [FromBody] PatientViewModel? patient = null)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _activeProcedureManager.AllocateNewProcedure(registrationMode, patient).ConfigureAwait(false);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception));
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Update procedure with PatientViewModelData
        /// </summary>
        /// <param name="registrationMode"></param>
        [HttpPut]
        [Route("updateprocedure")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateProcedure([FromBody] PatientViewModel patientViewModel)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _activeProcedureManager.UpdateActiveProcedure(patientViewModel).ConfigureAwait(false);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception));
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }
    }
}
