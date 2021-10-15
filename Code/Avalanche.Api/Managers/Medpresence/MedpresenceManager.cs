using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Services.Medpresence;
using Avalanche.Api.ViewModels;
using Ism.SystemState.Client;
using Ism.SystemState.Models.Medpresence;

namespace Avalanche.Api.Managers.Medpresence
{
    public class MedpresenceManager : IMedpresenceManager
    {
        private readonly IMedpresenceService _medpresenceService;
        private readonly IStateClient _stateClient;
        private readonly IMapper _mapper;
        public MedpresenceManager(IMedpresenceService medpresenceService, IStateClient stateClient, IMapper mapper)
        {
            _medpresenceService = medpresenceService;
            _stateClient = stateClient;
            _mapper = mapper;
        }

        public async Task<MedpresenceStateViewModel> GetMedpresenceStateAsync()
        {
            var state = await _stateClient.GetData<MedpresenceState>().ConfigureAwait(false);
            return _mapper.Map<MedpresenceStateViewModel>(state);
        }

        public async Task StartServiceAsync() => await _medpresenceService.StartServiceAsync().ConfigureAwait(false);
        public async Task StopServiceAsync() => await _medpresenceService.StopServiceAsync().ConfigureAwait(false);
        public async Task StartRecordingAsyc() => await _medpresenceService.StartRecordingAsyc().ConfigureAwait(false);
        public async Task StopRecordingAsync() => await _medpresenceService.StopRecordingAsync().ConfigureAwait(false);
        public async Task CaptureImageAsync() => await _medpresenceService.CaptureImageAsync().ConfigureAwait(false);
        public async Task DiscardSessionAsync(ulong sessionId) => await _medpresenceService.DiscardSessionAsync(sessionId).ConfigureAwait(false);
        public async Task SaveSessionAsync(ulong sessionId, string title, string physician, string procedure, string? department) =>
            await _medpresenceService.SaveSessionAsync(sessionId, title, physician, procedure, department).ConfigureAwait(false);
    }
}
