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

namespace Avalanche.Api.Managers.Devices
{
    public class DevicesManager : IDevicesManager
    {
        readonly IMediaService _mediaService;
        readonly ISettingsService _settingsService;
        ILogger<MediaManager> _appLoggerService;

        public DevicesManager(IMediaService mediaService, ISettingsService settingsService, ILogger<MediaManager> appLoggerService)
        {
            _mediaService = mediaService;
            _settingsService = settingsService;
            _appLoggerService = appLoggerService;
        }

        public async Task<List<CommandResponse>> SendCommandAsync(CommandViewModel command)
        {
            Preconditions.ThrowIfCountIsLessThan<Device>(nameof(command.Devices), command.Devices, 1);

            List<CommandResponse> responses = new List<CommandResponse>();

            foreach (var item in command.Devices)
            {
                CommandResponse response = await ExecuteCommandAsync(command.CommandType, new Command()
                {
                    OutputId = item.Id,
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
            _appLoggerService.LogInformation($"{commandType.GetDescription()} command executed on {command.OutputId} output.");

            switch (commandType)
            {
                #region PGS Commands
                case Shared.Domain.Enumerations.CommandTypes.TimeoutStopPdfSlides:
                    //TODO: if stop we can to restart the vide from the beginning or we should continue in the state before to start the timeout mode
                    return await PlayPgsVideo(command);

                case Shared.Domain.Enumerations.CommandTypes.PgsPlayVideo:
                    Preconditions.ThrowIfNull(nameof(command.Message), command.Message);
                    await SetMode(command.OutputId, TimeoutModes.Pgs);
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
                //case Shared.Domain.Enumerations.CommandTypes.PgsMuteAudio:
                //    return await _mediaService.PgsMuteAudioAsync(command);

                //case Shared.Domain.Enumerations.CommandTypes.PgsGetAudioVolumeUp:
                //    return await _mediaService.PgsGetAudioVolumeUpAsync(command);

                //case Shared.Domain.Enumerations.CommandTypes.PgsGetAudioVolumeDown:
                //    return await _mediaService.PgsGetAudioVolumeDownAsync(command);
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

                default:
                    return null;
            }
        }

        private async Task<CommandResponse> PlayPgsVideo(Command command)
        {
            var alwaysOnSettings = await _settingsService.GetTimeoutSettingsAsync();
            await SetMode(command.OutputId, alwaysOnSettings.PgsVideoAlwaysOn ? TimeoutModes.Pgs : TimeoutModes.Idle);

            if (alwaysOnSettings.PgsVideoAlwaysOn)
                return await _mediaService.PgsPlayVideoAsync(command);
            else
                return new CommandResponse()
                {
                    OutputId = command.OutputId,
                    ResponseCode = 0,
                    SessionId = command.SessionId
                };
        }

        private async Task SetMode(string outputId, TimeoutModes timeoutMode)
        {
            var setModeComment = new Command()
            {
                OutputId = outputId,
                Message = ((int)timeoutMode).ToString()
            };

            await _mediaService.TimeoutSetModeAsync(setModeComment);
        }

        public Task<List<Device>> GetOperationsSources()
        {
            List<Device> outputs = new List<Device>();
            outputs.Add(new Device()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Name = "MAIN TV 1",
            });

            outputs.Add(new Device()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = false,
                Name = "MAIN TV 2",
            });

            outputs.Add(new Device()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Name = "AUX TV 1",
            });

            outputs.Add(new Device()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = false,
                Name = "MAIN TV 2",
            });

            return Task.FromResult(outputs);
        }

        public Task<List<Device>> GetOperationsDestinations()
        {
            List<Device> outputs = new List<Device>();
            outputs.Add(new Device()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Name = "MAIN TV 1",
                Thumbnail = "https://www.olympus-oste.eu/media/innovations/images_5/2n_research_and_development/entwicklung_produktinnovationen_intro.jpg"
            });

            outputs.Add(new Device()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = false,
                Name = "MAIN TV 2",
                Thumbnail = "https://www.olympus-oste.eu/media/innovations/images_5/2n_research_and_development/entwicklung_produktinnovationen_intro.jpg"
            });

            outputs.Add(new Device()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Name = "AUX TV 1",
                Thumbnail = "https://www.olympus-oste.eu/media/innovations/images_5/2n_research_and_development/entwicklung_produktinnovationen_systemintegration_6.jpg"
            });

            outputs.Add(new Device()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = false,
                Name = "MAIN TV 2",
                Thumbnail = "https://www.olympus-oste.eu/media/innovations/images_5/2n_research_and_development/entwicklung_produktinnovationen_systemintegration_6.jpg"
            });

            return Task.FromResult(outputs);
        }

        public Task<List<Device>> GetSetupDestinations()
        {
            List<Device> outputs = new List<Device>();
            outputs.Add(new Device()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Name = "MAIN TV 1",
                Thumbnail = "https://www.olympus-oste.eu/media/innovations/images_5/2n_research_and_development/entwicklung_produktinnovationen_intro.jpg"
            });

            outputs.Add(new Device()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = false,
                Name = "MAIN TV 2",
                Thumbnail = "https://www.olympus-oste.eu/media/innovations/images_5/2n_research_and_development/entwicklung_produktinnovationen_intro.jpg"
            });

            outputs.Add(new Device()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Name = "AUX TV 1",
                Thumbnail = "https://www.olympus-oste.eu/media/innovations/images_5/2n_research_and_development/entwicklung_produktinnovationen_systemintegration_6.jpg"
            });

            outputs.Add(new Device()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = false,
                Name = "MAIN TV 2",
                Thumbnail = "https://www.olympus-oste.eu/media/innovations/images_5/2n_research_and_development/entwicklung_produktinnovationen_systemintegration_6.jpg"
            });


            return Task.FromResult(outputs);
        }

        public Task<List<Device>> GetTimeoutDestinations()
        {
            List<Device> sources = new List<Device>();
            sources.Add(new Device()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Name = "Charting System",
            });

            sources.Add(new Device()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Name = "Nurse PC",
            });

            sources.Add(new Device()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Name = "Phys. PC",
            });

            return Task.FromResult(sources);
        }
    }
}
