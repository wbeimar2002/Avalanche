using AutoMapper;
using Avalanche.Api.Managers.Devices;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using AvidisDeviceInterface.V1.Protos;
using Castle.Core.Configuration;
using Google.Protobuf.WellKnownTypes;
using Ism.Common.Core.Configuration.Models;
using Ism.Recorder.Core.V1.Protos;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Api.Tests.Managers
{
    [TestFixture()]
    public partial class DeviceManagerTests
    {
        [Test]
        public void StartRecordingShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = CommandTypes.StartRecording,
                Devices = new List<VideoDevice>() { new VideoDevice() { Id = new AliasIndexApiModel("TP1", 0) } }
            };

            var commandResponse = _manager.SendCommand(commandViewModel);

            _recorderService.Verify(mock => mock.StartRecording(It.IsAny<RecordMessage>()), Times.Once);

            Assert.IsNotNull(commandResponse);
        }

        [Test]
        public void StopRecordingShouldReturnResponse()
        {
            CommandViewModel commandViewModel = new CommandViewModel()
            {
                CommandType = CommandTypes.StopRecording,
                Devices = new List<VideoDevice>() { new VideoDevice() { Id = new AliasIndexApiModel("TP1", 0) } }
            };

            var commandResponse = _manager.SendCommand(commandViewModel);

            _recorderService.Verify(mock => mock.StopRecording(), Times.Once);

            Assert.IsNotNull(commandResponse);
        }
    }
}
