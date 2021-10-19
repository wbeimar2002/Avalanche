using System.Threading.Tasks;

namespace Avalanche.Api.Services.Medpresence
{
    public interface IMedpresenceService
    {
        Task StartServiceAsync();
        Task StopServiceAsync();
        Task StartRecordingAsyc();
        Task StopRecordingAsync();
        Task CaptureImageAsync();
        Task DiscardSessionAsync(ulong sessionId);
        Task SaveSessionAsync(ulong sessionId, string title, string physician, string procedure, string? department);
    }
}
