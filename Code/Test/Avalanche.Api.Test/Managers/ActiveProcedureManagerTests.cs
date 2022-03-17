using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
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
using Avalanche.Security.Server.Client.V1.Protos;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Configuration;
using Avalanche.Shared.Infrastructure.Enumerations;
using Google.Protobuf.WellKnownTypes;
using Google.Rpc;
using Ism.IsmLogCommon.Core;
using Ism.Library.V1.Protos;
using Ism.PatientInfoEngine.V1.Protos;
using Ism.Storage.DataManagement.Client.V1.Protos;
using Ism.SystemState.Client;
using Ism.SystemState.Models.Procedure;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using BackgroundRecordingMode = Ism.SystemState.Models.Procedure.BackgroundRecordingMode;
using Enum = System.Enum;
using ProcedureIdMessage = Ism.Library.V1.Protos.ProcedureIdMessage;

namespace Avalanche.Api.Test.Managers
{
#pragma warning disable CA1707 // Identifiers should not contain underscores
    [TestFixture()]
    public class ActiveProcedureManagerTests
    {
        private Mock<IAccessInfoFactory> _accessInfoFactory;
        private IDataManagementService _dataManagementService;
        private Mock<IDataManager> _dataManager;
        private Mock<IHttpContextAccessor> _httpContextAccessor;
        LabelsConfiguration _labelsConfig;
        private Mock<ILibraryService> _libraryService;
        private ActiveProcedureManager _manager;
        private IMapper _mapper;
        private Mock<IPieService> _pieService;
        private Mock<IRecorderService> _recorderService;
        private Mock<IRoutingManager> _routingManager;
        private Mock<ISecurityService> _securityService;
        private Mock<SetupConfiguration> _setupConfiguration;
        private Mock<IStateClient> _stateClient;

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

        [Test]
        public async Task ActiveProcedureManager_AllocateNewProcedure_QuickRegister_Succeeds()
        {
            // Arrange
            var patientInfoConfig = GetPatientInfoConfigWithRequiredFields();
            var setupConfig = new SetupConfiguration
            {
                PatientInfo = patientInfoConfig,
                Registration = new RegistrationSetupConfiguration
                {
                    Manual = new ManualSetupConfiguration { PhysicianAsLoggedInUser = false },
                    Quick = new QuickSetupConfiguration()
                }
            };

            var formattedDate = DateTime.UtcNow.ToLocalTime().ToString(setupConfig.Registration.Quick.DateFormat, CultureInfo.InvariantCulture);

            var expectedUser = new UserMessage
            {
                UserName = setupConfig.Registration.Quick.DefaultUserName,
                Id = 1,
                FirstName = "System",
                LastName = "Admin"
            };

            var manager = GetActiveProcedureManager(
                setupConfig,
                out var stateClientMock,
                out var libraryServiceMock,
                out var routingManagerMock,
                out var accessInfoFactoryMock,
                out var securityServiceMock,
                out var pieServiceMock,
                out _
            );

            (var expectedAccessInfo, var expectedAllocateNewProcedureResponse) = SetupLibraryServiceMock(libraryServiceMock, accessInfoFactoryMock);

            _ = securityServiceMock.Setup(x => x.GetUser(It.Is<string>(
                x => x.Equals(setupConfig.Registration.Quick.DefaultUserName, StringComparison.Ordinal)
            ))).ReturnsAsync(new GetUserResponse
            {
                User = expectedUser
            });

            // Capture a reference to the ActiveProcedureState passed into PersistData so we can assert against it
            ActiveProcedureState? actualState = null;
            _ = stateClientMock
                .Setup(x => x.PersistData(It.IsAny<ActiveProcedureState>()))
                .Callback<ActiveProcedureState>(x => actualState = x);
            const PatientRegistrationMode mode = PatientRegistrationMode.Quick;

            // Act
            await manager.AllocateNewProcedure(mode, null).ConfigureAwait(false);

            // Assert
            accessInfoFactoryMock.Verify(x => x.GenerateAccessInfo(It.IsAny<string>()), Times.Once());
            libraryServiceMock.Verify(x => x.AllocateNewProcedure(It.Is<AllocateNewProcedureRequest>(param =>
                param.Clinical
                && param.AccessInfo.Ip == expectedAccessInfo.Ip
            )), Times.Once);
            pieServiceMock.Verify(x => x.GetPatientListSource(), Times.Once());
            routingManagerMock.Verify(x => x.PublishDefaultDisplayRecordingState(), Times.Once());
            stateClientMock.Verify(x => x.GetData<ActiveProcedureState>(), Times.Once());
            // Just check that method was called here.  Then Assert it was called with correct data
            stateClientMock.Verify(x => x.PersistData(It.IsAny<ActiveProcedureState>()), Times.Once());

            securityServiceMock.Verify(x => x.GetUser(It.Is<string>(
                x => x.Equals(setupConfig.Registration.Quick.DefaultUserName, StringComparison.Ordinal)
            )), Times.Once());

            Assert.AreEqual(ActiveProcedureManager.QuickRegisterDefaultStringValue, actualState!.Accession);
            Assert.AreEqual(BackgroundRecordingMode.StartImmediately, actualState.BackgroundRecordingMode);
            Assert.AreEqual(0, actualState.BackgroundVideos.Count);
            Assert.AreEqual(ActiveProcedureManager.QuickRegisterDefaultStringValue, actualState.Department!.Name);
            Assert.AreEqual(0, actualState.Department.Id);
            Assert.AreEqual(0, actualState.Images.Count);
            Assert.False(actualState.IsBackgroundRecording);
            Assert.True(actualState.IsClinical);
            Assert.False(actualState.IsRecording);
            Assert.AreEqual(expectedAllocateNewProcedureResponse.ProcedureId.Id, actualState.LibraryId);
            Assert.AreEqual(0, actualState.Notes.Count);

            // Patient
            Assert.NotNull(actualState.Patient.FirstName); // TODO check quickregister format once UtcNow has been DI'd
            Assert.NotNull(actualState.Patient.LastName); // TODO check quickregister format once UtcNow has been DI'd
            Assert.AreEqual(setupConfig.Registration.Quick.DefaultSex.ToString(), actualState.Patient.Sex);
            Assert.AreEqual(DateTime.MaxValue, actualState.Patient.DateOfBirth);
            Assert.Null(actualState.Patient.Id);

            Assert.AreEqual(PatientListSource.Local, actualState.PatientListSource);

            // Physician
            Assert.AreEqual(expectedUser.FirstName, actualState.Physician.FirstName);
            Assert.AreEqual(expectedUser.LastName, actualState.Physician.LastName);
            Assert.AreEqual(expectedUser.Id.ToString(CultureInfo.InvariantCulture), actualState.Physician.Id);

            Assert.AreEqual(expectedAllocateNewProcedureResponse.RelativePath, actualState.ProcedureRelativePath);
            // TODO: Add clock actualState.ProcedureStartTimeUtc
            Assert.AreEqual(TimeZoneInfo.Local.Id, actualState.ProcedureTimezoneId);
            Assert.AreEqual(ActiveProcedureManager.QuickRegisterDefaultStringValue, actualState.ProcedureType!.Name);
            Assert.AreEqual(0, actualState.ProcedureType.Id);
            Assert.AreEqual(0, actualState.RecordingEvents.Count);
            Assert.AreEqual(mode, Enum.Parse<PatientRegistrationMode>(actualState.RegistrationMode.ToString()));
            Assert.AreEqual(expectedAllocateNewProcedureResponse.ProcedureId.RepositoryName, actualState.RepositoryId);
            Assert.False(actualState.RequiresUserConfirmation);
            Assert.AreEqual(0, actualState.Videos.Count);
        }

        [Test]
        public async Task ActiveProcedureManager_AllocateNewProcedure_ManualRegister_Succeeds()
        {
            // Arrange
            var manualRegistration = Fakers.GetPatientFaker().Generate();

            var patientInfoConfig = GetPatientInfoConfigWithRequiredFields();
            var setupConfig = new SetupConfiguration
            {
                PatientInfo = patientInfoConfig,
                Registration = new RegistrationSetupConfiguration
                {
                    Manual = new ManualSetupConfiguration { PhysicianAsLoggedInUser = false },
                    Quick = new QuickSetupConfiguration()
                }
            };

            const int patientListSource = 1;

            var manager = GetActiveProcedureManager(
                setupConfig,
                out var stateClientMock,
                out var libraryServiceMock,
                out var routingManagerMock,
                out var accessInfoFactoryMock,
                out var securityServiceMock,
                out var pieServiceMock,
                out var dataManagerMock,
                patientListSource
            );

            (var expectedAccessInfo, var expectedAllocateNewProcedureResponse) = SetupLibraryServiceMock(libraryServiceMock, accessInfoFactoryMock);

            _ = dataManagerMock.Setup(x => x.GetProcedureType(It.IsAny<GetProcedureTypeRequest>()))
                .ReturnsAsync(new ProcedureTypeMessage
                {
                    Name = manualRegistration.ProcedureType!.Name,
                    Id = manualRegistration.ProcedureType.Id,
                    DepartmentId = manualRegistration.ProcedureType.DepartmentId
                });

            // Capture a reference to the ActiveProcedureState passed into PersistData so we can assert against it
            ActiveProcedureState? actualState = null;
            _ = stateClientMock
                .Setup(x => x.PersistData(It.IsAny<ActiveProcedureState>()))
                .Callback<ActiveProcedureState>(x => actualState = x);
            const PatientRegistrationMode mode = PatientRegistrationMode.Manual;

            // Act
            await manager.AllocateNewProcedure(mode, manualRegistration).ConfigureAwait(false);

            // Assert
            accessInfoFactoryMock.Verify(x => x.GenerateAccessInfo(It.IsAny<string>()), Times.Once());
            libraryServiceMock.Verify(x => x.AllocateNewProcedure(It.Is<AllocateNewProcedureRequest>(param =>
                param.Clinical
                && param.AccessInfo.Ip == expectedAccessInfo.Ip
            )), Times.Once);
            pieServiceMock.Verify(x => x.GetPatientListSource(), Times.Once());
            routingManagerMock.Verify(x => x.PublishDefaultDisplayRecordingState(), Times.Once());
            stateClientMock.Verify(x => x.GetData<ActiveProcedureState>(), Times.Once());
            // Just check that method was called here.  Then Assert it was called with correct data
            stateClientMock.Verify(x => x.PersistData(It.IsAny<ActiveProcedureState>()), Times.Once());

            securityServiceMock.Verify(x => x.GetUser(It.IsAny<string>()), Times.Never());

            Assert.AreEqual(manualRegistration.AccessionNumber, actualState!.Accession);

            Assert.AreEqual(manualRegistration.BackgroundRecordingMode, Enum.Parse<Shared.Infrastructure.Enumerations.BackgroundRecordingMode>(actualState.BackgroundRecordingMode.ToString()));
            Assert.AreEqual(0, actualState.BackgroundVideos.Count);
            Assert.AreEqual(manualRegistration.Department!.Name, actualState.Department!.Name);
            Assert.AreEqual(manualRegistration.Department.Id, actualState.Department.Id);
            Assert.AreEqual(0, actualState.Images.Count);
            Assert.False(actualState.IsBackgroundRecording);
            Assert.True(actualState.IsClinical);
            Assert.False(actualState.IsRecording);
            Assert.AreEqual(expectedAllocateNewProcedureResponse.ProcedureId.Id, actualState.LibraryId);
            Assert.AreEqual(0, actualState.Notes.Count);

            // Patient
            Assert.AreEqual(manualRegistration.FirstName, actualState.Patient.FirstName);
            Assert.AreEqual(manualRegistration.LastName, actualState.Patient.LastName);
            Assert.AreEqual(manualRegistration.DateOfBirth, actualState.Patient.DateOfBirth);
            Assert.AreEqual(manualRegistration.Sex!.Id, actualState.Patient.Sex);
            Assert.AreEqual(manualRegistration.Id, actualState.Patient.Id);

            Assert.AreEqual((PatientListSource)patientListSource, actualState.PatientListSource);

            // Physician
            Assert.AreEqual(manualRegistration.Physician!.FirstName, actualState.Physician.FirstName);
            Assert.AreEqual(manualRegistration.Physician.LastName, actualState.Physician.LastName);
            Assert.AreEqual(manualRegistration.Physician.Id.ToString(), actualState.Physician.Id);

            Assert.AreEqual(expectedAllocateNewProcedureResponse.RelativePath, actualState.ProcedureRelativePath);
            // TODO: Add clock actualState.ProcedureStartTimeUtc
            Assert.AreEqual(TimeZoneInfo.Local.Id, actualState.ProcedureTimezoneId);
            Assert.AreEqual(manualRegistration.ProcedureType!.Name, actualState.ProcedureType!.Name);
            Assert.AreEqual(manualRegistration.ProcedureType.Id, actualState.ProcedureType.Id);
            Assert.AreEqual(0, actualState.RecordingEvents.Count);
            Assert.AreEqual(mode, Enum.Parse<PatientRegistrationMode>(actualState.RegistrationMode.ToString()));
            Assert.AreEqual(expectedAllocateNewProcedureResponse.ProcedureId.RepositoryName, actualState.RepositoryId);
            Assert.False(actualState.RequiresUserConfirmation);
            Assert.AreEqual(0, actualState.Videos.Count);

        }

        private static (AccessInfo expectedAccessInfo, AllocateNewProcedureResponse expectedAllocateNewProcedureResponse)
            SetupLibraryServiceMock(Mock<ILibraryService> libraryServiceMock, Mock<IAccessInfoFactory> accessInfoFactoryMock)
        {
            var expectedAccessInfo = GetAccessInfo();
            var expectedAllocateNewProcedureResponse = Fakers.GetAllocateNewProcedureResponseFaker().Generate();
            _ = accessInfoFactoryMock.Setup(x => x.GenerateAccessInfo(It.IsAny<string>())).Returns(expectedAccessInfo);

            _ = libraryServiceMock.Setup(x => x.AllocateNewProcedure(
                It.Is<AllocateNewProcedureRequest>(args =>
                    args.Clinical
                    && args.AccessInfo.Ip == expectedAccessInfo.Ip
                )
            )).ReturnsAsync(expectedAllocateNewProcedureResponse);

            return (expectedAccessInfo, expectedAllocateNewProcedureResponse);
        }

        private static AccessInfo GetAccessInfo() =>
            new AccessInfo("192.168.1.1", "user", "test", "mypc", "");

        private static List<PatientInfoSetupConfiguration> GetPatientInfoConfigWithRequiredFields()
        {
            var patientInfoConfig = new List<PatientInfoSetupConfiguration>();
            // Iterate all configurable fields and make them all required.
            // This will ensure the test will break if a new field is added to the enum but not handled by business logic
#pragma warning disable CS8605 // Unboxing a possibly null value.
            foreach (ProcedureInfoField field in Enum.GetValues(typeof(ProcedureInfoField)))
            {
                patientInfoConfig.Add(new PatientInfoSetupConfiguration { Id = field, Required = true });
            }
#pragma warning restore CS8605 // Unboxing a possibly null value.

            return patientInfoConfig;
        }

        [Test]
        public void ActiveProcedureManager_AllocateNewProcedure_WhenAnotherProcedureIsActive_Throws()
        {
            // Arrange
            var activeProcedure = Fakers.GetActiveProcedureWithProcedureTypeFaker();
            const PatientRegistrationMode patientRegistrationMode = PatientRegistrationMode.Quick;
            var manager = GetActiveProcedureManager(null!,
                out var stateClientMock,
                out _,
                out _,
                out _,
                out _,
                out _,
                out _
            );

            _ = stateClientMock.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);

            // Act & Assert
            _ = Assert.ThrowsAsync<InvalidOperationException>(async () => await manager.AllocateNewProcedure(patientRegistrationMode, null).ConfigureAwait(false));
        }

        [Test]
        public async Task ActiveProcedureManager_ApplyLabelToActiveProcedure_ApplyAdhocLabelWhenEnabled_AddingLabelToStore_Fails()
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
            _dataManager.Setup(d => d.GetLabel(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(labelModel);

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
        public async Task ActiveProcedureManager_ApplyLabelToActiveProcedure_ApplyAdhocLabelWhenEnabledToProcedureImage_Succeeds()
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
        public async Task ActiveProcedureManager_ApplyLabelToActiveProcedure_ApplyAdhocLabelWhenEnabledToProcedureVideo_Succeeds()
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
            _dataManager.Setup(d => d.GetLabel(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(labelModel);

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
        public async Task ActiveProcedureManager_ApplyLabelToActiveProcedure_ExistingLabelToProcedureImage_Succeeds()
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
        public async Task ActiveProcedureManager_ApplyLabelToActiveProcedure_ExistingLabelToProcedureVideo_Succeeds()
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
            _dataManager.Setup(d => d.GetLabel(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(labelModel);

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
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public async Task ActiveProcedureManager_ApplyLabelToLatestImages_LabelNameEmpty_Fails(string label)
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
        public async Task ActiveProcedureManager_ApplyLabelToLatestImages_NoImagesExists_Fails()
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
        public async Task ActiveProcedureManager_ApplyLabelToLatestImages_Succeeds()
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
        public async Task ActiveProcedureManager_ApplyLabelToLatestImages_WithDifferentCorrelationIds_Succeeds()
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
        public void ActiveProcedureManager_DeleteActiveProcedureMediaItem_IfVideoIsRecording_Fails()
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
                    null!,
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
        public async Task ActiveProcedureManager_DeleteActiveProcedureMediaItem_Succeeds()
        {
            var activeProcedure = Fakers.GetActiveProcedureWithOneImageFaker();

            var id = Guid.NewGuid();
            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);

            await _manager.DeleteActiveProcedureMediaItem(Shared.Domain.Enumerations.ProcedureContentType.Image, id).ConfigureAwait(false);
        }

        [Test]
        public void ActiveProcedureManager_DeleteActiveProcedureMediaItems_IfVideoIsRecording_Fails()
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
                    null!,
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
        public async Task ActiveProcedureManager_DeleteActiveProcedureMediaItems_Succeeds()
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
                    null!,
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
        public async Task ActiveProcedureManager_DiscardActiveProcedure__DoesNotPublishFinishEvent()
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
        public async Task ActiveProcedureManager_FinishActiveProcedure_PublishesFinishEvent()
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
        public async Task ActiveProcedureManager_GetActiveProcedure_Succeeds()
        {
            var activeProcedure = Fakers.GetActiveProcedureFaker().Generate();

            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(activeProcedure);

            _recorderService.Setup(mock => mock.GetRecorderState()).ReturnsAsync(new Ism.Recorder.Core.V1.Protos.RecorderState() { State = 0 });

            var result = await _manager.GetActiveProcedure().ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.AreEqual(activeProcedure.Patient.LastName, result.Patient?.LastName);
        }

        private ActiveProcedureManager GetActiveProcedureManager(
            SetupConfiguration setupConfig,
            out Mock<IStateClient> stateClient,
            out Mock<ILibraryService> libraryService,
            out Mock<IRoutingManager> routingManager,
            out Mock<IAccessInfoFactory> accessInfoFactory,
            out Mock<ISecurityService> securityService,
            out Mock<IPieService> pieService,
            out Mock<IDataManagementService> dataService,
            int patientListSource = 0,
            ActiveProcedureState? activeProcedureState = null
        )
        {
            stateClient = new Mock<IStateClient>();
            libraryService = new Mock<ILibraryService>();
            routingManager = new Mock<IRoutingManager>();
            accessInfoFactory = new Mock<IAccessInfoFactory>();
            securityService = new Mock<ISecurityService>();
            pieService = new Mock<IPieService>();
            dataService = new Mock<IDataManagementService>();

            // Make default Setups
            _ = pieService.Setup(x => x.GetPatientListSource())
                .ReturnsAsync(new GetSourceResponse
                {
                    Source = patientListSource
                }
            );
            _ = routingManager.Setup(x => x.PublishDefaultDisplayRecordingState());

            _ = stateClient.Setup(x => x.GetData<ActiveProcedureState>())
               .ReturnsAsync(activeProcedureState!);


            var config = new MapperConfiguration(cfg => cfg.AddProfile(new ProceduresMappingConfiguration()));
            var mapper = config.CreateMapper();
            var dataManager = new Mock<IDataManager>();
            var recorderService = new Mock<IRecorderService>();
            var labelsConfig = new LabelsConfiguration();
            var httpContextAccessor = new Mock<IHttpContextAccessor>();

            return new ActiveProcedureManager(
                stateClient.Object,
                libraryService.Object,
                accessInfoFactory.Object,
                mapper,
                recorderService.Object,
                dataManager.Object,
                labelsConfig,
                dataService.Object,
                routingManager.Object,
                setupConfig,
                httpContextAccessor.Object,
                securityService.Object,
                pieService.Object
            );
        }
    }
#pragma warning restore CA1707 // Identifiers should not contain underscores
}
