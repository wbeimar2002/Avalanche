using Avalanche.Api.Utilities.Files;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core.Testing;
using Ism.Security.Grpc.Helpers;
using Ism.Storage.Common.Core.Configuration.Proto;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Avalanche.Shared.Infrastructure.Extensions;
using Newtonsoft.Json;
using System.IO;

namespace Avalanche.Api.Services.Configuration
{
    public class StorageService : IStorageService
    {
        readonly IConfigurationService _configurationService;
        readonly string _hostIpAddress;

        public bool IgnoreGrpcServicesMocks { get; set; }

        public PatientListStorage.PatientListStorageClient PatientListStorageClient { get; set; }

        public StorageService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;

            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");

            var PatientListStoragePort = _configurationService.GetEnvironmentVariable("PatientListStoragePort");
            var grpcCertificate = _configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = _configurationService.GetEnvironmentVariable("grpcPassword");

            IgnoreGrpcServicesMocks = Convert.ToBoolean(_configurationService.GetEnvironmentVariable("IgnoreGrpcServicesMocks"));

            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(grpcCertificate, grpcPassword);

            //Client = ClientHelper.GetSecureClient<WebRtcStreamer.WebRtcStreamerClient>($"https://{hostIpAddress}:{WebRTCGrpcPort}", certificate);
            PatientListStorageClient = ClientHelper.GetInsecureClient<PatientListStorage.PatientListStorageClient>($"https://{_hostIpAddress}:{PatientListStoragePort}");
        }

        public async Task<T> GetJson<T>(string configurationKey, int version)
        {
            var request = new GetConfigurationRequest()
            {
                Version = Convert.ToUInt32(version),
                Section = configurationKey
            };
            
            //Faking calls while I have the real server
            if (!IgnoreGrpcServicesMocks)
            {
                Mock<PatientListStorage.PatientListStorageClient> mockGrpcClient = new Mock<PatientListStorage.PatientListStorageClient>();
                var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new GetConfigurationResponse() { ConfigurationJson = string.Empty }), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
                mockGrpcClient.Setup(mock => mock.GetConfigurationAsync(request, null, null, CancellationToken.None)).Returns(fakeCall);

                PatientListStorageClient = mockGrpcClient.Object;
            }

            var actionResponse = await PatientListStorageClient.GetConfigurationAsync(request);

            return actionResponse.ConfigurationJson.Get<T>();
        }

        public async Task SaveJson<T>(string configurationKey, T jsonObject)
        {
            //Faking calls while I have the real server
            if (!IgnoreGrpcServicesMocks)
            {
                Mock<PatientListStorage.PatientListStorageClient> mockGrpcClient = new Mock<PatientListStorage.PatientListStorageClient>();
                var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new Empty()), Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
                mockGrpcClient.Setup(mock => mock.SaveConfigurationAsync(Moq.It.IsAny<SaveConfigurationRequest>(), null, null, CancellationToken.None)).Returns(fakeCall);

                PatientListStorageClient = mockGrpcClient.Object;

                //await Task.Run(() =>
                //{
                //    var fileName = $"/config/{configurationKey}.json";
                //    var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                //    var filePath = Path.Combine(documentsPath, fileName);

                //    if (File.Exists(filePath))
                //    {
                //        File.Delete(filePath);
                //    }
                //    string result = JsonConvert.SerializeObject(jsonObject);
                //    File.WriteAllText(filePath, result);
                //});
            }

            var actionResponse = await PatientListStorageClient.SaveConfigurationAsync(new SaveConfigurationRequest()
            {
                Section = configurationKey,
                ConfigurationJson = jsonObject.Json()
            });
        }
    }
}
