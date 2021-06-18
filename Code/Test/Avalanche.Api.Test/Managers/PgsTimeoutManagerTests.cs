using AutoMapper;

using Avalanche.Api.Managers.Media;
using Avalanche.Api.MappingConfigurations;
using Avalanche.Api.Services.Media;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models.Media;
using Avalanche.Shared.Infrastructure.Configuration;

using Ism.PgsTimeout.V1.Protos;
using Ism.Routing.V1.Protos;
using Ism.SystemState.Client;
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
    //[TestFixture]
    public class PgsTimeoutManagerTests
    {
        private IMapper _mapper;

        /// <summary>
        /// Represents the current checked state of pgs displays
        /// Used by the state client mock
        /// </summary>
        private PgsDisplayStateData _pgsDisplayStateData;

        [Test]
        public async Task Pgs_StartPgs_VideoRoutes()
        {
            // Arrange
            var currentRoutes = new Dictionary<AliasIndexMessage, AliasIndexMessage>();
            var pgsTimeoutManager = GetTestPgsTimeoutManager(currentRoutes, out var routingService, out var pgsTimeoutService);

            // Act
            // starts pgs and ensures that route is called on the displays
            await pgsTimeoutManager.StartPgs();

            // internally, pgs should be routed to all displays
            var pgsConfig = GetConfig().PgsConfig;
            var request = new RouteVideoBatchRequest();
            request.Routes.AddRange(pgsConfig.Sinks.Select(x => new RouteVideoRequest
            {
                Source = _mapper.Map<AliasIndexModel, AliasIndexMessage>(pgsConfig.Source),
                Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(x)
            }));

            // Assert route video was called
            routingService.Verify(x => x.RouteVideoBatch(request), Times.Once());
            // Assert the player started playing
            pgsTimeoutService.Verify(x => x.SetPgsTimeoutMode(new SetPgsTimeoutModeRequest { Mode = PgsTimeoutModeEnum.PgsTimeoutModePgs }), Times.Once());
        }

        [Test]
        public async Task Pgs_StartPgsUncheckedDisplay_VideoRoutes()
        {
            // Arrange
            var currentRoutes = new Dictionary<AliasIndexMessage, AliasIndexMessage>();
            var pgsTimeoutManager = GetTestPgsTimeoutManager(currentRoutes, out var routingService, out var pgsTimeoutService);

            // Act
            // unchecks pgs on the first display and verifies that it is not routed to
            var pgsConfig = GetConfig().PgsConfig;
            // uncheck pgs on the first display
            await pgsTimeoutManager.SetPgsStateForSink(new PgsSinkStateViewModel
            {
                Enabled = false,
                Sink = _mapper.Map<AliasIndexModel, AliasIndexViewModel>(pgsConfig.Sinks.First())
            });
            // start pgs
            await pgsTimeoutManager.StartPgs();

            // internally, pgs should be routed to all but the first display
            // Skip(1) means the first display should have been skipped
            var request = new RouteVideoBatchRequest();
            request.Routes.AddRange(pgsConfig.Sinks.Skip(1).Select(x => new RouteVideoRequest
            {
                Source = _mapper.Map<AliasIndexModel, AliasIndexMessage>(pgsConfig.Source),
                Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(x)
            }));

            // Assert
            routingService.Verify(x => x.RouteVideoBatch(request), Times.Once());
            pgsTimeoutService.Verify(x => x.SetPgsTimeoutMode(new SetPgsTimeoutModeRequest { Mode = PgsTimeoutModeEnum.PgsTimeoutModePgs }), Times.Once());
        }

        [Test]
        public void Pgs_UnCheckInvalidDisplay_Throws()
        {
            // Arrange
            var currentRoutes = new Dictionary<AliasIndexMessage, AliasIndexMessage>();
            var pgsTimeoutManager = GetTestPgsTimeoutManager(currentRoutes, out var routingService, out var pgsTimeoutService);


            // Act
            // Assert
            // changes the checked state of a display that does not exist and ensures it throws
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await pgsTimeoutManager.SetPgsStateForSink(new PgsSinkStateViewModel
                {
                    Enabled = false,
                    Sink = new AliasIndexViewModel { Alias = "foobar", Index = "Does not exist" }
                });
            });
        }

        [Test]
        public async Task PgsTimeout_StartPgsThenStartTimeoutThenStopBoth_VideoRoutes()
        {
            // Arrange
            var currentRoutes = new Dictionary<AliasIndexMessage, AliasIndexMessage>();
            var pgsTimeoutManager = GetTestPgsTimeoutManager(currentRoutes, out var routingService, out var pgsTimeoutService);

            // Act
            // this test starts pgs, then starts timeout, then stops timeout in the same manner that navigating to the video tab does
            // it then ensures that the room is in idle mode
            var pgsConfig = GetConfig().PgsConfig;
            // uncheck pgs on the first display
            await pgsTimeoutManager.SetPgsStateForSink(new PgsSinkStateViewModel
            {
                Enabled = false,
                Sink = _mapper.Map<AliasIndexModel, AliasIndexViewModel>(pgsConfig.Sinks.First())
            });

            // start pgs
            await pgsTimeoutManager.StartPgs();
            var routePgsRequest = new RouteVideoBatchRequest();

            routePgsRequest.Routes.AddRange(pgsConfig.Sinks.Skip(1).Select(x => new RouteVideoRequest
            {
                Source = _mapper.Map<AliasIndexModel, AliasIndexMessage>(pgsConfig.Source),
                Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(x)
            }));

            // Assert pgs was started and routed to all but the first display
            routingService.Verify(x => x.RouteVideoBatch(routePgsRequest), Times.Once());
            pgsTimeoutService.Verify(x => x.SetPgsTimeoutMode(new SetPgsTimeoutModeRequest { Mode = PgsTimeoutModeEnum.PgsTimeoutModePgs }));


            // start timeout
            await pgsTimeoutManager.StartTimeout();

            // internally, timeout should be routed to all displays
            var timeoutConfig = GetConfig().TimeoutConfig;
            var routeTimeoutRequest = new RouteVideoBatchRequest();
            routeTimeoutRequest.Routes.AddRange(timeoutConfig.Sinks.Select(x => new RouteVideoRequest
            {
                Source = _mapper.Map<AliasIndexModel, AliasIndexMessage>(timeoutConfig.Source),
                Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(x)
            }));

            // assert that timeout was started and routed everywhere
            routingService.Verify(x => x.RouteVideoBatch(routeTimeoutRequest), Times.Once());
            pgsTimeoutService.Verify(x => x.SetPgsTimeoutMode(new SetPgsTimeoutModeRequest { Mode = PgsTimeoutModeEnum.PgsTimeoutModeTimeout }));

            // stop timeout by going to the routing tab
            // should stop both and put the room back in idle mode
            await pgsTimeoutManager.StopPgsAndTimeout();

            // tests have no saved routes so restoring the route should do nothing
            routingService.Verify(x => x.RouteVideoBatch(new RouteVideoBatchRequest()));
            // pgs player is almost always in pgs mode
            pgsTimeoutService.Verify(x => x.SetPgsTimeoutMode(new SetPgsTimeoutModeRequest { Mode = PgsTimeoutModeEnum.PgsTimeoutModePgs }));
        }

        [Test]
        public async Task Timeout_StartTimeout_VideoRoutes()
        {
            // Arrange
            var currentRoutes = new Dictionary<AliasIndexMessage, AliasIndexMessage>();
            var pgsTimeoutManager = GetTestPgsTimeoutManager(currentRoutes, out var routingService, out var pgsTimeoutService);

            // Act
            // starts timeout and ensures that route is called on the displays
            await pgsTimeoutManager.StartTimeout();

            // internally, timeout should be routed to all displays
            var timeoutConfig = GetConfig().TimeoutConfig;
            var request = new RouteVideoBatchRequest();
            request.Routes.AddRange(timeoutConfig.Sinks.Select(x => new RouteVideoRequest
            {
                Source = _mapper.Map<AliasIndexModel, AliasIndexMessage>(timeoutConfig.Source),
                Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(x)
            }));

            // Assert route video was called
            routingService.Verify(x => x.RouteVideoBatch(request), Times.Once());
            // Assert the player started playing
            pgsTimeoutService.Verify(x => x.SetPgsTimeoutMode(new SetPgsTimeoutModeRequest { Mode = PgsTimeoutModeEnum.PgsTimeoutModeTimeout }), Times.Once());
        }

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

        private PgsTimeoutManager GetTestPgsTimeoutManager(Dictionary<AliasIndexMessage, AliasIndexMessage> currentRoutes, out Mock<IRoutingService> routingService, out Mock<IPgsTimeoutService> pgsTimeoutService)
        {
            routingService = new Mock<IRoutingService>();
            pgsTimeoutService = new Mock<IPgsTimeoutService>();
            var stateClient = new Mock<IStateClient>();
            

            // set up state client so we can add/get the pgs display state data
            // for the test, only need to fake the add
            stateClient.Setup(x => x.AddOrUpdateData(It.IsAny<PgsDisplayStateData>(), It.IsAny<Action<JsonPatchDocument<PgsDisplayStateData>>>()))
            .Callback<PgsDisplayStateData, Action<JsonPatchDocument<PgsDisplayStateData>>>((add, update) =>
            {
                _pgsDisplayStateData = add;
            });
            stateClient.Setup(x => x.GetData<PgsDisplayStateData>()).ReturnsAsync(() =>
            {
                return _pgsDisplayStateData;
            });


            // routing service needs to do a bit of work
            // route and route batch need to maintain some level of state
            routingService.Setup(x => x.RouteVideoBatch(It.IsAny<RouteVideoBatchRequest>())).Callback<RouteVideoBatchRequest>(x =>
            {
                foreach (var route in x.Routes)
                    currentRoutes[route.Sink] = route.Source;
            });
            routingService.Setup(x => x.RouteVideo(It.IsAny<RouteVideoRequest>())).Callback<RouteVideoRequest>(x =>
            {
                currentRoutes[x.Sink] = x.Source;
            });
            routingService.Setup(x => x.GetCurrentRoutes()).ReturnsAsync(() =>
            {
                var response = new GetCurrentRoutesResponse();
                response.Routes.AddRange(currentRoutes.Select(kv => new VideoRouteMessage { Sink = kv.Key, Source = kv.Value }));
                return response;
            });

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MediaMappingConfiguration());
                cfg.AddProfile(new RoutingMappingConfiguration());
            });

            _mapper = mapperConfig.CreateMapper();

            var (PgsConfig, TimeoutConfig) = GetConfig();

            return new PgsTimeoutManager(
                routingService.Object,
                stateClient.Object,
                pgsTimeoutService.Object,
                _mapper,
                PgsConfig,
                TimeoutConfig);
        }
    }
}
