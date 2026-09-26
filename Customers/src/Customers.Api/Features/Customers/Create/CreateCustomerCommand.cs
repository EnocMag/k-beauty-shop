using AutoMapper;
using Customers.Domain.Entities;
using Customers.Features.Customers.Common;
using Customers.Infrastructure.DbContexts;
using Customers.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Customers.Features.Customers.Create;

public record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber
) : IRequest<Result<CustomerResponse>>;

public class CreateCustomerCommandHandler(
    CustomersDbContext context,
    AutoMapper.IMapper mapper,
    ILogger<CreateCustomerCommandHandler> logger)
    : IRequestHandler<CreateCustomerCommand, Result<CustomerResponse>>
{
    public async Task<Result<CustomerResponse>> Handle(
        CreateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        if (await context.Customers.AnyAsync(c => c.Email == command.Email, cancellationToken))
        {
            logger.LogWarning("Customer creation failed: Email {Email} already exists", command.Email);
            return Result<CustomerResponse>.Fail(MsgConstants.CUSTOMER_ALREADY_EXISTS);
        }

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = command.FirstName.Trim(),
            LastName = command.LastName.Trim(),
            Email = command.Email.Trim().ToLowerInvariant(),
            PhoneNumber = command.PhoneNumber.Trim(),
            CreatedAt = DateTime.UtcNow,
            Status = CustomerStatus.Active
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created customer with ID {CustomerId}", customer.Id);

        var response = mapper.Map<CustomerResponse>(customer);

        return Result<CustomerResponse>.Ok(MsgConstants.SUCCESS, response);
    }
}
