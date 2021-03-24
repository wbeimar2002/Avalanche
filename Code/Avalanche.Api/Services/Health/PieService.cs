using Ism.PatientInfoEngine.Grpc;
using Ism.Storage.PatientList.Client.V1;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Health
{
    [ExcludeFromCodeCoverage]
    public class PieService : IPieService
    {
        private readonly PatientListSecureClient _patientListClient;
        private readonly PatientListStorageSecureClient _storageClient;

        public PieService(PatientListStorageSecureClient storageClient, PatientListSecureClient patientListClient)
        {
            _patientListClient = patientListClient;
            _storageClient = storageClient;
        }

        public async Task<Ism.Storage.PatientList.Client.V1.Protos.DeletePatientRecordResponse> DeletePatient(Ism.Storage.PatientList.Client.V1.Protos.DeletePatientRecordRequest deletePatientRecordRequest)
        {
            return await _storageClient.DeletePatientRecord(deletePatientRecordRequest);
        }

        public async Task<Ism.Storage.PatientList.Client.V1.Protos.AddPatientRecordResponse> RegisterPatient(Ism.Storage.PatientList.Client.V1.Protos.AddPatientRecordRequest addPatientRecordRequest)
        {
            return await _storageClient.AddPatientRecord(addPatientRecordRequest);
        }

        public async Task<Ism.PatientInfoEngine.V1.Protos.SearchResponse> Search(Ism.PatientInfoEngine.V1.Protos.SearchRequest searchRequest)
        {
            return await _patientListClient.Search(searchRequest);
        }

        public async Task UpdatePatient(Ism.Storage.PatientList.Client.V1.Protos.UpdatePatientRecordRequest updatePatientRecordRequest)
        {
            await _storageClient.UpdatePatientRecord(updatePatientRecordRequest);
        }
    }
}
