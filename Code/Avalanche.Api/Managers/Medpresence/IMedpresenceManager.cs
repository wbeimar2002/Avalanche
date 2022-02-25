using System.Collections.Generic;
using System.Threading.Tasks;
using Avalanche.Api.ViewModels;

namespace Avalanche.Api.Managers.Medpresence
{
    public interface IMedpresenceManager
    {
        Task<MedpresenceStateViewModel> GetMedpresenceStateAsync();
        Task StartServiceAsync();
        Task StopServiceAsync();
        Task StartRecordingAsyc();
        Task StopRecordingAsync();
        Task CaptureImageAsync();
        Task DiscardSessionAsync(ulong sessionId);
        Task ArchiveSessionAsync(ArchiveServiceViewModel request);
        Task<MedpresenceGuestListViewModel> GetGuestList();
        Task InviteGuests(MedpresenceInviteViewModel request);
        Task ExecuteInSessionCommand(MedpresenceInSessionCommandViewModel request);
        Task ProvisionMedpresence(MedpresenceProvisioningViewModel request);
    }
}
