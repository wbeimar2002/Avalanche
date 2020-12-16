using Avalanche.Shared.Domain.Models;
using Ism.Recorder.Core.V1.Protos;
using System;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Devices
{
    public partial class DevicesManager : IDevicesManager
    {
        private async Task<CommandResponse> StartRecording(Command command)
        {
            #warning TODO: determine sourcing of this info.
            //NOTE: it seems a bit awkward for the UI/API to need to know and/or generate this? Especially "libId"? 
            var message = new RecordMessage
            {
                LibId = string.Empty,
                RepositoryId = string.Empty
            };

            await _recorderService.StartRecording(message);
            return new CommandResponse(command.Device);
        }

        private async Task<CommandResponse> StopRecording(Command command)
        {
            await _recorderService.StopRecording();
            return new CommandResponse(command.Device);
        }

        private async Task<CommandResponse> CaptureImage(Command command)
        {
            var message = new CaptureImageRequest()
            {
                Record = new RecordMessage
                {
                    LibId = DateTimeOffset.Now.ToString(),
                    RepositoryId = "TestRepository"
                }
            };
            await _recorderService.CaptureImage(message);
            return new CommandResponse(command.Device);
        }
    }
}
