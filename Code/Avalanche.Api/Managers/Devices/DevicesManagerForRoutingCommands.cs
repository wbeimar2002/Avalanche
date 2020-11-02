using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Helpers;
using Ism.Recorder.Core.V1.Protos;
using Newtonsoft.Json;
using System;
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
#warning TODO: determine sourcing of this info.
            // NOTE: it seems a bit awkward for the UI/API to need to know and/or generate this? Especially "libId"? 

            var now = DateTime.UtcNow;
            var libId = $"{now.Year}_{now.Month}_{now.Day}T{now.Hour}_{now.Minute}_{now.Second}";

            var message = new RecordMessage
            {
                LibId = libId,
                RepositoryId = Guid.NewGuid().ToString()
            };

            await _recorderService.StartRecording(message);
            return new CommandResponse(command.Device);
        }

        private async Task<CommandResponse> StopRecording(Command command)
        {
            await _recorderService.StopRecording();
            return new CommandResponse(command.Device);
        }
    }
}
