using System.Collections.Generic;
using Avalanche.Shared.Infrastructure.Configuration;

namespace Avalanche.Api.Managers.Maintenance
{
    public interface ISharedConfigurationManager
    {
        GeneralApiConfiguration GetGeneralApiConfigurationSettings();
        PrintingConfiguration GetPrintingConfigurationSettings();
        MedPresenceConfiguration GetMedPresenceConfigurationSettings();

        void UpdateProceduresSearchConfigurationColumns(List<ColumnProceduresSearchConfiguration> columnProceduresSearchConfigurations);
        void UpdatePatientInfo(List<PatientInfoSetupConfiguration> patientInfoSetupConfigurations);
        void UseVSSPrintingService(bool useVSSPrintingService);
    }
}
