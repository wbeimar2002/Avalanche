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
    public class DeviceConfigurationManager : IConfigurationManager
    {
        private readonly ConfigurationContext _configurationContext;

        private readonly AutoLabelsConfiguration _autoLabelsConfiguration;
        private readonly LabelsConfiguration _labelsConfiguration;
        private readonly RecorderConfiguration _recorderConfiguration;
        private readonly SetupConfiguration _setupConfiguration;
        private readonly ProceduresSearchConfiguration _proceduresSearchConfiguration;
        private readonly GeneralApiConfiguration _generalApiConfiguration;
        private readonly PrintingConfiguration _printingConfiguration;
        private readonly MedPresenceConfiguration _medPresenceConfiguration;

        private readonly IStorageService _storageService;

        public DeviceConfigurationManager(
            AutoLabelsConfiguration autoLabelsConfiguration,
            LabelsConfiguration labelsConfiguration,
            SetupConfiguration setupConfiguration,
            RecorderConfiguration recorderConfiguration,
            ProceduresSearchConfiguration proceduresSearchConfiguration,
            GeneralApiConfiguration generalApiConfiguration,
            PrintingConfiguration printingConfiguration,
            MedPresenceConfiguration medPresenceConfiguration,
            IStorageService storageService,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper)
        {
            _autoLabelsConfiguration = autoLabelsConfiguration;
            _labelsConfiguration = labelsConfiguration;
            _setupConfiguration = setupConfiguration;
            _recorderConfiguration = recorderConfiguration;
            _proceduresSearchConfiguration = proceduresSearchConfiguration;
            _storageService = storageService;
            _generalApiConfiguration = generalApiConfiguration;
            _printingConfiguration = printingConfiguration;
            _medPresenceConfiguration = medPresenceConfiguration;

            var user = HttpContextUtilities.GetUser(httpContextAccessor.HttpContext);
            _configurationContext = mapper.Map<UserModel, ConfigurationContext>(user);
            _configurationContext.IdnId = Guid.NewGuid().ToString();
        }

        public AutoLabelsConfiguration GetAutoLabelsConfigurationSettings(int? procedureTypeId) => new AutoLabelsConfiguration()
        {
            AutoLabels = _autoLabelsConfiguration.AutoLabels?.Where(l => l.ProcedureTypeId == procedureTypeId).ToList(),
            Colors = _autoLabelsConfiguration.Colors
        };

        public LabelsConfiguration GetLabelsConfigurationSettings() => _labelsConfiguration;

        public SetupConfiguration GetSetupConfigurationSettings() => _setupConfiguration;

        public RecorderConfiguration GetRecorderConfigurationSettings() => _recorderConfiguration;

        public ProceduresSearchConfiguration GetProceduresSearchConfigurationSettings() => _proceduresSearchConfiguration;

        public GeneralApiConfiguration GetGeneralApiConfigurationSettings() => _generalApiConfiguration;

        public PrintingConfiguration GetPrintingConfigurationSettings() => _printingConfiguration;

        public MedPresenceConfiguration GetMedPresenceConfigurationSettings() => _medPresenceConfiguration;

        public void UpdateProceduresSearchConfigurationColumns(List<ColumnProceduresSearchConfiguration> columnProceduresSearchConfigurations)
        {
            _proceduresSearchConfiguration.Columns = columnProceduresSearchConfigurations;
        }

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

            await _storageService.SaveJsonObject(nameof(AutoLabelsConfiguration), _autoLabelsConfiguration.Json(), 1, _configurationContext);
        }

        public void UpdatePatientInfo(List<PatientInfoSetupConfiguration> patientInfoSetupConfigurations)
        {
            _setupConfiguration.PatientInfo = patientInfoSetupConfigurations;
        }
    }
}
