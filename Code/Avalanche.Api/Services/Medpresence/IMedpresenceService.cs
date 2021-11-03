using System.Threading.Tasks;
using Avalanche.Api.ViewModels;

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
        Task ArchiveSessionAsync(ArchiveServiceViewModel request);
    }
}
