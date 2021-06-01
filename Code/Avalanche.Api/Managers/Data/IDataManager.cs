using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Data
{
    public interface IDataManager
    {
        Task<DepartmentModel> AddDepartment(DepartmentModel department);
        Task DeleteDepartment(int departmentId);
        Task<IList<DepartmentModel>> GetAllDepartments();
        Task<ProcedureTypeModel> AddProcedureType(ProcedureTypeModel procedureType);
        Task DeleteProcedureType(ProcedureTypeModel procedureType);
        Task<List<ProcedureTypeModel>> GetProcedureTypesByDepartment(int? departmentId);
        Task ValidateDepartmentsSupport();
        Task ValidateDepartmentsSupport(int? departmentId);
        Task<List<ProcedureTypeModel>> GetAllProcedureTypes();
        Task<List<dynamic>> GetList(string sourceKey, string jsonKey = null);
    }
}
