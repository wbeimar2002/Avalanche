using AutoMapper;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Devices
{
    public partial class DevicesManager : IDevicesManager
    {
        readonly IAvidisService _avidisService;
        readonly IRecorderService _recorderService;
        readonly IMediaService _mediaService;
        readonly ISettingsService _settingsService;
        readonly IRoutingService _routingService;
        readonly ILogger<MediaManager> _appLoggerService;
        readonly IMapper _mapper;
        readonly IAccessInfoFactory _accessInfoFactory;
        readonly IHttpContextAccessor _httpContextAccessor;

        public DevicesManager(IMediaService mediaService,
            ISettingsService settingsService,
            IRoutingService routingService,
            ILogger<MediaManager> appLoggerService,
            IAvidisService avidisService,
            IRecorderService recorderService,
            IAccessInfoFactory accessInfoFactory,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _recorderService = recorderService;
            _avidisService = avidisService;
            _mediaService = mediaService;
            _settingsService = settingsService;
            _routingService = routingService;
            _appLoggerService = appLoggerService;
            _accessInfoFactory = accessInfoFactory;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<CommandResponse>> SendCommand(CommandViewModel command)
        {
            Preconditions.ThrowIfCountIsLessThan(nameof(command.Devices), command.Devices, 1);

            List<CommandResponse> responses = new List<CommandResponse>();

            foreach (var item in command.Devices)
            {
                var device = _mapper.Map<Device, Source>(item);

                CommandResponse response = await ExecuteCommandAsync(command.CommandType, new Command()
                {
                    Device = device,
                    Destinations = command.Destinations,
                    Message = command.Message,
                    AdditionalInfo = command.AdditionalInfo,
                    Type = command.Type, 
                    User = command.User
                });

                responses.Add(response);
            }

            return responses;
        }

        private async Task<CommandResponse> ExecuteCommandAsync(CommandTypes commandType, Command command)
        {
            _appLoggerService.LogInformation($"{commandType.GetDescription()} command executed on {command.Device.Id} device.");

            switch (commandType)
            {
                #region PGS Commands
                case CommandTypes.PgsPlayVideo:
                    return await InitializeVideo(command);
                case CommandTypes.PgsStopVideo:
                    return await StopVideo(command);
                case CommandTypes.PgsHandleMessageForVideo:
                    return await HandleMessageForVideo(command);
                #endregion

                #region Timeout Commands
                case CommandTypes.TimeoutPlayPdfSlides:
                    //TODO: What happens with the Pgs Tab??
                    return await PlayTimeoutSlides(command);
                case CommandTypes.TimeoutStopPdfSlides:
                    //TODO: if stop we can to restart the vide from the beginning or we should continue 
                    //in the state before to start the timeout mode. How to do this?
                    return await StopSlidesAndResumeVideo(command);
                case CommandTypes.TimeoutNextPdfSlide:
                    return await GoToNextTimeoutSlide(command);
                case CommandTypes.TimeoutPreviousPdfSlide:
                    return await GoToPreviousTimeoutSlide(command);
                case CommandTypes.TimeoutSetCurrentSlide:
                    return await SetTimeoutCurrentSlide(command);
                case CommandTypes.SetTimeoutMode:
                    return await SetTimeoutMode(command);
                #endregion

                #region Operate Commands
                case CommandTypes.EnterFullScreen:
                    return await EnterFullScreen(command);
                case CommandTypes.ExitFullScreen:
                    return await ExitFullScreen(command);
                case CommandTypes.RouteVideoSource:
                    return await RouteVideoSource(command);
                case CommandTypes.UnrouteVideoSource:
                    return await UnrouteVideoSource(command);
                case CommandTypes.ShowVideoRoutingPreview:
                    return await ShowVideoRoutingPreview(command);
                case CommandTypes.HideVideoRoutingPreview:
                    return await HideVideoRoutingPreview(command);
                #endregion Operate Commands

                #region Recorder Commands
                case CommandTypes.StartRecording:
                    return await StartRecording(command);
                case CommandTypes.StopRecording:
                    return await StopRecording(command);
                #endregion

                default:
                    throw new NotImplementedException();
            }
        }

        public Task<List<Output>> GetPgsOutputs()
        {
            List<Output> outputs = new List<Output>();

            outputs.Add(new Output()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Name = "MAIN TV 1",
                Thumbnail = "https://www.olympus-oste.eu/media/innovations/images_5/2n_research_and_development/entwicklung_produktinnovationen_intro.jpg"
            });

            outputs.Add(new Output()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = false,
                Name = "MAIN TV 2",
                Thumbnail = "https://www.olympus-oste.eu/media/innovations/images_5/2n_research_and_development/entwicklung_produktinnovationen_intro.jpg"
            });

            outputs.Add(new Output()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Name = "AUX TV 1",
                Thumbnail = "https://www.olympus-oste.eu/media/innovations/images_5/2n_research_and_development/entwicklung_produktinnovationen_systemintegration_6.jpg"
            });

            outputs.Add(new Output()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = false,
                Name = "MAIN TV 2",
                Thumbnail = "https://www.olympus-oste.eu/media/innovations/images_5/2n_research_and_development/entwicklung_produktinnovationen_systemintegration_6.jpg"
            });

            return Task.FromResult(outputs);
        }
        
        public Task<List<Output>> GetTimeoutOutputs()
        {
            List<Output> outputs = new List<Output>();
            outputs.Add(new Output()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Name = "Charting System",
            });

            outputs.Add(new Output()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Name = "Nurse PC",
            });

            outputs.Add(new Output()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Name = "Phys. PC",
            });

            return Task.FromResult(outputs);
        }
        
        public async Task<IList<Source>> GetOperationsSources()
        {
            var sources = await _routingService.GetVideoSources();
            var currentRoutes = await _routingService.GetCurrentRoutes();

            IList<Source> listResult = _mapper.Map<IList<Ism.Routing.V1.Protos.VideoSourceMessage>, IList<Source>>(sources.VideoSources);

            foreach (var item in currentRoutes.Routes)
            {
                var source = listResult.Where(s => s.Id.Equals(item.Source.Alias) && s.InternalIndex.Equals(item.Source.Index)).FirstOrDefault();
                if (source != null)
                    source.Output = _mapper.Map<Ism.Routing.V1.Protos.AliasIndexMessage, Output>(item.Sink);
            }

            return listResult;
        }

        public async Task<IList<Output>> GetOperationsOutputs()
        {
            var outputs = await _routingService.GetVideoSinks();
            IList<Output> listResult = _mapper.Map<IList<Ism.Routing.V1.Protos.VideoSinkMessage>, IList<Output>>(outputs.VideoSinks);
            return listResult;
        }
    }
}
