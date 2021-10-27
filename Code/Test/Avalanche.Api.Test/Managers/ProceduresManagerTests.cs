using System;
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
        GeneralApiConfiguration _generalApiConfig;
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

            _generalApiConfig = new GeneralApiConfiguration();
            _setupConfiguration = new SetupConfiguration();

            _manager = new ProceduresManager(_stateClient.Object, _libraryService.Object, _accessInfoFactory.Object, _mapper, _dataManager.Object, _generalApiConfig, _setupConfiguration);
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
