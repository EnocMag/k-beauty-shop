using MediatR;
using Microsoft.AspNetCore.Mvc;
using Products.Domain.DTOs;
using System.Net;

namespace Products.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public abstract class BaseController(IMediator mediator, ILogger<BaseController> logger) : ControllerBase
{
    protected async Task<IActionResult> processCommand<T>(IRequest<Result<T>> request, CancellationToken cancellationToken) where T : class
    {
        try
        {
            var result = await mediator.Send(request, cancellationToken);
            return StatusCode((int)result.State, result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while processing the request.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                Result<T>.Fail("An error occurred while processing the request.", HttpStatusCode.InternalServerError));
        }
    }
}