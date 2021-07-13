using AutoMapper;
using Avalanche.Api.Managers.Presets;
using Avalanche.Shared.Domain.Models.Presets;
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
using System.Threading.Tasks;
using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class PresetsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IMapper _mapper;
        private readonly IPresetManager _presetManager;

        public PresetsController(ILogger<PresetsController> logger, IPresetManager presetManager, IMapper mapper, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = ThrowIfNullOrReturn(nameof(logger), logger);
            _mapper = ThrowIfNullOrReturn(nameof(mapper), mapper);
            _presetManager = ThrowIfNullOrReturn(nameof(presetManager), presetManager);
        }

        /// <summary>
        /// Gets presets by user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Produces(typeof(UserPresetsModel))]
        public async Task<IActionResult> GetPresets(string userId)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _presetManager.GetPresets(userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Apply preset by index
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        [HttpPut("apply")]
        public async Task<IActionResult> ApplyPreset(string userId, int index)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _presetManager.ApplyPreset(userId, index);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Save preset by index
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="index"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPut("save")]
        public async Task<IActionResult> SavePreset(string userId, int index, string name)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                               
                await _presetManager.SavePreset(userId, index, name);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }

        /// <summary>
        /// Remove preset by index
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        [HttpDelete("remove")]
        public async Task<IActionResult> RemovePreset(string userId, int index)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _presetManager.RemovePreset(userId, index);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggerHelper.GetLogMessage(DebugLogType.Exception), ex);
                return new BadRequestObjectResult(ex.Get(_environment.IsDevelopment()));
            }
            finally
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Completed));
            }
        }
    }
}
