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
    public class DeviceConfigurationManager : IDeviceConfigurationManager
    {
        private readonly IMapper _mapper;
        private readonly UserModel _user;
        private readonly ConfigurationContext _configurationContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly AutoLabelsConfiguration _autoLabelsConfiguration;
        private readonly LabelsConfiguration _labelsConfiguration;
        private readonly RecorderConfiguration _recorderConfiguration;
        private readonly SetupConfiguration _setupConfiguration;

        private readonly IStorageService _storageService;

        public DeviceConfigurationManager(
            AutoLabelsConfiguration autoLabelsConfiguration,
            LabelsConfiguration labelsConfiguration,
            SetupConfiguration setupConfiguration,
            RecorderConfiguration recorderConfiguration,
            IStorageService storageService,
            IMapper mapper)
        {
            _autoLabelsConfiguration = autoLabelsConfiguration;
            _labelsConfiguration = labelsConfiguration;
            _setupConfiguration = setupConfiguration;
            _recorderConfiguration = recorderConfiguration;
            _storageService = storageService;
            _mapper = mapper;

            _user = HttpContextUtilities.GetUser(_httpContextAccessor.HttpContext);
            _configurationContext = _mapper.Map<UserModel, ConfigurationContext>(_user);
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
