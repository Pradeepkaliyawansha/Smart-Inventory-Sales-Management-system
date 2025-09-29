using System.Net;
using System.Text.Json;
using InventoryAPI.Services; // <-- FIX 1: Add this using directive
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace InventoryAPI.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(
            RequestDelegate next, 
            ILogger<ExceptionMiddleware> logger, 
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception: {ExceptionMessage}", ex.Message);
                
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Determine the correct status code and response based on exception type
                var statusCode = (int)HttpStatusCode.InternalServerError;
                var message = "An internal server error has occurred.";

                switch (ex)
                {
                    case UnauthorizedAccessException unauthorizedEx:
                        statusCode = (int)HttpStatusCode.Unauthorized;
                        message = unauthorizedEx.Message;
                        break;
                    case ArgumentException argumentEx:
                        statusCode = (int)HttpStatusCode.BadRequest;
                        message = argumentEx.Message;
                        break;
                    case NotFoundException notFoundEx:
                        statusCode = (int)HttpStatusCode.NotFound;
                        message = notFoundEx.Message;
                        break;
                    // FIX 2: Check for BadRequestException and use a unique variable name (e.g., badRequestEx)
                    case BadRequestException badRequestEx:
                        statusCode = (int)HttpStatusCode.BadRequest;
                        message = badRequestEx.Message;
                        break;
                    default:
                        // Use the default status code and message for general exceptions
                        // Log full details for internal server errors
                        _logger.LogError(ex, "A critical unhandled error occurred: {Message}", ex.Message);
                        break;
                }

                // Create the error response object
                var response = new
                {
                    statusCode = statusCode,
                    message = message,
                    // Only include StackTrace in Development environment
                    details = _env.IsDevelopment() ? ex.StackTrace?.ToString() : null 
                };

                context.Response.StatusCode = statusCode;

                var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(response, jsonOptions);

                await context.Response.WriteAsync(json);
            }
        }
    }
}