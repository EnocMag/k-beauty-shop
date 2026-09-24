using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Customers.Utils;

[Serializable]
public class ProblemsException : Exception
{
    public string Msg { get; }
    public IEnumerable<string> Errors { get; }
	public HttpStatusCode StatusCode { get; }

	public ProblemsException(string msg, IEnumerable<string> errors) : base(msg)
    {
        Msg = msg;
        Errors = errors;
    }

    public ProblemsException(string msg, IEnumerable<string> errors, HttpStatusCode statusCode) : this(msg, errors)
    {
        this.StatusCode = statusCode;
    }

    public ProblemsException(string msg) : this(msg, [msg])
    {
    }
}

public class ProblemsExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ProblemsException problemsException)
        {
            return false;
        }

        var details = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = (int)problemsException.StatusCode,
            Title = problemsException.Msg,
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Extensions = new Dictionary<string, object?>
            {
                { "errors", problemsException.Errors }
            }
        };

        httpContext.Response.StatusCode = (int)problemsException.StatusCode;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = details
        });
    }
}
