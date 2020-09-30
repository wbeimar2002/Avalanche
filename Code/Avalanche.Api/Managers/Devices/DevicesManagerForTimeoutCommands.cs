using AutoMapper;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Ism.PgsTimeout.Common.Core;
using Ism.Streaming.V1.Protos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Devices
{
    public partial class DevicesManager : IDevicesManager
    {
        private async Task SetTimeoutMode(Command command)
        {
            await _mediaService.SetPgsTimeoutModeAsync(_mapper.Map<Command, SetPgsTimeoutModeRequest>(command));
        }

        private async Task SetTimeoutCurrentSlide(Command command)
        {
            Preconditions.ThrowIfStringIsNotNumber(nameof(command.Message), command.Message);
            await _mediaService.SetTimeoutPageAsync(_mapper.Map<Command, SetTimeoutPageRequest>(command));
        }

        private async Task GoToPreviousTimeoutSlide(Command command)
        {
            await _mediaService.PreviousPageAsync(command);
        }

        private async Task GoToNextTimeoutSlide(Command command)
        {
            await _mediaService.NextPageAsync(command);
        }

        private async Task PlayTimeoutSlides(Command command)
        {
            command.Message = ((int)TimeoutModes.Timeout).ToString();
            var setTimeOutModeCommand = new Command()
            {
                Device = command.Device,
                Message = ((int)TimeoutModes.Timeout).ToString()
            };

            await _mediaService.SetPgsTimeoutModeAsync(_mapper.Map<Command, SetPgsTimeoutModeRequest>(command));
        }
    }
}
