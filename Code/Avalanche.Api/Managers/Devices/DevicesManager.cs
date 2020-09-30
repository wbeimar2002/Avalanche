using AutoMapper;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Ism.PgsTimeout.Common.Core;
using Ism.Streaming.V1.Protos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

        public DevicesManager(IMediaService mediaService,
            ISettingsService settingsService,
            IRoutingService routingService,
            ILogger<MediaManager> appLoggerService,
            IAvidisService avidisService,
            IRecorderService recorderService,
            IAccessInfoFactory accessInfoFactory,
            IMapper mapper)
        {
            _recorderService = recorderService;
            _avidisService = avidisService;
            _mediaService = mediaService;
            _settingsService = settingsService;
            _routingService = routingService;
            _appLoggerService = appLoggerService;
            _accessInfoFactory = accessInfoFactory;
            _mapper = mapper;
        }

        public async Task SendCommandAsync(CommandViewModel command)
        {
            Preconditions.ThrowIfCountIsLessThan(nameof(command.Devices), command.Devices, 1);
            foreach (var item in command.Devices)
            {
                var accessInfo = _accessInfoFactory.GenerateAccessInfo();

                await ExecuteCommandAsync(command.CommandType, new Command()
                {
                    Device = _mapper.Map<Device, Source>(item),
                    Destinations = command.Destinations,
                    Message = command.Message,
                    AdditionalInfo = command.AdditionalInfo,
                    Type = command.Type,
                    AccessInformation = new AccessInfo
                    {
                        ApplicationName = accessInfo.ApplicationName,
                        Details = command.CommandType.GetDescription(),
                        Id = accessInfo.Id,
                        Ip = accessInfo.Ip,
                        MachineName = accessInfo.MachineName,
                        UserName = accessInfo.UserName
                    },
                });
            }
        }

        private async Task ExecuteCommandAsync(CommandTypes commandType, Command command)
        {
            _appLoggerService.LogInformation($"{commandType.GetDescription()} command executed on {command.Device.Id} device.");

            switch (commandType)
            {
                #region PGS Commands
                case CommandTypes.TimeoutStopPdfSlides:
                    //TODO: if stop we can to restart the vide from the beginning or we should continue 
                    //in the state before to start the timeout mode. How to do this?
                    await ResumeVideo(command);
                    break;
                case CommandTypes.PgsPlayVideo:
                    await InitializeVideo(command);
                    break;
                case CommandTypes.PgsStopVideo:
                    await StopVideo(command);
                    break;
                case CommandTypes.PgsHandleMessageForVideo:
                    await HandleMessageForVideo(command);
                    break;
                #endregion

                #region Timeout Commands
                case CommandTypes.TimeoutPlayPdfSlides:
                    //TODO: What happens with the Pgs Tab??
                    await PlayTimeoutSlides(command);
                    break;
                case CommandTypes.TimeoutNextPdfSlide:
                    await GoToNextTimeoutSlide(command);
                    break;
                case CommandTypes.TimeoutPreviousPdfSlide:
                    await GoToPreviousTimeoutSlide(command);
                    break;

                case CommandTypes.TimeoutSetCurrentSlide:
                    await SetTimeoutCurrentSlide(command);
                    break;
                case CommandTypes.SetTimeoutMode:
                    await SetTimeoutMode(command);
                    break;
                #endregion

                #region Operate Commands
                case CommandTypes.EnterFullScreen:
                    await EnterFullScreen(command);
                    break;
                case CommandTypes.ExitFullScreen:
                    await ExitFullScreen(command);
                    break;
                case CommandTypes.RouteVideoSource:
                    await RouteVideoSource(command);
                    break;
                case CommandTypes.UnrouteVideoSource:
                    await UnrouteVideoSource(command);
                    break;
                case CommandTypes.ShowVideoRoutingPreview:
                    await ShowVideoRoutingPreview(command);
                    break;
                case CommandTypes.HideVideoRoutingPreview:
                    await HideVideoRoutingPreview(command);
                    break;
                #endregion Operate Commands

                #region Recorder Commands
                case CommandTypes.StartRecording:
                    await _recorderService.StartRecording();
                    break;
                case CommandTypes.StopRecording:
                    await _recorderService.StopRecording();
                    break;
                #endregion
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
