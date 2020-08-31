using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Media;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.Security.Grpc.Helpers;
using Ism.Streaming.Common.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using Ism.Routing.Common.Core;
using AutoMapper;

namespace Avalanche.Api.Managers.Devices
{
    public class DevicesManager : IDevicesManager
    {
        readonly IMediaService _mediaService;
        readonly ISettingsService _settingsService;
        readonly IRoutingService _routingService;
        readonly ILogger<MediaManager> _appLoggerService;
        readonly IMapper _mapper;

        public DevicesManager(IMediaService mediaService, 
            ISettingsService settingsService, 
            IRoutingService routingService, 
            ILogger<MediaManager> appLoggerService, 
            IMapper mapper)
        {
            _mediaService = mediaService;
            _settingsService = settingsService;
            _routingService = routingService;
            _appLoggerService = appLoggerService;
            _mapper = mapper;
        }

        public async Task<List<CommandResponse>> SendCommandAsync(CommandViewModel command)
        {
            Preconditions.ThrowIfCountIsLessThan<Device>(nameof(command.Devices), command.Devices, 1);

            List<CommandResponse> responses = new List<CommandResponse>();

            foreach (var item in command.Devices)
            {
                CommandResponse response = await ExecuteCommandAsync(command.CommandType, new Command()
                {
                    Alias = item.Id,
                    Message = command.Message,
                    SessionId = command.SessionId,
                    Type = command.Type
                });

                responses.Add(response);
            }

            return responses;
        }

        private async Task<CommandResponse> ExecuteCommandAsync(Shared.Domain.Enumerations.CommandTypes commandType, Command command)
        {
            _appLoggerService.LogInformation($"{commandType.GetDescription()} command executed on {command.Alias} output.");

            switch (commandType)
            {
                #region PGS Commands
                case Shared.Domain.Enumerations.CommandTypes.TimeoutStopPdfSlides:
                    //TODO: if stop we can to restart the vide from the beginning or we should continue in the state before to start the timeout mode
                    return await PlayPgsVideo(command);

                case Shared.Domain.Enumerations.CommandTypes.PgsPlayVideo:
                    Preconditions.ThrowIfNull(nameof(command.Message), command.Message);
                    await SetMode(command.Alias, TimeoutModes.Pgs);
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
                #endregion Operate Commands

                default:
                    return null;
            }
        }

        private async Task<CommandResponse> ExitFullScreen(Command command)
        {
            await _routingService.ExitFullScreen(new ExitFullScreenRequest()
            {
                UserInterfaceId = 0 //TODO: What is this?
            });

            return new CommandResponse()
            {
                OutputId = command.Alias,
                ResponseCode = 0,
            };
        }

        private async Task<CommandResponse> EnterFullScreen(Command command)
        {
            await _routingService.EnterFullScreen(new EnterFullScreenRequest()
            {
                Source = new AliasIndexMessage()
                {
                    Alias = command.Alias,
                    Index = command.Index
                },
                UserInterfaceId = 0 //TODO: What is this?
            });

            return new CommandResponse()
            {
                OutputId = command.Alias,
                ResponseCode = 0,
            };
        }

        private async Task<CommandResponse> PlayPgsVideo(Command command)
        {
            var alwaysOnSettings = await _settingsService.GetTimeoutSettingsAsync();
            await SetMode(command.Alias, alwaysOnSettings.PgsVideoAlwaysOn ? TimeoutModes.Pgs : TimeoutModes.Idle);

            if (alwaysOnSettings.PgsVideoAlwaysOn)
                return await _mediaService.PgsPlayVideoAsync(command);
            else
                return new CommandResponse()
                {
                    OutputId = command.Alias,
                    ResponseCode = 0,
                    SessionId = command.SessionId
                };
        }

        private async Task SetMode(string outputId, TimeoutModes timeoutMode)
        {
            var setModeComment = new Command()
            {
                Alias = outputId,
                Message = ((int)timeoutMode).ToString()
            };

            await _mediaService.TimeoutSetModeAsync(setModeComment);
        }

        public async Task<IList<Source>> GetOperationsSources()
        {
            var sources = await _routingService.GetVideoSources();
            var currentRoutes = await _routingService.GetCurrentRoutes();

            IList<Source> listResult = _mapper.Map<IList<VideoSourceMessage>, IList<Source>>(sources.VideoSources);
            //listResult = _mapper.Map<IList<VideoRouteMessage>, IList<Source>>(listResult);

            return listResult;
        }

        public async Task<IList<Output>> GetOperationsOutputs()
        {
            var outputs = await _routingService.GetVideoSinks();
            IList<Output> listResult = _mapper.Map<IList<VideoSinkMessage>, IList<Output>>(outputs.VideoSinks);

            return listResult;
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
    }
}
