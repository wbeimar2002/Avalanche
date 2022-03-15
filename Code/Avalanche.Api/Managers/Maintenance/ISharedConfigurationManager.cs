using System.Collections.Generic;
using Avalanche.Shared.Infrastructure.Configuration;

namespace Avalanche.Api.Managers.Maintenance
{
    public interface ISharedConfigurationManager
    {
        PrintingConfiguration GetPrintingConfigurationSettings();
        void UpdateProceduresSearchConfigurationColumns(List<ColumnProceduresSearchConfiguration> columnProceduresSearchConfigurations);
        void UpdatePatientInfo(List<PatientInfoSetupConfiguration> patientInfoSetupConfigurations);
        void UseVSSPrintingService(bool useVSSPrintingService);
    }
}
