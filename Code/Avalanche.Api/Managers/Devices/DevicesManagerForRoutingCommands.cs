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
        private async Task<CommandResponse> RouteVideoSource(Command command)
        {
            Preconditions.ThrowIfCountIsLessThan(nameof(command.Destinations), command.Destinations, 1);
            foreach (var item in command.Destinations)
            {
                await _routingService.RouteVideo(new Ism.Routing.V1.Protos.RouteVideoRequest()
                {
                    Sink = _mapper.Map<Device, Ism.Routing.V1.Protos.AliasIndexMessage>(item),
                    Source = _mapper.Map<Device, Ism.Routing.V1.Protos.AliasIndexMessage>(command.Device),
                });
            }

            return new CommandResponse(command.Device);
        }

        private async Task<CommandResponse> UnrouteVideoSource(Command command)
        {
            await _routingService.RouteVideo(new Ism.Routing.V1.Protos.RouteVideoRequest()
            {
                Sink = _mapper.Map<Device, Ism.Routing.V1.Protos.AliasIndexMessage>(command.Device),
                Source = new Ism.Routing.V1.Protos.AliasIndexMessage(),
            });

            return new CommandResponse(command.Device);
        }

        private async Task<CommandResponse> ExitFullScreen(Command command)
        {
            await _routingService.ExitFullScreen(new Ism.Routing.V1.Protos.ExitFullScreenRequest()
            {
                UserInterfaceId = Convert.ToInt32(command.AdditionalInfo)
            });

            return new CommandResponse(command.Device);
        }

        private async Task<CommandResponse> EnterFullScreen(Command command)
        {
            await _routingService.EnterFullScreen(new Ism.Routing.V1.Protos.EnterFullScreenRequest()
            {
                Source = _mapper.Map<Device, Ism.Routing.V1.Protos.AliasIndexMessage>(command.Device),
                UserInterfaceId = Convert.ToInt32(command.AdditionalInfo)
            });

            return new CommandResponse(command.Device);
        }

        private async Task<CommandResponse> ShowVideoRoutingPreview(Command command)
        {
            var config = await _settingsService.GetVideoRoutingSettingsAsync();
            var region = JsonConvert.DeserializeObject<Region>(command.AdditionalInfo);

            if (config.Mode == VideoRoutingModes.Hardware)
                await _avidisService.ShowPreview(_mapper.Map<Region, AvidisDeviceInterface.V1.Protos.ShowPreviewRequest>(region));

            await RoutePreview(command);

            return new CommandResponse(command.Device);
        }

        private async Task<CommandResponse> HideVideoRoutingPreview(Command command)
        {
            await _avidisService.HidePreview(_mapper.Map<Command, AvidisDeviceInterface.V1.Protos.HidePreviewRequest>(command));
            return new CommandResponse(command.Device);
        }

        private async Task<CommandResponse> RoutePreview(Command command)
        {
            await _avidisService.RoutePreview(_mapper.Map<Command, AvidisDeviceInterface.V1.Protos.RoutePreviewRequest>(command));
            return new CommandResponse(command.Device);
        }

        private async Task<CommandResponse> StartRecording(Command command)
        {
            await _recorderService.StartRecording(_mapper.Map<Command, Ism.Recorder.Core.V1.Protos.RecordMessage>(command));
            return new CommandResponse(command.Device);
        }

        private async Task<CommandResponse> StopRecording(Command command)
        {
            await _recorderService.StopRecording();
            return new CommandResponse(command.Device);
        }
    }
}
