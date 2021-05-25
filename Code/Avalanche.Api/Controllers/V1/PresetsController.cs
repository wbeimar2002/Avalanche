using AutoMapper;
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
        /// Gets presets by physician
        /// </summary>
        /// <param name="physician"></param>
        /// <returns></returns>
        [HttpGet("presets/{physician}")]
        public async Task<IActionResult> GetPresets(string physician)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _presetManager.GetPresets(physician);

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
        /// <param name="index"></param>
        /// <returns></returns>
        [HttpPut("apply")]
        public async Task<IActionResult> ApplyPreset(int index)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                // Get preset for the physician 
                // TODO Apply preset based on index provided
                RoutingPresetModel model = new RoutingPresetModel();
                model.Id = index;

                await _presetManager.ApplyPreset(model);

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
        /// <param name="index"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPut("save")]
        public async Task<IActionResult> SavePreset(int index, [FromBody] string name)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

               
                // TODO Get current routes and save
                RoutingPresetModel model = new RoutingPresetModel();
                model.Id = index;
                model.Name = name;

                string physician = string.Empty; // TODO get physician

                await _presetManager.SavePreset(physician, model);

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
