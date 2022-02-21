using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalanche.Shared.Infrastructure.Configuration;

namespace Avalanche.Api.Managers.Maintenance
{
    public class ServerConfigurationManager : SharedConfigurationManager, IConfigurationManager
    {
        public ServerConfigurationManager(
            PrintingConfiguration printingConfiguration,
            MedPresenceConfiguration medPresenceConfiguration,
            SetupConfiguration setupConfiguration,
            ProceduresConfiguration proceduresConfiguration) : base(
                printingConfiguration,
                medPresenceConfiguration,
                setupConfiguration,
                proceduresConfiguration)
        {
        }

        public AutoLabelsConfiguration GetAutoLabelsConfigurationSettings(int? procedureTypeId) => throw new InvalidOperationException();
        public LabelsConfiguration GetLabelsConfigurationSettings() => throw new InvalidOperationException();
        public RecorderConfiguration GetRecorderConfigurationSettings() => throw new InvalidOperationException();
        public FinishOptionsConfiguration GetFinishOptionsConfigurationSettings() => throw new InvalidOperationException();

        public Task UpdateAutoLabelsConfigurationByProcedureType(int procedureTypeId, List<AutoLabelAutoLabelsConfiguration> autoLabels) => throw new InvalidOperationException();
    }
}
