using Avalanche.Api.Services.Health;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Testing;
using Ism.IsmLogCommon.Core;
using Ism.PatientInfoEngine.Common.Core;
using Ism.PatientInfoEngine.Common.Core.Protos;
using Ism.Storage.Common.Core.PatientList.Proto;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
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
            _service.IgnoreGrpcServicesMocks = true;
        }

        [Test]
        public void SearchAsyncShouldCallGrpcServiceAndReturnResponse()
        {
            int page = 1;
            int limit = 10;

            var cultureName = CultureInfo.CurrentCulture.Name;
            cultureName = string.IsNullOrEmpty(cultureName) ? "en-US" : cultureName;

            SearchResponse response = new SearchResponse()
            {
            };

            SearchRequest request = new SearchRequest()
            {
            };

            var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(response), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
            _mockListServiceClient.Setup(mock => mock.SearchAsync(request, null, null, CancellationToken.None)).Returns(fakeCall);

            _service.PatientListServiceClient = _mockListServiceClient.Object;

            var actionResult = _service.Search(It.IsAny<PatientSearchFieldsMessage>(), page * limit, limit, cultureName);

            _mockListServiceClient.Verify(mock => mock.SearchAsync(It.IsAny<SearchRequest>(), null, null, CancellationToken.None), Times.Once);

            Assert.AreSame(fakeCall, _mockListServiceClient.Object.SearchAsync(request));
        }

        [Test]
        public void RegisterPatientAsyncShouldCallGrpcServiceAndReturnResponse()
        {
            Patient newPatient = new Patient()
            { 
                FirstName = "Sample",
                DateOfBirth = DateTime.Now,
                Gender = "U",
                LastName = "Sample",
                MRN = "MRN",
            };

            var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(It.IsAny<AddPatientRecordResponse>()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
            _mockListStorageClient.Setup(mock => mock.AddPatientRecordAsync(It.IsAny<AddPatientRecordRequest>(), null, null, CancellationToken.None)).Returns(fakeCall);

            _service.PatientListStorageClient = _mockListStorageClient.Object;

            var actionResult = _service.RegisterPatient(newPatient, new ProcedureType() { Id = "Unknown" }, new Physician() { Id = "Unknown" });

            _mockListStorageClient.Verify(mock => mock.AddPatientRecordAsync(It.IsAny<AddPatientRecordRequest>(), null, null, CancellationToken.None), Times.Once);

            Assert.AreSame(fakeCall, _mockListStorageClient.Object.AddPatientRecordAsync(It.IsAny<AddPatientRecordRequest>()));
        }

        [Test]
        public void UpdatePatientAsyncShouldCallGrpcServiceAndReturnResponse()
        {
            Patient existinPatient = new Patient()
            {
                FirstName = "Sample",
                DateOfBirth = DateTime.Now,
                Gender = "U",
                LastName = "Sample",
                MRN = "MRN",
            };

            var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(It.IsAny<Empty>()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
            _mockListStorageClient.Setup(mock => mock.UpdatePatientRecordAsync(It.IsAny<UpdatePatientRecordRequest>(), null, null, CancellationToken.None)).Returns(fakeCall);

            _service.PatientListStorageClient = _mockListStorageClient.Object;

            var actionResult = _service.UpdatePatient(existinPatient);

            _mockListStorageClient.Verify(mock => mock.UpdatePatientRecordAsync(It.IsAny<UpdatePatientRecordRequest>(), null, null, CancellationToken.None), Times.Once);

            Assert.AreSame(fakeCall, _mockListStorageClient.Object.UpdatePatientRecordAsync(It.IsAny<UpdatePatientRecordRequest>()));
        }

        [Test]
        public void DeletePatientAsyncShouldCallGrpcServiceAndReturnResponse()
        {
            var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(It.IsAny<DeletePatientRecordResponse>()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
            _mockListStorageClient.Setup(mock => mock.DeletePatientRecordAsync(It.IsAny<DeletePatientRecordRequest>(), null, null, CancellationToken.None)).Returns(fakeCall);

            _service.PatientListStorageClient = _mockListStorageClient.Object;

            var actionResult = _service.DeletePatient(It.IsAny<ulong>());

            _mockListStorageClient.Verify(mock => mock.DeletePatientRecordAsync(It.IsAny<DeletePatientRecordRequest>(), null, null, CancellationToken.None), Times.Once);

            Assert.AreSame(fakeCall, _mockListStorageClient.Object.DeletePatientRecordAsync(It.IsAny<DeletePatientRecordRequest>()));
        }
    }
}
