using Kela.Api.Common.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Api.Common.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult<ApiResponse<T>> Success<T>(T data, string message = "", int statusCode = 200)
        => StatusCode(statusCode, ApiResponse.Success(data, message, statusCode));

    protected ActionResult<ApiResponse<string>> SuccessMessage(string message, int statusCode = 200)
        => StatusCode(statusCode, ApiResponse.Success(message, statusCode));

    protected ActionResult<ApiResponse<object?>> Failure(
        string message, int statusCode = 400, object? data = null,
        IDictionary<string, string[]>? errors = null)
        => StatusCode(statusCode, ApiResponse.Error(message, statusCode, data, errors));
}
