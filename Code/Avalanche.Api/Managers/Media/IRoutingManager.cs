using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Media
{
    public interface IRoutingManager
    {
        Task EnterFullScreen(RoutingActionViewModel routingActionViewModel);
        Task ExitFullScreen(RoutingActionViewModel routingActionViewModel);
        Task HidePreview(RoutingPreviewViewModel routingPreviewViewModel);
        Task RouteVideoSource(RoutesViewModel routesViewModel);
        Task ShowPreview(RoutingPreviewViewModel routingPreviewViewModel);
        Task UnrouteVideoSource(RoutesViewModel routesViewModel);

        Task<IList<VideoSourceModel>> GetRoutingSources();
        Task<VideoSourceModel> GetAlternativeSource(SinkModel sinkModel);
        Task<IList<VideoSinkModel>> GetRoutingSinks();
    }
}
