using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Managers.Data;
using Avalanche.Api.Managers.Media;
using Avalanche.Api.Managers.Procedures;
using Avalanche.Api.Mapping;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Services.Security;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Configuration;
using Avalanche.Shared.Infrastructure.Enumerations;
using Ism.SystemState.Client;
using Ism.SystemState.Models.Procedure;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using BackgroundRecordingMode = Ism.SystemState.Models.Procedure.BackgroundRecordingMode;

namespace Avalanche.Api.Test.Managers
{
    [TestFixture()]
    public class ActiveProcedureManagerTests
    {
        private IMapper _mapper;
        private Mock<IAccessInfoFactory> _accessInfoFactory;
        private Mock<ILibraryService> _libraryService;
        private Mock<IRecorderService> _recorderService;
        private Mock<IStateClient> _stateClient;
        private Mock<IDataManager> _dataManager;
        private IDataManagementService _dataManagementService;
        private Mock<IRoutingManager> _routingManager;
        private Mock<SetupConfiguration> _setupConfiguration;
        LabelsConfiguration _labelsConfig;
        private Mock<IHttpContextAccessor> _httpContextAccessor;
        private Mock<ISecurityService> _securityService;
        private Mock<IPieService> _pieService;

        private ActiveProcedureManager _manager;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile(new ProceduresMappingConfiguration()));

            _mapper = config.CreateMapper();
            _accessInfoFactory = new Mock<IAccessInfoFactory>();
            _libraryService = new Mock<ILibraryService>();
            _recorderService = new Mock<IRecorderService>();
            _stateClient = new Mock<IStateClient>();
            _dataManager = new Mock<IDataManager>();
            _labelsConfig = new LabelsConfiguration();
            _routingManager = new Mock<IRoutingManager>();
            _setupConfiguration = new Mock<SetupConfiguration>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _securityService = new Mock<ISecurityService>();
            _pieService = new Mock<IPieService>();

            _manager = new ActiveProcedureManager(_stateClient.Object, _libraryService.Object, _accessInfoFactory.Object, _mapper, _recorderService.Object, _dataManager.Object, _labelsConfig, _dataManagementService, _routingManager.Object, _setupConfiguration.Object, _httpContextAccessor.Object, _securityService.Object, _pieService.Object);
        }

        private ActiveProcedureManager GetActiveProcedureManager(out Mock<IStateClient> stateClient)
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile(new ProceduresMappingConfiguration()));
            var mapper = config.CreateMapper();
            var accessInfoFactory = new Mock<IAccessInfoFactory>();
            var libraryService = new Mock<ILibraryService>();
            var recorderService = new Mock<IRecorderService>();
            stateClient = new Mock<IStateClient>();
            var dataManager = new Mock<IDataManager>();
            var labelsConfig = new LabelsConfiguration();
            var routingManager = new Mock<IRoutingManager>();
            var setupConfiguration = new Mock<SetupConfiguration>();
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            var securityService = new Mock<ISecurityService>();
            var pieService = new Mock<IPieService>();

            return new ActiveProcedureManager(stateClient.Object, libraryService.Object, accessInfoFactory.Object, mapper, recorderService.Object, dataManager.Object, labelsConfig, _dataManagementService, routingManager.Object, setupConfiguration.Object, httpContextAccessor.Object, securityService.Object, pieService.Object);
        }

        [Test]
        public async Task TestGetActiveProcedureReturnsProcedure()
        {
            var activeProcedure = Fakers.GetActiveProcedureFaker();

            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);

            _recorderService.Setup(mock => mock.GetRecorderState()).ReturnsAsync(new Ism.Recorder.Core.V1.Protos.RecorderState() { State = 0 });

            var result = await _manager.GetActiveProcedure().ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.AreEqual("name", result.Patient?.LastName);
        }

        [Test]
        public async Task TestDeleteActiveProcedureMediaSucceedsIfValid()
        {
            var activeProcedure = Fakers.GetActiveProcedureWithOneImageFaker();

            var id = Guid.NewGuid();
            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);

            await _manager.DeleteActiveProcedureMediaItem(Shared.Domain.Enumerations.ProcedureContentType.Image, id).ConfigureAwait(false);
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
                    BackgroundRecordingMode.StartImmediately,
                    RegistrationMode.Quick,
                    PatientListSource.Local));

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
                    BackgroundRecordingMode.StartImmediately,
                    RegistrationMode.Quick,
                    PatientListSource.Local));

            await _manager.DeleteActiveProcedureMediaItems(Shared.Domain.Enumerations.ProcedureContentType.Image, new List<Guid>() { imageId }).ConfigureAwait(false);
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
                    BackgroundRecordingMode.StartImmediately,
                    RegistrationMode.Quick,
                    PatientListSource.Local));

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
                    BackgroundRecordingMode.StartImmediately,
                    RegistrationMode.Quick,
                    PatientListSource.Local);

            //arrange
            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);

            _dataManager.Setup(d => d.AddLabel(labelModel)).ReturnsAsync(labelModel);
            _dataManager.Setup(d => d.GetLabel(It.IsAny<string>(), It.IsAny<Int32>())).ReturnsAsync(labelModel);

            _labelsConfig = new LabelsConfiguration
            {
                AdHocLabelsAllowed = true
            };

            _recorderService.Setup(mock => mock.GetRecorderState()).ReturnsAsync(new Ism.Recorder.Core.V1.Protos.RecorderState() { State = 0 });

            //act
            await _manager.ApplyLabelToActiveProcedure(labelContent).ConfigureAwait(false);
            var result = await _manager.GetActiveProcedure().ConfigureAwait(false);

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
                    BackgroundRecordingMode.StartImmediately,
                    RegistrationMode.Quick,
                    PatientListSource.Local);

            //arrange
            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);

            _dataManager.Setup(d => d.AddLabel(labelModel)).ReturnsAsync(labelModel);
            _dataManager.Setup(d => d.GetLabel(It.IsAny<string>(), It.IsAny<Int32>())).ReturnsAsync(labelModel);

            _labelsConfig = new LabelsConfiguration
            {
                AdHocLabelsAllowed = true
            };

            _recorderService.Setup(mock => mock.GetRecorderState()).ReturnsAsync(new Ism.Recorder.Core.V1.Protos.RecorderState() { State = 0 });

            //act
            await _manager.ApplyLabelToActiveProcedure(labelContent).ConfigureAwait(false);
            var result = await _manager.GetActiveProcedure().ConfigureAwait(false);

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
                    BackgroundRecordingMode.StartImmediately,
                    RegistrationMode.Quick,
                    PatientListSource.Local);

            //arrange
            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);

            _dataManager.Setup(d => d.AddLabel(labelModel)).ReturnsAsync(labelModel);
            _dataManager.Setup(d => d.GetLabel(It.IsAny<string>(), It.IsAny<Int32>())).ReturnsAsync(labelModel);

            _labelsConfig = new LabelsConfiguration
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
                    BackgroundRecordingMode.StartImmediately,
                    RegistrationMode.Quick,
                    PatientListSource.Local);

            //arrange
            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);

            _dataManager.Setup(d => d.GetLabel(It.IsAny<string>(), It.IsAny<Int32>())).ReturnsAsync(labelModel);

            _labelsConfig = new LabelsConfiguration
            {
                AdHocLabelsAllowed = true
            };

            _recorderService.Setup(mock => mock.GetRecorderState()).ReturnsAsync(new Ism.Recorder.Core.V1.Protos.RecorderState() { State = 0 });

            //act
            await _manager.ApplyLabelToActiveProcedure(labelContent).ConfigureAwait(false);
            var result = await _manager.GetActiveProcedure().ConfigureAwait(false);

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
                    BackgroundRecordingMode.StartImmediately,
                    RegistrationMode.Quick,
                    PatientListSource.Local);

            //arrange
            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);
            _dataManager.Setup(d => d.GetLabel(It.IsAny<string>(), It.IsAny<Int32>())).ReturnsAsync(labelModel);

            _labelsConfig = new LabelsConfiguration
            {
                AdHocLabelsAllowed = true
            };

            _recorderService.Setup(mock => mock.GetRecorderState()).ReturnsAsync(new Ism.Recorder.Core.V1.Protos.RecorderState() { State = 0 });

            //act
            await _manager.ApplyLabelToActiveProcedure(labelContent).ConfigureAwait(false);
            var result = await _manager.GetActiveProcedure().ConfigureAwait(false);

            //assert
            Assert.NotNull(result);
            Assert.AreEqual("LabelB", result.Videos[0].Label);
        }

        [Test]
        public async Task ProcedureManager_ApplyLabelToLatestImages_Succeeds()
        {
            //arrange
            const string autolabel = "LabelA";
            var activeProcedure = Fakers.GetActiveProcedureWithCorrelationImagesFaker();

            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);
            _recorderService.Setup(mock => mock.GetRecorderState()).ReturnsAsync(new Ism.Recorder.Core.V1.Protos.RecorderState() { State = 0 });

            //act
            await _manager.ApplyLabelToLatestImages(autolabel).ConfigureAwait(false);
            var result = await _manager.GetActiveProcedure().ConfigureAwait(false);

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
            var activeProcedure = Fakers.GetActiveProcedureWithCorrelationImagesFaker();

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

            var activeProcedure = Fakers.GetActiveProcedureWithProcedureTypeFaker();

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
            var activeProcedure = Fakers.GetActiveProcedureWithImagesDifferentTimeFaker();

            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);
            _recorderService.Setup(mock => mock.GetRecorderState()).ReturnsAsync(new Ism.Recorder.Core.V1.Protos.RecorderState() { State = 0 });

            //act
            await _manager.ApplyLabelToLatestImages(autolabel).ConfigureAwait(false);
            var result = await _manager.GetActiveProcedure().ConfigureAwait(false);

            //assert
            Assert.NotNull(result);
            Assert.AreNotEqual(autolabel, result.Images[0].Label);
            Assert.AreNotEqual(autolabel, result.Images[1].Label);
            Assert.AreEqual(autolabel, result.Images[2].Label);
        }


        [Test]
        public async Task ProcedureManagerFinishPublishesFinishEvent()
        {
            var activeProcedure = Fakers.GetActiveProcedureWithProcedureTypeFaker();

            //arrange
            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);

            //act
            await _manager.FinishActiveProcedure().ConfigureAwait(false);

            //assert
            _stateClient.Verify(x => x.PublishEvent(It.IsAny<ProcedureFinishedEvent>()), Times.Once);
        }

        [Test]
        public async Task ProcedureManagerDiscardDoesNotPublishesFinishEvent()
        {
            var activeProcedure = Fakers.GetActiveProcedureWithProcedureTypeFaker();

            //arrange
            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);

            //act
            await _manager.DiscardActiveProcedure().ConfigureAwait(false);

            //assert
            _stateClient.Verify(x => x.PublishEvent(It.IsAny<ProcedureFinishedEvent>()), Times.Never);
        }

        [Test]
        public void AllocateNewProcedureWhenAnotherProcedureIsActive()
        {
            // Arrange
            var activeProcedure = Fakers.GetActiveProcedureWithProcedureTypeFaker();
            var patientRegistrationMode = PatientRegistrationMode.Quick;
            var manager = GetActiveProcedureManager(out var stateClientMock);

            _ = stateClientMock.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);

            // Act & Assert
            _ = Assert.ThrowsAsync<InvalidOperationException>(async () => await manager.AllocateNewProcedure(patientRegistrationMode, null).ConfigureAwait(false));
        }
    }
}
