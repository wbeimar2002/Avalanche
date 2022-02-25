using System.Collections.Generic;
using Avalanche.Shared.Infrastructure.Configuration;

namespace Avalanche.Api.Managers.Maintenance
{
    public class SharedConfigurationManager : ISharedConfigurationManager
    {
        //These values can be changed in execution time
        private readonly PrintingConfiguration _printingConfiguration;
        private readonly MedPresenceConfiguration _medPresenceConfiguration;
        private readonly SetupConfiguration _setupConfiguration;
        private readonly ProceduresConfiguration _proceduresConfiguration;
        private readonly MedPresenceProvisioningConfiguration _medPresenceProvisioningConfiguration;

        public SharedConfigurationManager(
            PrintingConfiguration printingConfiguration,
            MedPresenceConfiguration medPresenceConfiguration,
            SetupConfiguration setupConfiguration,
            ProceduresConfiguration proceduresConfiguration,
            MedPresenceProvisioningConfiguration medPresenceProvisioningConfiguration)
        {
            _printingConfiguration = printingConfiguration;
            _medPresenceConfiguration = medPresenceConfiguration;
            _setupConfiguration = setupConfiguration;
            _proceduresConfiguration = proceduresConfiguration;
            _medPresenceProvisioningConfiguration = medPresenceProvisioningConfiguration;
        }

        public PrintingConfiguration GetPrintingConfigurationSettings() => _printingConfiguration;
        public MedPresenceConfiguration GetMedPresenceConfigurationSettings() => _medPresenceConfiguration;
        public ProceduresConfiguration GetProceduresConfigurationSettings() => _proceduresConfiguration;
        public SetupConfiguration GetSetupConfigurationSettings() => _setupConfiguration;

        public void UpdateProceduresSearchConfigurationColumns(List<ColumnProceduresSearchConfiguration> columnProceduresSearchConfigurations) =>
            _proceduresConfiguration.Columns = columnProceduresSearchConfigurations;

        public void UpdatePatientInfo(List<PatientInfoSetupConfiguration> patientInfoSetupConfigurations) =>
            _setupConfiguration.PatientInfo = patientInfoSetupConfigurations;

        public void UseVSSPrintingService(bool useVSSPrintingService) =>
            _printingConfiguration.UseVSSPrintingService = useVSSPrintingService;

        public MedPresenceProvisioningConfiguration GetMedPresenceProvisioningConfigurationSettings() => _medPresenceProvisioningConfiguration;
    }
}
