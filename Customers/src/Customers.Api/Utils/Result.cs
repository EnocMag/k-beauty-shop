using System.Net;

namespace Customers.Utils;

public class Result<T> where T : class
{
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public HttpStatusCode State { get; }
    public bool IsError => State != HttpStatusCode.OK;
    public bool IsSuccess => State == HttpStatusCode.OK;
    public IEnumerable<string> Errors { get; set; } = new List<string>();

    private Result(T? data, string message, HttpStatusCode state, IEnumerable<string>? errors = null)
    {
        Data = data;
        Message = message;
        State = state;
        if (errors != null)
            Errors = errors;
    }

    public Result()
    {
    }

    public static Result<T> Ok(string message)
    {
        return new Result<T>(null, message, HttpStatusCode.OK);
    }

    public static Result<T> Ok(string message, T data)
    {
        return new Result<T>(data, message, HttpStatusCode.OK);
    }

    public static Result<T> Fail(string message, HttpStatusCode state = HttpStatusCode.BadRequest)
    {
        var result = new Result<T>(null, message, state)
        {
            Errors = [message]
        };
        return result;
    }

    public static Result<T> Fail(string message, IEnumerable<string> errors, HttpStatusCode state = HttpStatusCode.BadRequest)
    {
        var result = Fail(message, state);
        if (errors.Any())
            result.Errors = errors;
        return result;
    }

    public static Result<T> Fail(string message, IEnumerable<string> errors, T data, HttpStatusCode state = HttpStatusCode.BadRequest)
    {
        var result = Fail(message, errors, state);
        result.Data = data;
        return result;
    }

    public void EnsureSuccess()
    {
        if (IsError)
            throw new ProblemsException(Message, Errors, this.State);
    }
}
