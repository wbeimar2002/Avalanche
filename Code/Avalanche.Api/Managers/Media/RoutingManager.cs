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
using Ism.SystemState.Client;
using VideoRoutingModels = Ism.SystemState.Models.VideoRouting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalanche.Shared.Infrastructure.Configuration;
using Avalanche.Shared.Infrastructure.Configuration.Lists;

namespace Avalanche.Api.Managers.Media
{
    public class RoutingManager : IRoutingManager
    {
        private readonly IRoutingService _routingService;
        private readonly IRecorderService _recorderService;
        private readonly IAvidisService _avidisService;
        private readonly IStorageService _storageService;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly UserModel user;
        private readonly ConfigurationContext configurationContext;

        private readonly IStateClient _stateClient;

        public RoutingManager(
            IRoutingService routingService,
            IRecorderService recorderService,
            IAvidisService avidisService,
            IStorageService storageService,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IStateClient stateClient)
        {
            _routingService = routingService;
            _recorderService = recorderService;
            _avidisService = avidisService;
            _storageService = storageService;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _stateClient = stateClient;

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
            var setupSettings = await _storageService.GetJsonObject<SetupConfiguration>(nameof(SetupConfiguration), 1, configurationContext);

            if (setupSettings.SurgeryMode == RoutingModes.Hardware)
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

            // any display not in the will not have the record buttons next to it
            var dbrSinks = await _storageService.GetJsonObject<SinksList>("DisplayBasedRecordingSinks", 1, ConfigurationContext.FromEnvironment());

            var listResult = _mapper.Map<IList<VideoSinkMessage>, IList<VideoSinkModel>>(sinks.VideoSinks);
            foreach (var sink in listResult)
            {
                var route = routes.Routes.SingleOrDefault(x =>
                    string.Equals(x.Sink.Alias, sink.Sink.Alias, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.Sink.Index, sink.Sink.Index, StringComparison.OrdinalIgnoreCase));

                //get the current source
                sink.Source = new AliasIndexModel()
                {
                    Alias = route.Source.Alias,
                    Index = route.Source.Index
                };

                // if this sink is in the dbr sink list, enable it for recording
                sink.RecordEnabled = dbrSinks.Items.Any(x =>
                    string.Equals(x.Alias, sink.Sink.Alias, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.Index, sink.Sink.Index, StringComparison.OrdinalIgnoreCase));
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

            await UpdateDisplayRecordingState(displayRecordingViewModel);
        }

        public async Task HandleSinkSourceChanged(AliasIndexModel sink, AliasIndexModel source)
        {
            if (!string.IsNullOrEmpty(sink?.Alias) && !string.IsNullOrEmpty(sink?.Index))
            {
                // check if we have display-based-recording status for this sink (display)
                var displayRecordState = await _stateClient.GetData<VideoRoutingModels.DisplayRecordStateData>();

                var display = displayRecordState?.DisplayState?.FirstOrDefault(d =>
                    string.Equals(d.DisplayAliasIndex?.Alias, sink.Alias, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(d.DisplayAliasIndex?.Index, sink.Index, StringComparison.OrdinalIgnoreCase));

                // if yes - route the updated source to the display's bound record sink
                if (!string.IsNullOrEmpty(display?.RecordChannelAliasIndex?.Alias) && !string.IsNullOrEmpty(display?.RecordChannelAliasIndex?.Index))
                {
                    var sinkModel = _mapper.Map<VideoRoutingModels.AliasIndexModel, AliasIndexModel>(display.RecordChannelAliasIndex);
                    // note: source can be null/empty - clearing the route
                    var sourceModel = new Ism.Routing.V1.Protos.AliasIndexMessage();
                    if (null != source)
                    {
                        sourceModel = _mapper.Map<Ism.Routing.V1.Protos.AliasIndexMessage>(source);
                    }

                    await _routingService.RouteVideo(new RouteVideoRequest()
                    {
                        Sink = _mapper.Map<AliasIndexModel, Ism.Routing.V1.Protos.AliasIndexMessage>(sinkModel),
                        Source = sourceModel
                    });
                }
            }
        }

        private async Task UpdateDisplayRecordingState(DisplayRecordingViewModel displayRecordingViewModel)
        {
            var currentData = await _stateClient.GetData<VideoRoutingModels.DisplayRecordStateData>();

            var displayIndex = currentData?.DisplayState?.FindIndex(x =>
                string.Equals(x.DisplayAliasIndex.Alias, displayRecordingViewModel.Display.Alias, StringComparison.OrdinalIgnoreCase) &&
                x.DisplayAliasIndex.Index == displayRecordingViewModel.Display.Index) ?? -1;

            var displayAliasIndex = new VideoRoutingModels.AliasIndexModel(displayRecordingViewModel.Display.Alias, displayRecordingViewModel.Display.Index);

            var recordAliasIndex = displayRecordingViewModel.Enabled
                ? new VideoRoutingModels.AliasIndexModel(displayRecordingViewModel.RecordChannel.VideoSink.Alias, displayRecordingViewModel.RecordChannel.VideoSink.Index)
                : new VideoRoutingModels.AliasIndexModel();

            await _stateClient.AddOrUpdateData(
                new VideoRoutingModels.DisplayRecordStateData()
                {
                    DisplayState = new List<VideoRoutingModels.DisplayRecordState> 
                    { 
                        new VideoRoutingModels.DisplayRecordState() 
                        { 
                            DisplayAliasIndex = displayAliasIndex, 
                            RecordChannelAliasIndex = recordAliasIndex 
                        } 
                    }
                },
                x =>
                {
                    // default state won't have an entry for a display
                    if (displayIndex < 0)
                    {
                        x.Add(data => data.DisplayState, new VideoRoutingModels.DisplayRecordState { DisplayAliasIndex = displayAliasIndex, RecordChannelAliasIndex = recordAliasIndex });
                    }
                    else
                    {
                        x.Replace(data => data.DisplayState[displayIndex].RecordChannelAliasIndex, recordAliasIndex);
                    }
                });

        }

        /// <summary>
        /// Publishes the default display based recording state
        /// The state is simply the first X displays mapped to the first Y record channels
        /// </summary>
        /// <returns></returns>
        public async Task PublishDefaultDisplayRecordingState()
        {
            // TODO: make this a private event handler for when a patient is registered

            // get displays and record channels and convert them to system state model aliasIndexes
            var displays = (await _routingService.GetVideoSinks()).VideoSinks.Select(x => new VideoRoutingModels.AliasIndexModel(x.Sink.Alias, x.Sink.Index));
            var recordChannels = (await _recorderService.GetRecordingChannels()).Select(x => new VideoRoutingModels.AliasIndexModel(x.VideoSink.Alias, x.VideoSink.Index));

            // map the first X displays to the first Y record channels
            // Zip starts at the beginning of each collection and stop when it hits the end of either colleciton
            // {a, b, c, d}.Zip({Rec1, Rec2}) turns into {(a, Rec1), (b, Rec2)}
            var dbrStates = displays.Zip(recordChannels, (display, recordChannel) => new VideoRoutingModels.DisplayRecordState(display, recordChannel)).ToList();

            // publish the new DBR state data
            await _stateClient.PersistData(new VideoRoutingModels.DisplayRecordStateData(dbrStates));
        }
    }
}
