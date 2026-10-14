using System.Net;
using AutoMapper;
using Customers.Features.Customers.Common;
using Customers.Infrastructure.DbContexts;
using Customers.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Customers.Features.Customers.GetById;

public record GetCustomerByIdQuery(Guid Id) : IRequest<Result<CustomerResponse>>;

public class GetCustomerByIdQueryHandler(
    CustomersDbContext context,
    AutoMapper.IMapper mapper,
    ILogger<GetCustomerByIdQueryHandler> logger)
    : IRequestHandler<GetCustomerByIdQuery, Result<CustomerResponse>>
{
    public async Task<Result<CustomerResponse>> Handle(
        GetCustomerByIdQuery query,
        CancellationToken cancellationToken)
    {
        var customer = await context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.Id, cancellationToken);

        if (customer == null)
        {
            logger.LogInformation("Customer with ID {CustomerId} not found", query.Id);
            return Result<CustomerResponse>.Fail(MsgConstants.CUSTOMER_NOT_FOUND, HttpStatusCode.NotFound);
        }

        var response = mapper.Map<CustomerResponse>(customer);

        return Result<CustomerResponse>.Ok(MsgConstants.SUCCESS, response);
    }
}
