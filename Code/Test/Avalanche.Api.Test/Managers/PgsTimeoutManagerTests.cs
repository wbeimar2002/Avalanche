using AutoMapper;
using Avalanche.Api.Managers.Media;
using Avalanche.Api.MappingConfigurations;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models.Media;
using Avalanche.Shared.Infrastructure.Configuration;
using Ism.PgsTimeout.V1.Protos;
using Ism.Routing.V1.Protos;
using Ism.SystemState.Client;
using Ism.SystemState.Models;
using Ism.SystemState.Models.PgsTimeout;
using Microsoft.AspNetCore.JsonPatch;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Test.Managers
{
    [TestFixture]
    public class PgsTimeoutManagerTests
    {
        private Mock<IStorageService> _storageService;
        private Mock<IRoutingService> _routingService;
        private Mock<IStateClient> _stateClient;

        // mock of grpc state client for pgs player
        private Mock<IPgsTimeoutService> _pgsTimeoutService;

        private IMapper _mapper;

        /// <summary>
        /// Represents the current checked state of pgs displays
        /// Used by the state client mock
        /// </summary>
        private PgsDisplayStateData _pgsDisplayStateData;

        private Dictionary<AliasIndexMessage, AliasIndexMessage> _currentRoutes = new Dictionary<AliasIndexMessage, AliasIndexMessage>();

        // thing we're testing
        private PgsTimeoutManager _pgsTimeoutManager;

        private (PgsApiConfiguration PgsConfig, TimeoutApiConfiguration TimeoutConfig) GetConfig()
        {
            var source = new AliasIndexModel { Alias = "4kiDp0", Index = "0" };
            var sinks = new List<AliasIndexModel>
            {
                new AliasIndexModel { Alias = "4koDp2", Index = "0" },
                new AliasIndexModel { Alias = "4koDp3", Index = "0" },
                new AliasIndexModel { Alias = "4koDp4", Index = "0" },
                new AliasIndexModel { Alias = "4koSdi0", Index = "0" }
            };

            var pgsConfig = new PgsApiConfiguration
            {
                Source = source,
                Sinks = sinks
            };

            var timeoutConfig = new TimeoutApiConfiguration
            {
                Source = source,
                Sinks = sinks
            };

            return (pgsConfig, timeoutConfig);
        }

        [SetUp]
        public void Setup()
        {
            _storageService = new Mock<IStorageService>();
            _routingService = new Mock<IRoutingService>();
            _stateClient = new Mock<IStateClient>();
            _pgsTimeoutService = new Mock<IPgsTimeoutService>();

            // set up state client so we can add/get the pgs display state data
            // for the test, only need to fake the add
            _stateClient.Setup(x => x.AddOrUpdateData(It.IsAny<PgsDisplayStateData>(), It.IsAny<Action<JsonPatchDocument<PgsDisplayStateData>>>()))
            .Callback<PgsDisplayStateData, Action<JsonPatchDocument<PgsDisplayStateData>>>((add, update) =>
            {
                _pgsDisplayStateData = add;
            });
            _stateClient.Setup(x => x.GetData<PgsDisplayStateData>()).ReturnsAsync(() =>
            {
                return _pgsDisplayStateData;
            });


            // routing service needs to do a bit of work
            // route and route batch need to maintain some level of state
            _routingService.Setup(x => x.RouteVideoBatch(It.IsAny<RouteVideoBatchRequest>())).Callback<RouteVideoBatchRequest>(x =>
            {
                foreach (var route in x.Routes)
                    _currentRoutes[route.Sink] = route.Source;
            });
            _routingService.Setup(x => x.RouteVideo(It.IsAny<RouteVideoRequest>())).Callback<RouteVideoRequest>(x =>
            {
                _currentRoutes[x.Sink] = x.Source;
            });
            _routingService.Setup(x => x.GetCurrentRoutes()).ReturnsAsync(() =>
            {
                var response = new GetCurrentRoutesResponse();
                response.Routes.AddRange(_currentRoutes.Select(kv => new VideoRouteMessage { Sink = kv.Key, Source = kv.Value }));
                return response;
            });

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MediaMappingConfiguration());
                cfg.AddProfile(new RoutingMappingConfiguration());
            });

            _mapper = mapperConfig.CreateMapper();

            var config = GetConfig();

            _pgsTimeoutManager = new PgsTimeoutManager(
                _routingService.Object,
                _stateClient.Object,
                _pgsTimeoutService.Object,
                _mapper,
                config.PgsConfig,
                config.TimeoutConfig);
        }


        [Test]
        public async Task Pgs_StartPgs_VideoRoutes()
        {
            // Arrange is done in Setup
            // starts pgs and ensures that route is called on the displays

            // Act
            await _pgsTimeoutManager.StartPgs();

            // internally, pgs should be routed to all displays
            var pgsConfig = GetConfig().PgsConfig;
            var request = new RouteVideoBatchRequest();
            request.Routes.AddRange(pgsConfig.Sinks.Select(x => new RouteVideoRequest
            {
                Source = _mapper.Map<AliasIndexModel, AliasIndexMessage>(pgsConfig.Source),
                Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(x)
            }));

            // Assert route video was called
            _routingService.Verify(x => x.RouteVideoBatch(request), Times.Once());
            // Assert the player started playing
            _pgsTimeoutService.Verify(x => x.SetPgsTimeoutMode(new SetPgsTimeoutModeRequest { Mode = PgsTimeoutModeEnum.PgsTimeoutModePgs }), Times.Once());
        }

        [Test]
        public async Task Pgs_StartPgsUncheckedDisplay_VideoRoutes()
        {
            // Arrange is done in Setup
            // unchecks pgs on the first display and verifies that it is not routed to

            // Act
            var pgsConfig = GetConfig().PgsConfig;
            // uncheck pgs on the first display
            await _pgsTimeoutManager.SetPgsStateForSink(new PgsSinkStateViewModel
            {
                Enabled = false,
                Sink = _mapper.Map<AliasIndexModel, AliasIndexViewModel>(pgsConfig.Sinks.First())
            });
            // start pgs
            await _pgsTimeoutManager.StartPgs();

            // internally, pgs should be routed to all but the first display
            // Skip(1) means the first display should have been skipped
            var request = new RouteVideoBatchRequest();
            request.Routes.AddRange(pgsConfig.Sinks.Skip(1).Select(x => new RouteVideoRequest
            {
                Source = _mapper.Map<AliasIndexModel, AliasIndexMessage>(pgsConfig.Source),
                Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(x)
            }));

            // Assert
            _routingService.Verify(x => x.RouteVideoBatch(request), Times.Once());
            _pgsTimeoutService.Verify(x => x.SetPgsTimeoutMode(new SetPgsTimeoutModeRequest { Mode = PgsTimeoutModeEnum.PgsTimeoutModePgs }), Times.Once());
        }

        [Test]
        public async Task Timeout_StartTimeout_VideoRoutes()
        {
            // Arrange is done in Setup
            // starts timeout and ensures that route is called on the displays

            // Act
            await _pgsTimeoutManager.StartTimeout();

            // internally, timeout should be routed to all displays
            var timeoutConfig = GetConfig().TimeoutConfig;
            var request = new RouteVideoBatchRequest();
            request.Routes.AddRange(timeoutConfig.Sinks.Select(x => new RouteVideoRequest
            {
                Source = _mapper.Map<AliasIndexModel, AliasIndexMessage>(timeoutConfig.Source),
                Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(x)
            }));

            // Assert route video was called
            _routingService.Verify(x => x.RouteVideoBatch(request), Times.Once());
            // Assert the player started playing
            _pgsTimeoutService.Verify(x => x.SetPgsTimeoutMode(new SetPgsTimeoutModeRequest { Mode = PgsTimeoutModeEnum.PgsTimeoutModeTimeout }), Times.Once());
        }

        [Test]
        public async Task PgsTimeout_StartPgsThenStartTimeoutThenStopBoth_VideoRoutes()
        {
            // Arrange is done in Setup
            // this test starts pgs, then starts timeout, then stops timeout in the same manner that navigating to the video tab does
            // it then ensures that the room is in idle mode

            // Act
            var pgsConfig = GetConfig().PgsConfig;
            // uncheck pgs on the first display
            await _pgsTimeoutManager.SetPgsStateForSink(new PgsSinkStateViewModel
            {
                Enabled = false,
                Sink = _mapper.Map<AliasIndexModel, AliasIndexViewModel>(pgsConfig.Sinks.First())
            });
            // start pgs
            await _pgsTimeoutManager.StartPgs();
            var routePgsRequest = new RouteVideoBatchRequest();
            routePgsRequest.Routes.AddRange(pgsConfig.Sinks.Skip(1).Select(x => new RouteVideoRequest
            {
                Source = _mapper.Map<AliasIndexModel, AliasIndexMessage>(pgsConfig.Source),
                Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(x)
            }));

            // Assert pgs was started and routed to all but the first display
            _routingService.Verify(x => x.RouteVideoBatch(routePgsRequest), Times.Once());
            _pgsTimeoutService.Verify(x => x.SetPgsTimeoutMode(new SetPgsTimeoutModeRequest { Mode = PgsTimeoutModeEnum.PgsTimeoutModePgs }));


            // start timeout
            await _pgsTimeoutManager.StartTimeout();

            // internally, timeout should be routed to all displays
            var timeoutConfig = GetConfig().TimeoutConfig;
            var routeTimeoutRequest = new RouteVideoBatchRequest();
            routeTimeoutRequest.Routes.AddRange(timeoutConfig.Sinks.Select(x => new RouteVideoRequest
            {
                Source = _mapper.Map<AliasIndexModel, AliasIndexMessage>(timeoutConfig.Source),
                Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(x)
            }));

            // assert that timeout was started and routed everywhere
            _routingService.Verify(x => x.RouteVideoBatch(routeTimeoutRequest), Times.Once());
            _pgsTimeoutService.Verify(x => x.SetPgsTimeoutMode(new SetPgsTimeoutModeRequest { Mode = PgsTimeoutModeEnum.PgsTimeoutModeTimeout }));

            // stop timeout by going to the routing tab
            // should stop both and put the room back in idle mode
            await _pgsTimeoutManager.StopPgsAndTimeout();

            // tests have no saved routes so restoring the route should do nothing
            _routingService.Verify(x => x.RouteVideoBatch(new RouteVideoBatchRequest()));
            // pgs player is almost always in pgs mode
            _pgsTimeoutService.Verify(x => x.SetPgsTimeoutMode(new SetPgsTimeoutModeRequest { Mode = PgsTimeoutModeEnum.PgsTimeoutModePgs }));
        }

        [Test]
        public void Pgs_UnCheckInvalidDisplay_Throws()
        {
            // Arrange is done in Setup
            // changes the checked state of a display that does not exist and ensures it throws

            // Act
            // Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _pgsTimeoutManager.SetPgsStateForSink(new PgsSinkStateViewModel
                {
                    Enabled = false,
                    Sink = new AliasIndexViewModel { Alias = "foobar", Index = "Does not exist" }
                });
            });
        }
    }
}
