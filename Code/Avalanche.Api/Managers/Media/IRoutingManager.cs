using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models.Media;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Media
{
    public interface IRoutingManager
    {
        Task EnterFullScreen(FullScreenRequestViewModel routingActionViewModel);
        Task ExitFullScreen(FullScreenRequestViewModel routingActionViewModel);
        Task HidePreview(RoutingPreviewViewModel routingPreviewViewModel);
        Task ShowPreview(RoutingPreviewViewModel routingPreviewViewModel);

        Task RouteVideoSource(RouteModel routesViewModel);
        Task UnrouteVideoSource(AliasIndexModel sinkViewModel);

        Task<IList<VideoSourceModel>> GetRoutingSources();
        Task<VideoSourceModel> GetAlternativeSource(AliasIndexModel sinkModel);
        Task<IList<VideoSinkModel>> GetRoutingSinks();

        Task SetDisplayRecordingEnabled(DisplayRecordingViewModel displayRecordingViewModel);
        Task HandleSinkSourceChanged(AliasIndexModel sink, AliasIndexModel source);

        Task PublishDefaultDisplayRecordingState();
    }
}
