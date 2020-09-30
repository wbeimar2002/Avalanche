using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.Security.Grpc.Helpers;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Health
{
    public class PieService : IPieService
    {
        readonly IConfigurationService _configurationService;
        readonly string _hostIpAddress;

        public bool IgnoreGrpcServicesMocks { get; set; }

        public Ism.PatientInfoEngine.V1.Protos.PatientListService.PatientListServiceClient PatientListServiceClient { get; set; }
        public Ism.Storage.Core.PatientList.V1.Protos.PatientListStorage.PatientListStorageClient PatientListStorageClient { get; set; }

        public PieService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");

            var pieServiceGrpcPort = _configurationService.GetEnvironmentVariable("pieServiceGrpcPort");
            var storageServiceGrpcPort = _configurationService.GetEnvironmentVariable("storageServiceGrpcPort");

            var grpcCertificate = _configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = _configurationService.GetEnvironmentVariable("grpcPassword");

            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(grpcCertificate, grpcPassword);

            PatientListServiceClient = ClientHelper.GetInsecureClient<Ism.PatientInfoEngine.V1.Protos.PatientListService.PatientListServiceClient>($"https://{_hostIpAddress}:{pieServiceGrpcPort}");
            PatientListStorageClient = ClientHelper.GetInsecureClient<Ism.Storage.Core.PatientList.V1.Protos.PatientListStorage.PatientListStorageClient>($"https://{_hostIpAddress}:{storageServiceGrpcPort}");

            //PatientListServiceClient = ClientHelper.GetSecureClient<PatientListService.PatientListServiceClient>($"https://{_hostIpAddress}:{patientListServiceGrpcPort}", certificate);
            //PatientListStorageClient = ClientHelper.GetSecureClient<PatientListStorage.PatientListStorageClient>($"https://{_hostIpAddress}:{patientListStorageGrpcPort}", certificate);
        }

        public async Task<Ism.PatientInfoEngine.V1.Protos.SearchResponse> Search(Ism.PatientInfoEngine.V1.Protos.SearchRequest searchRequest)
        {
            return await PatientListServiceClient.SearchAsync(searchRequest);
        }

        public async Task<Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordResponse> RegisterPatient(Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordRequest addPatientRecordRequest)
        {
            return await PatientListStorageClient.AddPatientRecordAsync(addPatientRecordRequest);
        }

        public async Task UpdatePatient(Ism.Storage.Core.PatientList.V1.Protos.UpdatePatientRecordRequest updatePatientRecordRequest)
        {
            await PatientListStorageClient.UpdatePatientRecordAsync(updatePatientRecordRequest);
        }

        public async Task<Ism.Storage.Core.PatientList.V1.Protos.DeletePatientRecordResponse> DeletePatient(Ism.Storage.Core.PatientList.V1.Protos.DeletePatientRecordRequest deletePatientRecordRequest)
        {
            return await PatientListStorageClient.DeletePatientRecordAsync(deletePatientRecordRequest);
        }
    }
}
