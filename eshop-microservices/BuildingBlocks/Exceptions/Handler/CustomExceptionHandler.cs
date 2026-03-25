using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Exceptions.Handler
{
    public class CustomExceptionHandler(ILogger<CustomExceptionHandler> logger) : IExceptionHandler
    {
        private readonly ILogger<CustomExceptionHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));
            if (exception is null) throw new ArgumentNullException(nameof(exception));

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response has already started. Unable to write error response for {TraceId}.", context.TraceIdentifier);
                return false;
            }

            _logger.LogError(exception, "Error occurred (TraceId: {TraceId})", context.TraceIdentifier);

            int statusCode = exception switch
            {
                InternalServerException => StatusCodes.Status500InternalServerError,
                ValidationException => StatusCodes.Status400BadRequest,
                BadRequestException => StatusCodes.Status400BadRequest,
                NotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Title = exception.GetType().Name,
                Detail = exception.Message,
                Status = statusCode,
                Instance = context.Request?.Path
            };

            problemDetails.Extensions["traceId"] = context.TraceIdentifier;

            if (exception is ValidationException validationException)
            {
                var errors = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                problemDetails.Extensions["ValidationErrors"] = errors;
            }

            await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);
            return true;
        }
    }
}
