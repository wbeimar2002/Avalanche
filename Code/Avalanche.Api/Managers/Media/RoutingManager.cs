﻿using AutoMapper;
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
        readonly IRoutingService _routingService;
        readonly IAvidisService _avidisService;
        readonly IStorageService _storageService;
        readonly IMapper _mapper;
        readonly IHttpContextAccessor _httpContextAccessor;

        readonly UserModel user;
        readonly ConfigurationContext configurationContext;

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
            configurationContext = _mapper.Map<Shared.Domain.Models.UserModel, ConfigurationContext>(user);
            configurationContext.IdnId = Guid.NewGuid().ToString();
        }

        public async Task EnterFullScreen(RoutingActionViewModel routingActionViewModel)
        {
            await _routingService.EnterFullScreen(new Ism.Routing.V1.Protos.EnterFullScreenRequest()
            {
                Source = _mapper.Map<SinkModel, Ism.Routing.V1.Protos.AliasIndexMessage>(routingActionViewModel.Sink),
                UserInterfaceId = routingActionViewModel.UserInterfaceId
            });
        }

        public async Task ExitFullScreen(RoutingActionViewModel routingActionViewModel)
        {
            await _routingService.ExitFullScreen(new Ism.Routing.V1.Protos.ExitFullScreenRequest()
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
            var surgerySettings = await _storageService.GetJsonDynamic("SurgerySettingsData", 1, configurationContext);

            if (surgerySettings.Mode == RoutingModes.Hardware)
                await _avidisService.ShowPreview(_mapper.Map<RegionModel, AvidisDeviceInterface.V1.Protos.ShowPreviewRequest>(routingPreviewViewModel.Region));

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
                Source = _mapper.Map<SinkModel, AvidisDeviceInterface.V1.Protos.AliasIndexMessage>(routingPreviewViewModel.Sink),
            });
        }

        public async Task RouteVideoSource(RoutesViewModel routesViewModel)
        {
            foreach (var destination in routesViewModel.Destinations)
            {
                foreach (var source in routesViewModel.Sources)
                {
                    await _routingService.RouteVideo(new Ism.Routing.V1.Protos.RouteVideoRequest()
                    {
                        Sink = _mapper.Map<VideoDeviceModel, Ism.Routing.V1.Protos.AliasIndexMessage>(destination),
                        Source = _mapper.Map<VideoDeviceModel, Ism.Routing.V1.Protos.AliasIndexMessage>(source),
                    });
                }
            }
        }

        public async Task UnrouteVideoSource(RoutesViewModel routesViewModel)
        {
            foreach (var destination in routesViewModel.Destinations)
            {
                foreach (var source in routesViewModel.Sources)
                {
                    await _routingService.RouteVideo(new Ism.Routing.V1.Protos.RouteVideoRequest()
                    {
                        Sink = _mapper.Map<VideoDeviceModel, Ism.Routing.V1.Protos.AliasIndexMessage>(destination),
                        Source = new Ism.Routing.V1.Protos.AliasIndexMessage()
                    });
                }
            }
        }

        public async Task<IList<VideoSourceModel>> GetRoutingSources()
        {
            // get video sources and their states
            // the state collection only contains the AliasIndex and a bool
            var sources = await _routingService.GetVideoSources();
            var states = await _routingService.GetVideoStateForAllSources();

            var listResult = _mapper.Map<IList<Ism.Routing.V1.Protos.VideoSourceMessage>, IList<VideoSourceModel>>(sources.VideoSources);

            foreach (var source in listResult)
            {
                // need to merge the HasVideo and VideoSource collections
                var state = states.SourceStates.SingleOrDefault(x => string.Equals(x.Source.Alias, source.Sink.Alias, StringComparison.OrdinalIgnoreCase)
                && x.Source.Index == source.Sink.Index);

                source.HasVideo = state?.HasVideo ?? false;
            }

            return listResult;
        }

        public async Task<VideoSourceModel> GetAlternativeSource(SinkModel sinkModel)
        {
            var source = await _routingService.GetAlternativeVideoSource(
                new Ism.Routing.V1.Protos.GetAlternativeVideoSourceRequest 
                { 
                    Source = new Ism.Routing.V1.Protos.AliasIndexMessage 
                    { 
                        Alias = sinkModel.Alias, 
                        Index = sinkModel.Index 
                    } 
                });

            var hasVideo = await _routingService.GetVideoStateForSource(
                new Ism.Routing.V1.Protos.GetVideoStateForSourceRequest 
                { 
                    Source = new Ism.Routing.V1.Protos.AliasIndexMessage
                    {
                        Alias = sinkModel.Alias,
                        Index = sinkModel.Index
                    }
                });

            var mappedSource = _mapper.Map<Ism.Routing.V1.Protos.VideoSourceMessage, VideoSourceModel>(source.Source);

            mappedSource.Sink = sinkModel;

            // you could plug in an ela that has no video connected to it
            mappedSource.HasVideo = hasVideo.HasVideo;

            return mappedSource;
        }

        public async Task<IList<VideoSinkModel>> GetRoutingSinks()
        {
            var sinks = await _routingService.GetVideoSinks();
            var routes = await _routingService.GetCurrentRoutes();

            var listResult = _mapper.Map<IList<Ism.Routing.V1.Protos.VideoSinkMessage>, IList<VideoSinkModel>>(sinks.VideoSinks);
            foreach (var sink in listResult)
            {
                var route = routes.Routes.SingleOrDefault(x => string.Equals(x.Sink.Alias, sink.Sink.Alias, StringComparison.OrdinalIgnoreCase)
                    && x.Sink.Index == sink.Sink.Index);

                //get the current source
                sink.Source = new SinkModel() 
                {
                    Alias = route.Source.Alias, 
                    Index = route.Source.Index
                };
            }
            return listResult;
        }
    }
}