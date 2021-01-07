using Avalanche.Api.Extensions;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Infrastructure.Enumerations;
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
                        if (Convert.ToInt32(setting.Policy) == (int)SettingsPolicies.UseDefaultValue)
                            rootSection.Add(new JProperty(setting.JsonKey, setting.DefaultValue));
                        else
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

        public static void SetSettingValues(SectionViewModel section, dynamic setingsValues, List<KeyValuePairViewModel> policiesValues)
        {
            if (section.Settings != null)
            {
                foreach (var setting in section.Settings)
                {
                    setting.Policy = string.IsNullOrEmpty(setting.Policy) ? ((int)SettingsPolicies.AllowEdit).ToString() : setting.Policy;
                    setting.PoliciesValues = policiesValues;

                    if (Convert.ToInt32(setting.Policy) == (int)SettingsPolicies.UseDefaultValue)
                        setting.Value = setting.DefaultValue; //TODO: Validate this rule
                    else
                    {
                        if (setingsValues == null)
                            setting.Value = setting.DefaultValue;
                        else
                        {
                            if (string.IsNullOrEmpty(setting.JsonKey))
                                setting.Value = setting.DefaultValue; //TODO: Validate this rule
                            else
                                setting.Value = setingsValues[setting.JsonKey];
                        }
                    }
                }
            }
        }
    }
}
