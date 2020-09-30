﻿using AutoMapper;
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
        private async Task InitializeVideo(Command command)
        {
            Preconditions.ThrowIfNull(nameof(command.Message), command.Message);
            Preconditions.ThrowIfNull(nameof(command.AdditionalInfo), command.AdditionalInfo);

            var setModeCommand = new Command()
            {
                Device = command.Device,
                Message = ((int)TimeoutModes.Pgs).ToString()
            };

            await ExecuteCommandAsync(CommandTypes.SetTimeoutMode, setModeCommand);
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var accessInfoMessage = _mapper.Map<Ism.Storage.Core.PatientList.V1.Protos.AccessInfoMessage>(accessInfo);

            await _mediaService.InitSessionAsync(_mapper.Map<Command, InitSessionRequest>(command));
        }

        private async Task ResumeVideo(Command command)
        {
            var alwaysOnSettings = await _settingsService.GetTimeoutSettingsAsync();
            var timeoutMode = alwaysOnSettings.PgsVideoAlwaysOn ? TimeoutModes.Pgs : TimeoutModes.Idle;

            var setModeCommand = new Command()
            {
                Device = command.Device,
                Message = ((int)timeoutMode).ToString()
            };

            await ExecuteCommandAsync(CommandTypes.SetTimeoutMode, setModeCommand);

            if (alwaysOnSettings.PgsVideoAlwaysOn)
                await _mediaService.InitSessionAsync(_mapper.Map<Command, InitSessionRequest>(command));
        }

        private async Task HandleMessageForVideo(Command command)
        {
            Preconditions.ThrowIfNull(nameof(command.Message), command.Message);
            await _mediaService.HandleMessageAsync(_mapper.Map<Command, HandleMessageRequest>(command));
        }

        private async Task StopVideo(Command command)
        {
            await _mediaService.DeInitSessionAsync(_mapper.Map<Command, DeInitSessionRequest>(command));
        }
    }
}
