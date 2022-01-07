using System.Threading.Tasks;
using Ism.MP.V1.Protos;

namespace Avalanche.Api.Services.Medpresence
{
    public interface IMedpresenceService
    {
        Task StartServiceAsync();
        Task StopServiceAsync();
        Task StartRecordingAsyc();
        Task StopRecordingAsync();
        Task CaptureImageAsync();
        Task DiscardSessionAsync(DiscardSessionRequest request);
        Task ArchiveSessionAsync(ArchiveSessionRequest request);
        Task<GuestListReply> GetGuestList();
        Task InviteGuests(InviteRequest request);
        Task ExecuteInMeetingCommand(InMeetingCommandRequest request);
    }
}
