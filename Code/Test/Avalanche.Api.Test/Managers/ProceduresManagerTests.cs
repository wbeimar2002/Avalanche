using AutoMapper;
using Avalanche.Api.Managers.Media;
using Avalanche.Api.Managers.Procedures;
using Avalanche.Api.MappingConfigurations;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Ism.SystemState.Client;
using Ism.SystemState.Models.Procedure;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Avalanche.Api.Test.Managers
{
    [TestFixture()]
    public class ProceduresManagerTests
    {
        IMapper _mapper;
        Mock<IAccessInfoFactory> _accessInfoFactory;
        Mock<ILibraryService> _libraryService;
        Mock<IRecorderService> _recorderService;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ProceduresMappingConfiguration());
            });

            _mapper = config.CreateMapper();
            _accessInfoFactory = new Mock<IAccessInfoFactory>();
            _libraryService = new Mock<ILibraryService>();
            _recorderService = new Mock<IRecorderService>();
        }

        [Test]
        public async Task TestGetActiveProcedureReturnsProcedure()
        {
            var stateClient = new Mock<IStateClient>();

            stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(
                new ActiveProcedureState(
                    new Patient() { LastName = "name" },
                    new List<ProcedureImage>(),
                    new List<ProcedureVideo>(),
                    "libId",
                    "repId",
                    "path",
                    null,
                    null,
                    null,
                    false,
                    DateTimeOffset.UtcNow,
                    TimeZoneInfo.Local.Id,
                    false,
                    new Dictionary<string, string>(),
                    null, 
                    null, 
                    null,
                    new List<VideoRecordingEvent>()));

            var manager = new ProceduresManager(stateClient.Object, _libraryService.Object, _accessInfoFactory.Object, _mapper, _recorderService.Object);

            _recorderService.Setup(mock => mock.GetRecorderState()).ReturnsAsync(new Ism.Recorder.Core.V1.Protos.RecorderState() { State = 0 });

            var result = await manager.GetActiveProcedure();

            Assert.NotNull(result);
            Assert.AreEqual("name", result.Patient?.LastName);
        }

        [Test]
        public async Task TestDeleteActiveProcedureMediaSucceedsIfValid()
        {
            var stateClient = new Mock<IStateClient>();

            var id = Guid.NewGuid();
            stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(
                new ActiveProcedureState(
                    new Patient() { LastName = "name" },
                    new List<ProcedureImage>() { new ProcedureImage(id, "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow) },
                    new List<ProcedureVideo>(),
                    "libId",
                    "repId",
                    "path",
                    null,
                    null,
                    null,
                    false,
                    DateTimeOffset.UtcNow,
                    TimeZoneInfo.Local.Id,
                    false,
                    new Dictionary<string, string>(),
                    null,
                    null,
                    null,
                    new List<VideoRecordingEvent>()));

            var manager = new ProceduresManager(stateClient.Object, _libraryService.Object, _accessInfoFactory.Object, _mapper, _recorderService.Object);

            await manager.DeleteActiveProcedureMedia(Shared.Domain.Enumerations.ProcedureContentType.Image, id);
        }

        [Test]
        public void TestDeleteActiveProcedureMediaFailsIfVideoIsRecording()
        {
            var stateClient = new Mock<IStateClient>();

            var id = Guid.NewGuid();
            stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(
                new ActiveProcedureState(
                    new Patient() { LastName = "name" },
                    new List<ProcedureImage>(),
                    new List<ProcedureVideo>() { new ProcedureVideo(id, "source", "channel", "path", "path", DateTimeOffset.UtcNow, null, TimeSpan.FromSeconds(1)) },
                    "libId",
                    "repId",
                    "path",
                    null,
                    null,
                    null,
                    false,
                    DateTimeOffset.UtcNow,
                    TimeZoneInfo.Local.Id,
                    false,
                    new Dictionary<string, string>(),
                    null,
                    null,
                    null,
                    new List<VideoRecordingEvent>()));

            var manager = new ProceduresManager(stateClient.Object, _libraryService.Object, _accessInfoFactory.Object, _mapper, _recorderService.Object);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => manager.DeleteActiveProcedureMedia(Shared.Domain.Enumerations.ProcedureContentType.Video, id));
            Assert.True(ex.Message.Contains("Can not delete video that is currently recording"));
        }

    }
}
