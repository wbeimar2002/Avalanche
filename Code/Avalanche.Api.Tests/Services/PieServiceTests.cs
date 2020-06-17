using Avalanche.Api.Mapping.Health;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Grpc.Core;
using Grpc.Core.Testing;
using Ism.IsmLogCommon.Core;
using Ism.PatientInfoEngine.Common.Core;
using Ism.PatientInfoEngine.Common.Core.Models;
using Ism.Storage.Common.Core.PatientList.Models;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Avalanche.Api.Tests.Services
{
    [TestFixture()]
    public class PieServiceTests
    {
        Mock<IConfigurationService> _configurationService;
        Mock<IPatientList> _mockPatListClient;
        Mock<IAccessInfoFactory> _mockAccessInfoFactory;
        PieService _service;


        [SetUp]
        public void Setup()
        {
            _configurationService = new Mock<IConfigurationService>();
            _mockPatListClient = new Mock<IPatientList>();
            _mockAccessInfoFactory = new Mock<IAccessInfoFactory>();

            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var certificateFile = assemblyFolder + @"/grpc_localhost_root_l1.pfx";

            if (!File.Exists(certificateFile))
                Console.WriteLine("Test certificate can not be reached.");

            _configurationService.Setup(mock => mock.GetEnvironmentVariable("hostIpAddress")).Returns("10.0.75.1");
            _configurationService.Setup(mock => mock.GetEnvironmentVariable("PatientInfoEngineGrpcPort")).Returns("9089");
            _configurationService.Setup(mock => mock.GetEnvironmentVariable("grpcCertificate")).Returns(certificateFile);
            _configurationService.Setup(mock => mock.GetEnvironmentVariable("grpcPassword")).Returns("0123456789");
            _mockAccessInfoFactory.Setup(mock => mock.GenerateAccessInfo(It.IsAny<string>())).Returns(new Ism.IsmLogCommon.Core.AccessInfo("192.168.0.1", "use", "app", "machine", "details", false));

            _service = new PieService(_configurationService.Object, new PieMapping(), _mockAccessInfoFactory.Object);
        }

        [Test]
        public void ExecuteSearchShouldReturnResponse()
        {
            PatientKeywordSearchFilterViewModel search = new PatientKeywordSearchFilterViewModel()
            {
                Limit = 50,
                Page = 0,
                Term = "last"
            };

            var response = new List<PatientRecord>
            {
                new PatientRecord 
                { 
                    Id= 0, 
                    MRN = "mrn",
                    Patient = new Ism.Storage.Common.Core.PatientList.Models.Patient("first", "last", new NodaTime.LocalDate(), Ism.Storage.Common.Core.PatientList.Enums.Sex.F) 
                }
            };

            _mockPatListClient
                .Setup(mock => mock.Search(It.IsAny<PatientSearchFields>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<AccessInfo>()))
                .ReturnsAsync(response);

            _service.Client= _mockPatListClient.Object;

            var actionResult = _service.Search(search);

            _mockPatListClient.Verify(mock => mock.Search(Moq.It.IsAny<PatientSearchFields>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<AccessInfo>()), Times.Once);

            Assert.AreEqual(1, actionResult.Result.Count);
            Assert.AreEqual(0, actionResult.Result[0].Id);
            Assert.AreEqual("last", actionResult.Result[0].LastName);
            Assert.AreEqual("mrn", actionResult.Result[0].MRN);
        }
    }
}
