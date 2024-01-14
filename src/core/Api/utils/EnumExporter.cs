using System;
using System.Collections.Generic;

namespace SM64DSe.core.Api.utils
{
    public class EnumExporter
    {
        public static Dictionary<int, string> ExportEnumAsKeyValue<T>() where T : Enum
        {
            var enumType = typeof(T);
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type.");
            }

            var enumValues = (T[])Enum.GetValues(enumType);
            var enumKeyValue = new Dictionary<int, string>();

            foreach (var enumValue in enumValues)
            {
                int enumIntValue = Convert.ToInt32(enumValue);
                string enumName = Enum.GetName(enumType, enumValue);
                enumKeyValue.Add(enumIntValue, enumName);
            }

            return enumKeyValue;
        }
    }
}