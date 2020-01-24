using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Settings
{
    public class SettingsManagerMock : ISettingsManager
    {
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
                SettingType = SettingType.Text,
                VisualStyle = VisualStyle.Text,
            });

            settingsMock.Add(new Setting()
            {
                TranslationKey = "AllowEditSettingsLabel",
                DefaultLabelValue = "Allow Edit Settings",
                SettingType = SettingType.Boolean,
                VisualStyle = VisualStyle.Toggle,
            });

            settingsMock.Add(new Setting()
            {
                TranslationKey = "AutoLoginLabel",
                DefaultLabelValue = "Auto Login",
                SettingType = SettingType.Boolean,
                VisualStyle = VisualStyle.Switch,
            });

            settingsMock.Add(new Setting()
            {
                TranslationKey = "LogVerboseLabel",
                DefaultLabelValue = "Loggin Verbose Level",
                SettingType = SettingType.Int,
                VisualStyle = VisualStyle.DropDown,
                MinimumValue = 1,
                MaximumValue = 5
            });

            settingsMock.Add(new Setting()
            {
                TranslationKey = "PaperOrientationLabel",
                DefaultLabelValue = "Paper Orientation",
                SettingType = SettingType.Text,
                VisualStyle = VisualStyle.DropDown,
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
