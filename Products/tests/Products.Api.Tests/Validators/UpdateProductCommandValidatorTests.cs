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
    public void Should_Have_Error_When_Id_Is_Less_Than_Or_Equal_To_Zero()
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
        result.ShouldHaveValidationErrorFor(x => x.UpdatedFields.Keys);
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
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new UpdateProductCommand { Id = 1 };
        command.UpdatedFields.Add("name", JsonDocument.Parse("\"Valid Name\"").RootElement);
        command.UpdatedFields.Add("price", JsonDocument.Parse("15.5").RootElement);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
