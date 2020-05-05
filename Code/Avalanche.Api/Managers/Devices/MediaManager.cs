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
            //var _files = Directory.EnumerateFiles("/config");
            var settings = await _configurationService.LoadAsync<ConfigSettings>("/config/appsettings.json");
            return settings.Timeout;
        }

        public async Task<List<CommandResponse>> SendCommandAsync(CommandViewModel command)
        {
            List<CommandResponse> responses = new List<CommandResponse>();
            
            switch (command.CommandType)
            {
                case Shared.Domain.Enumerations.CommandTypes.PlayVideo:
                    foreach (var item in command.Outputs)
                    {
                        var response = await _mediaService.PlayVideoAsync(new Command()
                        {
                            StreamId = item.Id,
                            Message = command.Message,
                            SessionId = command.SessionId,
                            Type = command.Type
                        });

                        responses.Add(response);
                    }
                    return responses;
                case Shared.Domain.Enumerations.CommandTypes.StopVideo:
                    foreach (var item in command.Outputs)
                    {
                        var response = await _mediaService.StopVideoAsync(new Command()
                        {
                            StreamId = item.Id,
                            Message = command.Message,
                            SessionId = command.SessionId,
                            Type = command.Type
                        });

                        responses.Add(response);
                    }
                    return responses;
                case Shared.Domain.Enumerations.CommandTypes.HandleMessage:
                    
                    foreach (var item in command.Outputs)
                    {
                        var handleMessageResponse = await _mediaService.HandleMessageAsync(new Command()
                        {
                            StreamId = item.Id,
                            Message = command.Message,
                            SessionId = command.SessionId,
                            Type = command.Type
                        });

                        responses.Add(handleMessageResponse);
                    }
                    return responses;
                default:
                    return new List<CommandResponse>();
            }
        }
    }
}
