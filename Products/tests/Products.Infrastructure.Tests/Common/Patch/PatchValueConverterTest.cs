using System.Text.Json;
using Products.Infrastructure.Common.Patch;

namespace Products.Infrastructure.Tests.Common.Patch;

public class PatchValueConverterTest
{
    [Fact]
    public void Convert_ShouldReturnNull_WhenRawValueIsNullAndTargetTypeIsReferenceType()
    {
        // Arrange
        object? rawValue = null;

        // Act
        var result = PatchValueConverter.Convert(
            rawValue,
            typeof(string));

        // Assert
        Assert.Null(result);
    }
    [Fact]
    public void Convert_ShouldReturnNull_WhenRawValueIsNullAndTargetTypeIsNullableValueType()
    {
        // Arrange
        object? rawValue = null;

        // Act
        var result = PatchValueConverter.Convert(
            rawValue,
            typeof(int?));

        // Assert
        Assert.Null(result);
    }
    [Fact]
    public void Convert_ShouldThrowInvalidOperationException_WhenRawValueIsNullAndTargetTypeIsNonNullableValueType()
    {
        // Arrange
        object? rawValue = null;

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() =>
            PatchValueConverter.Convert(
                rawValue,
                typeof(int)));

        // Assert
        Assert.Equal(
            "Cannot convert null to non-nullable type Int32.",
            exception.Message);
    }
    [Fact]
    public void Convert_ShouldNormalizeStringWhitespace()
    {
        // Arrange
        var rawValue = "   Hello     World   ";

        // Act
        var result = PatchValueConverter.Convert(
            rawValue,
            typeof(string));

        // Assert
        Assert.Equal("Hello World", result);
    }
    [Theory]
    [InlineData("Hello     World", "Hello World")]
    [InlineData("Hello\tWorld", "Hello World")]
    [InlineData("Hello\nWorld", "Hello World")]
    [InlineData("  Hello   World  ", "Hello World")]
    [InlineData("Hello\r\nWorld", "Hello World")]
    [InlineData("   Hello   ", "Hello")]
    public void Convert_ShouldNormalizeDifferentWhitespaceCharacters(
        string rawValue,
        string expected)
    {
        // Act
        var result = PatchValueConverter.Convert(
            rawValue,
            typeof(string));

        // Assert
        Assert.Equal(expected, result);
    }
    [Fact]
    public void Convert_ShouldReturnEmptyString_WhenStringValueIsOnlyWhitespace()
    {
        // Arrange
        var rawValue = "     \t   \n   ";

        // Act
        var result = PatchValueConverter.Convert(
            rawValue,
            typeof(string));

        // Assert
        Assert.Equal(string.Empty, result);
    }
    [Fact]
    public void Convert_ShouldConvertToGuid()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        var rawValue = expectedGuid.ToString();

        // Act
        var result = PatchValueConverter.Convert(
            rawValue,
            typeof(Guid));

        // Assert
        Assert.IsType<Guid>(result);
        Assert.Equal(expectedGuid, result);
    }
    [Fact]
    public void Convert_ShouldThrowFormatException_WhenGuidIsInvalid()
    {
        // Arrange
        var rawValue = "invalid-guid";

        // Act & Assert
        Assert.Throws<FormatException>(() =>
            PatchValueConverter.Convert(
                rawValue,
                typeof(Guid)));
    }

    [Fact]
    public void Convert_ShouldConvertJsonElementToString()
    {
        // Arrange
        using var document = JsonDocument.Parse("\"   Hello     World   \"");
        var rawValue = document.RootElement;

        // Act
        var result = PatchValueConverter.Convert(
            rawValue,
            typeof(string));

        // Assert
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void Convert_ShouldConvertJsonElementToInt()
    {
        // Arrange
        using var document = JsonDocument.Parse("123");
        var rawValue = document.RootElement;

        // Act
        var result = PatchValueConverter.Convert(
            rawValue,
            typeof(int));

        // Assert
        Assert.Equal(123, result);
    }

    [Fact]
    public void Convert_ShouldConvertJsonElementToDecimal()
    {
        // Arrange
        using var document = JsonDocument.Parse("150.75");
        var rawValue = document.RootElement;

        // Act
        var result = PatchValueConverter.Convert(
            rawValue,
            typeof(decimal));

        // Assert
        Assert.Equal(150.75m, result);
    }

    [Fact]
    public void Convert_ShouldConvertJsonElementToBoolean()
    {
        // Arrange
        using var document = JsonDocument.Parse("true");
        var rawValue = document.RootElement;

        // Act
        var result = PatchValueConverter.Convert(
            rawValue,
            typeof(bool));

        // Assert
        Assert.True((bool)result!);
    }

    [Fact]
    public void Convert_ShouldConvertJsonElementToGuid()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();

        using var document = JsonDocument.Parse(
            $"\"{expectedGuid}\"");

        var rawValue = document.RootElement;

        // Act
        var result = PatchValueConverter.Convert(
            rawValue,
            typeof(Guid));

        // Assert
        Assert.Equal(expectedGuid, result);
    }

    [Theory]
    [InlineData("123", typeof(int), 123)]
    [InlineData("123", typeof(long), 123L)]
    [InlineData("123.45", typeof(double), 123.45)]
    public void Convert_ShouldConvertPrimitiveTypes(
        string rawValue,
        Type targetType,
        object expected)
    {
        // Act
        var result = PatchValueConverter.Convert(
            rawValue,
            targetType);

        // Assert
        Assert.Equal(expected, result);
    }
    [Fact]
    public void Convert_ShouldConvertStringToDecimal()
    {
        // Arrange
        var rawValue = "123.45";

        // Act
        var result = PatchValueConverter.Convert(
            rawValue,
            typeof(decimal));

        // Assert
        Assert.IsType<decimal>(result);
        Assert.Equal(123.45m, result);
    }

    [Fact]
    public void Convert_ShouldConvertToNullableInt()
    {
        // Arrange
        var rawValue = "123";

        // Act
        var result = PatchValueConverter.Convert(
            rawValue,
            typeof(int?));

        // Assert
        Assert.Equal(123, result);
    }

    [Fact]
    public void Convert_ShouldConvertToDateTime()
    {
        // Arrange
        var expected = new DateTime(2026, 8, 31, 15, 30, 0);
        var rawValue = expected.ToString("O");

        // Act
        var result = PatchValueConverter.Convert(
            rawValue,
            typeof(DateTime));

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_ShouldThrowFormatException_WhenValueCannotBeConvertedToInt()
    {
        // Arrange
        var rawValue = "not-a-number";

        // Act & Assert
        Assert.Throws<FormatException>(() =>
            PatchValueConverter.Convert(
                rawValue,
                typeof(int)));
    }
}
