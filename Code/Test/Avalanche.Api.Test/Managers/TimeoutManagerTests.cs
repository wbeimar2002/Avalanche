using AutoMapper;
using Avalanche.Api.Managers.Media;
using Avalanche.Api.MappingConfigurations;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Api.ViewModels;
using Ism.PgsTimeout.V1.Protos;
using Ism.Routing.V1.Protos;
using Ism.SystemState.Client;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Avalanche.Api.Test.Managers
{
    [TestFixture()]
    public class TimeoutManagerTests
    {
        Mock<IStorageService> _storageService;
        Mock<IRoutingService> _routingService;
        Mock<IStateClient> _stateClient;
        Mock<IPgsTimeoutService> _pgsTimeoutService;
        Mock<IPgsManager> _pgsManager;
        
        IMapper _mapper;
        ITimeoutManager _timeoutManager;

        [SetUp]
        public void Setup()
        {
            _storageService = new Mock<IStorageService>();
            _routingService = new Mock<IRoutingService>();
            _stateClient = new Mock<IStateClient>();
            _pgsTimeoutService = new Mock<IPgsTimeoutService>();
            _pgsManager = new Mock<IPgsManager>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MediaMappingConfiguration());
            });

            _mapper = config.CreateMapper();

            _timeoutManager = new TimeoutManager(_storageService.Object,
                                                    _routingService.Object,
                                                    _pgsTimeoutService.Object,
                                                    _pgsManager.Object,
                                                    _mapper);
        }

        [Test]
        public async Task GetTimeoutPdfPath_ValidFile_Test()
        {
            var testPdfPath = "TestPdf";
            _pgsTimeoutService.Setup(x => x.GetTimeoutPdfPath()).Returns(Task.FromResult<GetTimeoutPdfPathResponse>(new GetTimeoutPdfPathResponse {PdfPath = testPdfPath }));
            var pdfPath = await _timeoutManager.GetTimeoutPdfPath();

            Assert.NotNull(pdfPath);
            Assert.IsNotEmpty(pdfPath);
            Assert.AreEqual(testPdfPath, pdfPath);
        }

        [Test]
        public async Task GetTimeoutPdfPath_NoFile_Test()
        {
            var pdfPath = await _timeoutManager.GetTimeoutPdfPath();
            Assert.Null(pdfPath);
        }

        [Test]
        public async Task GetTimeoutPdfPath_Called()
        {
            _pgsTimeoutService.Setup(mock => mock.GetTimeoutPdfPath()).ReturnsAsync(new GetTimeoutPdfPathResponse() { PdfPath = "Sample" });
            var pdfPath = await _timeoutManager.GetTimeoutPdfPath();

            _pgsTimeoutService.Verify(mock => mock.GetTimeoutPdfPath(), Times.Once);
        }

        [Test]
        public async Task SetTimeoutPage_Called()
        {
            _pgsTimeoutService.Setup(x => x.SetTimeoutPage(It.IsAny<SetTimeoutPageRequest>()));
            await _timeoutManager.SetTimeoutPage(0);

            _pgsTimeoutService.Verify(mock => mock.SetTimeoutPage(It.IsAny<SetTimeoutPageRequest>()), Times.Once);
        }

        [Test]
        public async Task GetTimeoutPage_Called()
        {
            _pgsTimeoutService.Setup(x => x.GetTimeoutPage());
            await _timeoutManager.GetTimeoutPage();

            _pgsTimeoutService.Verify(mock => mock.GetTimeoutPage(), Times.Once);
        }

        [Test]
        public async Task GetTimeoutPageCount_Called()
        {
            _pgsTimeoutService.Setup(x => x.GetTimeoutPageCount());
            await _timeoutManager.GetTimeoutPageCount();

            _pgsTimeoutService.Verify(mock => mock.GetTimeoutPageCount(), Times.Once);
        }

        [Test]
        public async Task TimeoutNextPage_Called()
        {
            _pgsTimeoutService.Setup(x => x.NextPage());
            await _timeoutManager.NextPage();

            _pgsTimeoutService.Verify(mock => mock.NextPage(), Times.Once);
        }

        [Test]
        public async Task TimeoutPreviousPage_Called()
        {
            _pgsTimeoutService.Setup(x => x.PreviousPage());
            await _timeoutManager.PreviousPage();

            _pgsTimeoutService.Verify(mock => mock.PreviousPage(), Times.Once);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task TimeoutState_StartTimeout_DoesNotThrow(bool value)
        {
            _pgsTimeoutService.Setup(x => x.GetPgsPlaybackState()).Returns(Task.FromResult(new GetPgsPlaybackStateResponse { IsPlaying = true}));
            _routingService.Setup(x => x.GetVideoSinks())
                .Returns(Task.FromResult(new GetVideoSinksResponse()));

            await _timeoutManager.StartTimeout();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task TimeoutState_StopTimeout_DoesNotThrow(bool value)
        {
            _pgsTimeoutService.Setup(x => x.GetPgsPlaybackState()).Returns(Task.FromResult(new GetPgsPlaybackStateResponse { IsPlaying = true }));
            _routingService.Setup(x => x.GetVideoSinks())
                .Returns(Task.FromResult(new GetVideoSinksResponse()));

            await _timeoutManager.StopTimeout(true);
        }
    }
}
