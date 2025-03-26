using MF_SecureApi.Data;
using MF_SecureApi.Models;
using System.Net;
using System.Text.Json;

namespace MF_SecureApi.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, dbContext);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex, AppDbContext dbContext)
        {
            _logger.LogError(ex, "Unhandled exception occurred!");

            // Log error details in the database
            var errorLog = new ErrorLog
            {
                Message = ex.Message,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                RequestPath = context.Request.Path,
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                LoggedAt = DateTime.UtcNow
            };

            try
            {
                dbContext.ErrorLogs.Add(errorLog);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Failed to save error log to database.");
            }

            // Return a generic error response
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var errorResponse = new { message = "An unexpected error occurred. Please try again later." };
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }
}