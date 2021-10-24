using System.Collections.Generic;
using System.Threading.Tasks;
using Avalanche.Shared.Infrastructure.Configuration;

namespace Avalanche.Api.Managers.Maintenance
{
    public interface IConfigurationManager
    {
        AutoLabelsConfiguration GetAutoLabelsConfigurationSettings(int? procedureTypeId);
        LabelsConfiguration GetLabelsConfigurationSettings();
        SetupConfiguration GetSetupConfigurationSettings();
        RecorderConfiguration GetRecorderConfigurationSettings();
        ProceduresSearchConfiguration GetProceduresSearchConfigurationSettings();
        Task UpdateAutoLabelsConfigurationByProcedureType(int procedureTypeId, List<AutoLabelAutoLabelsConfiguration> autoLabels);
        void UpdatePatientInfo(List<PatientInfoSetupConfiguration> patientInfoSetupConfigurations);
        void UpdateProceduresSearchConfigurationColumns(List<ColumnProceduresSearchConfiguration> columnProceduresSearchConfigurations);
    }
}
