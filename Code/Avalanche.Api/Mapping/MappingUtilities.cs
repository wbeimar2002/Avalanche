using Avalanche.Api.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Mapping
{
    public static class MappingUtilities
    {
        public static string GetSexTranslationKey(string id)
        {
            switch (id)
            {
                case "F":
                    return "sex.female";
                case "M":
                    return "sex.male";
                case "O":
                    return "sex.other";
                case "U":
                default:
                    return "sex.unspecified";
            }
        }

        public static KeyValuePairViewModel GetSexViewModel(string stringValue)
        {
            return new KeyValuePairViewModel()
            {
                Id = stringValue,
                TranslationKey = GetSexTranslationKey(stringValue)
            };
        }
    }
}
