using System.Net;
using System.Text.Json;
using InventoryAPI.Exceptions;

namespace InventoryAPI.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                await HandleExceptionAsync(httpContext, ex);
            }
        }
        
        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = new ErrorResponse();
            
            switch (exception)
            {
                case NotFoundException ex:
                    response.Message = ex.Message;
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                    
                case BadRequestException ex:
                case ArgumentException ex:
                    response.Message = ex.Message;
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                    
                case UnauthorizedAccessException ex:
                    response.Message = ex.Message;
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;
                    
                default:
                    response.Message = "An internal server error occurred.";
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }
            
            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
    
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public object Details { get; set; }
    }
}