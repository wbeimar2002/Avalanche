using System.Collections.Generic;
using System.Threading.Tasks;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Infrastructure.Configuration;
using Avalanche.Shared.Infrastructure.Enumerations;

namespace Avalanche.Api.Managers.Maintenance
{
    public interface IMaintenanceManager
    {
        Task SaveCategory(DynamicSectionViewModel category);
        Task SaveEntityChanges(DynamicListViewModel category, DynamicListActions action);
        Task<DynamicSectionViewModel> GetCategoryByKey(string key);
        Task<DynamicListViewModel> GetCategoryListByKey(string key, string parentId);
        Task<List<dynamic>> GetListValues(string key);
        Task<dynamic> GetSettingValues(string key);
        Task SaveCategoryPolicies(DynamicSectionViewModel category);

        Task<ReindexStatusViewModel> ReindexRepository(ReindexRepositoryRequestViewModel reindexRequest);
        Task UpdateAutoLabelsConfigurationByProcedureType(int procedureTypeId, List<AutoLabelAutoLabelsConfiguration> autoLabels);

        GeneralApiConfiguration GetGeneralApiConfigurationSettings();
        ProceduresSearchConfiguration GetProceduresSearchConfigurationSettings();
        AutoLabelsConfiguration GetAutoLabelsConfigurationSettings(int procedureTypeId);
        LabelsConfiguration GetLabelsConfigurationSettings();
        SetupConfiguration GetSetupConfigurationSettings();
        PrintingConfiguration GetPrintingConfigurationSettings();
        RecorderConfiguration GetRecorderConfigurationSettings();
    }
}
