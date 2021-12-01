using System.Collections.Generic;
using System.Threading.Tasks;
using Avalanche.Shared.Domain.Models;

namespace Avalanche.Api.Managers.Data
{
    public interface IDataManager
    {
        Task<List<dynamic>> GetList(string sourceKey, string jsonKey = null);
        Task<DepartmentModel> AddDepartment(DepartmentModel department);
        Task DeleteDepartment(int departmentId);
        Task<IList<DepartmentModel>> GetAllDepartments();
        Task<ProcedureTypeModel> AddProcedureType(ProcedureTypeModel procedureType);
        Task DeleteProcedureType(ProcedureTypeModel procedureType);
        Task<List<ProcedureTypeModel>> GetProcedureTypesByDepartment(int? departmentId);
        Task ValidateDepartmentsSupport();
        Task ValidateDepartmentsSupport(int? departmentId);
        Task<List<ProcedureTypeModel>> GetAllProcedureTypes();
        Task<LabelModel> AddLabel(LabelModel label);
        Task UpdateLabel(LabelModel label);
        Task DeleteLabel(LabelModel label);
        Task<List<LabelModel>> GetLabelsByProcedureType(int? procedureTypeId);
        Task<List<LabelModel>> GetAllLabels();
        Task<LabelModel> GetLabel(string labelName, int? procedureTypeId);
        Task<UserModel> AddUser(UserModel user);
        Task UpdateUser(UserModel user);
        Task DeleteUser(int userId);
        Task<IList<UserModel>> GetAllUsers();
    }
}
