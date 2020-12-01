using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Settings
{
    public class MaintenaceManagerMock : IMaintenaceManager
    {
        public async Task SaveCategory(Avalanche.Shared.Domain.Models.User user, SectionViewModel category)
        {
            //TODO: Save to storage
            await Task.CompletedTask;
        }

        public Task<SectionViewModel> GetCategoryByKey(Avalanche.Shared.Domain.Models.User user, string key)
        {
            var section = new SectionViewModel();
            switch (key)
            {
                case "SetupSettingsConfiguration":
                    section = new SectionViewModel()
                    {
                        Key = "SetupSettingsConfiguration",
                        TitleTranslationKey = "setup_settings.tittle",
                        ReadOnly = false,
                        Settings = GetSetupSettings(),
                        Sections = GetSetupSections(),
                    };
                    break;
                case "RoutingSettingsConfiguration":
                    section = new SectionViewModel()
                    {
                        Key = "RoutingSettingsConfiguration",
                        TitleTranslationKey = "routing_settings.tittle",
                        ReadOnly = true,
                        Settings = GetRoutingSettings()
                    };
                    break;
            }

            return Task.FromResult(section);
        }

        private List<SectionViewModel> GetSetupSections()
        {
            var sections = new List<SectionViewModel>();

            sections.Add(new SectionViewModel()
            {
                Key = "setup-settings-general-administrator",
                JsonKey = "Administrator",
                TitleTranslationKey = "setup_settings_administrator.tittle",
                ReadOnly = false,
                Settings = GetSetupAdministratorSettings(),
            });

            sections.Add(new SectionViewModel()
            {
                Key = "setup-settings-general-quick",
                JsonKey = "QuickRegistration",
                TitleTranslationKey = "setup_settings_quick_registration.tittle",
                ReadOnly = false,
                Settings = GetSetupQuickRegistrationSettings(),
            });

            return sections;
        }

        private List<SettingViewModel> GetSetupQuickRegistrationSettings()
        {
            var settings = new List<SettingViewModel>();

            settings.Add(new SettingViewModel()
            {
                JsonKey = "IsAllowed",
                LabelTranslationKey = "setup_settings_general_quick.is_allowed",
                Value = "true",
                SettingType = SettingTypes.Boolean,
                VisualStyle = VisualStyles.Switch,
            });

            settings.Add(new SettingViewModel()
            {
                JsonKey = "DateFormat",
                LabelTranslationKey = "setup_settings_general_quick.date_format",
                Value = "yyyy_MM_dd_T_HH_mm_ss_ff",
                Format = @"^\d{4}_\d{2}_\d{2}_T_\d{2}_\d{2}_\d{2}_\d{2}",
                SettingType = SettingTypes.Text,
                VisualStyle = VisualStyles.Text,
            });

            settings.Add(new SettingViewModel()
            {
                JsonKey = "UseAdministratorAsPhysician",
                LabelTranslationKey = "setup_settings_general_quick.administrator_physician",
                Value = "true",
                SettingType = SettingTypes.Boolean,
                VisualStyle = VisualStyles.Switch,
            });

            return settings;
        }

        private List<SettingViewModel> GetSetupAdministratorSettings()
        {
            var settings = new List<SettingViewModel>();

            settings.Add(new SettingViewModel()
            {
                JsonKey = "Id",
                LabelTranslationKey = "setup_settings_general_administrator.id",
                Value = "0000-0000-0000-000",
                SettingType = SettingTypes.Text,
                VisualStyle = VisualStyles.Text,
            });

            settings.Add(new SettingViewModel()
            {
                JsonKey = "FirstName",
                LabelTranslationKey = "setup_settings_general_administrator.first_name",
                Value = "Olympus",
                SettingType = SettingTypes.Text,
                VisualStyle = VisualStyles.Text,

            });

            settings.Add(new SettingViewModel()
            {
                JsonKey = "LastName",
                LabelTranslationKey = "setup_settings_general_administrator.last_name",
                Value = "Administrator",
                SettingType = SettingTypes.Text,
                VisualStyle = VisualStyles.Text,

            });

            return settings;
        }

        private List<SettingViewModel> GetSetupSettings()
        {
            var settings = new List<SettingViewModel>();

            settings.Add(new SettingViewModel()
            {
                JsonKey = "Mode",
                LabelTranslationKey = "setup_settings.mode_label",
                SourceKey = "SetupModes",
                Value = "0",
                SettingType = SettingTypes.Int,
                VisualStyle = VisualStyles.Toggle
            });

            settings.Add(new SettingViewModel()
            {
                JsonKey = "SearchColumns",
                LabelTranslationKey = "setup_settings.search_columns_label",
                SourceKey = "SearchColumns",
                SettingType = SettingTypes.List,
                VisualStyle = VisualStyles.Grid
            });

            settings.Add(new SettingViewModel()
            {
                JsonKey = "DepartmentsSupported",
                LabelTranslationKey = "setup_settings.departments_supported_label",
                Value = "true",
                SettingType = SettingTypes.Boolean,
                VisualStyle = VisualStyles.Switch
            });

            settings.Add(new SettingViewModel()
            {
                JsonKey = "CacheDuration",
                LabelTranslationKey = "setup_settings.cache_duration",
                Value = "10000",
                SettingType = SettingTypes.Int,
                VisualStyle = VisualStyles.Text,
                Format = @"^\d+$",
                MaximumValue = 20000,
                MinimumValue = 5000
            });

            return settings;
        }

        private List<SettingViewModel> GetRoutingSettings()
        {
            var settings = new List<SettingViewModel>();
            
            settings.Add(new SettingViewModel()
            {
                JsonKey = "Mode",
                LabelTranslationKey = "routing_settings.mode_label",
                SourceKey = "RoutingModes",
                Value = "0",
                SettingType = SettingTypes.Int,
                VisualStyle = VisualStyles.DropDown
            });

            return settings;
        }
    }
}
