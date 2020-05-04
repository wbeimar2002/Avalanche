using Avalanche.Api.Services.Media;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Devices
{
    public class MediaManager : IMediaManager
    {
        readonly IMediaService _mediaService;

        public MediaManager(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        public async Task<List<CommandResponse>> SendCommandAsync(CommandViewModel command)
        {
            List<CommandResponse> responses = new List<CommandResponse>();
            
            switch (command.CommandType)
            {
                case Shared.Domain.Enumerations.CommandTypes.Play:
                    foreach (var item in command.Outputs)
                    {
                        var response = await _mediaService.PlayAsync(new Command()
                        {
                            StreamId = item.Id,
                            Message = command.Message,
                            SessionId = command.SessionId,
                            Type = command.Type
                        });

                        responses.Add(response);
                    }
                    return responses;
                case Shared.Domain.Enumerations.CommandTypes.Stop:
                    foreach (var item in command.Outputs)
                    {
                        var response = await _mediaService.StopAsync(new Command()
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
