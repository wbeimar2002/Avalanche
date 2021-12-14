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
        private readonly ProceduresConfiguration _proceduresConfiguration;

        public SharedConfigurationManager(
            GeneralApiConfiguration generalApiConfiguration,
            PrintingConfiguration printingConfiguration,
            MedPresenceConfiguration medPresenceConfiguration,
            SetupConfiguration setupConfiguration,
            ProceduresConfiguration proceduresConfiguration)
        {
            _generalApiConfiguration = generalApiConfiguration;
            _printingConfiguration = printingConfiguration;
            _medPresenceConfiguration = medPresenceConfiguration;
            _setupConfiguration = setupConfiguration;
            _proceduresConfiguration = proceduresConfiguration;
        }

        public GeneralApiConfiguration GetGeneralApiConfigurationSettings() => _generalApiConfiguration;
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
    }
}
