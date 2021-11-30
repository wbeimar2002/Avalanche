using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Managers.Data;
using Avalanche.Api.Managers.Procedures;
using Avalanche.Api.Mapping;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Configuration;
using Ism.SystemState.Client;
using Ism.SystemState.Models.Procedure;
using Moq;
using NUnit.Framework;

namespace Avalanche.Api.Test.Managers
{
    [TestFixture()]
    public class ActiveProcedureManagerTests
    {
        IMapper _mapper;
        Mock<IAccessInfoFactory> _accessInfoFactory;
        Mock<ILibraryService> _libraryService;
        Mock<IRecorderService> _recorderService;
        Mock<IStateClient> _stateClient;
        Mock<IDataManager> _dataManager;
        GeneralApiConfiguration _generalApiConfig;

        ActiveProcedureManager _manager;

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
            _stateClient = new Mock<IStateClient>();
            _dataManager = new Mock<IDataManager>();

            _generalApiConfig = new GeneralApiConfiguration();

            _manager = new ActiveProcedureManager(_stateClient.Object, _libraryService.Object, _accessInfoFactory.Object, _mapper, _recorderService.Object, _dataManager.Object, _generalApiConfig);
        }

        [Test]
        public async Task TestGetActiveProcedureReturnsProcedure()
        {
            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(
                new ActiveProcedureState(
                    new Patient() { LastName = "name" },
                    new List<ProcedureImage>(),
                    new List<ProcedureVideo>(),
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
                    new List<ProcedureNote>(),
                    null,
                    new List<VideoRecordingEvent>(),
                    BackgroundRecordingMode.StartImmediately));

            _recorderService.Setup(mock => mock.GetRecorderState()).ReturnsAsync(new Ism.Recorder.Core.V1.Protos.RecorderState() { State = 0 });

            var result = await _manager.GetActiveProcedure();

            Assert.NotNull(result);
            Assert.AreEqual("name", result.Patient?.LastName);
        }

        [Test]
        public async Task TestDeleteActiveProcedureMediaSucceedsIfValid()
        {
            var id = Guid.NewGuid();
            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(
                new ActiveProcedureState(
                    new Patient() { LastName = "name" },
                    new List<ProcedureImage>() { new ProcedureImage(id, "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow, Guid.NewGuid()) },
                    new List<ProcedureVideo>(),
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
                    new List<ProcedureNote>(),
                    null,
                    new List<VideoRecordingEvent>(),
                    BackgroundRecordingMode.StartImmediately));

            await _manager.DeleteActiveProcedureMediaItem(Shared.Domain.Enumerations.ProcedureContentType.Image, id);
        }

        [Test]
        public void TestDeleteActiveProcedureMediaFailsIfVideoIsRecording()
        {
            var id = Guid.NewGuid();
            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(
                new ActiveProcedureState(
                    new Patient() { LastName = "name" },
                    new List<ProcedureImage>(),
                    new List<ProcedureVideo>() { new ProcedureVideo(id, "source", "channel", "path", "path", DateTimeOffset.UtcNow, null, TimeSpan.FromSeconds(1), Guid.NewGuid()) },
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
                    new List<ProcedureNote>(),
                    null,
                    new List<VideoRecordingEvent>(),
                    BackgroundRecordingMode.StartImmediately));

            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _manager.DeleteActiveProcedureMediaItem(Shared.Domain.Enumerations.ProcedureContentType.Video, id));
            Assert.True(ex.Message.Contains("Cannot delete video that is currently recording"));
        }

        [Test]
        public async Task TestDeleteActiveProcedureMediaItemsSucceedsIfValid()
        {
            var imageId = Guid.NewGuid();
            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(
                new ActiveProcedureState(
                    new Patient() { LastName = "name" },
                    new List<ProcedureImage>() { new ProcedureImage(imageId, "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow, Guid.NewGuid()) },
                    new List<ProcedureVideo>(),
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
                    new List<ProcedureNote>(),
                    null,
                    new List<VideoRecordingEvent>(),
                    BackgroundRecordingMode.StartImmediately));

            await _manager.DeleteActiveProcedureMediaItems(Shared.Domain.Enumerations.ProcedureContentType.Image, new List<Guid>() { imageId });
        }

        [Test]
        public void TestDeleteActiveProcedureMediaItemsFailsIfVideoIsRecording()
        {
            var videoId = Guid.NewGuid();
            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(
                new ActiveProcedureState(
                    new Patient() { LastName = "name" },
                    new List<ProcedureImage>(),
                    new List<ProcedureVideo>() { new ProcedureVideo(videoId, "source", "channel", "path", "path", DateTimeOffset.UtcNow, null, TimeSpan.FromSeconds(1), Guid.NewGuid()) },
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
                    new List<ProcedureNote>(),
                    null,
                    new List<VideoRecordingEvent>(),
                    BackgroundRecordingMode.StartImmediately));

            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _manager.DeleteActiveProcedureMediaItems(Shared.Domain.Enumerations.ProcedureContentType.Video, new List<Guid>() { videoId }));
            Assert.True(ex.Message.Contains("Cannot delete video that is currently recording"));
        }

        [Test]
        public async Task ProcedureManager_ApplyAdhocLabelWhenEnabledToProcedureImage_Succeeds()
        {
            var id = Guid.NewGuid();

            var labelContent = new ContentViewModel
            {
                ContentId = id,
                Label = "AdhocLabelA",
                ProcedureContentType = Shared.Domain.Enumerations.ProcedureContentType.Image
            };

            var labelModel = new LabelModel
            {
                Id = 1,
                ProcedureTypeId = 1,
                Name = "AdhocLabelA"
            };

            var activeProcedure = new ActiveProcedureState(
                    new Patient() { LastName = "name" },
                    new List<ProcedureImage>() { new ProcedureImage(id, "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow, Guid.NewGuid()) },
                    new List<ProcedureVideo>(),
                    new List<ProcedureVideo>(),
                    "libId",
                    "repId",
                    "path",
                    null,
                    new ProcedureType() { Id = 1, Name = "TestProceType" },
                    null,
                    false,
                    DateTimeOffset.UtcNow,
                    TimeZoneInfo.Local.Id,
                    false,
                    new List<ProcedureNote>(),
                    null,
                    new List<VideoRecordingEvent>(),
                    BackgroundRecordingMode.StartImmediately);

            //arrange
            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);

            _dataManager.Setup(d => d.AddLabel(labelModel)).ReturnsAsync(labelModel);
            _dataManager.Setup(d => d.GetLabel(It.IsAny<string>(), It.IsAny<Int32>())).ReturnsAsync(labelModel);

            _generalApiConfig = new GeneralApiConfiguration
            {
                AdHocLabelsAllowed = true
            };

            _recorderService.Setup(mock => mock.GetRecorderState()).ReturnsAsync(new Ism.Recorder.Core.V1.Protos.RecorderState() { State = 0 });

            //act
            await _manager.ApplyLabelToActiveProcedure(labelContent);
            var result = await _manager.GetActiveProcedure();

            //assert
            Assert.NotNull(result);
            Assert.AreEqual("AdhocLabelA", result.Images[0].Label);
        }

        [Test]
        public async Task ProcedureManager_ApplyAdhocLabelWhenEnabledToProcedureVideo_Succeeds()
        {
            var id = Guid.NewGuid();

            var labelContent = new ContentViewModel
            {
                ContentId = id,
                Label = "AdhocLabelB",
                ProcedureContentType = Shared.Domain.Enumerations.ProcedureContentType.Video
            };

            var labelModel = new LabelModel
            {
                Id = 1,
                ProcedureTypeId = 1,
                Name = "AdhocLabelB"
            };

            var activeProcedure = new ActiveProcedureState(
                    new Patient() { LastName = "name" },
                    new List<ProcedureImage>(),
                    new List<ProcedureVideo>() { new ProcedureVideo(id, "source", "channel", "path", "path", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddSeconds(10), TimeSpan.FromSeconds(10), Guid.NewGuid()) },
                    new List<ProcedureVideo>(),
                    "libId",
                    "repId",
                    "path",
                    null,
                    new ProcedureType() { Id = 1, Name = "TestProceType" },
                    null,
                    false,
                    DateTimeOffset.UtcNow,
                    TimeZoneInfo.Local.Id,
                    false,
                    new List<ProcedureNote>(),
                    null,
                    new List<VideoRecordingEvent>(),
                    BackgroundRecordingMode.StartImmediately);

            //arrange
            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);

            _dataManager.Setup(d => d.AddLabel(labelModel)).ReturnsAsync(labelModel);
            _dataManager.Setup(d => d.GetLabel(It.IsAny<string>(), It.IsAny<Int32>())).ReturnsAsync(labelModel);

            _generalApiConfig = new GeneralApiConfiguration
            {
                AdHocLabelsAllowed = true
            };

            _recorderService.Setup(mock => mock.GetRecorderState()).ReturnsAsync(new Ism.Recorder.Core.V1.Protos.RecorderState() { State = 0 });

            //act
            await _manager.ApplyLabelToActiveProcedure(labelContent);
            var result = await _manager.GetActiveProcedure();

            //assert
            Assert.NotNull(result);
            Assert.AreEqual("AdhocLabelB", result.Videos[0].Label);
        }

        [Test]
        public async Task ProcedureManager_ApplyAdhocLabelWhenEnabled_AddingLabelToStore_Fails()
        {
            var id = Guid.NewGuid();

            var labelContent = new ContentViewModel
            {
                ContentId = id,
                Label = "AdhocLabelA",
                ProcedureContentType = Shared.Domain.Enumerations.ProcedureContentType.Image
            };

            var labelModel = new LabelModel
            {
                ProcedureTypeId = null,
                Name = ""
            };

            var activeProcedure = new ActiveProcedureState(
                    new Patient() { LastName = "name" },
                    new List<ProcedureImage>() { new ProcedureImage(id, "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow, Guid.NewGuid()) },
                    new List<ProcedureVideo>(),
                    new List<ProcedureVideo>(),
                    "libId",
                    "repId",
                    "path",
                    null,
                    new ProcedureType() { Id = 1, Name = "TestProceType" },
                    null,
                    false,
                    DateTimeOffset.UtcNow,
                    TimeZoneInfo.Local.Id,
                    false,
                    new List<ProcedureNote>(),
                    null,
                    new List<VideoRecordingEvent>(),
                    BackgroundRecordingMode.StartImmediately);

            //arrange
            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);

            _dataManager.Setup(d => d.AddLabel(labelModel)).ReturnsAsync(labelModel);
            _dataManager.Setup(d => d.GetLabel(It.IsAny<string>(), It.IsAny<Int32>())).ReturnsAsync(labelModel);

            _generalApiConfig = new GeneralApiConfiguration
            {
                AdHocLabelsAllowed = true
            };

            _recorderService.Setup(mock => mock.GetRecorderState()).ReturnsAsync(new Ism.Recorder.Core.V1.Protos.RecorderState() { State = 0 });

            //act
            Task Act() => _manager.ApplyLabelToActiveProcedure(labelContent);

            //assert
            Assert.That(Act, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public async Task ProcedureManager_ApplyExistingLabelToProcedureImage_Succeeds()
        {
            var id = Guid.NewGuid();

            var labelContent = new ContentViewModel
            {
                ContentId = id,
                Label = "LabelA",
                ProcedureContentType = Shared.Domain.Enumerations.ProcedureContentType.Image
            };

            var labelModel = new LabelModel
            {
                Id = 1,
                ProcedureTypeId = 1,
                Name = "LabelA"
            };

            var activeProcedure = new ActiveProcedureState(
                    new Patient() { LastName = "name" },
                    new List<ProcedureImage>() { new ProcedureImage(id, "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow, Guid.NewGuid()) },
                    new List<ProcedureVideo>(),
                    new List<ProcedureVideo>(),
                    "libId",
                    "repId",
                    "path",
                    null,
                    new ProcedureType() { Id = 1, Name = "TestProceType" },
                    null,
                    false,
                    DateTimeOffset.UtcNow,
                    TimeZoneInfo.Local.Id,
                    false,
                    new List<ProcedureNote>(),
                    null,
                    new List<VideoRecordingEvent>(),
                    BackgroundRecordingMode.StartImmediately);

            //arrange
            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);

            _dataManager.Setup(d => d.GetLabel(It.IsAny<string>(), It.IsAny<Int32>())).ReturnsAsync(labelModel);

            _generalApiConfig = new GeneralApiConfiguration
            {
                AdHocLabelsAllowed = true
            };

            _recorderService.Setup(mock => mock.GetRecorderState()).ReturnsAsync(new Ism.Recorder.Core.V1.Protos.RecorderState() { State = 0 });

            //act
            await _manager.ApplyLabelToActiveProcedure(labelContent);
            var result = await _manager.GetActiveProcedure();

            //assert
            Assert.NotNull(result);
            Assert.AreEqual("LabelA", result.Images[0].Label);
        }

        [Test]
        public async Task ProcedureManager_ApplyExistingLabelToProcedureVideo_Succeeds()
        {
            var id = Guid.NewGuid();

            var labelContent = new ContentViewModel
            {
                ContentId = id,
                Label = "LabelB",
                ProcedureContentType = Shared.Domain.Enumerations.ProcedureContentType.Video
            };

            var labelModel = new LabelModel
            {
                Id = 1,
                ProcedureTypeId = 1,
                Name = "LabelB"
            };

            var activeProcedure = new ActiveProcedureState(
                    new Patient() { LastName = "name" },
                    new List<ProcedureImage>(),
                    new List<ProcedureVideo>() { new ProcedureVideo(id, "source", "channel", "path", "path", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddSeconds(10), TimeSpan.FromSeconds(10), Guid.NewGuid()) },
                    new List<ProcedureVideo>(),
                    "libId",
                    "repId",
                    "path",
                    null,
                    new ProcedureType() { Id = 1, Name = "TestProceType" },
                    null,
                    false,
                    DateTimeOffset.UtcNow,
                    TimeZoneInfo.Local.Id,
                    false,
                    new List<ProcedureNote>(),
                    null,
                    new List<VideoRecordingEvent>(),
                    BackgroundRecordingMode.StartImmediately);

            //arrange
            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);
            _dataManager.Setup(d => d.GetLabel(It.IsAny<string>(), It.IsAny<Int32>())).ReturnsAsync(labelModel);

            _generalApiConfig = new GeneralApiConfiguration
            {
                AdHocLabelsAllowed = true
            };

            _recorderService.Setup(mock => mock.GetRecorderState()).ReturnsAsync(new Ism.Recorder.Core.V1.Protos.RecorderState() { State = 0 });

            //act
            await _manager.ApplyLabelToActiveProcedure(labelContent);
            var result = await _manager.GetActiveProcedure();

            //assert
            Assert.NotNull(result);
            Assert.AreEqual("LabelB", result.Videos[0].Label);
        }

        [Test]
        public async Task ProcedureManager_ApplyLabelToLatestImages_Succeeds()
        {
            //arrange
            const string autolabel = "LabelA";
            var correlationId = Guid.NewGuid();

            var activeProcedure = new ActiveProcedureState(
                    new Patient() { LastName = "name" },
                    new List<ProcedureImage>
                    {
                        new ProcedureImage(Guid.NewGuid(), "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow, correlationId),
                        new ProcedureImage(Guid.NewGuid(), "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow, correlationId),
                        new ProcedureImage(Guid.NewGuid(), "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow, correlationId),
                    },
                    new List<ProcedureVideo>(),
                    new List<ProcedureVideo>(),
                    "libId",
                    "repId",
                    "path",
                    null,
                    new ProcedureType() { Id = 1, Name = "TestProceType" },
                    null,
                    false,
                    DateTimeOffset.UtcNow,
                    TimeZoneInfo.Local.Id,
                    false,
                    new List<ProcedureNote>(),
                    null,
                    new List<VideoRecordingEvent>(),
                    BackgroundRecordingMode.StartImmediately);

            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);
            _recorderService.Setup(mock => mock.GetRecorderState()).ReturnsAsync(new Ism.Recorder.Core.V1.Protos.RecorderState() { State = 0 });

            //act
            await _manager.ApplyLabelToLatestImages(autolabel);
            var result = await _manager.GetActiveProcedure();

            //assert
            Assert.NotNull(result);
            Assert.AreEqual(autolabel, result.Images[0].Label);
            Assert.AreEqual(autolabel, result.Images[1].Label);
            Assert.AreEqual(autolabel, result.Images[2].Label);
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public async Task ProcedureManager_ApplyLabelToLatestImages_LabelNameEmpty_Fails(string label)
        {
            //arrange
            var correlationId = Guid.NewGuid();

            var activeProcedure = new ActiveProcedureState(
                    new Patient() { LastName = "name" },
                    new List<ProcedureImage>
                    {
                        new ProcedureImage(Guid.NewGuid(), "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow, correlationId),
                        new ProcedureImage(Guid.NewGuid(), "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow, correlationId),
                        new ProcedureImage(Guid.NewGuid(), "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow, correlationId),
                    },
                    new List<ProcedureVideo>(),
                    new List<ProcedureVideo>(),
                    "libId",
                    "repId",
                    "path",
                    null,
                    new ProcedureType() { Id = 1, Name = "TestProceType" },
                    null,
                    false,
                    DateTimeOffset.UtcNow,
                    TimeZoneInfo.Local.Id,
                    false,
                    new List<ProcedureNote>(),
                    null,
                    new List<VideoRecordingEvent>(),
                    BackgroundRecordingMode.StartImmediately);

            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);
            _recorderService.Setup(mock => mock.GetRecorderState()).ReturnsAsync(new Ism.Recorder.Core.V1.Protos.RecorderState() { State = 0 });

            //act
            Task Act() => _manager.ApplyLabelToLatestImages(label);

            //assert
            Assert.That(Act, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public async Task ProcedureManager_ApplyLabelToLatestImages_NoImagesExists_Fails()
        {
            //arrange
            const string autolabel = "LabelA";

            var activeProcedure = new ActiveProcedureState(
                    new Patient() { LastName = "name" },
                    new List<ProcedureImage>(),
                    new List<ProcedureVideo>(),
                    new List<ProcedureVideo>(),
                    "libId",
                    "repId",
                    "path",
                    null,
                    new ProcedureType() { Id = 1, Name = "TestProceType" },
                    null,
                    false,
                    DateTimeOffset.UtcNow,
                    TimeZoneInfo.Local.Id,
                    false,
                    new List<ProcedureNote>(),
                    null,
                    new List<VideoRecordingEvent>(),
                    BackgroundRecordingMode.StartImmediately);

            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);
            _recorderService.Setup(mock => mock.GetRecorderState()).ReturnsAsync(new Ism.Recorder.Core.V1.Protos.RecorderState() { State = 0 });

            //act
            Task Act() => _manager.ApplyLabelToLatestImages(autolabel);

            //assert
            Assert.That(Act, Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public async Task ProcedureManager_ApplyLabelToLatestImages_WithDifferentCorrelationIds_Succeeds()
        {
            //arrange
            const string autolabel = "LabelA";
            var activeProcedure = new ActiveProcedureState(
                    new Patient() { LastName = "name" },
                    new List<ProcedureImage>
                    {
                        new ProcedureImage(Guid.NewGuid(), "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow.AddSeconds(-2), Guid.NewGuid()),
                        new ProcedureImage(Guid.NewGuid(), "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow.AddSeconds(-1), Guid.NewGuid()),
                        new ProcedureImage(Guid.NewGuid(), "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow, Guid.NewGuid()),
                    },
                    new List<ProcedureVideo>(),
                    new List<ProcedureVideo>(),
                    "libId",
                    "repId",
                    "path",
                    null,
                    new ProcedureType() { Id = 1, Name = "TestProceType" },
                    null,
                    false,
                    DateTimeOffset.UtcNow,
                    TimeZoneInfo.Local.Id,
                    false,
                    new List<ProcedureNote>(),
                    null,
                    new List<VideoRecordingEvent>(),
                    BackgroundRecordingMode.StartImmediately);

            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);
            _recorderService.Setup(mock => mock.GetRecorderState()).ReturnsAsync(new Ism.Recorder.Core.V1.Protos.RecorderState() { State = 0 });

            //act
            await _manager.ApplyLabelToLatestImages(autolabel);
            var result = await _manager.GetActiveProcedure();

            //assert
            Assert.NotNull(result);
            Assert.AreNotEqual(autolabel, result.Images[0].Label);
            Assert.AreNotEqual(autolabel, result.Images[1].Label);
            Assert.AreEqual(autolabel, result.Images[2].Label);
        }
    }
}
