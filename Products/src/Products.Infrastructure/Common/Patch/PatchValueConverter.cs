using System.Text.Json;
using System.Text.RegularExpressions;

namespace Products.Infrastructure.Common.Patch;

public static class PatchValueConverter
{
    public static object? Convert(object? rawValue, Type targetType)
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (rawValue == null)
        {
            if (targetType.IsValueType && underlyingType == null)
                throw new InvalidOperationException($"Cannot convert null to non-nullable type {targetType.Name}.");
            return null;
        }

        if (rawValue is JsonElement element)
        {
            rawValue = element.Deserialize(targetType);
        }

        if (underlyingType == typeof(string))
        {
            return Regex.Replace(
                rawValue?.ToString()?.Trim() ?? string.Empty,
                @"\s+",
                " ");
        }

        if (underlyingType == typeof(Guid))
            return Guid.Parse(rawValue.ToString()!);

        if (underlyingType.IsEnum)
            return Enum.Parse(
                underlyingType,
                rawValue.ToString()!,
                ignoreCase: true);

        return System.Convert.ChangeType(
            rawValue,
            underlyingType);
    }
}
