using Avalanche.Api.Utilities;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Helpers;
using Ism.Streaming.V1.Protos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Devices
{
    public partial class DevicesManager : IDevicesManager
    {
        private async Task<CommandResponse> InitializeVideo(Command command)
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
            command.AccessInformation = _mapper.Map<Ism.IsmLogCommon.Core.AccessInfo, AccessInfo>(accessInfo);

            var initRequest = _mapper.Map<Command, InitSessionRequest>(command);
            SetInitRequestIpInfo(initRequest);

            var actionResponse = await _mediaService.InitSessionAsync(initRequest);

            var response = new CommandResponse(command.Device)
            {
                Messages = new List<string>()
            };

            foreach (var item in actionResponse.Answer)
            {
                response.Messages.Add(item.Message);
            }

            return response;
        }

        private async Task<CommandResponse> StopSlidesAndResumeVideo(Command command)
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
            {
                var accessInfo = _accessInfoFactory.GenerateAccessInfo();
                command.AccessInformation = _mapper.Map<Ism.IsmLogCommon.Core.AccessInfo, AccessInfo>(accessInfo);

                var initRequest = _mapper.Map<Command, InitSessionRequest>(command);
                SetInitRequestIpInfo(initRequest);

                var actionResponse = await _mediaService.InitSessionAsync(initRequest);

                var response = new CommandResponse(command.Device)
                {
                    Messages = new List<string>()
                };

                foreach (var item in actionResponse.Answer)
                {
                    response.Messages.Add(item.Message);
                }

                return response;
            }

            return new CommandResponse(command.Device);
        }

        private async Task<CommandResponse> HandleMessageForVideo(Command command)
        {
            Preconditions.ThrowIfNull(nameof(command.Message), command.Message);
            await _mediaService.HandleMessageAsync(_mapper.Map<Command, HandleMessageRequest>(command));
            return new CommandResponse(command.Device);
        }

        private async Task<CommandResponse> StopVideo(Command command)
        {
            await _mediaService.DeInitSessionAsync(_mapper.Map<Command, DeInitSessionRequest>(command));
            return new CommandResponse(command.Device);
        }

        private void SetInitRequestIpInfo(InitSessionRequest initRequest)
        {
#warning FIX this: Correct solution depends on determining avalanche-web hosting model
            // TODO: this is a hack to get local webrtc working until we implement a hosting strategy for the web application. 
            //          - running via ng-serve means we will never get a correct remote IP as observed by AvalancheApi
            initRequest.AccessInfo.Ip = "127.0.0.1";
            // NOTE: "ExternalObservedIp" needs to be the IP address the browser contacts media service on. So:
            //          - if the browser is running local, it should be 127.0.0.1. 
            //          - If the browser is remote, it must be the external IP of the host.
            //      - the following is probably ok for pgs streams requested directly from the box, since the "host" header is likely to only ever be localhost or the correct IP.
            initRequest.ExternalObservedIp = HttpContextUtilities.GetHostAddress(_httpContextAccessor.HttpContext, true);
        }
    }
}
