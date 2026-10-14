using FluentValidation;

namespace Customers.Features.Customers.GetById;

public class GetCustomerByIdValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Customer ID must not be empty.");
    }
}
