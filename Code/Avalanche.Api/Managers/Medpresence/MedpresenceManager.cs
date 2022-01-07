using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Mapping;
using Avalanche.Api.Services.Medpresence;
using Avalanche.Api.ViewModels;
using Ism.MP.V1.Protos;
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

            var config = new MapperConfiguration(cfg => cfg.AddProfile(new MedpresenceMappingConfiguration()));
            _mapper = config.CreateMapper();
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

        public async Task DiscardSessionAsync(ulong sessionId) => await _medpresenceService.DiscardSessionAsync(new DiscardSessionRequest {
            SessionId = sessionId
        }).ConfigureAwait(false);

        public async Task ArchiveSessionAsync(ArchiveServiceViewModel request) =>
            await _medpresenceService.ArchiveSessionAsync(_mapper.Map<ArchiveSessionRequest>(request)).ConfigureAwait(false);

        public async Task<MedpresenceGuestListViewModel> GetGuestList()
        {
            var result = await _medpresenceService.GetGuestList().ConfigureAwait(false);
            return new MedpresenceGuestListViewModel
            {
                GuestList = _mapper.Map<IEnumerable<MedpresencePractitioner>>(result.Guests).ToList()
            };
        }

        public async Task InviteGuests(MedpresenceInviteViewModel request)
        {
            var message = new InviteRequest();
            message.Invitees.AddRange(_mapper.Map<IEnumerable<MedpresenceSecureGuestMessage>>(request.Invitees));
            await _medpresenceService.InviteGuests(message).ConfigureAwait(false);
        }

        public async Task ExecuteInSessionCommand(MedpresenceInSessionCommandViewModel request) => await _medpresenceService.ExecuteInMeetingCommand(new InMeetingCommandRequest
        {
            Request = request.Command
        }).ConfigureAwait(false);
    }
}
