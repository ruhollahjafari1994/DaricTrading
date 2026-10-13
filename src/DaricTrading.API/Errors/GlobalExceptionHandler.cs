using DaricTrading.Application.Orders;
using DaricTrading.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DaricTrading.API.Errors;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var result = exception switch
        {
            ValidationException validation => Results.ValidationProblem(
                validation.Errors.GroupBy(error => error.PropertyName)
                    .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).Distinct().ToArray()),
                statusCode: StatusCodes.Status400BadRequest,
                title: "Request validation failed."),
            NotFoundException notFound => Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Resource not found.", detail: notFound.Message),
            DomainRuleViolationException ruleViolation => Results.Problem(statusCode: StatusCodes.Status422UnprocessableEntity, title: "Business rule violation.", detail: ruleViolation.Message),
            _ => null
        };

        if (result is not null)
        {
            await result.ExecuteAsync(httpContext);
            return true;
        }

        logger.LogError(exception, "Unhandled exception while processing {Method} {Path}.", httpContext.Request.Method, httpContext.Request.Path);
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "An unexpected error occurred.").ExecuteAsync(httpContext);
        return true;
    }
}
