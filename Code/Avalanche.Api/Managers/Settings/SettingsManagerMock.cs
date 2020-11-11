using Avalanche.Api.Services.Configuration;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Settings
{
    [ExcludeFromCodeCoverage]
    public class SettingsManagerMock : ISettingsManager
    {
        readonly ISettingsService _settingsService;
        public SettingsManagerMock(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<VideoRoutingSettings> GetVideoRoutingSettingsAsync()
        {
            return await _settingsService.GetVideoRoutingSettingsAsync();
        }


        public async Task<TimeoutSettings> GetTimeoutSettingsAsync()
        {
            return await _settingsService.GetTimeoutSettingsAsync();
        }

        public async Task<SetupSettings> GetSetupSettingsAsync()
        {
            return await _settingsService.GetSetupSettingsAsync();
        }

        public Task<List<SettingCategory>> GetCategories()
        {
            var mock = new List<SettingCategory>();
            mock.Add(new SettingCategory()
            {
                DefaultLabelValue = "Sample Read Only",
                TranslationKey = "SampleCategory",
                ReadOnly = true
            });

            mock.Add(new SettingCategory()
            {
                DefaultLabelValue = "Sample Editable",
                TranslationKey = "EditableCategory",
                ReadOnly = false
            });

            return Task.FromResult(mock);
        }

        public Task<SettingCategoryViewModel> GetSettingsByCategory(string categoryKey)
        {
            var settingsMock = new List<Setting>();
            settingsMock.Add(new Setting() 
            { 
                Format = @"^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})$",
                TranslationKey = "EmailFieldLabel",
                DefaultLabelValue = "Email",
                SettingType = SettingTypes.Text,
                VisualStyle = VisualStyles.Text,
            });

            settingsMock.Add(new Setting()
            {
                TranslationKey = "AllowEditSettingsLabel",
                DefaultLabelValue = "Allow Edit Settings",
                SettingType = SettingTypes.Boolean,
                VisualStyle = VisualStyles.Toggle,
            });

            settingsMock.Add(new Setting()
            {
                TranslationKey = "AutoLoginLabel",
                DefaultLabelValue = "Auto Login",
                SettingType = SettingTypes.Boolean,
                VisualStyle = VisualStyles.Switch,
            });

            settingsMock.Add(new Setting()
            {
                TranslationKey = "LogVerboseLabel",
                DefaultLabelValue = "Loggin Verbose Level",
                SettingType = SettingTypes.Int,
                VisualStyle = VisualStyles.DropDown,
                MinimumValue = 1,
                MaximumValue = 5
            });

            settingsMock.Add(new Setting()
            {
                TranslationKey = "PaperOrientationLabel",
                DefaultLabelValue = "Paper Orientation",
                SettingType = SettingTypes.Text,
                VisualStyle = VisualStyles.DropDown,
                SourceKey = "PaperOrientation"
            });

            var mock = new SettingCategoryViewModel()
            {
                Category = new SettingCategory()
                {
                    DefaultLabelValue = "Sample Editable",
                    TranslationKey = "EditableCategory",
                    ReadOnly = false
                },
                Settings = settingsMock
            };

            return Task.FromResult(mock);
        }

        public List<KeyValuePairViewModel> GetSourceValuesByCategory(string categoryKey, string sourceKey)
        {
            switch (categoryKey)
            {
                case "PaperOrientation":
                    return GetPaperOrientations();
                default:
                    return new List<KeyValuePairViewModel>();
            }
        }

        public async Task SaveSettingsByCategory(string categoryKey, List<KeyValuePairViewModel> settings)
        {
            await Task.CompletedTask;
        }

        private List<KeyValuePairViewModel> GetPaperOrientations()
        {
            var list = new List<KeyValuePairViewModel>();

            list.Add(new KeyValuePairViewModel() {
                TranslationKey = "PaperOrientationHorizontal",
                Id = "1",
                Value = "Horizontal Layout"
            });

            list.Add(new KeyValuePairViewModel()
            {
                TranslationKey = "PaperOrientationVertical",
                Id = "1",
                Value = "Horizontal Layout"
            });

            return list;
        }
    }
}
