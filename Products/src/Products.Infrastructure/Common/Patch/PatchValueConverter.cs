using System.Text.Json;
using System.Text.RegularExpressions;

namespace Products.Infrastructure.Common.Patch;

public static class PatchValueConverter
{
    /// <summary>
    /// Converts a raw value to the specified target type, handling null values, JSON elements, strings and GUIDs
    /// </summary>
    /// <param name="rawValue"></param>
    /// <param name="targetType"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static object? Convert(object? rawValue, Type targetType)
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (rawValue == null)
        {
            if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
            {
                throw new InvalidOperationException(
                    $"Cannot convert null to non-nullable type {targetType.Name}.");
            }
            return null;
        }

        if (rawValue is JsonElement element)
        {
            rawValue = element.Deserialize(targetType);
        }

        if (underlyingType == typeof(string))
        {
            // Normalizes whitespace by replacing one or more consecutive whitespace characters
            // (spaces, tabs, line breaks, etc.) with a single space.
            return Regex.Replace(
                rawValue?.ToString()?.Trim() ?? string.Empty,
                @"\s+",
                " ");
        }

        if (underlyingType == typeof(Guid))
            return Guid.Parse(rawValue.ToString()!);

        return System.Convert.ChangeType(
            rawValue,
            underlyingType);
    }
}
