using Avalanche.Api.Services.Health;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Grpc.Core;
using Grpc.Core.Testing;
using Ism.IsmLogCommon.Core;
using Ism.PatientInfoEngine.Common.Core;
using Ism.Storage.Common.Core.PatientList.Proto;
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
        Mock<IAccessInfoFactory> _accessInfoFactory;

        Moq.Mock<PatientListService.PatientListServiceClient> _mockListServiceClient;
        Moq.Mock<PatientListStorage.PatientListStorageClient> _mockListStorageClient;

        PieService _service;

        [SetUp]
        public void Setup()
        {
            _configurationService = new Mock<IConfigurationService>();
            _accessInfoFactory = new Mock<IAccessInfoFactory>();
            _mockListServiceClient = new Moq.Mock<PatientListService.PatientListServiceClient>();
            _mockListStorageClient = new Moq.Mock<PatientListStorage.PatientListStorageClient>();

            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var certificateFile = assemblyFolder + @"/grpc_localhost_root_l1.pfx";

            if (!File.Exists(certificateFile))
                Console.WriteLine("Test certificate can not be reached.");

            _configurationService.Setup(mock => mock.GetEnvironmentVariable("hostIpAddress")).Returns("10.0.75.1");
            _configurationService.Setup(mock => mock.GetEnvironmentVariable("WebRTCGrpcPort")).Returns("8001");
            _configurationService.Setup(mock => mock.GetEnvironmentVariable("grpcCertificate")).Returns(certificateFile);
            _configurationService.Setup(mock => mock.GetEnvironmentVariable("grpcPassword")).Returns("0123456789");

            _accessInfoFactory.Setup(mock => mock.GenerateAccessInfo(It.IsAny<string>())).Returns(new Ism.IsmLogCommon.Core.AccessInfo("192.168.0.1", "use", "app", "machine", "details", false));

            _service = new PieService(_configurationService.Object, _accessInfoFactory.Object);
        }


    }
}
