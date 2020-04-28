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

        public async Task<List<CommandResponseViewModel>> SendCommand(CommandViewModel command)
        {
            List<CommandResponseViewModel> responses = new List<CommandResponseViewModel>();
            switch (command.CommandType)
            {
                case Shared.Domain.Enumerations.CommandTypes.Play:
                    foreach (var item in command.Outputs)
                    {
                        var response = await _mediaService.Play(command.SessionId, item.Id, command.Message, command.Type);
                        responses.Add(response);
                    }
                    return responses;
                case Shared.Domain.Enumerations.CommandTypes.HandleMessage:
                    
                    foreach (var item in command.Outputs)
                    {
                        var handleMessageResponse = await _mediaService.HandleMesssage(command.SessionId, item.Id, command.Message, command.Type);
                        responses.Add(handleMessageResponse);
                    }
                    return responses;
                default:
                    return new List<CommandResponseViewModel>();
            }
        }
    }
}
