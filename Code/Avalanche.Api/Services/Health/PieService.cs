using Avalanche.Shared.Infrastructure.Services.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Runtime.ConstrainedExecution;
using Ism.PatientInfoEngine.Grpc;
using Ism.Security.Grpc.Interfaces;

using Ism.PatientInfoEngine.V1.Protos;
using static Ism.PatientInfoEngine.V1.Protos.PatientListService;
using static Ism.Storage.Core.PatientList.V1.Protos.PatientListStorage;
using Ism.Security.Grpc;
using Ism.Storage.PatientList.Client.V1;
using Ism.Storage.Core.PatientList.V1.Protos;
using System.Diagnostics.CodeAnalysis;

namespace Avalanche.Api.Services.Health
{
    [ExcludeFromCodeCoverage]
    public class PieService : IPieService
    {
        readonly IConfigurationService _configurationService;

        PatientListSecureClient PatientListServiceClient { get; set; }
        PatientListStorageSecureClient PatientListStorageClient { get; set; }

        public PieService(IConfigurationService configurationService, IGrpcClientFactory<PatientListServiceClient> grpcClientFactory, IGrpcClientFactory<PatientListStorageClient> storageClientFactory, ICertificateProvider certificateProvider)
        {
            _configurationService = configurationService;
            var hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");
            var pieAddress = _configurationService.GetEnvironmentVariable("pieServiceAddress");

            var pieServiceGrpcPort = _configurationService.GetEnvironmentVariable("pieServiceGrpcPort");
            var storageServiceGrpcPort = _configurationService.GetEnvironmentVariable("storageServiceGrpcPort");
            
            PatientListServiceClient = new PatientListSecureClient(grpcClientFactory, /*pieAddress*/hostIpAddress, pieServiceGrpcPort, certificateProvider);
            PatientListStorageClient = new PatientListStorageSecureClient(storageClientFactory, hostIpAddress, storageServiceGrpcPort, certificateProvider);
        }

        public async Task<Ism.PatientInfoEngine.V1.Protos.SearchResponse> Search(Ism.PatientInfoEngine.V1.Protos.SearchRequest searchRequest)
        {
            return await PatientListServiceClient.Search(searchRequest);
        }

        public async Task<Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordResponse> RegisterPatient(Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordRequest addPatientRecordRequest)
        {
            return await PatientListStorageClient.AddPatientRecord(addPatientRecordRequest);
        }

        public async Task UpdatePatient(Ism.Storage.Core.PatientList.V1.Protos.UpdatePatientRecordRequest updatePatientRecordRequest)
        {
            await PatientListStorageClient.UpdatePatientRecord(updatePatientRecordRequest);
        }

        public async Task<Ism.Storage.Core.PatientList.V1.Protos.DeletePatientRecordResponse> DeletePatient(Ism.Storage.Core.PatientList.V1.Protos.DeletePatientRecordRequest deletePatientRecordRequest)
        {
            return await PatientListStorageClient.DeletePatientRecord(deletePatientRecordRequest);
        }
    }
}
