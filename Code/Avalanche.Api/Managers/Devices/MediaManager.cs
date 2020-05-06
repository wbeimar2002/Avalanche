using Avalanche.Api.Services.Media;
using Avalanche.Api.ViewModels;
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
        readonly IConfigurationService _configurationService;

        public MediaManager(IMediaService mediaService, IConfigurationService _configurationService)
        {
            _mediaService = mediaService;
        }

        public async Task<TimeoutSettings> GetTimeoutSettingsAsync()
        {
            var settings = await _configurationService.LoadAsync<ConfigSettings>("/config/appsettings.json");
            return settings.Timeout;
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
                case Shared.Domain.Enumerations.CommandTypes.PlayVideo:
                    return await _mediaService.PlayVideoAsync(command);
                case Shared.Domain.Enumerations.CommandTypes.StopVideo:
                    return await _mediaService.StopVideoAsync(command);
                case Shared.Domain.Enumerations.CommandTypes.PlayAudio:
                    return await _mediaService.PlayAudioAsync(command);
                case Shared.Domain.Enumerations.CommandTypes.StopAudio:
                    return await _mediaService.StopAudioAsync(command);
                case Shared.Domain.Enumerations.CommandTypes.MuteAudio:
                    return await _mediaService.MuteAudioAsync(command);
                case Shared.Domain.Enumerations.CommandTypes.HandleMessageForVideo:
                    return await _mediaService.HandleMessageForVideoAsync(command);
                case Shared.Domain.Enumerations.CommandTypes.PlaySlides:
                    return await _mediaService.PlaySlidesAsync(command);
                case Shared.Domain.Enumerations.CommandTypes.StopSlides:
                    return await _mediaService.StopSlidesAsync(command);
                case Shared.Domain.Enumerations.CommandTypes.NextSlide:
                    return await _mediaService.NextSlideAsync(command);
                case Shared.Domain.Enumerations.CommandTypes.PreviousSlide:
                    return await _mediaService.PreviousSlideAsync(command);
                case Shared.Domain.Enumerations.CommandTypes.GetVolumeUp:
                    return await _mediaService.GetVolumeUpAsync(command);
                case Shared.Domain.Enumerations.CommandTypes.GetVolumeDown:
                    return await _mediaService.GetVolumeDownAsync(command);
                default:
                    return null;
            }
        }
    }
}
