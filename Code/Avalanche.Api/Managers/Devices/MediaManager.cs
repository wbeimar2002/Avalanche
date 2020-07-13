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

namespace Avalanche.Api.Managers.Devices
{
    public class MediaManager : IMediaManager
    {
        readonly IMediaService _mediaService;
        readonly ISettingsService _settingsService;
        ILogger<MediaManager> _appLoggerService;

        public MediaManager(IMediaService mediaService, ISettingsService settingsService, ILogger<MediaManager> appLoggerService)
        {
            _mediaService = mediaService;
            _settingsService = settingsService;
            _appLoggerService = appLoggerService;
        }

        public async Task<TimeoutSettings> GetTimeoutSettingsAsync()
        {
            return await _settingsService.GetTimeoutSettingsAsync();
        }

        public async Task SaveFileAsync(IFormFile file)
        {
            //TODO: Pending Service that upload file
            await Task.CompletedTask;
        }

        public async Task<List<CommandResponse>> SendCommandAsync(CommandViewModel command)
        {
            Preconditions.ThrowIfCountIsLessThan<Output>(nameof(command.Outputs), command.Outputs, 1);

            List<CommandResponse> responses = new List<CommandResponse>();

            foreach (var item in command.Outputs)
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

                case Shared.Domain.Enumerations.CommandTypes.PgsMuteAudio:
                    return await _mediaService.PgsMuteAudioAsync(command);

                case Shared.Domain.Enumerations.CommandTypes.PgsHandleMessageForVideo:
                    Preconditions.ThrowIfNull(nameof(command.Message), command.Message);
                    return await _mediaService.PgsHandleMessageForVideoAsync(command);

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
    }
}
