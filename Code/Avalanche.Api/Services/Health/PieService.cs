using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.PatientInfoEngine.Grpc;
using Ism.Security.Grpc.Interfaces;
using Ism.Storage.PatientList.Client.V1;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using static Ism.PatientInfoEngine.V1.Protos.PatientList;
using static Ism.Storage.PatientList.Client.V1.Protos.PatientListStorage;

namespace Avalanche.Api.Services.Health
{
    [ExcludeFromCodeCoverage]
    public class PieService : IPieService
    {
        readonly IConfigurationService _configurationService;

        PatientListSecureClient PatientListServiceClient { get; set; }
        PatientListStorageSecureClient PatientListStorageClient { get; set; }

        public PieService(IConfigurationService configurationService, IGrpcClientFactory<PatientListClient> grpcClientFactory, IGrpcClientFactory<PatientListStorageClient> storageClientFactory, ICertificateProvider certificateProvider)
        {
            _configurationService = configurationService;
            var hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");
            var pieAddress = _configurationService.GetEnvironmentVariable("pieServiceAddress");

            var pieServiceGrpcPort = _configurationService.GetEnvironmentVariable("pieServiceGrpcPort");
            var storageServiceGrpcPort = _configurationService.GetEnvironmentVariable("storageServiceGrpcPort");
            
            PatientListServiceClient = new PatientListSecureClient(grpcClientFactory, /*pieAddress*/hostIpAddress, System.Convert.ToUInt32(pieServiceGrpcPort), certificateProvider);
            PatientListStorageClient = new PatientListStorageSecureClient(storageClientFactory, hostIpAddress, storageServiceGrpcPort, certificateProvider);
        }

        public async Task<Ism.PatientInfoEngine.V1.Protos.SearchResponse> Search(Ism.PatientInfoEngine.V1.Protos.SearchRequest searchRequest)
        {
            return await PatientListServiceClient.Search(searchRequest);
        }

        public async Task<Ism.Storage.PatientList.Client.V1.Protos.AddPatientRecordResponse> RegisterPatient(Ism.Storage.PatientList.Client.V1.Protos.AddPatientRecordRequest addPatientRecordRequest)
        {
            return await PatientListStorageClient.AddPatientRecord(addPatientRecordRequest);
        }

        public async Task UpdatePatient(Ism.Storage.PatientList.Client.V1.Protos.UpdatePatientRecordRequest updatePatientRecordRequest)
        {
            await PatientListStorageClient.UpdatePatientRecord(updatePatientRecordRequest);
        }

        public async Task<Ism.Storage.PatientList.Client.V1.Protos.DeletePatientRecordResponse> DeletePatient(Ism.Storage.PatientList.Client.V1.Protos.DeletePatientRecordRequest deletePatientRecordRequest)
        {
            return await PatientListStorageClient.DeletePatientRecord(deletePatientRecordRequest);
        }
    }
}
