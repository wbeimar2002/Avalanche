using Avalanche.Api.Extensions;
using Avalanche.Api.ViewModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Avalanche.Api.Helpers
{
    public static class SettingsHelper
    {
        public static string GetJsonValues(SectionViewModel category)
        {
            string json = @"{}";
            JObject jsonRoot = JObject.Parse(json);

            AddSettingValues(jsonRoot, category);
            return jsonRoot.ToString();
        }

        public static void AddSettingValues(JObject rootSection, SectionViewModel section)
        {
            if (section.Settings != null)
            {
                foreach (var setting in section.Settings)
                {
                    if (!string.IsNullOrEmpty(setting.JsonKey))
                    {
                        rootSection.Add(new JProperty(setting.JsonKey, setting.Value));
                    }
                }
            }

            if (section.Sections != null)
            {
                foreach (var item in section.Sections)
                {
                    if (!string.IsNullOrEmpty(item.JsonKey))
                    {
                        rootSection.Add(new JProperty(item.JsonKey, JObject.Parse("{}")));
                        JObject childSection = (JObject)rootSection[item.JsonKey];

                        AddSettingValues(childSection, item);
                    }
                }
            }
        }

        public static void SetSettingValues(SectionViewModel section, dynamic settings)
        {
            if (section.Settings != null)
            {
                foreach (var setting in section.Settings)
                {
                    if (settings == null)
                        setting.Value = setting.DefaultValue == null ? setting.Value : setting.DefaultValue;
                    else
                    {
                        if (!string.IsNullOrEmpty(setting.JsonKey))
                            setting.Value = settings[setting.JsonKey];
                    }
                }
            }
        }
    }
}
