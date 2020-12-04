using Avalanche.Api.Services.Configuration;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Infrastructure.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.Common.Core.Configuration.Models;
using Ism.Security.Grpc.Interfaces;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using static Ism.PgsTimeout.V1.Protos.PgsTimeout;

namespace Avalanche.Api.Services.Maintenance
{
    [ExcludeFromCodeCoverage]
    public class SettingsService : ISettingsService
    {
        readonly IConfigurationService _configurationService;
        readonly IStorageService _storageService;

        public SettingsService(IConfigurationService configurationService, IStorageService storageService, IGrpcClientFactory<PgsTimeoutClient> grpcPgsClientFactory, ICertificateProvider certificateProvider)
        {
            _configurationService = configurationService;
            _storageService = storageService;
        }

        public async Task<TimeoutSettings> GetTimeoutSettings(ConfigurationContext context)
        {
            var section = await _storageService.GetJson<SectionReadOnlyViewModel>("TimeoutSettings", 1, context);

            return new TimeoutSettings()
            {
                CheckListFileName = GetSetting(section, "CheckListFileName")?.Value
            };
        }

        public async Task<SetupSettings> GetSetupSettings(ConfigurationContext context)
        {
            var section = await _storageService.GetJson<SectionReadOnlyViewModel>("SetupSettings", 1, context);
            return new SetupSettings()
            {
                General = new GeneralSetupSettings()
                {
                    Mode = (Shared.Domain.Enumerations.SetupModes)Convert.ToInt32(GetSettingBySection(section, "General", "Mode")?.Value),
                    DepartmentsSupported = Convert.ToBoolean(GetSettingBySection(section, "General", "DepartmentsSupported")?.Value),
                    CacheDuration = Convert.ToInt32(GetSettingBySection(section, "General", "CacheDuration")?.Value),
                    Administrator = new AdministratorSettings()
                    {
                        Id = GetSettingBySubSection(section, "General", "Administrator", "Id")?.Value,
                        FirstName = GetSettingBySubSection(section, "General", "Administrator", "FirstName")?.Value,
                        LastName = GetSettingBySubSection(section, "General", "Administrator", "LastName")?.Value
                    }
                },
                Registration = new RegistrationSettings() 
                {
                    Manual = new ManualRegistrationSettings()
                    { 
                        AutoFillPhysician = Convert.ToBoolean(GetSettingBySubSection(section, "Registration", "Manual", "AutoFillPhysician")?.Value)
                    },
                    Quick = new QuickRegistrationSettings()
                    { 
                        IsAllowed = Convert.ToBoolean(GetSettingBySubSection(section, "Registration", "Quick", "IsAllowed")?.Value),
                        UseAdministratorAsPhysician = Convert.ToBoolean(GetSettingBySubSection(section, "Registration", "Quick", "UseAdministratorAsPhysician")?.Value),
                        DateFormat = GetSettingBySubSection(section, "Registration", "Quick", "DateFormat")?.Value
                    }
                }
            };
        }

        public async Task<RoutingSettings> GetRoutingSettings(ConfigurationContext context)
        {
            var section = await _storageService.GetJson<SectionReadOnlyViewModel>("RoutingSettings", 1, context);

            return new RoutingSettings()
            {
                Mode = (Shared.Domain.Enumerations.RoutingModes)Convert.ToInt32(GetSetting(section, "Mode")?.Value)
            };
        }

        private static SettingReadOnlyViewModel GetSetting(SectionReadOnlyViewModel section, string settingName)
        {
            return section.Settings.Where(s => s.JsonKey.Equals(settingName)).FirstOrDefault();
        }

        private static SettingReadOnlyViewModel GetSettingBySection(SectionReadOnlyViewModel section, string sectionName, string settingName)
        {
            var selectedSection = section.Sections.Where(s => s.JsonKey.Equals(sectionName)).FirstOrDefault();
            return selectedSection.Settings.Where(s => s.JsonKey.Equals(settingName)).FirstOrDefault();
        }
        private static SettingReadOnlyViewModel GetSettingBySubSection(SectionReadOnlyViewModel section, string sectionName, string subsectionName, string settingName)
        {
            var selectedSection = section.Sections.Where(s => s.JsonKey.Equals(sectionName)).FirstOrDefault();
            var selectedSubSection = selectedSection.Sections.Where(s => s.JsonKey.Equals(subsectionName)).FirstOrDefault();
            return selectedSubSection.Settings.Where(s => s.JsonKey.Equals(settingName)).FirstOrDefault();
        }

        public async Task<PgsSettings> GetPgsSettings(ConfigurationContext context)
        {
            var section = await _storageService.GetJson<SectionReadOnlyViewModel>("PgsSettings", 1, context);

            return new PgsSettings()
            {
                PgsVideoAlwaysOn = Convert.ToBoolean(GetSetting(section, "PgsVideoAlwaysOn")?.Value)
            };
        }
    }
}
