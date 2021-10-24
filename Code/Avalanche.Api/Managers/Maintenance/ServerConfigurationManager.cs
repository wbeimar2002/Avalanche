using System.Collections.Generic;
using System.Threading.Tasks;
using Avalanche.Shared.Infrastructure.Configuration;

namespace Avalanche.Api.Managers.Maintenance
{
    public class ServerConfigurationManager : IConfigurationManager
    {
        private readonly ProceduresSearchConfiguration _proceduresSearchConfiguration;
        private readonly GeneralApiConfiguration _generalApiConfiguration;
        private readonly PrintingConfiguration _printingConfiguration;
        private readonly MedPresenceConfiguration _medPresenceConfiguration;
        private readonly SetupConfiguration _setupConfiguration;

        public ServerConfigurationManager(
            ProceduresSearchConfiguration proceduresSearchConfiguration,
            GeneralApiConfiguration generalApiConfiguration,
            PrintingConfiguration printingConfiguration,
            MedPresenceConfiguration medPresenceConfiguration,
            SetupConfiguration setupConfiguration)
        {
            _proceduresSearchConfiguration = proceduresSearchConfiguration;
            _generalApiConfiguration = generalApiConfiguration;
            _printingConfiguration = printingConfiguration;
            _medPresenceConfiguration = medPresenceConfiguration;
            _setupConfiguration = setupConfiguration;
        }

        public AutoLabelsConfiguration GetAutoLabelsConfigurationSettings(int? procedureTypeId) => throw new System.InvalidOperationException();
        public LabelsConfiguration GetLabelsConfigurationSettings() => throw new System.InvalidOperationException();
        public RecorderConfiguration GetRecorderConfigurationSettings() => throw new System.InvalidOperationException();
        public Task UpdateAutoLabelsConfigurationByProcedureType(int procedureTypeId, List<AutoLabelAutoLabelsConfiguration> autoLabels) => throw new System.InvalidOperationException();
        public void UpdatePatientInfo(List<PatientInfoSetupConfiguration> patientInfoSetupConfigurations) => throw new System.InvalidOperationException();

        public GeneralApiConfiguration GetGeneralApiConfigurationSettings() => _generalApiConfiguration;
        public PrintingConfiguration GetPrintingConfigurationSettings() => _printingConfiguration;
        public MedPresenceConfiguration GetMedPresenceConfigurationSettings() => _medPresenceConfiguration;
        public ProceduresSearchConfiguration GetProceduresSearchConfigurationSettings() => _proceduresSearchConfiguration;
        public SetupConfiguration GetSetupConfigurationSettings() => _setupConfiguration;

        public void UpdateProceduresSearchConfigurationColumns(List<ColumnProceduresSearchConfiguration> columnProceduresSearchConfigurations)
        {
            _proceduresSearchConfiguration.Columns = columnProceduresSearchConfigurations;
        }
    }
}
