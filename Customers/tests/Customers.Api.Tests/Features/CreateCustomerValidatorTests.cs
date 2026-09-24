using Customers.Features.Customers.Create;
using FluentAssertions;
using Xunit;

namespace Customers.Api.Tests.Features;

public class CreateCustomerValidatorTests
{
    private readonly CreateCustomerValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_ShouldPassValidation()
    {
        // Arrange
        var command = new CreateCustomerCommand("Jane", "Doe", "jane.doe@example.com", "+1-555-123-4567");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_WithEmptyOrNullFirstName_ShouldFailValidation(string? firstName)
    {
        // Arrange
        var command = new CreateCustomerCommand(firstName!, "Doe", "jane.doe@example.com", "+1-555-123-4567");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCustomerCommand.FirstName));
    }

    [Fact]
    public async Task Validate_WithFirstNameExceedingMaxLength_ShouldFailValidation()
    {
        // Arrange
        var longFirstName = new string('a', 101);
        var command = new CreateCustomerCommand(longFirstName, "Doe", "jane.doe@example.com", "+1-555-123-4567");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCustomerCommand.FirstName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_WithEmptyOrNullLastName_ShouldFailValidation(string? lastName)
    {
        // Arrange
        var command = new CreateCustomerCommand("Jane", lastName!, "jane.doe@example.com", "+1-555-123-4567");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCustomerCommand.LastName));
    }

    [Fact]
    public async Task Validate_WithLastNameExceedingMaxLength_ShouldFailValidation()
    {
        // Arrange
        var longLastName = new string('a', 101);
        var command = new CreateCustomerCommand("Jane", longLastName, "jane.doe@example.com", "+1-555-123-4567");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCustomerCommand.LastName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_WithEmptyOrNullEmail_ShouldFailValidation(string? email)
    {
        // Arrange
        var command = new CreateCustomerCommand("Jane", "Doe", email!, "+1-555-123-4567");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCustomerCommand.Email));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("jane.doe")]
    [InlineData("@example.com")]
    [InlineData("jane.doe@")]
    public async Task Validate_WithInvalidEmailFormat_ShouldFailValidation(string invalidEmail)
    {
        // Arrange
        var command = new CreateCustomerCommand("Jane", "Doe", invalidEmail, "+1-555-123-4567");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCustomerCommand.Email));
    }

    [Fact]
    public async Task Validate_WithEmailExceedingMaxLength_ShouldFailValidation()
    {
        // Arrange
        var longEmail = new string('a', 244) + "@example.com"; // 256 chars
        var command = new CreateCustomerCommand("Jane", "Doe", longEmail, "+1-555-123-4567");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCustomerCommand.Email));
    }

    [Fact]
    public async Task Validate_WithPhoneNumberExceedingMaxLength_ShouldFailValidation()
    {
        // Arrange
        var longPhone = new string('1', 21);
        var command = new CreateCustomerCommand("Jane", "Doe", "jane.doe@example.com", longPhone);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCustomerCommand.PhoneNumber));
    }
}
