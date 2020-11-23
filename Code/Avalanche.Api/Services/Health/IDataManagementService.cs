using Ism.Storage.Core.DataManagement.V1.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Health
{
    public interface IDataManagementService
    {
        Task<AddDepartmentResponse> AddDepartment(AddDepartmentRequest request);
        Task<AddProcedureTypeResponse> AddProcedureType(AddProcedureTypeRequest request);
        Task DeleteDepartment(DeleteDepartmentRequest request);
        Task DeleteProcedureType(DeleteProcedureTypeRequest request);
        Task<GetDepartmentsResponse> GetAllDepartments();
        Task<GetProceduresByDepartmentResponse> GetProceduresByDepartment(GetProceduresByDepartmentRequest request);
        Task<ProcedureTypeMessage> GetProcedureType(ProcedureTypeMessage request);
    }
}
