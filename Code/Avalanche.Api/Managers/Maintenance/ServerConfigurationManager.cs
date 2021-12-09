using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Shared.Infrastructure.Configuration;

namespace Avalanche.Api.Managers.Maintenance
{
    public class ServerConfigurationManager : SharedConfigurationManager, IConfigurationManager
    {
        public ServerConfigurationManager(
            GeneralApiConfiguration generalApiConfiguration,
            PrintingConfiguration printingConfiguration,
            MedPresenceConfiguration medPresenceConfiguration,
            SetupConfiguration setupConfiguration,
            ProceduresSearchConfiguration proceduresSearchConfiguration) : base(generalApiConfiguration,
                printingConfiguration,
                medPresenceConfiguration,
                setupConfiguration,
                proceduresSearchConfiguration)
        {
        }

        public AutoLabelsConfiguration GetAutoLabelsConfigurationSettings(int? procedureTypeId) => throw new InvalidOperationException();
        public LabelsConfiguration GetLabelsConfigurationSettings() => throw new InvalidOperationException();
        public RecorderConfiguration GetRecorderConfigurationSettings() => throw new InvalidOperationException();
        public Task UpdateAutoLabelsConfigurationByProcedureType(int procedureTypeId, List<AutoLabelAutoLabelsConfiguration> autoLabels) => throw new InvalidOperationException();
    }
}
