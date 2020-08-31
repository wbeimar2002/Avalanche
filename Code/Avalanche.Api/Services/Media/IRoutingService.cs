using Ism.Routing.Common.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public interface IRoutingService
    {
        Task<GetVideoSourcesResponse> GetVideoSources();
        Task<GetVideoSinksResponse> GetVideoSinks();
        Task<GetCurrentRoutesResponse> GetCurrentRoutes();
        Task<GetVideoStateForSourceResponse> GetVideoStateForSource(GetVideoStateForSourceRequest getVideoStateForSourceRequest);
        Task EnterFullScreen(EnterFullScreenRequest enterFullScreenRequest);
        Task ExitFullScreen(ExitFullScreenRequest exitFullScreenRequest);
    }
}
