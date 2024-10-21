using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace worklog_api.error;

public class ExceptionMidddleware
{
    private readonly RequestDelegate _next;
    private readonly ProblemDetailsFactory _problemDetailsFactory;

    public ExceptionMidddleware(RequestDelegate next, ProblemDetailsFactory problemDetailsFactory)
    {
        _next = next;
        _problemDetailsFactory = problemDetailsFactory;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private  Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            NotFoundException _ => (StatusCodes.Status404NotFound, "Resource not found"),
            InternalServerError _ => (StatusCodes.Status500InternalServerError, "Internal Server Error"),
            BadRequestException _ => (StatusCodes.Status400BadRequest, "Bad Request"),
            AuthorizationException _ => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ForbiddenException _ => (StatusCodes.Status403Forbidden, "Forbidden"),
            _ => (StatusCodes.Status500InternalServerError, "Internal server error")
        };

        context.Response.StatusCode = statusCode;
        var problemDetails = _problemDetailsFactory.CreateProblemDetails(
            context,
            statusCode: statusCode,
            title: message
        );
        problemDetails.Extensions.Add("errors",exception.Message);
        return context.Response.WriteAsJsonAsync(problemDetails);
    }
}