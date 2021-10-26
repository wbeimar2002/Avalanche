using AutoMapper;
using Avalanche.Api.ViewModels;
using Google.Protobuf.Collections;
using System.Collections.Generic;

namespace Avalanche.Api.Mapping
{
    public static class MappingUtilities
    {
        public class EnumerableToRepeatedFieldTypeConverter<TITemSource, TITemDest> : ITypeConverter<IEnumerable<TITemSource>, RepeatedField<TITemDest>>
        {
            public RepeatedField<TITemDest> Convert(IEnumerable<TITemSource> source, RepeatedField<TITemDest> destination, ResolutionContext context)
            {
                destination = destination ?? new RepeatedField<TITemDest>();
                if (source != null)
                {
                    foreach (var item in source)
                    {
                        destination.Add(context.Mapper.Map<TITemDest>(item));
                    }
                }
                return destination;
            }
        }

        public class RepeatedFieldToListTypeConverter<TITemSource, TITemDest> : ITypeConverter<RepeatedField<TITemSource>, List<TITemDest>>
        {
            public List<TITemDest> Convert(RepeatedField<TITemSource> source, List<TITemDest> destination, ResolutionContext context)
            {
                destination = destination ?? new List<TITemDest>();
                foreach (var item in source)
                {
                    destination.Add(context.Mapper.Map<TITemDest>(item));
                }
                return destination;
            }
        }

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

        public static KeyValuePairViewModel GetSexViewModel(string stringValue) =>
            new KeyValuePairViewModel()
            {
                Id = stringValue,
                TranslationKey = GetSexTranslationKey(stringValue)
            };
    }
}
