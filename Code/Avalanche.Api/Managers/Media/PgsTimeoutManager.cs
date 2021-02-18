using AutoMapper;
using Avalanche.Api.Services.Media;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models.Media;
using Ism.PgsTimeout.V1.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Media
{
    public class PgsTimeoutManager : IPgsTimeoutManager
    {
        readonly IPgsTimeoutService _pgsTimeoutService;
        readonly IMapper _mapper;

        public PgsTimeoutManager(IPgsTimeoutService pgsTimeoutService, IMapper mapper)
        {
            _mapper = mapper;
            _pgsTimeoutService = pgsTimeoutService;
        }

        public async Task SetPgsVideoFile(GreetingVideo video)
        {
            var request = _mapper.Map<GreetingVideo, SetPgsVideoFileRequest>(video);
            await _pgsTimeoutService.SetPgsVideoFile(request);
        }

        public async Task<GreetingVideo> GetPgsVideoFile()
        {
            var result = await _pgsTimeoutService.GetPgsVideoFile();
            return _mapper.Map<GetPgsVideoFileResponse, GreetingVideo>(result);
        }

        public async Task<List<GreetingVideo>> GetPgsVideoFileList()
        {
            var result = await _pgsTimeoutService.GetPgsVideoFileList();
            return _mapper.Map<IList<PgsVideoFileMessage>, IList<GreetingVideo>>(result.VideoFiles).ToList();
        }

        public async Task<StateViewModel> GetPgsMute()
        {
            var result = await _pgsTimeoutService.GetPgsMute();
            return _mapper.Map<GetPgsMuteResponse, StateViewModel>(result);
        }

        public async Task<StateViewModel> GetPgsPlaybackState()
        {
            var result = await _pgsTimeoutService.GetPgsPlaybackState();
            return _mapper.Map<GetPgsPlaybackStateResponse, StateViewModel>(result);
        }

        public async Task<StateViewModel> GetPgsTimeoutMode()
        {
            var result = await _pgsTimeoutService.GetPgsTimeoutMode();
            return _mapper.Map<GetPgsTimeoutModeResponse, StateViewModel>(result);
        }

        public async Task<StateViewModel> GetPgsVolume()
        {
            var result = await _pgsTimeoutService.GetPgsVolume();
            return _mapper.Map<GetPgsVolumeResponse, StateViewModel>(result);
        }

        public async Task<StateViewModel> GetTimeoutPage()
        {
            var result = await _pgsTimeoutService.GetTimeoutPage();
            return _mapper.Map<GetTimeoutPageResponse, StateViewModel>(result);
        }

        public async Task<StateViewModel> GetTimeoutPageCount()
        {
            var result = await _pgsTimeoutService.GetTimeoutPageCount();
            return _mapper.Map<GetTimeoutPageCountResponse, StateViewModel>(result);
        }

        public async Task<StateViewModel> GetTimeoutPdfPath()
        {
            var result = await _pgsTimeoutService.GetTimeoutPdfPath();
            return _mapper.Map<GetTimeoutPdfPathResponse, StateViewModel>(result);
        }

        public async Task NextPage()
        {
            await _pgsTimeoutService.NextPage();
        }

        public async Task PreviousPage()
        {
            await _pgsTimeoutService.PreviousPage();
        }        

        public async Task SetPgsMute(StateViewModel requestViewModel)
        {
            var request = _mapper.Map<StateViewModel, SetPgsMuteRequest>(requestViewModel);
            await _pgsTimeoutService.SetPgsMute(request);
        }

        public async Task SetPgsPlaybackState(StateViewModel requestViewModel)
        {
            var request = _mapper.Map<StateViewModel, SetPgsPlaybackStateRequest>(requestViewModel);
            await _pgsTimeoutService.SetPgsPlaybackState(request);
        }

        public async Task SetPgsTimeoutMode(StateViewModel requestViewModel)
        {
            var request = _mapper.Map<StateViewModel, SetPgsTimeoutModeRequest>(requestViewModel);
            await _pgsTimeoutService.SetPgsTimeoutMode(request);
        }

        public async Task SetPgsVideoPosition(StateViewModel requestViewModel)
        {
            var request = _mapper.Map<StateViewModel, SetPgsVideoPositionRequest>(requestViewModel);
            await _pgsTimeoutService.SetPgsVideoPosition(request);
        }

        public async Task SetPgsVolume(StateViewModel requestViewModel)
        {
            var request = _mapper.Map<StateViewModel, SetPgsVolumeRequest>(requestViewModel);
            await _pgsTimeoutService.SetPgsVolume(request);
        }

        public async Task SetTimeoutPage(StateViewModel requestViewModel)
        {
            var request = _mapper.Map<StateViewModel, SetTimeoutPageRequest>(requestViewModel);
            await _pgsTimeoutService.SetTimeoutPage(request);
        }
    }
}
