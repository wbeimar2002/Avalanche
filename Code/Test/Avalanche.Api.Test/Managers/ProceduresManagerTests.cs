using AutoMapper;
using Avalanche.Api.Managers.Data;
using Avalanche.Api.Managers.Media;
using Avalanche.Api.Managers.Procedures;
using Avalanche.Api.Mapping;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Configuration;
using Ism.Library.V1.Protos;
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
        Mock<IStateClient> _stateClient;
        Mock<IDataManager> _dataManager;
        GeneralApiConfiguration _generalApiConfig;

        ProceduresManager _manager;

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

            _manager = new ProceduresManager(_stateClient.Object, _libraryService.Object, _accessInfoFactory.Object, _mapper, _recorderService.Object, _dataManager.Object, _generalApiConfig);
        }

        [Test]
        public async Task TestGetActiveProcedureReturnsProcedure()
        {
            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(
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

            await _manager.DeleteActiveProcedureMedia(Shared.Domain.Enumerations.ProcedureContentType.Image, id);
        }

        [Test]
        public void TestDeleteActiveProcedureMediaFailsIfVideoIsRecording()
        {
            var id = Guid.NewGuid();
            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(
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

            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _manager.DeleteActiveProcedureMedia(Shared.Domain.Enumerations.ProcedureContentType.Video, id));
            Assert.True(ex.Message.Contains("Cannot delete video that is currently recording"));
        }

        [Test]
        public async Task TestDeleteActiveProcedureMediaItemsSucceedsIfValid()
        {
            var imageId = Guid.NewGuid();
            _stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(
                new ActiveProcedureState(
                    new Patient() { LastName = "name" },
                    new List<ProcedureImage>() { new ProcedureImage(imageId, "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow) },
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
                    new List<ProcedureVideo>() { new ProcedureVideo(videoId, "source", "channel", "path", "path", DateTimeOffset.UtcNow, null, TimeSpan.FromSeconds(1)) },
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

            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _manager.DeleteActiveProcedureMediaItems(Shared.Domain.Enumerations.ProcedureContentType.Video, new List<Guid>() { videoId }));
            Assert.True(ex.Message.Contains("Cannot delete video that is currently recording"));
        }

        [Test]
        public async Task GetProcedureDetails_VerifyCalls()
        {
            var libraryId = "2021_06_18T19_52_44_TODO";

            var response = new GetFinishedProcedureResponse();
            { };

            _libraryService.Setup(mock => mock.GetFinishedProcedure(new GetFinishedProcedureRequest()
            {
                LibraryId = libraryId
            })).ReturnsAsync(response);

            await _manager.GetProcedureDetails(libraryId);

            _libraryService.Verify(mock => mock.GetFinishedProcedure(new GetFinishedProcedureRequest()
            {
                LibraryId = libraryId
            }), Times.Once);
        }

        [Test]
        [TestCase("   ")]
        [TestCase("")]
        [TestCase(null)]
        public async Task GetProcedureDetails_FailsWithEmptyOrWhiteSpaceLibraryId(string libraryId)
        {
            var response = new GetFinishedProcedureResponse();
            { };

            _libraryService.Setup(mock => mock.GetFinishedProcedure(new GetFinishedProcedureRequest()
            {
                LibraryId = string.Empty
            })).ReturnsAsync(response);

            Task Act() => _manager.GetProcedureDetails(libraryId); 
            Assert.That(Act, Throws.TypeOf<ArgumentNullException>());

            _libraryService.Verify(mock => mock.GetFinishedProcedure(new GetFinishedProcedureRequest()
            {
                LibraryId = string.Empty
            }), Times.Never);
        }

        [Test]
        public async Task GetProcedures_VerifyCalls()
        {
            var filter = new ProcedureSearchFilterViewModel()
            {
                IsDescending = false,
                Limit = 25,
                Page = 1,
                ProcedureIndexSortingColumn = Shared.Infrastructure.Enumerations.ProcedureIndexSortingColumns.Created                
            };

            var response = new GetFinishedProceduresResponse()
            { };

            _libraryService.Setup(mock => mock.GetFinishedProcedures(new GetFinishedProceduresRequest()
            {
                Page = filter.Page,
                PageSize = filter.Limit,
                IsDescending = filter.IsDescending,
                ProcedureIndexSortingColumn = (ProcedureIndexSortingColumns)filter.ProcedureIndexSortingColumn
            })).ReturnsAsync(response);

            await _manager.BasicSearch(filter);

            _libraryService.Verify(mock => mock.GetFinishedProcedures(new GetFinishedProceduresRequest()
            {
                Page = filter.Page,
                PageSize = filter.Limit,
                IsDescending = filter.IsDescending,
                ProcedureIndexSortingColumn = (ProcedureIndexSortingColumns)filter.ProcedureIndexSortingColumn
            }), Times.Once);
        }

        [Test]
        public async Task GetProcedures_FailsIfLimitIsLowerThanMinPageSize()
        {
            var filter = new ProcedureSearchFilterViewModel()
            {
                IsDescending = false,
                Limit = 10,
                Page = 1,
                ProcedureIndexSortingColumn = Shared.Infrastructure.Enumerations.ProcedureIndexSortingColumns.Created
            };

            var response = new GetFinishedProceduresResponse()
            {
            };

            _libraryService.Setup(mock => mock.GetFinishedProcedures(new GetFinishedProceduresRequest()
            {
                Page = filter.Page,
                PageSize = filter.Limit,
                IsDescending = filter.IsDescending,
                ProcedureIndexSortingColumn = (ProcedureIndexSortingColumns)filter.ProcedureIndexSortingColumn
            })).ReturnsAsync(response);

            Task Act() => _manager.BasicSearch(filter);
            Assert.That(Act, Throws.TypeOf<ArgumentException>());

            _libraryService.Verify(mock => mock.GetFinishedProcedures(new GetFinishedProceduresRequest()
            {
                Page = filter.Page,
                PageSize = filter.Limit,
                IsDescending = filter.IsDescending,
                ProcedureIndexSortingColumn = (ProcedureIndexSortingColumns)filter.ProcedureIndexSortingColumn
            }), Times.Never);
        }

        [Test]
        public async Task GetProcedures_FailsIfLimitIsLargerThanMaxPageSize()
        {
            var filter = new ProcedureSearchFilterViewModel()
            {
                IsDescending = false,
                Limit = 110,
                Page = 1,
                ProcedureIndexSortingColumn = Shared.Infrastructure.Enumerations.ProcedureIndexSortingColumns.Created
            };

            var response = new GetFinishedProceduresResponse()
            { };

            _libraryService.Setup(mock => mock.GetFinishedProcedures(new GetFinishedProceduresRequest()
            {
                Page = filter.Page,
                PageSize = filter.Limit,
                IsDescending = filter.IsDescending,
                ProcedureIndexSortingColumn = (ProcedureIndexSortingColumns)filter.ProcedureIndexSortingColumn
            })).ReturnsAsync(response);

            Task Act() => _manager.BasicSearch(filter);
            Assert.That(Act, Throws.TypeOf<ArgumentException>());
            
            _libraryService.Verify(mock => mock.GetFinishedProcedures(new GetFinishedProceduresRequest()
            {
                Page = filter.Page,
                PageSize = filter.Limit,
                IsDescending = filter.IsDescending,
                ProcedureIndexSortingColumn = (ProcedureIndexSortingColumns)filter.ProcedureIndexSortingColumn
            }), Times.Never);
        }

        [Test]
        public async Task GetProcedures_FailsIfPageIsLowerThanZero()
        {
            var filter = new ProcedureSearchFilterViewModel()
            {
                IsDescending = false,
                Limit = 25,
                Page = -1,
                ProcedureIndexSortingColumn = Shared.Infrastructure.Enumerations.ProcedureIndexSortingColumns.Created
            };

            var response = new GetFinishedProceduresResponse()
            { };

            _libraryService.Setup(mock => mock.GetFinishedProcedures(new GetFinishedProceduresRequest()
            {
                Page = filter.Page,
                PageSize = filter.Limit,
                IsDescending = filter.IsDescending,
                ProcedureIndexSortingColumn = (ProcedureIndexSortingColumns)filter.ProcedureIndexSortingColumn
            })).ReturnsAsync(response);

            Task Act() => _manager.BasicSearch(filter);
            Assert.That(Act, Throws.TypeOf<ArgumentException>());

            _libraryService.Verify(mock => mock.GetFinishedProcedures(new GetFinishedProceduresRequest()
            {
                Page = filter.Page,
                PageSize = filter.Limit,
                IsDescending = filter.IsDescending,
                ProcedureIndexSortingColumn = (ProcedureIndexSortingColumns)filter.ProcedureIndexSortingColumn
            }), Times.Never);
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
                    new List<ProcedureImage>() { new ProcedureImage(id, "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow) },
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
                    new Dictionary<string, string>(),
                    null,
                    null,
                    null,
                    new List<VideoRecordingEvent>());

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
                    new List<ProcedureVideo>() { new ProcedureVideo(id, "source", "channel", "path", "path", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddSeconds(10), TimeSpan.FromSeconds(10)) },
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
                    new Dictionary<string, string>(),
                    null,
                    null,
                    null,
                    new List<VideoRecordingEvent>());

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
                    new List<ProcedureImage>() { new ProcedureImage(id, "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow) },
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
                    new Dictionary<string, string>(),
                    null,
                    null,
                    null,
                    new List<VideoRecordingEvent>());

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
                    new List<ProcedureImage>() { new ProcedureImage(id, "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow) },
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
                    new Dictionary<string, string>(),
                    null,
                    null,
                    null,
                    new List<VideoRecordingEvent>());

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
                    new List<ProcedureVideo>() { new ProcedureVideo(id, "source", "channel", "path", "path", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddSeconds(10), TimeSpan.FromSeconds(10)) },
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
                    new Dictionary<string, string>(),
                    null,
                    null,
                    null,
                    new List<VideoRecordingEvent>());

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
    }
}
