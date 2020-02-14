using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Extensions
{
    public static class EnumExtension
    {
        public static string EnumDescription(this Enum value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("Enum value is null !");
            }

            string description = value.ToString();
            FieldInfo fieldInfo = value.GetType().GetField(description);
            DescriptionAttribute[] attributes = (DescriptionAttribute[])
            fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
            {
                description = attributes[0].Description;
            }
            return description;
        }

        public static TYourEnum FindEnumFromDescription<TYourEnum>(this string value)
           where TYourEnum : struct, IConvertible
        {
            if (!typeof(TYourEnum).IsEnum || string.IsNullOrWhiteSpace(value))
            {
                return default(TYourEnum);
            }

            var enumValues = Enum.GetValues(typeof(TYourEnum));

            foreach (var item in enumValues)
            {
                if (value.ToLower().Equals((item as Enum).EnumDescription().ToLower()))
                {
                    return (TYourEnum)item;
                }
            }

            return default(TYourEnum);
        }
    }
}
