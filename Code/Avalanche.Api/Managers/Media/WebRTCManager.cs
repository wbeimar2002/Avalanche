using AutoMapper;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Domain.Models.Media;
using Avalanche.Shared.Infrastructure.Helpers;
using Ism.Streaming.V1.Protos;
using Ism.Utility.Core;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Media
{
    public class WebRTCManager : IWebRTCManager
    {
        private readonly IWebRTCService _webRTCService;
        private readonly IMapper _mapper;
        private readonly IAccessInfoFactory _accessInfoFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WebRTCManager(IWebRTCService webRTCService,
            IAccessInfoFactory accessInfoFactory,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper)
        {
            _webRTCService = webRTCService;
            _accessInfoFactory = accessInfoFactory;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        public async Task<IList<VideoDeviceModel>> GetSourceStreams()
        {
            var result = await _webRTCService.GetSourceStreamsAsync();
            IList<VideoDeviceModel> listResult = _mapper.Map<IList<Ism.Streaming.V1.Protos.WebRtcSourceMessage>, IList<VideoDeviceModel>>(result.Sources);
            return listResult;
        }

        public async Task HandleMessageForVideo(WebRTCMessaggeModel message)
        {
            Preconditions.ThrowIfNull(nameof(message.SessionId), message.SessionId);
            Preconditions.ThrowIfNull(nameof(message.Message), message.Message);
            Preconditions.ThrowIfNull(nameof(message.Type), message.Type);

            await _webRTCService.HandleMessageAsync(_mapper.Map<WebRTCMessaggeModel, HandleMessageRequest>(message));
        }

        public async Task<List<string>> InitSessionAsync(WebRTCSessionModel session)
        {
            Preconditions.ThrowIfNull(nameof(session.SessionId), session.SessionId);
            Preconditions.ThrowIfNull(nameof(session.Message), session.Message);
            Preconditions.ThrowIfNull(nameof(session.Type), session.Type);

            //TODO: Complete preconditions

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();

            var initRequest = _mapper.Map<WebRTCMessaggeModel, InitSessionRequest>(session);
            initRequest.AccessInfo = _mapper.Map<Ism.IsmLogCommon.Core.AccessInfo, AccessInfoMessage>(accessInfo);

            SetInitRequestIpInfo(initRequest);

            var actionResponse = await _webRTCService.InitSessionAsync(initRequest);

            var messages = new List<string>();

            foreach (var item in actionResponse.Answer)
            {
                messages.Add(item.Message);
            }

            return messages;
        }

        public async Task DeInitSessionAsync(WebRTCMessaggeModel message)
        {
            Preconditions.ThrowIfNull(nameof(message.SessionId), message.SessionId);
            await _webRTCService.DeInitSessionAsync(_mapper.Map<WebRTCMessaggeModel, DeInitSessionRequest>(message));
        }

        private void SetInitRequestIpInfo(InitSessionRequest initRequest)
        {
            //Migrated Note of Zac
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
