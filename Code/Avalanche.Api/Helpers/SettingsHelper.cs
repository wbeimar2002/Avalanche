using Avalanche.Api.Extensions;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Infrastructure.Enumerations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Avalanche.Api.Helpers
{
    public static class SettingsHelper
    {
        public static bool IsPropertyExist(dynamic entity, string name)
        {
            if (entity is ExpandoObject)
                return ((IDictionary<string, object>)entity).ContainsKey(name);

            return entity.GetType().GetProperty(name) != null;
        }

        public static string GetJsonValues(DynamicSectionViewModel category)
        {
            string json = @"{}";
            JObject jsonRoot = JObject.Parse(json);

            AddSettingValues(jsonRoot, category);
            return jsonRoot.ToString();
        }

        public static void AddSettingValues(JObject rootSection, DynamicSectionViewModel section)
        {
            if (section.Settings != null)
            {
                foreach (var setting in section.Settings)
                {
                    if (!string.IsNullOrEmpty(setting.JsonKey))
                    {
                        var finalValue = Convert.ToInt32(setting.Policy) == (int)SettingsPolicies.UseDefaultValue ? setting.DefaultValue : setting.Value;

                        switch (setting.SettingType)
                        {
                            case SettingTypes.Number:
                                rootSection.Add(new JProperty(setting.JsonKey, Convert.ToInt32(finalValue)));
                                break;
                            case SettingTypes.Boolean:
                                rootSection.Add(new JProperty(setting.JsonKey, Convert.ToBoolean(finalValue)));
                                break;
                            case SettingTypes.Date:
                                rootSection.Add(new JProperty(setting.JsonKey, Convert.ToDateTime(finalValue)));
                                break;
                            case SettingTypes.Undefined:
                            case SettingTypes.Text:
                            default:
                                rootSection.Add(new JProperty(setting.JsonKey, finalValue));
                                break;
                        }
                        
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

        internal static void CleanSettings(DynamicSectionViewModel section)
        {
            if (section.Settings != null)
            {
                foreach (var setting in section.Settings)
                {
                    setting.SourceValues = null;
                    setting.Dependencies = null;
                    setting.PoliciesValues = null;
                    setting.CustomList = null;
                    setting.Value = null;
                }
            }

            if (section.Sections != null)
            {
                foreach (var item in section.Sections)
                {
                    CleanSettings(item);
                }
            }
        }

        public static void SetSettingValues(DynamicSectionViewModel section, dynamic settingsValues, IList<KeyValuePairViewModel> policiesValues)
        {
            if (section.Settings != null)
            {
                foreach (var setting in section.Settings)
                {
                    setting.Policy = string.IsNullOrEmpty(setting.Policy) ? ((int)SettingsPolicies.AllowEdit).ToString() : setting.Policy;
                    setting.PoliciesValues = policiesValues;

                    if (settingsValues == null)
                        setting.Value = setting.DefaultValue;
                    else
                    {
                        if (string.IsNullOrEmpty(setting.JsonKey))
                            setting.Value = null;
                        else
                        {
                            setting.Value = setting.SettingType == SettingTypes.Boolean ? Convert.ToString(settingsValues[setting.JsonKey]).ToLower() : settingsValues[setting.JsonKey];
                        }
                    }
                }
            }
        }
    }
}
