using AutoMapper;
using Avalanche.Api.Managers.Media;
using Avalanche.Api.Mapping;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Shared.Domain.Models.Media;
using Ism.Common.Core.Configuration.Models;
using Ism.Routing.V1.Protos;
using Ism.SystemState.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Avalanche.Api.Test.Managers
{
    [TestFixture()]
    public class RoutingManagerTests
    {
        IMapper _mapper;
        Mock<IAccessInfoFactory> _accessInfoFactory;
        Mock<IRoutingService> _routingService;
        Mock<IRecorderService> _recorderService;
        Mock<IAvidisService> _avidisService;
        Mock<IStorageService> _storageService;
        Mock<IHttpContextAccessor> _httpContextAccessor;
        Mock<IStateClient> _stateClient;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new RoutingMappingConfiguration());
                cfg.AddProfile(new PatientMappingConfiguration());
                cfg.AddProfile(new DataMappingConfiguration());
                cfg.AddProfile(new MediaMappingConfiguration());
            });

            _mapper = config.CreateMapper();
            _accessInfoFactory = new Mock<IAccessInfoFactory>();
            _routingService = new Mock<IRoutingService>();
            _recorderService = new Mock<IRecorderService>();
            _avidisService = new Mock<IAvidisService>();
            _storageService = new Mock<IStorageService>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _stateClient = new Mock<IStateClient>();
        }

        [Test]
        public async Task TestSetDisplayRecordingEnabledTriggersRoute()
        {
            _routingService.Setup(r => r.GetRouteForSink(It.IsAny<GetRouteForSinkRequest>()))
                .ReturnsAsync(new GetRouteForSinkResponse { Route = new VideoRouteMessage { Source = new AliasIndexMessage { Alias = "source", Index = "a" } } })
                .Verifiable();

            _routingService.Setup(r => r.RouteVideo(It.IsAny<RouteVideoRequest>()))
                .Verifiable();

            var manager = new RoutingManager(_routingService.Object, _recorderService.Object, _avidisService.Object, _storageService.Object, _mapper, _httpContextAccessor.Object, _stateClient.Object);

            await manager.SetDisplayRecordingStatus(new ViewModels.DisplayRecordingRequestViewModel
            {
                Display = new AliasIndexModel { Alias = "one", Index = "two" },
                RecordChannel = new RecordingChannelModel { ChannelName = "channel", VideoSink = new AliasIndexModel { Alias = "three", Index = "four" } },
                Enabled = true
            });

            _routingService.Verify(r => r.GetRouteForSink(It.Is<GetRouteForSinkRequest>(req => req.Sink.Alias == "one" && req.Sink.Index == "two")), Times.Once);
            _routingService.Verify(r => r.RouteVideo(It.Is<RouteVideoRequest>(req => req.Sink.Alias == "three" && req.Sink.Index == "four" && req.Source.Alias == "source" && req.Source.Index == "a")), Times.Once);
        }

        [Test]
        public async Task TestSetDisplayRecordingEnabledTriggersStateUpdate()
        {
            _stateClient.Setup(m => m.AddOrUpdateData(
                It.IsAny<Ism.SystemState.Models.VideoRouting.DisplayRecordStateData>(),
                It.IsAny<Action<JsonPatchDocument<Ism.SystemState.Models.VideoRouting.DisplayRecordStateData>>>()))
                .Verifiable();

            var manager = new RoutingManager(_routingService.Object, _recorderService.Object, _avidisService.Object, _storageService.Object, _mapper, _httpContextAccessor.Object, _stateClient.Object);

            await manager.SetDisplayRecordingStatus(new ViewModels.DisplayRecordingRequestViewModel
            {
                Display = new AliasIndexModel { Alias = "one", Index = "two" },
                RecordChannel = new RecordingChannelModel { ChannelName = "channel", VideoSink = new AliasIndexModel { Alias = "three", Index = "four" } },
                Enabled = true
            });

            _stateClient.Verify(
                m => m.PersistData(It.IsAny<Ism.SystemState.Models.VideoRouting.DisplayRecordStateData>()),
                Times.Once);
        }

        [Test]
        public async Task TestSetDisplayRecordingEnabledClearsRouteIfSetToDisable()
        {
            _routingService.Setup(r => r.GetRouteForSink(It.IsAny<GetRouteForSinkRequest>()))
                .ReturnsAsync(new GetRouteForSinkResponse { Route = new VideoRouteMessage { Source = new AliasIndexMessage { Alias = "source", Index = "a" } } })
                .Verifiable();

            _routingService.Setup(r => r.RouteVideo(It.IsAny<RouteVideoRequest>()))
                .Verifiable();

            var manager = new RoutingManager(_routingService.Object, _recorderService.Object, _avidisService.Object, _storageService.Object, _mapper, _httpContextAccessor.Object, _stateClient.Object);

            await manager.SetDisplayRecordingStatus(new ViewModels.DisplayRecordingRequestViewModel
            {
                Display = new AliasIndexModel { Alias = "one", Index = "two" },
                RecordChannel = new RecordingChannelModel { ChannelName = "channel", VideoSink = new AliasIndexModel { Alias = "three", Index = "four" } },
                Enabled = false
            });

            _routingService.Verify(r => r.GetRouteForSink(It.Is<GetRouteForSinkRequest>(req => req.Sink.Alias == "one" && req.Sink.Index == "two")), Times.Never);
            _routingService.Verify(r => r.RouteVideo(It.Is<RouteVideoRequest>(req => req.Sink.Alias == "three" && req.Sink.Index == "four" && string.IsNullOrEmpty(req.Source.Alias) && string.IsNullOrEmpty(req.Source.Index))), Times.Once);

        }



        [Test]
        public async Task TestSetDisplayRecordingEnabledForEmptyDisplayRoutesNothing()
        {
            _routingService.Setup(r => r.GetRouteForSink(It.IsAny<GetRouteForSinkRequest>()))
                .ReturnsAsync(new GetRouteForSinkResponse { Route = new VideoRouteMessage { } })
                .Verifiable();

            _routingService.Setup(r => r.RouteVideo(It.IsAny<RouteVideoRequest>()))
                .Verifiable();

            var manager = new RoutingManager(_routingService.Object, _recorderService.Object, _avidisService.Object, _storageService.Object, _mapper, _httpContextAccessor.Object, _stateClient.Object);

            await manager.SetDisplayRecordingStatus(new ViewModels.DisplayRecordingRequestViewModel
            {
                Display = new AliasIndexModel { Alias = "one", Index = "two" },
                RecordChannel = new RecordingChannelModel { ChannelName = "channel", VideoSink = new AliasIndexModel { Alias = "three", Index = "four" } },
                Enabled = true
            }); ;

            _routingService.Verify(r => r.GetRouteForSink(It.Is<GetRouteForSinkRequest>(req => req.Sink.Alias == "one" && req.Sink.Index == "two")), Times.Once);
            _routingService.Verify(r => r.RouteVideo(It.Is<RouteVideoRequest>(req => req.Sink.Alias == "three" && req.Sink.Index == "four" && string.IsNullOrEmpty(req.Source.Alias) && string.IsNullOrEmpty(req.Source.Index))), Times.Once);

        }

        [Test]
        public async Task TestPublishDefaultDisplayRecordingStatePicksFirstDisplays()
        {
            _stateClient.Setup(s => s.PersistData(It.IsAny<Ism.SystemState.Models.VideoRouting.DisplayRecordStateData>()))
                .Verifiable();

            _routingService.Setup(s => s.GetVideoSinks()).ReturnsAsync(() =>
            {
                var response = new GetVideoSinksResponse();
                response.VideoSinks.Add(new VideoSinkMessage { Sink = new AliasIndexMessage { Alias = "alias1", Index = "1" } });
                response.VideoSinks.Add(new VideoSinkMessage { Sink = new AliasIndexMessage { Alias = "alias2", Index = "2" } });
                response.VideoSinks.Add(new VideoSinkMessage { Sink = new AliasIndexMessage { Alias = "alias3", Index = "3" } });
                response.VideoSinks.Add(new VideoSinkMessage { Sink = new AliasIndexMessage { Alias = "alias4", Index = "4" } });
                return response;
            });
            _recorderService.Setup(s => s.GetRecordingChannels()).ReturnsAsync(new List<Ism.Recorder.Core.V1.Protos.RecordChannelMessage>()
            {
                new Ism.Recorder.Core.V1.Protos.RecordChannelMessage { ChannelName = "rec1", VideoSink = new Ism.Recorder.Core.V1.Protos.AliasIndexMessage { Alias = "rec1", Index = "1" } },
                new Ism.Recorder.Core.V1.Protos.RecordChannelMessage { ChannelName = "rec2", VideoSink = new Ism.Recorder.Core.V1.Protos.AliasIndexMessage { Alias = "rec2", Index = "2" } }
            });

            _storageService.Setup(s => s.GetJsonObject<List<AliasIndexModel>>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ConfigurationContext>()))
                .ReturnsAsync(new List<AliasIndexModel>()
                {
                    new AliasIndexModel { Alias = "alias1", Index = "1" },
                    new AliasIndexModel { Alias = "alias2", Index = "2" },
                    new AliasIndexModel { Alias = "alias3", Index = "3" },
                    new AliasIndexModel { Alias = "alias4", Index = "4" }
                });

            var manager = new RoutingManager(_routingService.Object, _recorderService.Object, _avidisService.Object, _storageService.Object, _mapper, _httpContextAccessor.Object, _stateClient.Object);
            await manager.PublishDefaultDisplayRecordingState();

            _stateClient.Verify(s => s.PersistData(It.Is<Ism.SystemState.Models.VideoRouting.DisplayRecordStateData>(
                req => (2 == req.DisplayState.Count)
                    && req.DisplayState[0].DisplayAliasIndex.Alias == "alias1" && req.DisplayState[0].DisplayAliasIndex.Index == "1"
                    && req.DisplayState[0].RecordChannelAliasIndexes[0].Alias == "rec1" && req.DisplayState[0].RecordChannelAliasIndexes[0].Index == "1"
                    && req.DisplayState[1].DisplayAliasIndex.Alias == "alias2" && req.DisplayState[1].DisplayAliasIndex.Index == "2"
                    && req.DisplayState[1].RecordChannelAliasIndexes[0].Alias == "rec2" && req.DisplayState[1].RecordChannelAliasIndexes[0].Index == "2"
                )), Times.Once);
        }

        [Test]
        public async Task TestPublishDefaultDisplayRecordingStateSkipsDisplaysNotEnabled()
        {
            _stateClient.Setup(s => s.PersistData(It.IsAny<Ism.SystemState.Models.VideoRouting.DisplayRecordStateData>()))
                .Verifiable();

            _routingService.Setup(s => s.GetVideoSinks()).ReturnsAsync(() =>
            {
                var response = new GetVideoSinksResponse();
                response.VideoSinks.Add(new VideoSinkMessage { Sink = new AliasIndexMessage { Alias = "alias1", Index = "1" } });
                response.VideoSinks.Add(new VideoSinkMessage { Sink = new AliasIndexMessage { Alias = "alias2", Index = "2" } });
                response.VideoSinks.Add(new VideoSinkMessage { Sink = new AliasIndexMessage { Alias = "alias3", Index = "3" } });
                response.VideoSinks.Add(new VideoSinkMessage { Sink = new AliasIndexMessage { Alias = "alias4", Index = "4" } });
                return response;
            });
            _recorderService.Setup(s => s.GetRecordingChannels()).ReturnsAsync(new List<Ism.Recorder.Core.V1.Protos.RecordChannelMessage>()
            {
                new Ism.Recorder.Core.V1.Protos.RecordChannelMessage { ChannelName = "rec1", VideoSink = new Ism.Recorder.Core.V1.Protos.AliasIndexMessage { Alias = "rec1", Index = "1" } },
                new Ism.Recorder.Core.V1.Protos.RecordChannelMessage { ChannelName = "rec2", VideoSink = new Ism.Recorder.Core.V1.Protos.AliasIndexMessage { Alias = "rec2", Index = "2" } }
            });
            _storageService.Setup(s => s.GetJsonObject<List<AliasIndexModel>>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ConfigurationContext>()))
                .ReturnsAsync(new List<AliasIndexModel>()
                {
                    new AliasIndexModel { Alias = "alias3", Index = "3" },
                    new AliasIndexModel { Alias = "alias4", Index = "4" }
                });

            var manager = new RoutingManager(_routingService.Object, _recorderService.Object, _avidisService.Object, _storageService.Object, _mapper, _httpContextAccessor.Object, _stateClient.Object);
            await manager.PublishDefaultDisplayRecordingState();

            _stateClient.Verify(s => s.PersistData(It.Is<Ism.SystemState.Models.VideoRouting.DisplayRecordStateData>(
                req => (2 == req.DisplayState.Count)
                    && req.DisplayState[0].DisplayAliasIndex.Alias == "alias3" && req.DisplayState[0].DisplayAliasIndex.Index == "3"
                    && req.DisplayState[0].RecordChannelAliasIndexes[0].Alias == "rec1" && req.DisplayState[0].RecordChannelAliasIndexes[0].Index == "1"
                    && req.DisplayState[1].DisplayAliasIndex.Alias == "alias4" && req.DisplayState[1].DisplayAliasIndex.Index == "4"
                    && req.DisplayState[1].RecordChannelAliasIndexes[0].Alias == "rec2" && req.DisplayState[1].RecordChannelAliasIndexes[0].Index == "2"
                )), Times.Once);
        }

        [Test]
        public async Task RoutingManager_SetSelectedSource_Succeeds()
        {
            var manager = new RoutingManager(_routingService.Object, _recorderService.Object, _avidisService.Object, _storageService.Object, _mapper, _httpContextAccessor.Object, _stateClient.Object);
            await manager.SetSelectedSource(new AliasIndexModel { Alias = "alias3", Index = "3" });
            _stateClient.Verify(s => s.PersistData(It.Is<Ism.SystemState.Models.VideoRouting.SelectedSourceStateData>(req => (req.SelectedSource.Alias == "alias3" && req.SelectedSource.Index == "3"))), Times.Once);
        }

        [Test]
        public async Task RoutingManager_SetSelectedSource_Overrides_ExistingSource_Succeeds()
        {
            var manager = new RoutingManager(_routingService.Object, _recorderService.Object, _avidisService.Object, _storageService.Object, _mapper, _httpContextAccessor.Object, _stateClient.Object);
            _stateClient.Setup(s => s.GetData<Ism.SystemState.Models.VideoRouting.SelectedSourceStateData>()).ReturnsAsync(
                new Ism.SystemState.Models.VideoRouting.SelectedSourceStateData(new Ism.SystemState.Models.VideoRouting.AliasIndexModel { Alias = "alias3", Index = "3" }));

            await manager.SetSelectedSource(new AliasIndexModel { Alias = "alias4", Index = "4" });
            _stateClient.Verify(s => s.PersistData(It.Is<Ism.SystemState.Models.VideoRouting.SelectedSourceStateData>(req => (req.SelectedSource.Alias == "alias4" && req.SelectedSource.Index == "4"))), Times.Once);
        }

        [Test]
        public async Task RoutingManager_SetSelectedSource_NoSelectedSource_Fails()
        {
            var manager = new RoutingManager(_routingService.Object, _recorderService.Object, _avidisService.Object, _storageService.Object, _mapper, _httpContextAccessor.Object, _stateClient.Object);
            _stateClient.Setup(s => s.GetData<Ism.SystemState.Models.VideoRouting.SelectedSourceStateData>()).ReturnsAsync(
                new Ism.SystemState.Models.VideoRouting.SelectedSourceStateData(new Ism.SystemState.Models.VideoRouting.AliasIndexModel { Alias = "alias3", Index = "3" }));

            Assert.ThrowsAsync<ArgumentNullException>(async () => await manager.SetSelectedSource(null));
        }

        [Test]
        public async Task RoutingManager_SetSelectedSource_EmptyAlias_Fails()
        {
            var manager = new RoutingManager(_routingService.Object, _recorderService.Object, _avidisService.Object, _storageService.Object, _mapper, _httpContextAccessor.Object, _stateClient.Object);
            _stateClient.Setup(s => s.GetData<Ism.SystemState.Models.VideoRouting.SelectedSourceStateData>()).ReturnsAsync(
                new Ism.SystemState.Models.VideoRouting.SelectedSourceStateData(new Ism.SystemState.Models.VideoRouting.AliasIndexModel { Alias = "alias3", Index = "3" }));

            Assert.ThrowsAsync<ArgumentNullException>(async () => await manager.SetSelectedSource(new AliasIndexModel { Alias = "", Index = "4" }));
        }

        [Test]
        public async Task RoutingManager_SetSelectedSource_EmptyIndex_Fails()
        {
            var manager = new RoutingManager(_routingService.Object, _recorderService.Object, _avidisService.Object, _storageService.Object, _mapper, _httpContextAccessor.Object, _stateClient.Object);
            _stateClient.Setup(s => s.GetData<Ism.SystemState.Models.VideoRouting.SelectedSourceStateData>()).ReturnsAsync(
                new Ism.SystemState.Models.VideoRouting.SelectedSourceStateData(new Ism.SystemState.Models.VideoRouting.AliasIndexModel { Alias = "alias3", Index = "3" }));

            Assert.ThrowsAsync<ArgumentNullException>(async () => await manager.SetSelectedSource(new AliasIndexModel { Alias = "alias4", Index = "" }));
        }

        [Test]
        public async Task RoutingManager_GetLayoutsForSink_Fails()
        {
            var manager = new RoutingManager(_routingService.Object, _recorderService.Object, _avidisService.Object, _storageService.Object, _mapper, _httpContextAccessor.Object, _stateClient.Object);
            var layouts = await manager.GetLayoutsForSink(new AliasIndexModel { Alias = "test", Index = "test" }).ConfigureAwait(false);
            Assert.Null(layouts);
        }

        [Test]
        public async Task RoutingManager_GetLayoutsForSink_Succeedes()
        {
            var layoutResponse = new GetTileLayoutsForSinkResponse();
            var tileLayout = new TileLayoutMessage
            {
                LayoutName = "test"
            };
            tileLayout.Viewports.Add(new TileViewportMessage { Layer = 1, X = 10, Y = 10, Width = 10, Height = 10 });
            layoutResponse.Layouts.Add(tileLayout);

            var manager = new RoutingManager(_routingService.Object, _recorderService.Object, _avidisService.Object, _storageService.Object, _mapper, _httpContextAccessor.Object, _stateClient.Object);
            _routingService.Setup(x => x.GetLayoutsForSink(It.IsAny<GetTileLayoutsForSinkRequest>())).ReturnsAsync(layoutResponse);

            var layouts = await manager.GetLayoutsForSink(new AliasIndexModel { Alias = "test", Index = "test" }).ConfigureAwait(false);
            Assert.NotNull(layouts);
        }
    }
}
