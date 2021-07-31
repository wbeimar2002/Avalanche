using Avalanche.Api.Helpers;
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
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

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
        [HttpPost("filtered/basic")]
        [Produces(typeof(ProceduresContainerReponseViewModel))]
        public async Task<IActionResult> BasicSearch(ProcedureSearchFilterViewModel filter)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _proceduresManager.BasicSearch(filter);

                var procedures = new PagedCollectionViewModel<ProcedureViewModel>
                {
                    Items = result.Procedures
                };

                PagingHelper.AppendPagingContext(this.Url, this.Request, filter, procedures);

                return Ok(new ProceduresContainerReponseViewModel { TotalCount = result.TotalCount, PagedProcedures = procedures });
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

        [HttpPost("filtered/advanced")]
        [Produces(typeof(ProceduresContainerReponseViewModel))]
        public async Task<IActionResult> AdvancedSearch(ProcedureAdvancedSearchFilterViewModel filter)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                var result = await _proceduresManager.AdvancedSearch(filter);

                var procedures = new PagedCollectionViewModel<ProcedureViewModel>
                {
                    Items = result.Procedures
                };

                PagingHelper.AppendPagingContext(this.Url, this.Request, filter, procedures);

                return Ok(new ProceduresContainerReponseViewModel { TotalCount = result.TotalCount, PagedProcedures = procedures });
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
        [Produces(typeof(ProcedureViewModel))]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                var result = await _proceduresManager.GetProcedureDetails(id);
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

        [HttpPut("{id}")]
        [Produces(typeof(ProcedureViewModel))]
        public async Task<IActionResult> Update(string id, [FromBody] ProcedureViewModel procedureViewModel)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));
                await _proceduresManager.UpdateProcedure(procedureViewModel);
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
        public async Task<IActionResult> DeleteActiveProcedureContentItem(ProcedureContentType contentType, Guid contentId)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _proceduresManager.DeleteActiveProcedureMediaItem(contentType, contentId);
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
        /// <returns></returns>
        [HttpDelete("active/contents/{contentType}")]
        public async Task<IActionResult> DeleteActiveProcedureContentItems(ProcedureContentType contentType, [FromBody] IEnumerable<Guid> contentIds)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _proceduresManager.DeleteActiveProcedureMediaItems(contentType, contentIds);
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

        /// <summary>
        /// Apply label to image 
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("active/images/{id}")]
        public async Task<IActionResult> ApplyLabelToImage(string id, string label)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _proceduresManager.ApplyLabelToActiveProcedure(new ContentViewModel()
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
        /// <returns></returns>
        [Route("active/videos/{id}")]
        public async Task<IActionResult> ApplyLabelToVideo(string id, string label)
        {
            try
            {
                _logger.LogDebug(LoggerHelper.GetLogMessage(DebugLogType.Requested));

                await _proceduresManager.ApplyLabelToActiveProcedure(new ContentViewModel()
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
    }
}
