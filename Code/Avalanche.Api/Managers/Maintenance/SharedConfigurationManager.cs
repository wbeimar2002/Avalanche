using System.Collections.Generic;
using Avalanche.Shared.Infrastructure.Configuration;

namespace Avalanche.Api.Managers.Maintenance
{
    public class SharedConfigurationManager : ISharedConfigurationManager
    {
        //These values can be changed in execution time
        private readonly PrintingConfiguration _printingConfiguration;
        private readonly MedPresenceConfiguration _medPresenceConfiguration;
        private readonly GeneralApiConfiguration _generalApiConfiguration;
        private readonly SetupConfiguration _setupConfiguration;
        private readonly ProceduresSearchConfiguration _proceduresSearchConfiguration;

        public SharedConfigurationManager(
            GeneralApiConfiguration generalApiConfiguration,
            PrintingConfiguration printingConfiguration,
            MedPresenceConfiguration medPresenceConfiguration,
            SetupConfiguration setupConfiguration,
            ProceduresSearchConfiguration proceduresSearchConfiguration)
        {
            _generalApiConfiguration = generalApiConfiguration;
            _printingConfiguration = printingConfiguration;
            _medPresenceConfiguration = medPresenceConfiguration;
            _setupConfiguration = setupConfiguration;
            _proceduresSearchConfiguration = proceduresSearchConfiguration;
        }

        public GeneralApiConfiguration GetGeneralApiConfigurationSettings() => _generalApiConfiguration;
        public PrintingConfiguration GetPrintingConfigurationSettings() => _printingConfiguration;
        public MedPresenceConfiguration GetMedPresenceConfigurationSettings() => _medPresenceConfiguration;
        public ProceduresSearchConfiguration GetProceduresSearchConfigurationSettings() => _proceduresSearchConfiguration;
        public SetupConfiguration GetSetupConfigurationSettings() => _setupConfiguration;

        public void UpdateProceduresSearchConfigurationColumns(List<ColumnProceduresSearchConfiguration> columnProceduresSearchConfigurations) =>
            _proceduresSearchConfiguration.Columns = columnProceduresSearchConfigurations;

        public void UpdatePatientInfo(List<PatientInfoSetupConfiguration> patientInfoSetupConfigurations) =>
            _setupConfiguration.PatientInfo = patientInfoSetupConfigurations;

        public void UseVSSPrintingService(bool useVSSPrintingService) =>
            _printingConfiguration.UseVSSPrintingService = useVSSPrintingService;
    }
}
