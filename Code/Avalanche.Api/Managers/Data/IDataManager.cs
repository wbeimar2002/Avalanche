using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Data
{
    public interface IDataManager
    {
        Task<IList<KeyValuePairViewModel>> GetMetadata(MetadataTypes type);
        Task<IList<DynamicSourceKeyValuePairViewModel>> GetSource(MetadataTypes settingTypes);
        Task<Department> AddDepartment(Department department);
        Task DeleteDepartment(int departmentId);
        Task<IList<Department>> GetAllDepartments();
        Task<ProcedureType> AddProcedureType(ProcedureType procedureType);
        Task DeleteProcedureType(ProcedureType procedureType);
        Task<List<ProcedureType>> GetProcedureTypesByDepartment(int? departmentId);
        Task ValidateDepartmentsSupport();
        Task ValidateDepartmentsSupport(int? departmentId);
        Task<ExpandoObject> GetDynamicSource(string key);
        Task<List<ProcedureType>> GetAllProcedureTypes();
    }
}
