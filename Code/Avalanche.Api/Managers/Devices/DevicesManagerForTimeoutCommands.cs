﻿using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Helpers;
using Ism.PgsTimeout.V1.Protos;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Devices
{
    public partial class DevicesManager : IDevicesManager
    {
        private async Task<CommandResponse> SetTimeoutMode(Command command)
        {
            await _mediaService.SetPgsTimeoutModeAsync(_mapper.Map<Command, SetPgsTimeoutModeRequest>(command));
            return new CommandResponse(command.Device);
        }

        private async Task<CommandResponse> SetTimeoutCurrentSlide(Command command)
        {
            Preconditions.ThrowIfStringIsNotNumber(nameof(command.Message), command.Message);
            await _mediaService.SetTimeoutPageAsync(_mapper.Map<Command, SetTimeoutPageRequest>(command));
            return new CommandResponse(command.Device);
        }

        private async Task<CommandResponse> GoToPreviousTimeoutSlide(Command command)
        {
            await _mediaService.PreviousPageAsync();
            return new CommandResponse(command.Device);
        }

        private async Task<CommandResponse> GoToNextTimeoutSlide(Command command)
        {
            await _mediaService.NextPageAsync();
            return new CommandResponse(command.Device);
        }

        private async Task<CommandResponse> PlayTimeoutSlides(Command command)
        {
            command.Message = ((int)TimeoutModes.Timeout).ToString();
            var setTimeOutModeCommand = new Command()
            {
                Device = command.Device,
                Message = ((int)TimeoutModes.Timeout).ToString()
            };

            await _mediaService.SetPgsTimeoutModeAsync(_mapper.Map<Command, SetPgsTimeoutModeRequest>(command));
            return new CommandResponse(command.Device);
        }
    }
}
