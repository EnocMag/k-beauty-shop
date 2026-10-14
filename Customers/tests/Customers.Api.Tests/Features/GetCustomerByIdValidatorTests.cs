using Customers.Features.Customers.GetById;
using FluentAssertions;
using Xunit;

namespace Customers.Api.Tests.Features;

public class GetCustomerByIdValidatorTests
{
    private readonly GetCustomerByIdValidator _validator = new();

    [Fact]
    public void Validate_WithValidId_IsValid()
    {
        var result = _validator.Validate(new GetCustomerByIdQuery(Guid.NewGuid()));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyId_IsInvalidWithMessage()
    {
        var result = _validator.Validate(new GetCustomerByIdQuery(Guid.Empty));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Customer ID must not be empty.");
    }
}
