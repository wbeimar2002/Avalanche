using AutoMapper;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Domain.Models.Media;
using AvidisDeviceInterface.V1.Protos;
using Ism.Common.Core.Configuration.Models;
using Ism.Routing.V1.Protos;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Media
{
    public class RoutingManager : IRoutingManager
    {
        private readonly IRoutingService _routingService;
        private readonly IAvidisService _avidisService;
        private readonly IStorageService _storageService;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly UserModel user;
        private readonly ConfigurationContext configurationContext;

        public RoutingManager(IRoutingService routingService,
            IAvidisService avidisService,
            IStorageService storageService,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _routingService = routingService;
            _avidisService = avidisService;
            _storageService = storageService;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;

            user = HttpContextUtilities.GetUser(_httpContextAccessor.HttpContext);
            configurationContext = _mapper.Map<UserModel, ConfigurationContext>(user);
            configurationContext.IdnId = Guid.NewGuid().ToString();
        }

        public async Task EnterFullScreen(FullScreenRequestViewModel routingActionViewModel)
        {
            await _routingService.EnterFullScreen(new EnterFullScreenRequest()
            {
                Source = _mapper.Map<AliasIndexViewModel, Ism.Routing.V1.Protos.AliasIndexMessage>(routingActionViewModel.Source),
                UserInterfaceId = routingActionViewModel.UserInterfaceId
            });
        }

        public async Task ExitFullScreen(FullScreenRequestViewModel routingActionViewModel)
        {
            await _routingService.ExitFullScreen(new ExitFullScreenRequest()
            {
                UserInterfaceId = Convert.ToInt32(routingActionViewModel.UserInterfaceId)
            });
        }

        public async Task HidePreview(RoutingPreviewViewModel routingPreviewViewModel)
        {
            await _avidisService.HidePreview(new HidePreviewRequest()
            {
                PreviewIndex = routingPreviewViewModel.Index
            });
        }

        public async Task ShowPreview(RoutingPreviewViewModel routingPreviewViewModel)
        {
            //TODO: This rules is not in any PBI yet.
            var surgerySettings = await _storageService.GetJsonDynamic("SurgerySettingsValues", 1, configurationContext);

            if (surgerySettings.Mode == RoutingModes.Hardware)
                await _avidisService.ShowPreview(_mapper.Map<RegionModel, ShowPreviewRequest>(routingPreviewViewModel.Region));

            //TODO: Map this
            await _avidisService.ShowPreview(new ShowPreviewRequest()
            {
                PreviewIndex = routingPreviewViewModel.Index,
                Height = routingPreviewViewModel.Region.Height,
                Width = routingPreviewViewModel.Region.Width,
                X = routingPreviewViewModel.Region.X,
                Y = routingPreviewViewModel.Region.Y
            });

            await RoutePreview(routingPreviewViewModel);
        }

        public async Task RoutePreview(RoutingPreviewViewModel routingPreviewViewModel)
        {
            await _avidisService.RoutePreview(new RoutePreviewRequest()
            {
                PreviewIndex = routingPreviewViewModel.Index,
                Source = _mapper.Map<AliasIndexViewModel, AvidisDeviceInterface.V1.Protos.AliasIndexMessage>(routingPreviewViewModel.Source),
            });
        }

        public async Task RouteVideoSource(RouteModel route)
        {
            await _routingService.RouteVideo(new RouteVideoRequest()
            {
                Sink = _mapper.Map<AliasIndexModel, Ism.Routing.V1.Protos.AliasIndexMessage>(route.Sink),
                Source = _mapper.Map<AliasIndexModel, Ism.Routing.V1.Protos.AliasIndexMessage>(route.Source),
            });
        }

        public async Task UnrouteVideoSource(AliasIndexModel sink)
        {
            await _routingService.RouteVideo(new RouteVideoRequest()
            {
                Sink = _mapper.Map<AliasIndexModel, Ism.Routing.V1.Protos.AliasIndexMessage>(sink),
                Source = new Ism.Routing.V1.Protos.AliasIndexMessage { Alias = string.Empty, Index = string.Empty }
            });
        }

        public async Task<IList<VideoSourceModel>> GetRoutingSources()
        {
            // get video sources and their states
            // the state collection only contains the AliasIndex and a bool
            var sources = await _routingService.GetVideoSources();
            var states = await _routingService.GetVideoStateForAllSources();

            var listResult = _mapper.Map<IList<VideoSourceMessage>, IList<VideoSourceModel>>(sources.VideoSources);

            foreach (var source in listResult)
            {
                // need to merge the HasVideo and VideoSource collections
                var state = states.SourceStates.SingleOrDefault(x =>
                string.Equals(x.Source.Alias, source.Sink.Alias, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Source.Index, source.Sink.Index, StringComparison.OrdinalIgnoreCase));

                source.HasVideo = state?.HasVideo ?? false;
            }

            return listResult;
        }

        public async Task<VideoSourceModel> GetAlternativeSource(AliasIndexModel sinkModel)
        {
            var source = await _routingService.GetAlternativeVideoSource(
                new GetAlternativeVideoSourceRequest
                {
                    Source = new Ism.Routing.V1.Protos.AliasIndexMessage
                    {
                        Alias = sinkModel.Alias,
                        Index = sinkModel.Index
                    }
                });

            var hasVideo = await _routingService.GetVideoStateForSource(
                new GetVideoStateForSourceRequest
                {
                    Source = new Ism.Routing.V1.Protos.AliasIndexMessage
                    {
                        Alias = sinkModel.Alias,
                        Index = sinkModel.Index
                    }
                });

            var mappedSource = _mapper.Map<VideoSourceMessage, VideoSourceModel>(source.Source);

            mappedSource.Sink = sinkModel;

            // you could plug in an ela that has no video connected to it
            mappedSource.HasVideo = hasVideo.HasVideo;

            return mappedSource;
        }

        public async Task<IList<VideoSinkModel>> GetRoutingSinks()
        {
            var sinks = await _routingService.GetVideoSinks();
            var routes = await _routingService.GetCurrentRoutes();

            var listResult = _mapper.Map<IList<VideoSinkMessage>, IList<VideoSinkModel>>(sinks.VideoSinks);
            foreach (var sink in listResult)
            {
                var route = routes.Routes.SingleOrDefault(x => string.Equals(x.Sink.Alias, sink.Sink.Alias, StringComparison.OrdinalIgnoreCase)
                    && x.Sink.Index == sink.Sink.Index);

                //get the current source
                sink.Source = new AliasIndexModel()
                {
                    Alias = route.Source.Alias,
                    Index = route.Source.Index
                };
            }
            return listResult;
        }

        public async Task SetDisplayRecordingEnabled(DisplayRecordingViewModel displayRecordingViewModel)
        {
            if (displayRecordingViewModel.Enabled)
            {
                // find the source routed to the display, and route it to the indicated record channel
                var displayRoute = await _routingService.GetRouteForSink(
                    new GetRouteForSinkRequest
                    {
                        Sink = _mapper.Map<AliasIndexModel, Ism.Routing.V1.Protos.AliasIndexMessage>(displayRecordingViewModel.Display)
                    });

                var source = displayRoute?.Route?.Source ?? new Ism.Routing.V1.Protos.AliasIndexMessage(); // none/empty => route nothing. This is ok.

                await _routingService.RouteVideo(new RouteVideoRequest()
                {
                    Sink = _mapper.Map<AliasIndexModel, Ism.Routing.V1.Protos.AliasIndexMessage>(displayRecordingViewModel.RecordChannel.VideoSink),
                    Source = source,
                });
            }
            else
            {
                // clear the route from to the record channel
                await _routingService.RouteVideo(new RouteVideoRequest()
                {
                    Sink = _mapper.Map<AliasIndexModel, Ism.Routing.V1.Protos.AliasIndexMessage>(displayRecordingViewModel.RecordChannel.VideoSink),
                    Source = new Ism.Routing.V1.Protos.AliasIndexMessage(), // empty => clear route
                });
            }
        }
    }
}
