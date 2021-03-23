using Google.Protobuf.WellKnownTypes;

using Ism.Storage.DataManagement.Client.V1;
using Ism.Storage.DataManagement.Client.V1.Protos;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Health
{
    [ExcludeFromCodeCoverage]
    public class DataManagementService : IDataManagementService
    {
        private readonly DataManagementStorageSecureClient _client;

        public DataManagementService(DataManagementStorageSecureClient client)
        {
            _client = client;
        }

        public async Task<AddDepartmentResponse> AddDepartment(AddDepartmentRequest request) => await _client.AddDepartment(request);

        public async Task<AddProcedureTypeResponse> AddProcedureType(AddProcedureTypeRequest request) => await _client.AddProcedureType(request);

        public async Task DeleteDepartment(DeleteDepartmentRequest request) => await _client.DeleteDepartment(request);

        public async Task DeleteProcedureType(DeleteProcedureTypeRequest request) => await _client.DeleteProcedureType(request);

        public async Task<GetDepartmentsResponse> GetAllDepartments() => await _client.GetAllDepartments(new Empty());

        public async Task<GetProcedureTypesResponse> GetProcedureTypesByDepartment(GetProcedureTypesByDepartmentRequest request) => await _client.GetProcedureTypesByDepartment(request);

        public async Task<GetProcedureTypesResponse> GetAllProcedureTypes() => await _client.GetProceduresTypes(new Empty());

        public async Task<ProcedureTypeMessage> GetProcedureType(GetProcedureTypeRequest request) => await _client.GetProcedureType(request);
    }
}
