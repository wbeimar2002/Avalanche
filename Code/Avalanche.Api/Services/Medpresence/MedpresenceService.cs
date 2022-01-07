using Ism.Common.Core.Aspects;
using Ism.Medpresence.Client.V1;
using Ism.MP.V1.Protos;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Medpresence
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "Pending refactoring...this would affect unit testing")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "Pending refactoring...this would affect unit testing")]
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
        public async Task DiscardSessionAsync(DiscardSessionRequest request)
        {
            await _medpresence.DiscardSessionAsync(request).ConfigureAwait(false);
            _logger.LogInformation($"Discarded session with id: {request.SessionId}");
        }

        [AspectLogger]
        public async Task ArchiveSessionAsync(ArchiveSessionRequest request)
        {
            await _medpresence.ArchiveSession(request).ConfigureAwait(false);
            _logger.LogInformation($"Saved session with id: {request.SessionId}");
        }

        [AspectLogger]
        public async Task<GuestListReply> GetGuestList()
        {
            _logger.LogInformation("Getting guest list...");
            return await _medpresence.GetGuestList().ConfigureAwait(false);
        }

        [AspectLogger]
        public async Task InviteGuests(InviteRequest request)
        {
            _logger.LogInformation("Requesting secure invitations...");
            await _medpresence.InviteGuests(request).ConfigureAwait(false);
        }

        [AspectLogger]
        public async Task ExecuteInMeetingCommand(InMeetingCommandRequest request)
        {
            _logger.LogInformation("Requesting secure invitations...");
            await _medpresence.ExecuteInMeetingCommand(request).ConfigureAwait(false);
        }
    }
}
