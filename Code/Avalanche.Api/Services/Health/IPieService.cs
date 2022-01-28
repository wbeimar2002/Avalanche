using System.Threading.Tasks;
using Ism.PatientInfoEngine.V1.Protos;

namespace Avalanche.Api.Services.Health
{
    public interface IPieService
    {
        Task<SearchResponse> Search(SearchRequest searchRequest);
        Task<AddPatientRecordResponse> RegisterPatient(AddPatientRecordRequest addPatientRecordRequest);
        Task UpdatePatient(UpdatePatientRecordRequest updatePatientRecordRequest);
        Task<DeletePatientRecordResponse> DeletePatient(DeletePatientRecordRequest deletePatientRecordRequest);
        //Task<GetSourceResponse> GetPatientListSource();
    }
}
