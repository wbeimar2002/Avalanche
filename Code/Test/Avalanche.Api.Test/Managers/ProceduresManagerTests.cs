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
using Avalanche.Shared.Infrastructure.Configuration;
using Ism.Library.V1.Protos;
using Ism.SystemState.Client;
using Moq;
using NUnit.Framework;

namespace Avalanche.Api.Test.Managers
{
    [TestFixture()]
    public class ProceduresManagerTests
    {
        IMapper _mapper;
        Mock<IAccessInfoFactory> _accessInfoFactory;
        Mock<ILibraryService> _libraryService;
        Mock<IStateClient> _stateClient;
        Mock<IDataManager> _dataManager;
        LabelsConfiguration _labelsConfig;
        SetupConfiguration _setupConfiguration;

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
            _stateClient = new Mock<IStateClient>();
            _dataManager = new Mock<IDataManager>();

            _labelsConfig = new LabelsConfiguration();
            _setupConfiguration = new SetupConfiguration();

            _manager = new ProceduresManager(_stateClient.Object, _libraryService.Object, _accessInfoFactory.Object, _mapper, _dataManager.Object, _labelsConfig, _setupConfiguration);
        }

        [Test]
        public async Task GetProcedureDetails_VerifyCalls()
        {
            var procedureId = new ProcedureIdViewModel("2021_06_18T19_52_44_TODO", "cache");

            var response = new GetFinishedProcedureResponse();
            { };

            _libraryService.Setup(mock => mock.GetFinishedProcedure(new GetFinishedProcedureRequest()
            {
                LibraryId = procedureId.Id,
                RepositoryName = procedureId.RepositoryName

            })).ReturnsAsync(response);

            await _manager.GetProcedureDetails(procedureId);

            _libraryService.Verify(mock => mock.GetFinishedProcedure(new GetFinishedProcedureRequest()
            {
                LibraryId = procedureId.Id,
                RepositoryName = procedureId.RepositoryName
            }), Times.Once);
        }

        [Test]
        [TestCase("   ")]
        [TestCase("")]
        [TestCase(null)]
        public async Task GetProcedureDetails_FailsWithEmptyOrWhiteSpaceLibraryId(string libraryId)
        {
            var procedureId = new ProcedureIdViewModel(libraryId, "cache");

            var response = new GetFinishedProcedureResponse();

            _libraryService.Setup(mock => mock.GetFinishedProcedure(new GetFinishedProcedureRequest()
            {
                LibraryId = string.Empty,
                RepositoryName = procedureId.RepositoryName
            })).ReturnsAsync(response);

            Task Act() => _manager.GetProcedureDetails(procedureId);

            Assert.That(Act, Throws.TypeOf<ArgumentNullException>());

            _libraryService.Verify(mock => mock.GetFinishedProcedure(new GetFinishedProcedureRequest()
            {
                LibraryId = string.Empty,
                RepositoryName = procedureId.RepositoryName
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


        [Test]
        public async Task GenerateProcedureZipTest()
        {
            var procedureId = new ProcedureIdViewModel("2021_06_18T19_52_44_TODO", "cache");
            var procedureZipRequest = new ProcedureZipRequestViewModel()
            {
                ProcedureId = procedureId,
                MediaFileNameList = new List<string> { "BX4RecA_2021_10_26T14_28_25_184.jpg", "BX4RecA_2021_10_26T14_28_25_184_t.jpg" },
                RequestId = "test"
            };

            _libraryService.Setup(mock => mock.GenerateProcedureZip(It.IsAny<GenerateProcedureZipRequest>()));
            await _manager.GenerateProcedureZip(procedureZipRequest).ConfigureAwait(true);
            _libraryService.Verify(mock => mock.GenerateProcedureZip(It.IsAny<GenerateProcedureZipRequest>()), Times.Once);
        }
    }
}
