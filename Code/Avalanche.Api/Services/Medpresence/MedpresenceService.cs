using Avalanche.Api.ViewModels;
using Ism.Common.Core.Aspects;
using Ism.Medpresence.Client.V1;
using Ism.MP.V1.Protos;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Medpresence
{
    public class MedpresenceService : IMedpresenceService
    {
        private readonly ILogger<MedpresenceService> _logger;
        private readonly MedpresenceSecureClient _medpresence;

        public MedpresenceService(ILogger<MedpresenceService> logger, MedpresenceSecureClient medpresence)
        {
            _logger = logger;
            _medpresence = medpresence;
        }

        [AspectLogger]
        public async Task StartServiceAsync()
        {
             await _medpresence.StartServiceAsync().ConfigureAwait(false);
            _logger.LogInformation("Starting a service session...");
        }

        [AspectLogger]
        public async Task StopServiceAsync()
        {
            await _medpresence.StopServiceAsync().ConfigureAwait(false);
            _logger.LogInformation("Stopping a service session...");
        }

        [AspectLogger]
        public async Task StartRecordingAsyc()
        {
            await _medpresence.StartRecordingAsync().ConfigureAwait(false);
            _logger.LogInformation("Starting a recording....");
        }

        [AspectLogger]
        public async Task StopRecordingAsync()
        {
            await _medpresence.StopRecordingAsync().ConfigureAwait(false);
            _logger.LogInformation("Stopping a recording session...");
        }

        [AspectLogger]
        public async Task CaptureImageAsync()
        {
            await _medpresence.CaptureImageAsync().ConfigureAwait(false);
            _logger.LogInformation("Capturing an image...");
        }

        [AspectLogger]
        public async Task DiscardSessionAsync(ulong sessionId)
        {
            await _medpresence.DiscardSessionAsync(new DiscardSessionRequest {
                SessionId = sessionId
            }).ConfigureAwait(false);
            _logger.LogInformation($"Discarded session with id: {sessionId}");
        }

        [AspectLogger]
        public async Task ArchiveSessionAsync(ArchiveServiceViewModel request)
        {
            await _medpresence.ArchiveSession(new ArchiveSessionRequest
            {
                SessionId = request.SessionId,
                Title = request.Title,
                Description = request.Description,
                Physician = request.Physician != null ? new PhysicianMessage
                {
                    Id = request.Physician.Id,
                    FirstName = request.Physician.FirstName,
                    LastName = request.Physician.LastName
                } : null,
                Department = request.Department != null ? new DepartmentMessage
                {
                    Id = request.Department.Id,
                    Name = request.Department.Name
                } : null,
                ProcedureType = request.ProcedureType != null ? new ProcedureTypeMessage
                {
                    Id = request.ProcedureType.Id,
                    Name = request.ProcedureType.Name
                } : null
            }).ConfigureAwait(false);
            _logger.LogInformation($"Saved session with id: {request.SessionId}");
        }
    }
}
