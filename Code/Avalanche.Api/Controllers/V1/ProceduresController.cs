using AutoMapper;
using Avalanche.Api.Managers.Procedures;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Ism.SystemState.Client;
using Ism.SystemState.Models.Procedure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ProceduresController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IProceduresManager _proceduresManager;
        private readonly IWebHostEnvironment _environment;

        public ProceduresController(ILogger<ProceduresController> logger, IProceduresManager proceduresManager, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
            _proceduresManager = proceduresManager;
        }

        /// <summary>
        /// Search procedures
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost("")]
        [Produces(typeof(List<ProcedureModel>))]
        public async Task<IActionResult> Search(ProcedureSearchFilterViewModel filter)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await Task.CompletedTask;
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
        /// Get procedure
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Produces(typeof(ProcedureDetailsViewModel))]
        public async Task<IActionResult> Get()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await Task.CompletedTask;
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
        /// Load the active procedure (if exists)
        /// </summary>
        /// <param name="env"></param>
        /// <returns>Active Procedure model or null</returns>
        [HttpGet("active")]
        [Produces(typeof(ProcedureDetailsViewModel))]
        public async Task<IActionResult> GetActive()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _proceduresManager.GetActiveProcedure();
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
        /// <returns></returns>
        [HttpDelete("active/{contentType}/{contentId}")]
        public async Task<IActionResult> DeleteActiveProcedureContent(ProcedureContentType contentType, Guid contentId)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _proceduresManager.DeleteActiveProcedureMedia(contentType, contentId);
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
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpDelete("active")]
        [Produces(typeof(ProcedureDetailsViewModel))]
        public async Task<IActionResult> DiscardActiveProcedure()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _proceduresManager.DiscardActiveProcedure();
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
        /// <returns></returns>
        [HttpPut("active")]
        [Produces(typeof(ProcedureDetailsViewModel))]
        public async Task<IActionResult> FinishActiveProcedure()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _proceduresManager.FinishActiveProcedure();
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
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpDelete("active/confirmation")]
        public async Task<IActionResult> ConfirmActiveProcedure()
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _proceduresManager.ConfirmActiveProcedure();
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