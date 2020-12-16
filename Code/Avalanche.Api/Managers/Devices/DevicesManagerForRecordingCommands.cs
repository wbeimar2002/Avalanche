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

            var now = DateTime.UtcNow;
            var libId = $"{now.Year}_{now.Month}_{now.Day}T{now.Hour}_{now.Minute}_{now.Second}";

            var message = new RecordMessage
            {
                LibId = libId,
                RepositoryId = "cache"
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
            var now = DateTime.UtcNow;
            var libId = $"{now.Year}_{now.Month}_{now.Day}T{now.Hour}_{now.Minute}_{now.Second}";

            var message = new CaptureImageRequest()
            {
                Record = new RecordMessage
                {
                    LibId = libId, // TODO: this is wrong and needs to come from the procedure
                    RepositoryId = "cache" // TODO: this is wrong and needs to come from the procedure
                },
        };
            await _recorderService.CaptureImage(message);
            return new CommandResponse(command.Device);
        }
    }
}
