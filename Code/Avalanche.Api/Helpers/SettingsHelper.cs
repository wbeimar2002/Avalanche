using Avalanche.Api.Extensions;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Infrastructure.Enumerations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Avalanche.Api.Helpers
{
    public static class SettingsHelper
    {
        public static void Map(ExpandoObject source, object destination)
        {
            source = source ?? throw new ArgumentNullException(nameof(source));
            destination = destination ?? throw new ArgumentNullException(nameof(destination));

            string normalizeName(string name) => name.ToLowerInvariant();

            IDictionary<string, object> dict = source;
            var type = destination.GetType();

            var setters = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.GetSetMethod() != null)
                .ToDictionary(p => normalizeName(p.Name));

            foreach (var item in dict)
            {
                if (setters.TryGetValue(normalizeName(item.Key), out var setter))
                {
                    var value = setter.PropertyType.ChangeType(item.Value);
                    setter.SetValue(destination, value);
                }
            }
        }

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

        public static void AddSettingValues(JObject jsonRoot, DynamicSectionViewModel section)
        {
            if (section.Settings != null)
            {
                foreach (var setting in section.Settings)
                {
                    if (!string.IsNullOrEmpty(setting.JsonKey))
                    {
                        var jObject = jsonRoot;
                        var keys = setting.JsonKey.Split('.');

                        for (int i = 0; i < keys.Length; i++)
                        {
                            if (i == keys.Length - 1)
                            {
                                var finalValue = Convert.ToInt32(setting.Policy) == (int)SettingsPolicies.UseDefaultValue ? setting.DefaultValue : setting.Value;

                                switch (setting.SettingType)
                                {
                                    case SettingTypes.Number:
                                        jObject.Add(new JProperty(keys[i], Convert.ToInt32(finalValue)));
                                        break;
                                    case SettingTypes.Boolean:
                                        jObject.Add(new JProperty(keys[i], Convert.ToBoolean(finalValue)));
                                        break;
                                    case SettingTypes.Date:
                                        jObject.Add(new JProperty(keys[i], Convert.ToDateTime(finalValue)));
                                        break;
                                    case SettingTypes.Undefined:
                                    case SettingTypes.Text:
                                    default:
                                        jObject.Add(new JProperty(keys[i], finalValue));
                                        break;
                                }
                            }
                            else
                            {
                                JToken token = null;
                                var exists = jObject.TryGetValue(keys[i], out token);

                                if (exists)
                                {
                                    if (token is JObject)
                                    {
                                        jObject = (JObject)jObject[keys[i]];
                                    }
                                }
                                else
                                {
                                    jObject.Add(new JProperty(keys[i], JObject.Parse("{}")));
                                    jObject = (JObject)jObject[keys[i]];
                                }
                            }
                        }
                    }
                }
            }

            if (section.Sections != null)
            {
                foreach (var item in section.Sections)
                {
                    AddSettingValues(jsonRoot, item);
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

        public static void SetSettingValues(DynamicSectionViewModel section, string settingsValues, IList<KeyValuePairViewModel> policiesValues)
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
                            var keys = setting.JsonKey.Split('.');
                            var jObject = JObject.Parse(settingsValues);

                            string value = null;

                            foreach (var key in keys)
                            {
                                var jValue = jObject[key];

                                if (jValue is JValue finalValue)
                                    value = finalValue.ToString();
                                else
                                    jObject = (JObject)jObject[key];
                            }                            

                            setting.Value = setting.SettingType == SettingTypes.Boolean ? value.ToLower() : value;
                        }
                    }
                }
            }
        }
    }
}
