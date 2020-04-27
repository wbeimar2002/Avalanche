﻿using Avalanche.Api.Services.Media;
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

        public async Task<List<CommandResponse>> SendCommand(CommandViewModel command)
        {
            List<CommandResponse> responses = new List<CommandResponse>();

            foreach (var item in command.Outputs)
            {
                var response = await _mediaService.Play(command.SessionId, item.Id, command.Message);
                responses.Add(response);
            }

            return responses;
        }
    }
}
