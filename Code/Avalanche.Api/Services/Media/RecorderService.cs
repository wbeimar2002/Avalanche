using Avalanche.Shared.Infrastructure.Services.Settings;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Core.Testing;
using Ism.Recorder.Client.V1;
using Ism.Recorder.Core.V1.Protos;
using Ism.Security.Grpc.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using static Ism.Recorder.Core.V1.Protos.Recorder;

namespace Avalanche.Api.Services.Media
{
    public class RecorderService : IRecorderService
    {
        readonly IConfigurationService _configurationService;

        public RecorderSecureClient RecorderClient { get; set; }

        public RecorderService(IConfigurationService configurationService, IGrpcClientFactory<RecorderClient> grpcClientFactory, ICertificateProvider certificateProvider)
        {
            _configurationService = configurationService;

            var hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");
            var mediaServiceGrpcPort = _configurationService.GetEnvironmentVariable("mediaServiceGrpcPort");

            RecorderClient = new RecorderSecureClient(grpcClientFactory, hostIpAddress, mediaServiceGrpcPort, certificateProvider);
        }

        public async Task StartRecording(RecordMessage recordMessage) => await RecorderClient.StartRecording(recordMessage);

        public async Task StopRecording() => await RecorderClient.StopRecording();
    }
}
