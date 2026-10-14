using DaricTrading.Application.Common;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DaricTrading.API.Errors;

/// <summary>خطاهای برنامه را به پاسخ‌های استاندارد HTTP و ProblemDetails تبدیل می‌کند.</summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    /// <summary>خطا را بررسی و در صورت پشتیبانی به پاسخ HTTP تبدیل می‌کند.</summary>
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
            OptimisticConcurrencyException conflict => Results.Problem(statusCode: StatusCodes.Status409Conflict, title: "Concurrency conflict.", detail: conflict.Message),
            BusinessRuleViolationException ruleViolation => Results.Problem(statusCode: StatusCodes.Status422UnprocessableEntity, title: "Business rule violation.", detail: ruleViolation.Message),
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

