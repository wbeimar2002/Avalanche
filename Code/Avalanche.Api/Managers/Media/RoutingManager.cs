using AutoMapper;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
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
using Ism.Utility.Core;

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

        private readonly UserModel _user;
        private readonly ConfigurationContext _configurationContext;

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

            _user = HttpContextUtilities.GetUser(_httpContextAccessor.HttpContext);
            _configurationContext = _mapper.Map<UserModel, ConfigurationContext>(_user);
            _configurationContext.IdnId = Guid.NewGuid().ToString();
        }

        public async Task EnterFullScreen(FullScreenRequestViewModel routingActionViewModel) => await _routingService.EnterFullScreen(new EnterFullScreenRequest()
        {
            Source = _mapper.Map<AliasIndexViewModel, AliasIndexMessage>(routingActionViewModel.Source),
            UserInterfaceId = routingActionViewModel.UserInterfaceId
        }).ConfigureAwait(false);

        public async Task ExitFullScreen(FullScreenRequestViewModel routingActionViewModel) => await _routingService.ExitFullScreen(new ExitFullScreenRequest()
        {
            UserInterfaceId = Convert.ToInt32(routingActionViewModel.UserInterfaceId)
        }).ConfigureAwait(false);

        public async Task HidePreview(RoutingPreviewViewModel routingPreviewViewModel) => await _avidisService.HidePreview(new HidePreviewRequest()
        {
            PreviewIndex = routingPreviewViewModel.Index
        }).ConfigureAwait(false);

        public async Task ShowPreview(RoutingPreviewViewModel routingPreviewViewModel)
        {
            await _avidisService.ShowPreview(_mapper.Map<RegionModel, ShowPreviewRequest>(routingPreviewViewModel.Region)).ConfigureAwait(false);

            //TODO: Map this
            await _avidisService.ShowPreview(new ShowPreviewRequest()
            {
                PreviewIndex = routingPreviewViewModel.Index,
                Height = routingPreviewViewModel.Region.Height,
                Width = routingPreviewViewModel.Region.Width,
                X = routingPreviewViewModel.Region.X,
                Y = routingPreviewViewModel.Region.Y
            }).ConfigureAwait(false);
        }

        public async Task RouteVideoSource(RouteModel route) => await _routingService.RouteVideo(new RouteVideoRequest()
        {
            Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(route.Sink),
            Source = _mapper.Map<AliasIndexModel, AliasIndexMessage>(route.Source),
        }).ConfigureAwait(false);

        public async Task UnrouteVideoSource(AliasIndexModel sink) => await _routingService.RouteVideo(new RouteVideoRequest()
        {
            Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(sink),
            Source = new AliasIndexMessage { Alias = string.Empty, Index = string.Empty }
        }).ConfigureAwait(false);

        public async Task<IList<VideoSourceModel>> GetRoutingSources()
        {
            // get video sources and their states
            // the state collection only contains the AliasIndex and a bool
            var sources = await _routingService.GetVideoSources().ConfigureAwait(false);
            var states = await _routingService.GetVideoStateForAllSources().ConfigureAwait(false);

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
                    Source = new AliasIndexMessage
                    {
                        Alias = sinkModel.Alias,
                        Index = sinkModel.Index
                    }
                }).ConfigureAwait(false);

            var hasVideo = await _routingService.GetVideoStateForSource(
                new GetVideoStateForSourceRequest
                {
                    Source = new AliasIndexMessage
                    {
                        Alias = sinkModel.Alias,
                        Index = sinkModel.Index
                    }
                }).ConfigureAwait(false);

            var mappedSource = _mapper.Map<VideoSourceMessage, VideoSourceModel>(source.Source);

            mappedSource.Sink = sinkModel;

            // you could plug in an ela that has no video connected to it
            mappedSource.HasVideo = hasVideo.HasVideo;

            return mappedSource;
        }

        public async Task<IList<VideoSinkModel>> GetRoutingSinks()
        {
            var sinks = await _routingService.GetVideoSinks().ConfigureAwait(false);
            var routes = await _routingService.GetCurrentRoutes().ConfigureAwait(false);

            // any display not in the will not have the record buttons next to it
            var dbrSinks = await _storageService.GetJsonObject<List<AliasIndexModel>>("DisplayBasedRecordingSinks", 1, ConfigurationContext.FromEnvironment()).ConfigureAwait(false);

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
                sink.RecordEnabled = dbrSinks.Any(x =>
                    string.Equals(x.Alias, sink.Sink.Alias, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.Index, sink.Sink.Index, StringComparison.OrdinalIgnoreCase));
            }
            return listResult;
        }

        public async Task<IList<DisplayRecordingViewModel>> GetDisplayRecordingStatuses()
        {
            var currentData = await _stateClient.GetData<VideoRoutingModels.DisplayRecordStateData>().ConfigureAwait(false);
            var recordChannels = await _recorderService.GetRecordingChannels().ConfigureAwait(false);

            return currentData?.DisplayState?.Select(d =>
            {
                var modelChannels = recordChannels?.Where(r =>
                        d.RecordChannelAliasIndexes.Any(stateRecChan => 
                            string.Equals(r.VideoSink.Alias, stateRecChan.Alias, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(r.VideoSink.Index, stateRecChan.Index, StringComparison.OrdinalIgnoreCase))
                        ).Select(r => _mapper.Map<RecordingChannelModel>(r)).ToList();

                return new DisplayRecordingViewModel(
                    _mapper.Map<AliasIndexModel>(d.DisplayAliasIndex),
                    modelChannels,
                    modelChannels?.Any() ?? false);
            })?.ToList();
        }

        public async Task SetDisplayRecordingStatus(DisplayRecordingRequestViewModel displayRecordingRequestModel)
        {
            if (displayRecordingRequestModel.Enabled)
            {
                // find the source routed to the display, and route it to the indicated record channel
                var displayRoute = await _routingService.GetRouteForSink(
                    new GetRouteForSinkRequest
                    {
                        Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(displayRecordingRequestModel.Display)
                    }).ConfigureAwait(false);

                var source = displayRoute?.Route?.Source ?? new AliasIndexMessage(); // none/empty => route nothing. This is ok.

                await _routingService.RouteVideo(new RouteVideoRequest()
                {
                    Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(displayRecordingRequestModel.RecordChannel.VideoSink),
                    Source = source,
                }).ConfigureAwait(false);
            }
            else
            {
                // clear the route from to the record channel
                await _routingService.RouteVideo(new RouteVideoRequest()
                {
                    Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(displayRecordingRequestModel.RecordChannel.VideoSink),
                    Source = new AliasIndexMessage(), // empty => clear route
                }).ConfigureAwait(false);
            }

            await UpdateDisplayRecordingState(displayRecordingRequestModel).ConfigureAwait(false);
        }

        public async Task HandleSinkSourceChanged(AliasIndexModel sink, AliasIndexModel source)
        {
            if (!string.IsNullOrEmpty(sink?.Alias) && !string.IsNullOrEmpty(sink?.Index))
            {
                // check if we have display-based-recording status for this sink (display)
                var displayRecordState = await _stateClient.GetData<VideoRoutingModels.DisplayRecordStateData>().ConfigureAwait(false);

                var display = displayRecordState?.DisplayState?.FirstOrDefault(d =>
                    string.Equals(d.DisplayAliasIndex?.Alias, sink.Alias, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(d.DisplayAliasIndex?.Index, sink.Index, StringComparison.OrdinalIgnoreCase));

                // if yes - route the updated source to the display's bound record sink
                var newRoutes = new List<RouteVideoRequest>();
                display?.RecordChannelAliasIndexes?.ForEach(recordChan =>
                {
                    if (!string.IsNullOrEmpty(recordChan?.Alias) && !string.IsNullOrEmpty(recordChan?.Index))
                    {
                        var sinkModel = _mapper.Map<VideoRoutingModels.AliasIndexModel, AliasIndexModel>(recordChan);
                        // note: source can be null/empty - clearing the route
                        var sourceModel = new AliasIndexMessage();
                        if (source != null)
                        {
                            sourceModel = _mapper.Map<AliasIndexMessage>(source);
                        }

                        newRoutes.Add(new RouteVideoRequest()
                        {
                            Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(sinkModel),
                            Source = sourceModel
                        });
                    }
                });

                if (newRoutes.Count > 0)
                {
                    var batchRequest = new RouteVideoBatchRequest();
                    batchRequest.Routes.AddRange(newRoutes);
                    await _routingService.RouteVideoBatch(batchRequest).ConfigureAwait(false);
                }
            }
        }

        private async Task UpdateDisplayRecordingState(DisplayRecordingRequestViewModel displayRecordingRequestModel)
        {
            var currentData = await _stateClient.GetData<VideoRoutingModels.DisplayRecordStateData>().ConfigureAwait(false) ?? new VideoRoutingModels.DisplayRecordStateData();
            if (currentData.DisplayState == null)
            {
                currentData.DisplayState = new List<VideoRoutingModels.DisplayRecordState>();
            }

            var displayAliasIndex = new VideoRoutingModels.AliasIndexModel(displayRecordingRequestModel.Display.Alias, displayRecordingRequestModel.Display.Index);
            var recordAliasIndex = new VideoRoutingModels.AliasIndexModel(displayRecordingRequestModel.RecordChannel.VideoSink.Alias, displayRecordingRequestModel.RecordChannel.VideoSink.Index);

            // find all set to the same record channel and clear them
            foreach (var displayState in currentData.DisplayState)
            {
                displayState.RecordChannelAliasIndexes.RemoveAll(r => 
                    string.Equals(r?.Alias, recordAliasIndex.Alias, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r?.Index, recordAliasIndex.Index, StringComparison.OrdinalIgnoreCase));
            }

            // remove any empties that result from the above
            currentData.DisplayState.RemoveAll(d => !(d.RecordChannelAliasIndexes?.Any() ?? false));

            if (displayRecordingRequestModel.Enabled)
            {
                var existingState = currentData.DisplayState.FirstOrDefault(d =>
                    string.Equals(d?.DisplayAliasIndex?.Alias, displayRecordingRequestModel.Display.Alias, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(d?.DisplayAliasIndex?.Index, displayRecordingRequestModel.Display.Index, StringComparison.OrdinalIgnoreCase));

                // add the new
                if (existingState == null)
                {
                    currentData.DisplayState.Add(new VideoRoutingModels.DisplayRecordState
                    {
                        DisplayAliasIndex = displayAliasIndex,
                        RecordChannelAliasIndexes = new List<VideoRoutingModels.AliasIndexModel> { recordAliasIndex }
                    });
                }
                else
                {
                    existingState.RecordChannelAliasIndexes.Add(recordAliasIndex);
                }
            }

            // replace existing state data (this is too complex for json patch)
            await _stateClient.PersistData(currentData).ConfigureAwait(false);
        }

        /// <summary>
        /// Publishes the default display based recording state
        /// The state is simply the first X displays mapped to the first Y record channels
        /// </summary>
        public async Task PublishDefaultDisplayRecordingState()
        {
            // TODO: make this a private event handler for when a patient is registered

            // get displays and record channels and convert them to system state model aliasIndexes
            var displays = (await _routingService.GetVideoSinks().ConfigureAwait(false)).VideoSinks.Select(x => new VideoRoutingModels.AliasIndexModel(x.Sink.Alias, x.Sink.Index));
            var recordChannels = (await _recorderService.GetRecordingChannels().ConfigureAwait(false)).Select(x => new VideoRoutingModels.AliasIndexModel(x.VideoSink.Alias, x.VideoSink.Index));

            // filter down to the displays that actually have dbr enabled
            var dbrSinks = await _storageService.GetJsonObject<List<AliasIndexModel>>("DisplayBasedRecordingSinks", 1, ConfigurationContext.FromEnvironment()).ConfigureAwait(false);
            displays = displays.Where(sink => dbrSinks.Any(x =>
                    string.Equals(x.Alias, sink.Alias, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.Index, sink.Index, StringComparison.OrdinalIgnoreCase)));

            // map the first X displays to the first Y record channels
            // Zip starts at the beginning of each collection and stop when it hits the end of either colleciton
            // {a, b, c, d}.Zip({Rec1, Rec2}) turns into {(a, Rec1), (b, Rec2)}
            var dbrStates = displays.Zip(recordChannels, (display, recordChannel) => new VideoRoutingModels.DisplayRecordState(display, new List<VideoRoutingModels.AliasIndexModel> { recordChannel })).ToList();

            // publish the new DBR state data
            await _stateClient.PersistData(new VideoRoutingModels.DisplayRecordStateData(dbrStates)).ConfigureAwait(false);
        }

        public async Task SetSelectedSource(AliasIndexModel selectedSource)
        {
            Preconditions.ThrowIfNull(nameof(selectedSource), selectedSource);
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(selectedSource.Alias), selectedSource.Alias);
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(selectedSource.Index), selectedSource.Index);

            var aliasIndexModel = new VideoRoutingModels.AliasIndexModel
            {
                Alias = selectedSource.Alias,
                Index = selectedSource.Index
            };

            var newData = new VideoRoutingModels.VideoRoutingStateData
            {
                SelectedSource = aliasIndexModel
            };

            await _stateClient.AddOrUpdateData(newData, x => x.Replace(data => data.SelectedSource, aliasIndexModel)).ConfigureAwait(false);
        }

        public async Task<AliasIndexModel> GetSelectedSource()
        {
            var currentData = await _stateClient.GetData<VideoRoutingModels.VideoRoutingStateData>().ConfigureAwait(false);
            return new AliasIndexModel
            {
                Alias = currentData?.SelectedSource?.Alias ?? string.Empty,
                Index = currentData?.SelectedSource?.Index ?? string.Empty
            };
        }

        public async Task<IList<TileLayoutModel>?> GetLayoutsForSink(AliasIndexModel sinkModel)
        {
            Preconditions.ThrowIfNull(nameof(sinkModel), sinkModel);
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(sinkModel.Alias), sinkModel.Alias);
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(sinkModel.Index), sinkModel.Index);

            var layouts = await _routingService.GetLayoutsForSink(new GetTileLayoutsForSinkRequest { Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(sinkModel) }).ConfigureAwait(false);
            return layouts?.Layouts?.Select(layoutModel => _mapper.Map<TileLayoutModel>(layoutModel)).ToList();
        }

        public async Task<TileLayoutModel> GetLayoutForSink(AliasIndexModel sinkModel)
        {
            Preconditions.ThrowIfNull(nameof(sinkModel), sinkModel);
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(sinkModel.Alias), sinkModel.Alias);
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(sinkModel.Index), sinkModel.Index);

            var layout = await _routingService.GetLayoutForSink(new GetTileLayoutRequest { Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(sinkModel) }).ConfigureAwait(false);
            return _mapper.Map<TileLayoutModel>(layout.Layout);
        }

        public async Task SetLayoutForSink(AliasIndexModel sinkModel, string layoutName)
        {
            Preconditions.ThrowIfNull(nameof(sinkModel), sinkModel);
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(sinkModel.Alias), sinkModel.Alias);
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(sinkModel.Index), sinkModel.Index);

            await _routingService.SetLayoutForSink(new SetTileLayoutRequest { Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(sinkModel), LayoutName = layoutName }).ConfigureAwait(false);

            var sinkAliasIndexModel = new VideoRoutingModels.AliasIndexModel
            {
                Alias = sinkModel.Alias,
                Index = sinkModel.Index
            };

            var currentData = await _stateClient.GetData<VideoRoutingModels.TilingStateData>().ConfigureAwait(false);
            var sinkIndex = currentData?.TilingSinks.FindIndex(x => x.Sink.Alias.Equals(sinkModel.Alias, StringComparison.OrdinalIgnoreCase) && x.Sink.Index.Equals(sinkModel.Index, StringComparison.OrdinalIgnoreCase)) ?? -1;

            if (sinkIndex >= 0)
            {
                _ = await _stateClient.UpdateData<VideoRoutingModels.TilingStateData>(patch =>
                  {
                      _ = patch.Replace(data => data.TilingSinks[sinkIndex].LayoutName, layoutName);
                      _ = patch.Replace(data => data.TilingSinks[sinkIndex].Sink, sinkAliasIndexModel);
                  }).ConfigureAwait(false);
            }
            else
            {
                var tilingSink = new VideoRoutingModels.TilingSink
                {
                    Sink = sinkAliasIndexModel,
                    LayoutName = layoutName
                };

                await _stateClient.AddOrUpdateData(new VideoRoutingModels.TilingStateData
                {
                    TilingSinks = new List<VideoRoutingModels.TilingSink> { tilingSink }
                },
                patch => patch.Add(data => data.TilingSinks, tilingSink)).ConfigureAwait(false);
            }
        }

        public async Task<TileVideoRouteModel> GetTileRouteForSink(AliasIndexModel sinkModel)
        {
            Preconditions.ThrowIfNull(nameof(sinkModel), sinkModel);
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(sinkModel.Alias), sinkModel.Alias);
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(sinkModel.Index), sinkModel.Index);

            var route = await _routingService.GetTileRouteForSink(new GetTileRouteForSinkRequest { Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(sinkModel) }).ConfigureAwait(false);
            return _mapper.Map<TileVideoRouteModel>(route.Route);
        }

        public async Task RouteVideoTiling(RouteVideoTilingModel route)
        {
            Preconditions.ThrowIfNull(nameof(route), route);
            Preconditions.ThrowIfNull(nameof(route.Source), route.Source);
            Preconditions.ThrowIfNull(nameof(route.Sink), route.Sink);
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(route.Source.Alias), route.Source.Alias);
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(route.Source.Index), route.Source.Index);
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(route.Sink.Alias), route.Sink.Alias);
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(route.Sink.Index), route.Sink.Index);

            await _routingService.RouteVideoTiling(new RouteVideoTilingRequest
            {
                Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(route.Sink),
                Source = _mapper.Map<AliasIndexModel, AliasIndexMessage>(route.Source),
                ViewportIndex = route.ViewportIndex
            }).ConfigureAwait(false);

            var viewportSource = new VideoRoutingModels.ViewportSource(
                new VideoRoutingModels.AliasIndexModel { Alias = route.Source.Alias, Index = route.Source.Index },
                route.ViewportIndex);

            var currentData = await _stateClient.GetData<VideoRoutingModels.TilingStateData>().ConfigureAwait(false);
            var sinkIndex = currentData?.TilingSinks.FindIndex(x => x.Sink.Alias.Equals(route.Sink.Alias, StringComparison.OrdinalIgnoreCase) && x.Sink.Index.Equals(route.Sink.Index, StringComparison.OrdinalIgnoreCase)) ?? -1;

            if (sinkIndex >= 0)
            {
                var sourceIndex = currentData?.TilingSinks[sinkIndex].Sources.FindIndex(x => x.ViewportIndex == route.ViewportIndex) ?? -1;
                if (sourceIndex >= 0)
                {
                    await _stateClient.UpdateData<VideoRoutingModels.TilingStateData>(patch => patch.Replace(data => data.TilingSinks[sinkIndex].Sources[sourceIndex], viewportSource)).ConfigureAwait(false);
                }
                else
                {
                    await _stateClient.UpdateData<VideoRoutingModels.TilingStateData>(patch => patch.Add(data => data.TilingSinks[sinkIndex].Sources, viewportSource)).ConfigureAwait(false);
                }
            }
            else
            {
                var sinkAliasIndexModel = new VideoRoutingModels.AliasIndexModel
                {
                    Alias = route.Sink.Alias,
                    Index = route.Sink.Index
                };

                await _stateClient.AddOrUpdateData(new VideoRoutingModels.TilingStateData
                {
                    TilingSinks = new List<VideoRoutingModels.TilingSink>
                    {
                        new VideoRoutingModels.TilingSink
                        {
                            Sink = sinkAliasIndexModel,
                            Sources = new List<VideoRoutingModels.ViewportSource>()
                            {
                                viewportSource
                            }
                        }
                    }
                },
                patch =>
                    patch.Add(data => data.TilingSinks,
                    new VideoRoutingModels.TilingSink
                    {
                        Sink = new VideoRoutingModels.AliasIndexModel { Alias = route.Sink.Alias, Index = route.Sink.Index },
                        Sources = new List<VideoRoutingModels.ViewportSource>() { viewportSource }
                    })).ConfigureAwait(false);
            }
        }
    }
}
