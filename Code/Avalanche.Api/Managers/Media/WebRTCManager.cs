using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Shared.Domain.Models.Media;
using Ism.Streaming.V1.Protos;
using Microsoft.AspNetCore.Http;
using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Api.Managers.Media
{
    public class WebRtcManager : IWebRtcManager
    {
        private readonly IWebRtcService _webRTCService;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WebRtcManager(
            IWebRtcService webRTCService,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper)
        {
            _webRTCService = ThrowIfNullOrReturn(nameof(webRTCService), webRTCService);
            _httpContextAccessor = ThrowIfNullOrReturn(nameof(httpContextAccessor), httpContextAccessor);
            _mapper = ThrowIfNullOrReturn(nameof(mapper), mapper);
        }

        public async Task<InitWebRtcSessionResponse> InitSession(InitWebRtcSessionRequest request)
        {
            ThrowIfNull(nameof(request), request);
            var initRequest = _mapper.Map<InitWebRtcSessionRequest, InitSessionRequest>(request);
            initRequest.RemoteUser = _httpContextAccessor.HttpContext.User.Identity.Name ?? string.Empty;

            // Migrated Note of Zac
#warning FIX this: Correct solution depends on determining avalanche-web hosting model
            // TODO: this is a hack to get local webrtc working until we implement a hosting strategy for the web application. 
            //          - running via ng-serve means we will never get a correct remote IP as observed by AvalancheApi
            initRequest.RemoteIp = "127.0.0.1";
            // initRequest.RemoteIp = HttpContextUtilities.GetRequestIP(_httpContextAccessor.HttpContext);

            // NOTE: "ExternalObservedIp" needs to be the IP address the browser contacts media service on. So:
            //          - if the browser is running local, it should be 127.0.0.1. 
            //          - If the browser is remote, it must be the external IP of the host.
            //      - the following is probably ok for pgs streams requested directly from the box, since the "host" header is likely to only ever be localhost or the correct IP.
            initRequest.ExternalObservedIp = HttpContextUtilities.GetHostAddress(_httpContextAccessor.HttpContext, true);

            var initResponse = await _webRTCService.InitSessionAsync(initRequest).ConfigureAwait(false);

            return _mapper.Map<InitSessionResponse, InitWebRtcSessionResponse>(initResponse);
        }
        public async Task HandleMessage(HandleWebRtcMessageRequest request)
        {
            ThrowIfNull(nameof(request), request);
            await _webRTCService.HandleMessageAsync(_mapper.Map<HandleWebRtcMessageRequest, HandleMessageRequest>(request)).ConfigureAwait(false);
        }
        public async Task DeInitSession(DeInitWebRtcSessionRequest request)
        {
            ThrowIfNull(nameof(request), request);
            await _webRTCService.DeInitSessionAsync(_mapper.Map<DeInitWebRtcSessionRequest, DeInitSessionRequest>(request)).ConfigureAwait(false);
        }

        public async Task<GetWebRtcStreamsResponse> GetSourceStreams()
        {
            var response = await _webRTCService.GetSourceStreamsAsync().ConfigureAwait(false);
            return _mapper.Map<GetSourceStreamsResponse, GetWebRtcStreamsResponse>(response);
        }
    }
}
