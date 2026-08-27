using System.Net;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Products.Domain.DTOs;

[Route("api/[controller]")]
[ApiController]
public abstract class BaseController(
    IMediator mediator,
    ILogger<BaseController> logger) : ControllerBase
{
    protected async Task<IActionResult> processCommand<T>(
        IRequest<Result<T>> request,
        CancellationToken cancellationToken)
        where T : class
    {
        try
        {
            var result = await mediator.Send(request, cancellationToken);

            return StatusCode((int)result.State, result);
        }
        catch (ValidationException e)
        {
            var errors = e.Errors
                .Select(error => error.ErrorMessage)
                .Distinct()
                .ToList();

            var result = Result<T>.Fail(
                "Validation failed.",
                errors,
                HttpStatusCode.BadRequest);

            return StatusCode(
                StatusCodes.Status400BadRequest,
                result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "An error occurred while processing the request.");

            var result = Result<T>.Fail(
                "An error occurred while processing the request.",
                HttpStatusCode.InternalServerError);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                result);
        }
    }
}
