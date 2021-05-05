using AutoMapper;
using Avalanche.Api.Managers.Media;
using Avalanche.Api.MappingConfigurations;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Shared.Domain.Models.Media;
using Ism.Routing.V1.Protos;
using Microsoft.AspNetCore.Http;
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
        Mock<IAvidisService> _avidisService;
        Mock<IStorageService> _storageService;
        Mock<IHttpContextAccessor> _httpContextAccessor;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new RoutingMappingConfiguration());
                cfg.AddProfile(new HealthMappingConfiguration());
                cfg.AddProfile(new MediaMappingConfiguration());
            });

            _mapper = config.CreateMapper();
            _accessInfoFactory = new Mock<IAccessInfoFactory>();
            _routingService = new Mock<IRoutingService>();
            _avidisService = new Mock<IAvidisService>();
            _storageService = new Mock<IStorageService>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
        }

        [Test]
        public async Task TestSetDisplayRecordingEnabledTriggersRoute()
        {
            _routingService.Setup(r => r.GetRouteForSink(It.IsAny<GetRouteForSinkRequest>()))
                .ReturnsAsync(new GetRouteForSinkResponse { Route = new VideoRouteMessage { Source = new AliasIndexMessage { Alias = "source", Index = "a" } } })
                .Verifiable();

            _routingService.Setup(r => r.RouteVideo(It.IsAny<RouteVideoRequest>()))
                .Verifiable();

            var manager = new RoutingManager(_routingService.Object, _avidisService.Object, _storageService.Object, _mapper, _httpContextAccessor.Object);

            await manager.SetDisplayRecordingEnabled(new ViewModels.DisplayRecordingViewModel
            {
                Display = new SinkModel { Alias = "one", Index = "two" },
                RecordChannel = new RecordingChannelModel { ChannelName = "channel", VideoSink = new SinkModel { Alias = "three", Index = "four" } },
                Enabled = true
            });

            _routingService.Verify(r => r.GetRouteForSink(It.Is<GetRouteForSinkRequest>(req => req.Sink.Alias == "one" && req.Sink.Index == "two")), Times.Once);
            _routingService.Verify(r => r.RouteVideo(It.Is<RouteVideoRequest>(req => req.Sink.Alias == "three" && req.Sink.Index == "four" && req.Source.Alias == "source" && req.Source.Index == "a")), Times.Once);
        }

        [Test]
        public async Task TestSetDisplayRecordingEnabledClearsRouteIfSetToDisable()
        {
            _routingService.Setup(r => r.GetRouteForSink(It.IsAny<GetRouteForSinkRequest>()))
                .ReturnsAsync(new GetRouteForSinkResponse { Route = new VideoRouteMessage { Source = new AliasIndexMessage { Alias = "source", Index = "a" } } })
                .Verifiable();

            _routingService.Setup(r => r.RouteVideo(It.IsAny<RouteVideoRequest>()))
                .Verifiable();

            var manager = new RoutingManager(_routingService.Object, _avidisService.Object, _storageService.Object, _mapper, _httpContextAccessor.Object);

            await manager.SetDisplayRecordingEnabled(new ViewModels.DisplayRecordingViewModel
            {
                Display = new SinkModel { Alias = "one", Index = "two" },
                RecordChannel = new RecordingChannelModel { ChannelName = "channel", VideoSink = new SinkModel { Alias = "three", Index = "four" } },
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

            var manager = new RoutingManager(_routingService.Object, _avidisService.Object, _storageService.Object, _mapper, _httpContextAccessor.Object);

            await manager.SetDisplayRecordingEnabled(new ViewModels.DisplayRecordingViewModel
            {
                Display = new SinkModel { Alias = "one", Index = "two" },
                RecordChannel = new RecordingChannelModel { ChannelName = "channel", VideoSink = new SinkModel { Alias = "three", Index = "four" } },
                Enabled = true
            }); ;

            _routingService.Verify(r => r.GetRouteForSink(It.Is<GetRouteForSinkRequest>(req => req.Sink.Alias == "one" && req.Sink.Index == "two")), Times.Once);
            _routingService.Verify(r => r.RouteVideo(It.Is<RouteVideoRequest>(req => req.Sink.Alias == "three" && req.Sink.Index == "four" && string.IsNullOrEmpty(req.Source.Alias) && string.IsNullOrEmpty(req.Source.Index))), Times.Once);

        }

    }
}
