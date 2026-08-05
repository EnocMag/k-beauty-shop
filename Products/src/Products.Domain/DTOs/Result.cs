using System.Net;

namespace Products.Domain.DTOs;

/// <summary>
/// Standard class to be used as a response on every single api call
/// </summary>
/// <typeparam name="T"></typeparam>
public class Result<T> where T : class
{
    /// <summary>
    /// Generic object which stores data to be returned if any.
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// General message from to be returned to the consumer of the api
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Status code used to be set on the http response
    /// </summary>
    public HttpStatusCode State { get; }

    /// <summary>
    /// Validates if the response on the object contains an error. Status != 200
    /// </summary>
    public bool IsError => State != HttpStatusCode.OK;

    /// <summary>
    /// Validates if the response on the object is success. Status == 200
    /// </summary>
    public bool IsSuccess => State == HttpStatusCode.OK;

    /// <summary>
    /// List of strings with all the errors if any. On front end we should always use this error array.
    /// </summary>
    public IEnumerable<string> Errors { get; set; } = new List<string> { };

    private Result(T data, string message, HttpStatusCode state, IEnumerable<string> errors = null)
    {
        this.Data = data;
        this.Message = message;
        this.State = state;
        if (errors != null)
            this.Errors = errors;
    }

    public Result()
    {
    }

    /// <summary>
    /// Creates a success Result object with no data to return
    /// </summary>
    /// <param name="message">Message of the response</param>
    /// <returns></returns>
    public static Result<T> Ok(string message)
    {
        return new Result<T>(null, message, HttpStatusCode.OK);
    }

    /// <summary>
    /// Creates a success Result object with a data object to return
    /// </summary>
    /// <param name="message">Message of the response<</param>
    /// <param name="data">Data object to be returned</param>
    /// <returns></returns>
    public static Result<T> Ok(string message, T data)
    {
        return new Result<T>(data, message, HttpStatusCode.OK);
    }

    /// <summary>
    /// Creates an error Result object with no data to return.
    /// </summary>
    /// <param name="message">Message of the response. It will be added to the errors list</param>
    /// <param name="state">Http status code of the error. Default status code is 400(Bad Request)</param>
    /// <returns></returns>
    public static Result<T> Fail(string message, HttpStatusCode state = HttpStatusCode.BadRequest)
    {
        var result = new Result<T>(null, message, state);
        result.Errors = new List<string> { message };
        return result;
    }

    /// <summary>
    /// Creates an error Result object with no data to return.
    /// </summary>
    /// <param name="message">Message of the response</param>
    /// <param name="errors">List of strings with all the errors to pass to the caller</param>
    /// <param name="state">Http status code of the error. Default status code is 400(Bad request)</param>
    /// <returns></returns>
    public static Result<T> Fail(string message, IEnumerable<string> errors, HttpStatusCode state = HttpStatusCode.BadRequest)
    {
        var result = Fail(message, state);
        if (errors.Count() > 0)
            result.Errors = errors;
        return result;
    }

    /// <summary>
    /// Creates an error Result object with a data object to return
    /// </summary>
    /// <param name="message">Message of the response</param>
    /// <param name="errors">List of strings with all the errors to pass to the caller</param>
    /// <param name="data">Data object to be returned</param>
    /// <param name="state">Http status code of the error. Default status code is 400(Bad requst)</param>
    /// <returns></returns>
    public static Result<T> Fail(string message, IEnumerable<string> errors, T data, HttpStatusCode state = HttpStatusCode.BadRequest)
    {
        var result = Fail(message, errors, state);
        result.Data = data;
        return result;
    }

}