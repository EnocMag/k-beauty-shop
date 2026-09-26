using FluentValidation;

namespace Customers.Features.Customers.Create;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    // local-part@domain.tld, TLD of at least 2 letters
    private const string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*\.[a-zA-Z]{2,}$";

    // Optional leading '+', 7-15 digits, separated by spaces, dashes, dots or parentheses
    private const string PhoneNumberPattern = @"^\+?(?:[\s\-.()]*\d){7,15}[\s\-.()]*$";

    public CreateCustomerValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .Matches(EmailPattern).WithMessage("A valid email address is required.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
            .Matches(PhoneNumberPattern).WithMessage("A valid phone number is required.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}
