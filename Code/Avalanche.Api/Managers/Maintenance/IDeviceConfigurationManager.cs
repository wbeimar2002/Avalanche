using System.Collections.Generic;
using System.Threading.Tasks;
using Avalanche.Shared.Infrastructure.Configuration;

namespace Avalanche.Api.Managers.Maintenance
{
    public interface IDeviceConfigurationManager
    {
        AutoLabelsConfiguration GetAutoLabelsConfigurationSettings(int? procedureTypeId);
        LabelsConfiguration GetLabelsConfigurationSettings();
        SetupConfiguration GetSetupConfigurationSettings();
        RecorderConfiguration GetRecorderConfigurationSettings();
        void UpdatePatientInfo(List<PatientInfoSetupConfiguration> patientInfoSetupConfigurations);

        Task UpdateAutoLabelsConfigurationByProcedureType(int procedureTypeId, List<AutoLabelAutoLabelsConfiguration> autoLabels);
    }
}
