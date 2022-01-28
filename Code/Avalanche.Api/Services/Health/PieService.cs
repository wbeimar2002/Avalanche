using Google.Protobuf.WellKnownTypes;
using Ism.PatientInfoEngine.Grpc;
using Ism.PatientInfoEngine.V1.Protos;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Health
{
    [ExcludeFromCodeCoverage]
    public class PieService : IPieService
    {
        private readonly PatientListSecureClient _patientListClient;

        public PieService(PatientListSecureClient patientListClient) => _patientListClient = patientListClient;

        public async Task<DeletePatientRecordResponse> DeletePatient(DeletePatientRecordRequest deletePatientRecordRequest) =>
            await _patientListClient.DeletePatientRecord(deletePatientRecordRequest).ConfigureAwait(false);

        public async Task<AddPatientRecordResponse> RegisterPatient(AddPatientRecordRequest addPatientRecordRequest) =>
            await _patientListClient.AddPatientRecord(addPatientRecordRequest).ConfigureAwait(false);

        public async Task<SearchResponse> Search(SearchRequest searchRequest) =>
            await _patientListClient.Search(searchRequest).ConfigureAwait(false);

        public async Task UpdatePatient(UpdatePatientRecordRequest updatePatientRecordRequest) =>
            await _patientListClient.UpdatePatientRecord(updatePatientRecordRequest).ConfigureAwait(false);

        //public async Task<GetSourceResponse> GetPatientListSource() =>
        //    await _patientListClient.GetPatientListSource().ConfigureAwait(false);
    }
}
