using AutoMapper;
using Avalanche.Api.Managers.Devices;
using Avalanche.Api.MappingConfigurations;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Avalanche.Api.Tests.Managers
{
    [TestFixture()]
    public partial class DeviceManagerTests
    {
        Mock<IAvidisService> _avidisService;
        Mock<IRecorderService> _recorderService;
        Mock<IMediaService> _mediaService;
        Mock<IStorageService> _storageService;
        Mock<IRoutingService> _routingService;
        Mock<ILogger<MediaManager>> _appLoggerService;
        Mock<IAccessInfoFactory> _accessInfoFactory;

        IMapper _mapper;
        MapperConfiguration _mapperConfiguration;

        DevicesManager _manager;

        [SetUp]
        public void Setup()
        {
            _mediaService = new Mock<IMediaService>();
            _storageService = new Mock<IStorageService>();
            _appLoggerService = new Mock<ILogger<MediaManager>>();
            _avidisService = new Mock<IAvidisService>();
            _recorderService = new Mock<IRecorderService>();
            _routingService = new Mock<IRoutingService>();
            _accessInfoFactory = new Mock<IAccessInfoFactory>();
        }

        [SetUp]
        public void SetUp()
        {
            _mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new HealthMappingConfigurations());
                cfg.AddProfile(new RoutingMappingConfigurations());
                cfg.AddProfile(new MediaMappingConfigurations());
            });

            _mapper = _mapperConfiguration.CreateMapper();

            Mock<IHttpContextAccessor> mockAccessor = new Mock<IHttpContextAccessor>();
            mockAccessor.Setup(m => m.HttpContext.Request.Host).Returns(new HostString("localhost", 5000));

            _manager = new DevicesManager(_mediaService.Object, _routingService.Object, _appLoggerService.Object,
                _avidisService.Object, _recorderService.Object, _accessInfoFactory.Object, _mapper, mockAccessor.Object, _storageService.Object);
        }

        [Test]
        public void PgsExecutePlayVideoShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = CommandTypes.PgsPlayVideo,
                Message = "SampleMessage",
                Type = "OfferType",
                Devices = new List<Device>() { new Device() { Id = "Preview", Name = string.Empty, Type = string.Empty } },
                Destinations = new List<Device>(),
                AdditionalInfo = Guid.NewGuid().ToString()
            };

            var response = new Ism.Streaming.V1.Protos.InitSessionResponse();
            response.Answer.Add(new Ism.Streaming.V1.Protos.WebRtcInfoMessage()
            {
                Message = "Success response"
            });

            Ism.IsmLogCommon.Core.AccessInfo accessInfo = new Ism.IsmLogCommon.Core.AccessInfo("10.0.75.1", "UnitTesting", "UnitTesting", "UnitTesting", "UnitTesting");

            _accessInfoFactory.Setup(mock => mock.GenerateAccessInfo(null)).Returns(accessInfo);
            _mediaService.Setup(mock => mock.InitSessionAsync(It.IsAny<Ism.Streaming.V1.Protos.InitSessionRequest>())).ReturnsAsync(response);

            var commandResponse = _manager.SendCommand(commandViewModel);

            _mediaService.Verify(mock => mock.SetPgsTimeoutModeAsync(It.Is<Ism.PgsTimeout.V1.Protos.SetPgsTimeoutModeRequest>(args => (int)args.Mode == (int)TimeoutModes.Pgs)), Times.Once);
            _mediaService.Verify(mock => mock.InitSessionAsync(It.IsAny<Ism.Streaming.V1.Protos.InitSessionRequest>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void PgsExecuteStopVideoShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = CommandTypes.PgsStopVideo,
                Message = "SampleMessage",
                Type = "OfferType",
                Devices = new List<Device>() { new Device() { Id = "Preview", Name = string.Empty, Type = string.Empty } },
                Destinations = new List<Device>(),
                AdditionalInfo = Guid.NewGuid().ToString()
            };

            Ism.IsmLogCommon.Core.AccessInfo accessInfo = new Ism.IsmLogCommon.Core.AccessInfo("10.0.75.1", "UnitTesting", "UnitTesting", "UnitTesting", "UnitTesting");

            _accessInfoFactory.Setup(mock => mock.GenerateAccessInfo(null)).Returns(accessInfo);
            _mediaService.Setup(mock => mock.DeInitSessionAsync(It.IsAny<Ism.Streaming.V1.Protos.DeInitSessionRequest>()));

            var commandResponse = _manager.SendCommand(commandViewModel);

            _mediaService.Verify(mock => mock.DeInitSessionAsync(It.IsAny<Ism.Streaming.V1.Protos.DeInitSessionRequest>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void PgsExecuteHandleMessageShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = CommandTypes.PgsHandleMessageForVideo,
                Message = "SampleMessage",
                Type = "OfferType",
                Devices = new List<Device>() { new Device() { Id = "Preview", Name = string.Empty, Type = string.Empty } },
                Destinations = new List<Device>(),
                AdditionalInfo = Guid.NewGuid().ToString()
            };

            var commandResponse = _manager.SendCommand(commandViewModel);

            _mediaService.Verify(mock => mock.HandleMessageAsync(It.IsAny<Ism.Streaming.V1.Protos.HandleMessageRequest>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }
    }
}
