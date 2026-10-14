using FluentValidation.TestHelper;
using Products.Api.Validators;
using Products.Domain.Commands.Categories;
using System.Text.Json;
using Xunit;

namespace Products.Api.Tests.Validators;

public class UpdateCategoryCommandValidatorTests
{
    private readonly UpdateCategoryCommandValidator _validator;

    public UpdateCategoryCommandValidatorTests()
    {
        _validator = new UpdateCategoryCommandValidator();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Have_Error_When_Id_Is_Zero_Or_Negative(int id)
    {
        var command = new UpdateCategoryCommand { Id = id };
        command.UpdatedFields.Add("name", "Valid Name");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Category ID must be greater than 0.");
    }

    [Fact]
    public void Should_Have_Error_When_UpdatedFields_Is_Null()
    {
        var command = new UpdateCategoryCommand { Id = 1, UpdatedFields = null! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UpdatedFields)
              .WithErrorMessage("UpdatedFields cannot be null.");
    }

    [Fact]
    public void Should_Have_Error_When_UpdatedFields_Is_Empty()
    {
        var command = new UpdateCategoryCommand { Id = 1 };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UpdatedFields.Keys)
              .WithErrorMessage("At least one field must be updated.");
    }

    [Fact]
    public void Should_Have_Error_When_Invalid_Field_Provided()
    {
        var command = new UpdateCategoryCommand { Id = 1 };
        command.UpdatedFields.Add("InvalidField", "Value");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UpdatedFields.Keys)
              .WithErrorMessage($"Invalid field. Valid fields are: {string.Join(", ", UpdateCategoryCommand.ValidFields)}");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Not_String()
    {
        var command = new UpdateCategoryCommand { Id = 1 };
        command.UpdatedFields.Add("Name", 12345);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UpdatedFields["Name"])
              .WithErrorMessage("Name must be a string.");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Whitespace()
    {
        var command = new UpdateCategoryCommand { Id = 1 };
        command.UpdatedFields.Add("Name", "   ");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UpdatedFields["Name"])
              .WithErrorMessage("Name cannot contain only whitespace.");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_Maximum_Length()
    {
        var command = new UpdateCategoryCommand { Id = 1 };
        command.UpdatedFields.Add("Name", new string('A', 101));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UpdatedFields["Name"])
              .WithErrorMessage("Name cannot exceed 100 characters.");
    }

    [Fact]
    public void Should_Have_Error_When_Description_Exceeds_Maximum_Length()
    {
        var command = new UpdateCategoryCommand { Id = 1 };
        command.UpdatedFields.Add("Description", new string('B', 501));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UpdatedFields["Description"])
              .WithErrorMessage("Description cannot exceed 500 characters.");
    }

    [Fact]
    public void Should_Have_Error_When_Description_Is_Only_Whitespace()
    {
        var command = new UpdateCategoryCommand { Id = 1 };
        command.UpdatedFields.Add("Description", "    ");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UpdatedFields["Description"])
              .WithErrorMessage("Description cannot contain only whitespace.");
    }

    [Fact]
    public void Should_Have_Error_When_ParentCategoryId_Is_Self()
    {
        var command = new UpdateCategoryCommand { Id = 5 };
        command.UpdatedFields.Add("ParentCategoryId", 5);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x)
              .WithErrorMessage("A category cannot be its own parent.");
    }

    [Fact]
    public void Should_Have_Error_When_ParentCategoryId_Is_Negative_Or_Zero()
    {
        var command = new UpdateCategoryCommand { Id = 5 };
        command.UpdatedFields.Add("ParentCategoryId", 0);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x)
              .WithErrorMessage("ParentCategoryId must be greater than 0.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_ParentCategoryId_Is_Null()
    {
        var command = new UpdateCategoryCommand { Id = 5 };
        command.UpdatedFields.Add("ParentCategoryId", null!);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Valid_Data_Provided()
    {
        var command = new UpdateCategoryCommand { Id = 5 };
        command.UpdatedFields.Add("Name", "Skincare");
        command.UpdatedFields.Add("Description", "All skincare items");
        command.UpdatedFields.Add("ParentCategoryId", 2);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Support_JsonElement_Values()
    {
        var command = new UpdateCategoryCommand { Id = 5 };
        command.UpdatedFields.Add("name", JsonDocument.Parse("\"Skincare\"").RootElement);
        command.UpdatedFields.Add("description", JsonDocument.Parse("\"Valid description\"").RootElement);
        command.UpdatedFields.Add("parentCategoryId", JsonDocument.Parse("2").RootElement);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
