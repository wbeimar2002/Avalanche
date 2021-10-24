using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Utilities;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Configuration;
using Ism.Common.Core.Configuration.Models;
using Microsoft.AspNetCore.Http;

namespace Avalanche.Api.Managers.Maintenance
{
    public class ServerConfigurationManager : SharedConfigurationManager, IConfigurationManager
    {
        public ServerConfigurationManager(
            GeneralApiConfiguration generalApiConfiguration,
            PrintingConfiguration printingConfiguration,
            MedPresenceConfiguration medPresenceConfiguration,
            SetupConfiguration setupConfiguration,
            ProceduresSearchConfiguration proceduresSearchConfiguration,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper) : base(generalApiConfiguration,
                printingConfiguration,
                medPresenceConfiguration,
                setupConfiguration,
                proceduresSearchConfiguration,
                httpContextAccessor, mapper)
        {
        }

        public AutoLabelsConfiguration GetAutoLabelsConfigurationSettings(int? procedureTypeId) => throw new InvalidOperationException();
        public LabelsConfiguration GetLabelsConfigurationSettings() => throw new InvalidOperationException();
        public RecorderConfiguration GetRecorderConfigurationSettings() => throw new InvalidOperationException();
        public Task UpdateAutoLabelsConfigurationByProcedureType(int procedureTypeId, List<AutoLabelAutoLabelsConfiguration> autoLabels) => throw new InvalidOperationException();
    }
}
