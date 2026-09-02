using FluentValidation.TestHelper;
using Products.Api.Validators;
using Products.Domain.Commands.Products;
using System.Text.Json;
using Xunit;

namespace Products.Api.Tests.Validators;

public class UpdateProductCommandValidatorTests
{
    private readonly UpdateProductCommandValidator _validator;

    public UpdateProductCommandValidatorTests()
    {
        _validator = new UpdateProductCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Id_Is_Zero()
    {
        // Arrange
        var command = new UpdateProductCommand { Id = 0 };
        command.UpdatedFields.Add("name", JsonDocument.Parse("\"Valid Name\"").RootElement);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Product ID must be greater than 0.");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Not_String()
    {
        // Arrange
        var command = new UpdateProductCommand { Id = 1 };
        command.UpdatedFields.Add("name", JsonDocument.Parse("123").RootElement);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdatedFields["name"])
              .WithErrorMessage("Name must be a string.");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_Maximum_Length()
    {
        // Arrange
        var command = new UpdateProductCommand { Id = 1 };
        var name = new string('A', 151);

        command.UpdatedFields.Add(
            "name",
            JsonDocument.Parse(JsonSerializer.Serialize(name)).RootElement);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdatedFields["name"].GetString())
              .WithErrorMessage("Name cannot exceed 150 characters.");
    }
    [Fact]
    public void Should_Not_Have_Error_When_Name_Has_Maximum_Allowed_Length()
    {
        // Arrange
        var command = new UpdateProductCommand { Id = 1 };
        var name = new string('A', 150);

        command.UpdatedFields.Add(
            "name",
            JsonDocument.Parse(JsonSerializer.Serialize(name)).RootElement);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }


    [Fact]
    public void Should_Have_Error_When_UpdatedFields_Is_Null()
    {
        // Arrange
        var command = new UpdateProductCommand { Id = 1, UpdatedFields = null! };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdatedFields)
              .WithErrorMessage("UpdatedFields cannot be null.");
    }

    [Fact]
    public void Should_Have_Error_When_No_Fields_Are_Updated()
    {
        // Arrange
        var command = new UpdateProductCommand { Id = 1 };
        // UpdatedFields is initialized to empty dictionary

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdatedFields.Keys)
              .WithErrorMessage("At least one field must be updated.");
    }

    [Fact]
    public void Should_Have_Error_When_Invalid_Field_Is_Provided()
    {
        // Arrange
        var command = new UpdateProductCommand { Id = 1 };
        command.UpdatedFields.Add("invalid_field", JsonDocument.Parse("\"value\"").RootElement);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdatedFields.Keys)
          .WithErrorMessage(
              $"Invalid field. Valid fields are: {string.Join(", ", UpdateProductCommand.ValidFields)}");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        // Arrange
        var command = new UpdateProductCommand { Id = 1 };
        command.UpdatedFields.Add("name", JsonDocument.Parse("\"\"").RootElement);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdatedFields["name"].GetString())
              .WithErrorMessage("Name is required when updating.");
    }

    [Fact]
    public void Should_Have_Error_When_Description_Is_Not_String()
    {
        // Arrange
        var command = new UpdateProductCommand { Id = 1 };
        command.UpdatedFields.Add(
            "description",
            JsonDocument.Parse("123").RootElement);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdatedFields["description"])
              .WithErrorMessage("Description must be a string.");
    }

    [Fact]
    public void Should_Have_Error_When_Description_Exceeds_Maximum_Length()
    {
        // Arrange
        var command = new UpdateProductCommand { Id = 1 };
        var description = new string('A', 501);

        command.UpdatedFields.Add(
            "description",
            JsonDocument.Parse(JsonSerializer.Serialize(description)).RootElement);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdatedFields["description"].GetString())
              .WithErrorMessage("Description cannot exceed 500 characters.");
    }
    [Fact]
    public void Should_Not_Have_Error_When_Description_Has_Maximum_Allowed_Length()
    {
        // Arrange
        var command = new UpdateProductCommand { Id = 1 };
        var description = new string('A', 500);

        command.UpdatedFields.Add(
            "description",
            JsonDocument.Parse(JsonSerializer.Serialize(description)).RootElement);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Price_Is_Less_Than_Or_Equal_To_Zero()
    {
        // Arrange
        var command = new UpdateProductCommand { Id = 1 };
        command.UpdatedFields.Add("price", JsonDocument.Parse("0").RootElement);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdatedFields["price"].GetDecimal())
              .WithErrorMessage("Price must be greater than 0.");
    }

    [Fact]
    public void Should_Have_Error_When_Price_Is_Not_Number()
    {
        // Arrange
        var command = new UpdateProductCommand { Id = 1 };
        command.UpdatedFields.Add(
            "price",
            JsonDocument.Parse("\"15.5\"").RootElement);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdatedFields["price"])
              .WithErrorMessage("Price must be a number.");
    }

    [Theory]
    [InlineData("weight", "Weight must be a number.")]
    [InlineData("height", "Height must be a number.")]
    [InlineData("width", "Width must be a number.")]
    [InlineData("length", "Length must be a number.")]
    public void Should_Have_Error_When_Dimension_Is_Not_Number(string field, string expectedMessage)
    {
        // Arrange
        var command = new UpdateProductCommand { Id = 1 };

        command.UpdatedFields.Add(
            field,
            JsonDocument.Parse("\"invalid\"").RootElement);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdatedFields[field])
              .WithErrorMessage(expectedMessage);
    }
    [Theory]
    [InlineData("weight", "Weight must be greater than 0.")]
    [InlineData("height", "Height must be greater than 0.")]
    [InlineData("width", "Width must be greater than 0.")]
    [InlineData("length", "Length must be greater than 0.")]
    public void Should_Have_Error_When_Dimension_Is_Less_Than_Or_Equal_To_Zero(
    string field, string expectedMessage)
    {
        // Arrange
        var command = new UpdateProductCommand { Id = 1 };

        command.UpdatedFields.Add(
            field,
            JsonDocument.Parse("0").RootElement);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(
            x => x.UpdatedFields[field].GetDecimal())
              .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        var command = new UpdateProductCommand { Id = 1 };

        command.UpdatedFields.Add(
            "name",
            JsonDocument.Parse("\"Valid Name\"").RootElement);

        command.UpdatedFields.Add(
            "description",
            JsonDocument.Parse("\"Valid description\"").RootElement);

        command.UpdatedFields.Add(
            "price",
            JsonDocument.Parse("15.5").RootElement);

        command.UpdatedFields.Add(
            "weight",
            JsonDocument.Parse("2.5").RootElement);

        command.UpdatedFields.Add(
            "height",
            JsonDocument.Parse("10").RootElement);

        command.UpdatedFields.Add(
            "width",
            JsonDocument.Parse("20").RootElement);

        command.UpdatedFields.Add(
            "length",
            JsonDocument.Parse("30").RootElement);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
