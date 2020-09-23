﻿using AutoMapper;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Media;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Devices
{
    public class DevicesManager : IDevicesManager
    {
        readonly IAvidisService _avidisService;
        readonly IRecorderService _recorderService;
        readonly IMediaService _mediaService;
        readonly ISettingsService _settingsService;
        readonly IRoutingService _routingService;
        readonly ILogger<MediaManager> _appLoggerService;
        readonly IMapper _mapper;

        public DevicesManager(IMediaService mediaService,
            ISettingsService settingsService,
            IRoutingService routingService,
            ILogger<MediaManager> appLoggerService,
            IAvidisService avidisService,
            IRecorderService recorderService,
            IMapper mapper)
        {
            _recorderService = recorderService;
            _avidisService = avidisService;
            _mediaService = mediaService;
            _settingsService = settingsService;
            _routingService = routingService;
            _appLoggerService = appLoggerService;
            _mapper = mapper;
        }

        public async Task<List<CommandResponse>> SendCommandAsync(CommandViewModel command)
        {
            Preconditions.ThrowIfCountIsLessThan(nameof(command.Devices), command.Devices, 1);

            List<CommandResponse> responses = new List<CommandResponse>();

            foreach (var item in command.Devices)
            {
                CommandResponse response = await ExecuteCommandAsync(command.CommandType, new Command()
                {
                    Device = _mapper.Map<Device, Source>(item),
                    Destinations = command.Destinations,
                    Message = command.Message,
                    AdditionalInfo = command.AdditionalInfo,
                    Type = command.Type
                });

                responses.Add(response);
            }

            return responses;
        }

        private async Task<CommandResponse> ExecuteCommandAsync(Shared.Domain.Enumerations.CommandTypes commandType, Command command)
        {
            _appLoggerService.LogInformation($"{commandType.GetDescription()} command executed on {command.Device.Id} device.");

            switch (commandType)
            {
                #region PGS Commands
                case Shared.Domain.Enumerations.CommandTypes.TimeoutStopPdfSlides:
                    //TODO: if stop we can to restart the vide from the beginning or we should continue in the state before to start the timeout mode
                    return await PlayPgsVideo(command);

                case Shared.Domain.Enumerations.CommandTypes.PgsPlayVideo:
                    Preconditions.ThrowIfNull(nameof(command.Message), command.Message);
                    await SetMode(command.Device, TimeoutModes.Pgs);
                    return await _mediaService.PgsPlayVideoAsync(command);

                case Shared.Domain.Enumerations.CommandTypes.PgsStopVideo:
                    return await _mediaService.PgsStopVideoAsync(command);

                case Shared.Domain.Enumerations.CommandTypes.PgsPlayAudio:
                    return await _mediaService.PgsPlayAudioAsync(command);

                case Shared.Domain.Enumerations.CommandTypes.PgsStopAudio:
                    return await _mediaService.PgsStopAudioAsync(command);

                case Shared.Domain.Enumerations.CommandTypes.PgsHandleMessageForVideo:
                    Preconditions.ThrowIfNull(nameof(command.Message), command.Message);
                    return await _mediaService.PgsHandleMessageForVideoAsync(command);

                //TODO: Still not used
                case Shared.Domain.Enumerations.CommandTypes.PgsMuteAudio:
                    return await _mediaService.PgsMuteAudioAsync(command);

                case Shared.Domain.Enumerations.CommandTypes.PgsGetAudioVolumeUp:
                    return await _mediaService.PgsGetAudioVolumeUpAsync(command);

                case Shared.Domain.Enumerations.CommandTypes.PgsGetAudioVolumeDown:
                    return await _mediaService.PgsGetAudioVolumeDownAsync(command);
                #endregion

                #region Timeout Commands
                case Shared.Domain.Enumerations.CommandTypes.TimeoutPlayPdfSlides:
                    //TODO: What happens with the Pgs Tab??
                    command.Message = ((int)TimeoutModes.Timeout).ToString();
                    return await _mediaService.TimeoutSetModeAsync(command);

                case Shared.Domain.Enumerations.CommandTypes.TimeoutNextPdfSlide:
                    return await _mediaService.TimeoutNextSlideAsync(command);

                case Shared.Domain.Enumerations.CommandTypes.TimeoutPreviousPdfSlide:
                    return await _mediaService.TimeoutPreviousSlideAsync(command);

                case Shared.Domain.Enumerations.CommandTypes.TimeoutSetCurrentSlide:
                    Preconditions.ThrowIfStringIsNotNumber(nameof(command.Message), command.Message);
                    return await _mediaService.TimeoutSetCurrentSlideAsync(command);
                #endregion

                #region Operate Commands
                case Shared.Domain.Enumerations.CommandTypes.EnterFullScreen:
                    return await EnterFullScreen(command);

                case Shared.Domain.Enumerations.CommandTypes.ExitFullScreen:
                    return await ExitFullScreen(command);

                case Shared.Domain.Enumerations.CommandTypes.RouteVideoSource:
                    return await RouteVideoSoure(command);

                case Shared.Domain.Enumerations.CommandTypes.UnrouteVideoSource:
                    return await UnrouteVideoSoure(command);

                case Shared.Domain.Enumerations.CommandTypes.ShowVideoRoutingPreview:
                    return await ShowVideoRoutingPreview(command);

                #endregion Operate Commands

                #region Recorder Commands

                case Shared.Domain.Enumerations.CommandTypes.StartRecording:
                    return await StartRecording(command);

                case Shared.Domain.Enumerations.CommandTypes.StopRecording:
                    return await StopRecording(command);
                #endregion

                default:
                    return null;
            }
        }

        #region Routing

        public async Task<IList<Source>> GetOperationsSources()
        {
            var sources = await _routingService.GetVideoSources();
            var currentRoutes = await _routingService.GetCurrentRoutes();

            IList<Source> listResult = _mapper.Map<IList<Ism.Routing.Common.Core.VideoSourceMessage>, IList<Source>>(sources.VideoSources);

            foreach (var item in currentRoutes.Routes)
            {
                var source = listResult.Where(s => s.Id.Equals(item.Source.Alias) && s.InternalIndex.Equals(item.Source.Index)).FirstOrDefault();
                if (source != null)
                    source.Output = _mapper.Map<Ism.Routing.Common.Core.AliasIndexMessage, Output>(item.Sink);
            }

            return listResult;
        }

        public async Task<IList<Output>> GetOperationsOutputs()
        {
            var outputs = await _routingService.GetVideoSinks();
            IList<Output> listResult = _mapper.Map<IList<Ism.Routing.Common.Core.VideoSinkMessage>, IList<Output>>(outputs.VideoSinks);

            return listResult;
        }

        private async Task<CommandResponse> RouteVideoSoure(Command command)
        {
            Preconditions.ThrowIfCountIsLessThan(nameof(command.Destinations), command.Destinations, 1);
            foreach (var item in command.Destinations)
            {
                await _routingService.RouteVideo(new Ism.Routing.Common.Core.RouteVideoRequest()
                {
                    Sink = _mapper.Map<Device, Ism.Routing.Common.Core.AliasIndexMessage>(item),
                    Source = _mapper.Map<Device, Ism.Routing.Common.Core.AliasIndexMessage>(command.Device),
                }); 
            }

            return GetSuccessfulCommandReponse(command);
        }

        private async Task<CommandResponse> UnrouteVideoSoure(Command command)
        {
            await _routingService.RouteVideo(new Ism.Routing.Common.Core.RouteVideoRequest()
            {
                Sink = _mapper.Map<Device, Ism.Routing.Common.Core.AliasIndexMessage>(command.Device),
                Source = new Ism.Routing.Common.Core.AliasIndexMessage(),
            });

            return GetSuccessfulCommandReponse(command);
        }

        private CommandResponse GetSuccessfulCommandReponse(Command command)
        {
            return new CommandResponse()
            {
                Device = command.Device,
                ResponseCode = 0,
            };
        }

        private async Task<CommandResponse> ExitFullScreen(Command command)
        {
            await _routingService.ExitFullScreen(new Ism.Routing.Common.Core.ExitFullScreenRequest()
            {
                UserInterfaceId = Convert.ToInt32(command.AdditionalInfo)
            });

            return GetSuccessfulCommandReponse(command);
        }

        private async Task<CommandResponse> EnterFullScreen(Command command)
        {
            await _routingService.EnterFullScreen(new Ism.Routing.Common.Core.EnterFullScreenRequest()
            {
                Source = _mapper.Map<Device, Ism.Routing.Common.Core.AliasIndexMessage>(command.Device),
                UserInterfaceId = Convert.ToInt32(command.AdditionalInfo)
            });

            return GetSuccessfulCommandReponse(command);
        }

        private async Task<CommandResponse> ShowVideoRoutingPreview(Command command)
        {
            var config = await _settingsService.GetVideoRoutingSettingsAsync();
            var region = JsonConvert.DeserializeObject<Region>(command.AdditionalInfo);

            if (config.Mode == VideoRoutingModes.Hardware)
            {
                await _avidisService.SetPreviewRegion(new AvidisDeviceInterface.Proto.SetPreviewRegionRequest()
                {
                    PreviewIndex = 0, //TODO: Temporary value
                    Height = region.Height,
                    Width = region.Width,
                    X = region.X,
                    Y = region.Y,
                });

                await RoutePreview(command);

                await _avidisService.SetPreviewVisible(new AvidisDeviceInterface.Proto.SetPreviewVisibleRequest()
                {
                    PreviewIndex = 0, //TODO: Temporary values
                    Visible = command.Device.IsActive
                });
            }

            if (config.Mode == VideoRoutingModes.Software)
                await RoutePreview(command);

            return GetSuccessfulCommandReponse(command);
        }

        private async Task RoutePreview(Command command)
        {
            await _avidisService.RoutePreview(new AvidisDeviceInterface.Proto.RoutePreviewRequest()
            {
                PreviewIndex = 0, //TODO: Temporary value
                Source = _mapper.Map<Device, AvidisDeviceInterface.Proto.AliasIndexMessage>(command.Device),
            });
        }
        #endregion Routing

        #region PGS - Timeout
        private async Task<CommandResponse> PlayPgsVideo(Command command)
        {
            var alwaysOnSettings = await _settingsService.GetTimeoutSettingsAsync();
            await SetMode(command.Device, alwaysOnSettings.PgsVideoAlwaysOn ? TimeoutModes.Pgs : TimeoutModes.Idle);

            if (alwaysOnSettings.PgsVideoAlwaysOn)
                return await _mediaService.PgsPlayVideoAsync(command);
            else
                return GetSuccessfulCommandReponse(command);
        }

        private async Task SetMode(Device source, TimeoutModes timeoutMode)
        {
            var setModeCommand = new Command()
            {
                Device = source,
                Message = ((int)timeoutMode).ToString()
            };

            await _mediaService.TimeoutSetModeAsync(setModeCommand);
        }

        public Task<List<Output>> GetPGSOutputs()
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

        #endregion PGS - Timeout

        #region Recording

        private async Task<CommandResponse> StartRecording(Command command)
        {
            await _recorderService.StartRecording();
            return GetSuccessfulCommandReponse(command);
        }

        private async Task<CommandResponse> StopRecording(Command command)
        {
            await _recorderService.StopRecording();
            return GetSuccessfulCommandReponse(command);
        }

        #endregion
    }
}
