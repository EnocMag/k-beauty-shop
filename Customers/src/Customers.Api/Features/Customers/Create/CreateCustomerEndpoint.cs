using Customers.Features.Customers.Common;
using Customers.Utils;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Customers.Features.Customers.Create;

public class CreateCustomerEndpoint(ISender sender)
    : Endpoint<CreateCustomerRequest, Results<Created<CustomerResponse>, ProblemDetails>>
{
    public override void Configure()
    {
        Post("/api/customers");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Creates a new customer";
            s.Description = "Validates input and creates a customer record stored in MongoDB";
        });
    }

    public override async Task<Results<Created<CustomerResponse>, ProblemDetails>> ExecuteAsync(
        CreateCustomerRequest req,
        CancellationToken ct)
    {
        var command = new CreateCustomerCommand(
            req.FirstName,
            req.LastName,
            req.Email,
            req.PhoneNumber);

        var result = await sender.Send(command, ct);
        result.EnsureSuccess();

        return TypedResults.Created($"/api/customers/{result.Data!.Id}", result.Data);
    }
}
