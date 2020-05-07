﻿using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Media;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.Security.Grpc.Helpers;
using Ism.Streaming.Common.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Devices
{
    public class MediaManager : IMediaManager
    {
        readonly IMediaService _mediaService;
        readonly ISettingsService _settingsService;

        public MediaManager(IMediaService mediaService, ISettingsService settingsService)
        {
            _mediaService = mediaService;
            _settingsService = settingsService;
        }

        public async Task<TimeoutSettings> GetTimeoutSettingsAsync()
        {
            return await _settingsService.GetTimeoutSettingsAsync();
        }

        public async Task<List<CommandResponse>> SendCommandAsync(CommandViewModel command)
        {
            List<CommandResponse> responses = new List<CommandResponse>();

            foreach (var item in command.Outputs)
            {
                CommandResponse response = await ExecuteCommandAsync(command.CommandType, new Command()
                {
                    StreamId = item.Id,
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
            switch (commandType)
            {
                case Shared.Domain.Enumerations.CommandTypes.PgsPlayVideo:
                    command.Message = ((int)TimeoutModes.Pgs).ToString();
                    await _mediaService.TimeoutSetModeAsync(command);
                    return await _mediaService.PgsPlayVideoAsync(command);

                case Shared.Domain.Enumerations.CommandTypes.PgsStopVideo:
                    return await _mediaService.PgsStopVideoAsync(command);

                case Shared.Domain.Enumerations.CommandTypes.PgsPlayAudio:
                    return await _mediaService.PgsPlayAudioAsync(command);

                case Shared.Domain.Enumerations.CommandTypes.PgsStopAudio:
                    return await _mediaService.PgsStopAudioAsync(command);

                case Shared.Domain.Enumerations.CommandTypes.PgsMuteAudio:
                    return await _mediaService.PgsMuteAudioAsync(command);

                case Shared.Domain.Enumerations.CommandTypes.PgsHandleMessageForVideo:
                    return await _mediaService.PgsHandleMessageForVideoAsync(command);

                case Shared.Domain.Enumerations.CommandTypes.PgsGetAudioVolumeUp:
                    return await _mediaService.PgsGetAudioVolumeUpAsync(command);

                case Shared.Domain.Enumerations.CommandTypes.PgsGetAudioVolumeDown:
                    return await _mediaService.PgsGetAudioVolumeDownAsync(command);

                case Shared.Domain.Enumerations.CommandTypes.TimeoutPlayPdfSlides:
                    command.Message = ((int)TimeoutModes.Timeout).ToString();
                    return await _mediaService.TimeoutSetModeAsync(command);

                case Shared.Domain.Enumerations.CommandTypes.TimeoutStopPdfSlides:
                    command.Message = ((int)TimeoutModes.Idle).ToString();
                    return await _mediaService.TimeoutSetModeAsync(command);

                case Shared.Domain.Enumerations.CommandTypes.TimeoutNextPdfSlide:
                    return await _mediaService.TimeoutNextSlideAsync(command);

                case Shared.Domain.Enumerations.CommandTypes.TimeoutPreviousPdfSlide:
                    return await _mediaService.TimeoutPreviousSlideAsync(command);

                case Shared.Domain.Enumerations.CommandTypes.TimeoutSetCurrentSlide:
                    return await _mediaService.TimeoutSetPageAsync(command);

                default:
                    return null;
            }
        }
    }
}
