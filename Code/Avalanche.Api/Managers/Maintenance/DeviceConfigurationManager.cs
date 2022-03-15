using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Utilities;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Configuration;
using Avalanche.Shared.Infrastructure.Extensions;
using Ism.Common.Core.Configuration.Models;
using Microsoft.AspNetCore.Http;

namespace Avalanche.Api.Managers.Maintenance
{
    public class DeviceConfigurationManager : SharedConfigurationManager, IConfigurationManager
    {
        private readonly ConfigurationContext _configurationContext;

        private readonly AutoLabelsConfiguration _autoLabelsConfiguration;
        private readonly LabelsConfiguration _labelsConfiguration;
        private readonly RecorderConfiguration _recorderConfiguration;
        private readonly FinishOptionsConfiguration _finishOptionsConfiguration;
        private readonly MedPresenceProvisioningConfiguration _medPresenceProvisioningConfiguration;

        private readonly IStorageService _storageService;

        public DeviceConfigurationManager(
            AutoLabelsConfiguration autoLabelsConfiguration,
            PrintingConfiguration printingConfiguration,
            MedPresenceConfiguration medPresenceConfiguration,
            SetupConfiguration setupConfiguration,
            ProceduresConfiguration proceduresConfiguration,
            LabelsConfiguration labelsConfiguration,
            RecorderConfiguration recorderConfiguration,
            FinishOptionsConfiguration finishOptionsConfiguration,
            IStorageService storageService,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper,
            MedPresenceProvisioningConfiguration medPresenceProvisioningConfiguration) : base(
                printingConfiguration,
                medPresenceConfiguration,
                setupConfiguration,
                proceduresConfiguration)
        {
            _autoLabelsConfiguration = autoLabelsConfiguration;
            _labelsConfiguration = labelsConfiguration;
            _recorderConfiguration = recorderConfiguration;
            _finishOptionsConfiguration = finishOptionsConfiguration;
            _medPresenceProvisioningConfiguration = medPresenceProvisioningConfiguration;

            _storageService = storageService;

            var user = HttpContextUtilities.GetUser(httpContextAccessor.HttpContext);
            _configurationContext = mapper.Map<UserModel, ConfigurationContext>(user);
            _configurationContext.IdnId = Guid.NewGuid().ToString();
        }

        public MedPresenceProvisioningConfiguration GetMedPresenceProvisioningConfigurationSettings() => _medPresenceProvisioningConfiguration;

        public AutoLabelsConfiguration GetAutoLabelsConfigurationSettings(int? procedureTypeId) => new AutoLabelsConfiguration()
        {
            AutoLabels = _autoLabelsConfiguration.AutoLabels?.Where(l => l.ProcedureTypeId == procedureTypeId).ToList(),
            Colors = _autoLabelsConfiguration.Colors
        };

        public LabelsConfiguration GetLabelsConfigurationSettings() => _labelsConfiguration;

        public RecorderConfiguration GetRecorderConfigurationSettings() => _recorderConfiguration;

        public FinishOptionsConfiguration GetFinishOptionsConfigurationSettings() => _finishOptionsConfiguration;

        public async Task UpdateAutoLabelsConfigurationByProcedureType(int procedureTypeId, List<AutoLabelAutoLabelsConfiguration> autoLabels)
        {
            if (_autoLabelsConfiguration.AutoLabels == null)
            {
                _autoLabelsConfiguration.AutoLabels = new List<AutoLabelAutoLabelsConfiguration>();
            }
            else
            {
                _autoLabelsConfiguration.AutoLabels.RemoveAll(l => l.ProcedureTypeId == procedureTypeId);
            }

            _autoLabelsConfiguration.AutoLabels.AddRange(autoLabels);

            await _storageService.SaveJsonObject(nameof(AutoLabelsConfiguration), _autoLabelsConfiguration.Json(), 1, _configurationContext).ConfigureAwait(false);
        }
    }
}
