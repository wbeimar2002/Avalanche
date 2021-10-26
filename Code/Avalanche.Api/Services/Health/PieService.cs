using Ism.PatientInfoEngine.Grpc;
using Ism.PatientInfoEngine.V1.Protos;
using Ism.Storage.PatientList.Client.V1;
using Ism.Storage.PatientList.Client.V1.Protos;
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

        public async Task<DeletePatientRecordResponse> DeletePatient(DeletePatientRecordRequest deletePatientRecordRequest) =>
            await _storageClient.DeletePatientRecord(deletePatientRecordRequest).ConfigureAwait(false);

        public async Task<AddPatientRecordResponse> RegisterPatient(AddPatientRecordRequest addPatientRecordRequest) =>
            await _storageClient.AddPatientRecord(addPatientRecordRequest).ConfigureAwait(false);

        public async Task<SearchResponse> Search(SearchRequest searchRequest) =>
            await _patientListClient.Search(searchRequest).ConfigureAwait(false);

        public async Task UpdatePatient(UpdatePatientRecordRequest updatePatientRecordRequest) =>
            await _storageClient.UpdatePatientRecord(updatePatientRecordRequest).ConfigureAwait(false);
    }
}
