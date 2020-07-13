using Avalanche.Api.Services.Configuration;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Grpc.Core.Testing;
using Grpc.Core;
using Ism.Storage.Common.Core.Configuration.Proto;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalanche.Api.ViewModels;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;

namespace Avalanche.Api.Tests.Services
{
    [TestFixture()]
    public class StorageServiceTests
    {
        Mock<IConfigurationService> _configurationService;

        Moq.Mock<ConfigurationStorage.ConfigurationStorageClient> _mockGrpcClient;
        StorageService _service;

        [SetUp]
        public void Setup()
        {
            _configurationService = new Mock<IConfigurationService>();

            _mockGrpcClient = new Moq.Mock<ConfigurationStorage.ConfigurationStorageClient>();

            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var certificateFile = assemblyFolder + @"/grpc_localhost_root_l1.pfx";

            if (!File.Exists(certificateFile))
                Console.WriteLine("Test certificate can not be reached.");

            _configurationService.Setup(mock => mock.GetEnvironmentVariable("hostIpAddress")).Returns("10.0.75.1");
            _configurationService.Setup(mock => mock.GetEnvironmentVariable("PatientListStoragePort")).Returns("8001");
            _configurationService.Setup(mock => mock.GetEnvironmentVariable("grpcCertificate")).Returns(certificateFile);
            _configurationService.Setup(mock => mock.GetEnvironmentVariable("grpcPassword")).Returns("0123456789");

            _service = new StorageService(_configurationService.Object);
            _service.IgnoreGrpcServicesMocks = true;
        }

        [Test]
        public void GetJsonShouldCallGrpcServiceAndReturnResponse()
        {
            GetConfigurationResponse response = new GetConfigurationResponse()
            { 
                ConfigurationJson = "[{}, {}]"
            };

            GetConfigurationRequest request = new GetConfigurationRequest()
            {
                Section = "Genders",
                Version = 1
            };

            var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(response), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
            _mockGrpcClient.Setup(mock => mock.GetConfigurationAsync(request, null, null, CancellationToken.None)).Returns(fakeCall);

            _service.ConfigurationStorageService = _mockGrpcClient.Object;

            var actionResult = _service.GetJson<List<KeyValuePairViewModel>>("Genders", 1);

            _mockGrpcClient.Verify(mock => mock.GetConfigurationAsync(It.IsAny<GetConfigurationRequest>(), null, null, CancellationToken.None), Times.Once);

            Assert.AreSame(fakeCall, _mockGrpcClient.Object.GetConfigurationAsync(request));
            Assert.AreEqual(actionResult.Result.Count, 2);
        }

        [Test]
        public void SaveJsonShouldCallGrpcService()
        {
            List<KeyValuePairViewModel> fakeData = new List<KeyValuePairViewModel>()
            {
                new KeyValuePairViewModel()
                {
                    Id = "S",
                    Value = "Sample",
                    TranslationKey = "SampleTranslationKey"
                }
            };

            SaveConfigurationRequest request = new SaveConfigurationRequest()
            {
                Section = "Sample",
                ConfigurationJson = JsonConvert.SerializeObject(fakeData)
            };

            var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
            _mockGrpcClient.Setup(mock => mock.SaveConfigurationAsync(request, null, null, CancellationToken.None)).Returns(fakeCall);

            _service.ConfigurationStorageService = _mockGrpcClient.Object;

            var actionResult = _service.SaveJson("Sample", fakeData);

            _mockGrpcClient.Verify(mock => mock.SaveConfigurationAsync(It.IsAny<SaveConfigurationRequest>(), null, null, CancellationToken.None), Times.Once);

            Assert.AreSame(fakeCall, _mockGrpcClient.Object.SaveConfigurationAsync(request));
        }
    }
}
