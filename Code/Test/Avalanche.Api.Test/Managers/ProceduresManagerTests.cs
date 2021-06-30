using AutoMapper;
using Avalanche.Api.Managers.Media;
using Avalanche.Api.Managers.Procedures;
using Avalanche.Api.MappingConfigurations;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
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

            _manager = new ProceduresManager(_stateClient.Object, _libraryService.Object, _accessInfoFactory.Object, _mapper, _recorderService.Object);
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

            await _manager.Search(filter);

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

            Task Act() => _manager.Search(filter);
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

            Task Act() => _manager.Search(filter);
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

            Task Act() => _manager.Search(filter);
            Assert.That(Act, Throws.TypeOf<ArgumentException>());

            _libraryService.Verify(mock => mock.GetFinishedProcedures(new GetFinishedProceduresRequest()
            {
                Page = filter.Page,
                PageSize = filter.Limit,
                IsDescending = filter.IsDescending,
                ProcedureIndexSortingColumn = (ProcedureIndexSortingColumns)filter.ProcedureIndexSortingColumn
            }), Times.Never);
        }
    }
}
