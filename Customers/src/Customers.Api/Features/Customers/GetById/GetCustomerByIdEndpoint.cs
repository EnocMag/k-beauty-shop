using System.Net;
using Customers.Features.Customers.Common;
using Customers.Utils;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Customers.Features.Customers.GetById;

public class GetCustomerByIdEndpoint(ISender sender)
    : Endpoint<GetCustomerByIdRequest, Results<Ok<CustomerResponse>, NotFound, ProblemDetails>>
{
    public override void Configure()
    {
        Get("/api/customers/{id:guid}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Gets a customer by ID";
            s.Description = "Retrieves a customer document from MongoDB by its unique identifier";
        });
    }

    public override async Task<Results<Ok<CustomerResponse>, NotFound, ProblemDetails>> ExecuteAsync(
        GetCustomerByIdRequest req,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetCustomerByIdQuery(req.Id), ct);

        if (result.State == HttpStatusCode.NotFound)
        {
            return TypedResults.NotFound();
        }

        result.EnsureSuccess();

        return TypedResults.Ok(result.Data!);
    }
}
